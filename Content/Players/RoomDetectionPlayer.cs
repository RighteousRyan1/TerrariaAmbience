using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System.Diagnostics;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content.Players;

public class RoomDetectionPlayer : ModSystem {
    // parameters to prevent checking the entire world or checking excessively
    static int MAX_ROOM_WIDTH = 100;
    static int MAX_ROOM_HEIGHT = 100;
    static int MAX_ROOM_AREA = 2000;

    public static bool IsTileSolid(Tile tile) {
        // note to self: Main.tileBlockLight to false!
        // tile is solid and unactuated
        if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && !tile.IsActuated)
            return true;

        // platforms are invalid
        /*if (tile.HasTile && Main.tileSolidTop[tile.TileType])
            return true;*/

        // closed doors are solid
        if (tile.HasTile && (tile.TileType == TileID.OpenDoor || tile.TileType == TileID.ClosedDoor) && tile.TileFrameY == 0)
            return true;

        // TODO: fix any problems by editing true or false cases

        return false;
    }

    /// <summary>
    /// Checks if a player is in an enclosed space with proper walls.
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <param name="room">If not null, will be filled with room size and other details</param>
    /// <param name="requireWalls">Whether to require player-placed walls in the enclosed space</param>
    /// <returns>True if in enclosed space with walls, false otherwise</returns>
    public static bool IsPlayerInRoom(Player player, Room room, bool requireWalls = true) {
        room.Tiles.Clear();
        int originX = (int)(player.Center.X / 16);
        int originY = (int)(player.Center.Y / 16);

        if (!WorldGen.InWorld(originX, originY))
            return false;

        // visited is a window around the player
        bool[,] visited = new bool[MAX_ROOM_WIDTH * 2, MAX_ROOM_HEIGHT * 2];
        Queue<Point> queue = new();

        // seed
        queue.Enqueue(new Point(originX, originY));
        visited[MAX_ROOM_WIDTH, MAX_ROOM_HEIGHT] = true;

        int minX = originX, maxX = originX;
        int minY = originY, maxY = originY;
        int areaCount = 0;
        bool reachedEdge = false;
        bool foundMissingWall = false;

        while (queue.Count > 0) {
            var dq = queue.Dequeue();
            int x = dq.X, y = dq.Y;
            areaCount++;

            // update bounds
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;

            if (!WorldGen.InWorld(x, y)) {
                reachedEdge = true;
                break;
            }

            // current tile & properties
            Tile cur = Main.tile[x, y];
            bool curSolid = IsTileSolid(cur);
            bool curHasWall = cur.WallType > 0;

            if (requireWalls && !curSolid && !curHasWall) {
                foundMissingWall = true;
                // break to save processing time if i want to lol
                
            }

            // bounds/area limits
            if (areaCount > MAX_ROOM_AREA ||
                (maxX - minX) > MAX_ROOM_WIDTH ||
                (maxY - minY) > MAX_ROOM_HEIGHT) {
                reachedEdge = true;
                break;
            }

            // proximity to world edge
            if (x <= 5 || x >= Main.maxTilesX - 5 || y <= 5 || y >= Main.maxTilesY - 5) {
                reachedEdge = true;
                break;
            }

            // enqueue neighbors (we pass origin to compute visited indices correctly)
            CheckAndEnqueue(x + 1, y, originX, originY, queue, room, visited);
            CheckAndEnqueue(x - 1, y, originX, originY, queue, room, visited);
            CheckAndEnqueue(x, y + 1, originX, originY, queue, room, visited);
            CheckAndEnqueue(x, y - 1, originX, originY, queue, room, visited);
        }

        bool isEnclosed = !reachedEdge && (!requireWalls || !foundMissingWall);

        if (isEnclosed) {
            room.Width = maxX - minX + 1;
            room.Height = maxY - minY + 1;
            room.Area = areaCount;
            room.MinX = minX;
            room.MinY = minY;
            room.MaxX = maxX;
            room.MaxY = maxY;
            room.WallsSatisfied = !foundMissingWall;
        }

        return isEnclosed;
    }

    static void CheckAndEnqueue(int x, int y, int originX, int originY, Queue<Point> queue, Room room, bool[,] visited) {
        if (!WorldGen.InWorld(x, y))
            return;

        Tile tile = Main.tile[x, y];

        var pt = new Point(x, y);
        if (!room.Tiles.Contains(pt))
            room.Tiles.Add(pt);

        var isSolid = IsTileSolid(tile);

        // stop flood at solid tiles
        if (isSolid)
            return;

        int relX = x - originX + MAX_ROOM_WIDTH;
        int relY = y - originY + MAX_ROOM_HEIGHT;

        if (relX < 0 || relX >= MAX_ROOM_WIDTH * 2 || relY < 0 || relY >= MAX_ROOM_HEIGHT * 2)
            return;

        if (visited[relX, relY])
            return;

        visited[relX, relY] = true;
        queue.Enqueue(pt);
    }

    // bool _wasInRoom = false;

    public static Room PlayerRoom = new();
    public static bool IsInRoom { get; private set; }

    public override void PostUpdateEverything() {
        if (Main.GameUpdateCount % ModContent.GetInstance<AudioAdditionsConfig>().audioFiltersRefreshTime != 0) return;

        MAX_ROOM_WIDTH = 50;
        MAX_ROOM_HEIGHT = 50;

        IsInRoom = IsPlayerInRoom(Main.LocalPlayer, PlayerRoom);

        /*if (IsInRoom && !_wasInRoom) {
            Main.NewText($"Entered room! Size: {_currentRoom.Width}x{_currentRoom.Height}, Area: {_currentRoom.Area}");
        }
        else if (!IsInRoom && _wasInRoom) {
            Main.NewText("Left enclosed space");
        }*/

        // _wasInRoom = IsInRoom;
    }
}
public class Room {
    public int Width { get; set; }
    public int Height { get; set; }
    public int Area { get; set; }
    public int MinX { get; set; }
    public int MinY { get; set; }
    public int MaxX { get; set; }
    public int MaxY { get; set; }
    public bool WallsSatisfied { get; set; } // = true..?

    public List<Point> Tiles = [];
}
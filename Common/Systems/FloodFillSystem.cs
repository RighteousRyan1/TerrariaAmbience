using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using TerrariaAmbience.Core;
using TerrariaAmbience.Sounds.SoundFilters;

namespace TerrariaAmbience.Common.Systems;

public class FloodFillSystem : ModSystem {
    // parameters to prevent checking the entire world or checking excessively
    internal static int MaxRoomWidth = 50;
    internal static int MaxRoomHeight = 50;
    internal static int MaxRoomArea = 5000; // 2250;

    public static bool IsTileSolid(Tile tile) {
        // note to self: Main.tileBlockLight to false!
        // tile is solid and unactuated
        if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && !tile.IsActuated)
            return true;

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
    public static bool IsWithinRoom(Vector2 position, Room room, bool requireWalls = true) {
        room.NumWalls = room.NumSolidTiles = room.NumEmpty = 0;
        room.Tiles.Clear();
        int originX = (int)(position.X / 16);
        int originY = (int)(position.Y / 16);

        if (!WorldGen.InWorld(originX, originY))
            return false;

        // visited is a window around the player
        bool[,] visited = new bool[MaxRoomWidth * 2, MaxRoomHeight * 2];
        Queue<Point> queue = new();

        // seed
        queue.Enqueue(new Point(originX, originY));
        visited[MaxRoomWidth, MaxRoomHeight] = true;

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
            // only consider the edge being reached if there was a missing wall found
            if ((areaCount > MaxRoomArea ||
                maxX - minX > MaxRoomWidth ||
                maxY - minY > MaxRoomHeight) &&
                foundMissingWall) {
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

        room.PercentEnclosed = MathHelper.Clamp(1f - (room.NumEmpty / (float)(room.NumSolidTiles + room.NumWalls)), 0, 1);
        // Main.NewText(room.PercentEnclosed);

        return isEnclosed;
    }

    static void CheckAndEnqueue(int x, int y, int originX, int originY, Queue<Point> queue, Room room, bool[,] visited) {
        if (!WorldGen.InWorld(x, y))
            return;

        Tile tile = Main.tile[x, y];

        var isSolid = IsTileSolid(tile);

        var pt = new Point(x, y);
        if (!room.Tiles.Contains(pt)) {
            room.Tiles.Add(pt);

            if (isSolid)
                room.NumSolidTiles++;
            else {
                if (tile.WallType > 0)
                    room.NumWalls++;
                else
                    room.NumEmpty++;
            }
        }

        // stop flood at solid tiles
        if (isSolid)
            return;

        int relX = x - originX + MaxRoomWidth;
        int relY = y - originY + MaxRoomHeight;

        if (relX < 0 || relX >= MaxRoomWidth * 2 || relY < 0 || relY >= MaxRoomHeight * 2)
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
        if (Main.GameUpdateCount % ModContent.GetInstance<AudioConfig>().audioFiltersRefreshTime != 0) return;

        /*var t = (int)(Main.MouseScreen.X / Main.screenWidth * 100);
        MAX_ROOM_WIDTH = t;
        MAX_ROOM_HEIGHT = t;

        MAX_ROOM_AREA = 5000;*/

        IsInRoom = IsWithinRoom(SoundFilterSystem.ScreenListeningPosition, PlayerRoom);

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
    public int NumSolidTiles { get; set; }
    public int NumWalls { get; set; }
    public int NumEmpty { get; set; }

    public bool WallsSatisfied { get; set; } // = true..?
    public float PercentEnclosed { get; set; }
    public List<Point> Tiles = [];
}
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

public class RoomDetectionPlayer : ModSystem
{
    // parameters to prevent checking the entire world or checking excessively
    private const int MAX_ROOM_WIDTH = 100;
    private const int MAX_ROOM_HEIGHT = 100;
    private const int MAX_ROOM_AREA = 2000;

    public static bool IsTileSolid(Tile tile) {
        // tile is solid and unactuated
        if (tile.HasTile && Main.tileSolid[tile.TileType] && !tile.IsActuated)
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
    /// <param name="roomDetails">If not null, will be filled with room size and other details</param>
    /// <param name="requireWalls">Whether to require player-placed walls in the enclosed space</param>
    /// <returns>True if in enclosed space with walls, false otherwise</returns>
    public static bool IsPlayerInEnclosedSpace(Player player, RoomDetails roomDetails = null, bool requireWalls = true) {
        int playerTileX = (int)(player.Center.X / 16);
        int playerTileY = (int)(player.Center.Y / 16);

        var tile = Main.tile[playerTileX, playerTileY];
        // bool isTileSolid = IsTileSolid(tile);

        bool hasValidWall = tile.WallType > 0;

        // prevent IOOB checks
        if (!WorldGen.InWorld(playerTileX, playerTileY))
            return false;

        /*bool isInPermeableTile = tile.HasTile &&
            (tile.TileType == TileID.OpenDoor || tile.TileType == TileID.ClosedDoor ||
            tile.TileType == TileID.Platforms);*/

        /*if (isTileSolid && isInPermeableTile)
            return false;*/

        // HasTile check is necessary since doors that do not have walls behind them may make this check return false.
        if (requireWalls && !hasValidWall && !tile.HasTile)
            return false;

        bool[,] visited = new bool[MAX_ROOM_WIDTH * 2, MAX_ROOM_HEIGHT * 2];

        Queue<Point> queue = [];

        // queue the player position first
        queue.Enqueue(new Point(playerTileX, playerTileY));
        visited[MAX_ROOM_WIDTH, MAX_ROOM_HEIGHT] = true;

        int minX = playerTileX;
        int maxX = playerTileX;
        int minY = playerTileY;
        int maxY = playerTileY;
        int areaCount = 0;
        bool reachedEdge = false;
        bool foundMissingWall = false;

        while (queue.Count > 0) {
            Point current = queue.Dequeue();
            int x = current.X;
            int y = current.Y;

            areaCount++;

            // update bounds
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);

            // Check if tile has a valid wall
            if (requireWalls && !hasValidWall && !tile.HasTile) {
                //foundMissingWall = true;
                // immediately fail if any tile lacks a wall... will save performance
                return false;
                // or...
                // continue filling but mark as not enclosed
                // reachedEdge = true;
                // or...
                // continue filling and mark this tile as the closet valid tile without a wall

            }

            // ensure that it's smaller than our checked dimensions
            if (areaCount > MAX_ROOM_AREA ||
                (maxX - minX) > MAX_ROOM_WIDTH ||
                (maxY - minY) > MAX_ROOM_HEIGHT) {
                reachedEdge = true;
                break;
            }

            // check for world edge
            if (x <= 5 || x >= Main.maxTilesX - 5 || y <= 5 || y >= Main.maxTilesY - 5) {
                reachedEdge = true;
                break;
            }

            // check in cardinal directions
            CheckAndEnqueue(x + 1, y, queue, visited, requireWalls);
            CheckAndEnqueue(x - 1, y, queue, visited, requireWalls);
            CheckAndEnqueue(x, y + 1, queue, visited, requireWalls);
            CheckAndEnqueue(x, y - 1, queue, visited, requireWalls);
        }

        bool isEnclosed = !reachedEdge && (!requireWalls || !foundMissingWall);

        // give the details of the room if one is passed in
        if (roomDetails != null && isEnclosed) {
            roomDetails.Width = maxX - minX + 1;
            roomDetails.Height = maxY - minY + 1;
            roomDetails.Area = areaCount;
            roomDetails.MinX = minX;
            roomDetails.MinY = minY;
            roomDetails.MaxX = maxX;
            roomDetails.MaxY = maxY;
            roomDetails.WallsSatisfied = !foundMissingWall;
        }

        return isEnclosed;
    }

    private static void CheckAndEnqueue(int x, int y, Queue<Point> queue, bool[,] visited, bool requireWalls) {
        var tile = Main.tile[x, y];

        int relX = x + MAX_ROOM_WIDTH - (int)(Main.LocalPlayer.position.X / 16);
        int relY = y + MAX_ROOM_HEIGHT - (int)(Main.LocalPlayer.position.Y / 16);

        if (relX < 0 || relX >= MAX_ROOM_WIDTH * 2 || relY < 0 || relY >= MAX_ROOM_HEIGHT * 2)
            return;

        // skip this tile if it's been visisted already
        if (visited[relX, relY])
            return;

        // skip outside of the world bounds
        if (!WorldGen.InWorld(x, y))
            return;

        // skip if solid
        if (IsTileSolid(tile)) {
            // literal mario_cumming checks
            // Main.NewText(TileID.Search.GetName(tile.TileType));
            // Main.NewText(TileID.Search.GetName(tile.TileType) + ": " + Main.tileSolidTop[tile.TileType] + " " + Main.tileSolid[tile.TileType]);
            return;
        }

        // set as visited
        visited[relX, relY] = true;
        queue.Enqueue(new Point(x, y));
    }

    private bool _wasInRoom = false;
    private RoomDetails _currentRoom = new();

    public static bool IsInRoom { get; private set; }

    public override void PostUpdateEverything() {
        // 4 times a second might be excessive...? idk.
        // now every frame. but make it configurable, methinks

        if (!ModContent.GetInstance<AudioAdditionsConfig>().floodFillAmbientOcclusion)
            return;

        IsInRoom = IsPlayerInEnclosedSpace(Main.LocalPlayer, _currentRoom);

        /*if (IsInRoom && !_wasInRoom) {
            Main.NewText($"Entered room! Size: {_currentRoom.Width}x{_currentRoom.Height}, Area: {_currentRoom.Area}");
        }
        else if (!IsInRoom && _wasInRoom) {
            Main.NewText("Left enclosed space");
        }*/

        _wasInRoom = IsInRoom;
    }
}
public class RoomDetails
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Area { get; set; }
    public int MinX { get; set; }
    public int MinY { get; set; }
    public int MaxX { get; set; }
    public int MaxY { get; set; }
    public bool WallsSatisfied { get; set; } = true;
}
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using TerrariaAmbience.Core;
using TerrariaAmbience.Sounds.SoundFilters;
using System;

namespace TerrariaAmbience.Common.Systems;

public class FloodFillSystem : ModSystem {
    // parameters to prevent checking the entire world or checking excessively
    internal static int MaxRoomWidth = 50;
    internal static int MaxRoomHeight = 50;
    internal static int MaxRoomArea = 3000; // 2250;

    static readonly bool[,] _visitedCache = new bool[MaxRoomWidth * 2 + 1, MaxRoomHeight * 2 + 1];
    static readonly Queue<Point> _queueCache = new(MaxRoomArea);
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
    /// <param name="room">If not null, will be filled with room size and other details</param>
    /// <param name="requireWalls">Whether to require player-placed walls in the enclosed space</param>
    /// <returns>True if in enclosed space with walls, false otherwise</returns>
    public static bool IsWithinRoom(Vector2 position, Room room, bool requireWalls = true) {
        room.NumWalls = room.NumSolidTiles = room.NumEmpty = 0;
        room.Tiles.Clear();

        Array.Clear(_visitedCache, 0, _visitedCache.Length);
        _queueCache.Clear();

        int originX = (int)(position.X / 16);
        int originY = (int)(position.Y / 16);

        if (!WorldGen.InWorld(originX, originY))
            return false;

        _queueCache.Enqueue(new Point(originX, originY));

        int cacheCenterX = MaxRoomWidth;
        int cacheCenterY = MaxRoomHeight;
        _visitedCache[cacheCenterX, cacheCenterY] = true;

        int minX = originX, maxX = originX;
        int minY = originY, maxY = originY;
        int areaCount = 0;
        bool reachedEdge = false;
        bool foundMissingWall = false;

        // process each point
        while (_queueCache.Count > 0) {
            Point p = _queueCache.Dequeue();
            int x = p.X;
            int y = p.Y;

            Tile cur = Main.tile[x, y];
            bool curSolid = IsTileSolid(cur);
            bool curHasWall = cur.WallType > 0;

            room.Tiles.Add(p);

            if (curSolid) room.NumSolidTiles++;
            else if (curHasWall) room.NumWalls++;
            else room.NumEmpty++;

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;

            if (curSolid) continue;

            areaCount++;

            if (requireWalls && !curHasWall) {
                foundMissingWall = true;
                // again, i could break here, but then we wouldn't get accurate area/size data
            }

            // limits of the "room"
            if (areaCount >= MaxRoomArea ||
                maxX - minX >= MaxRoomWidth ||
                maxY - minY >= MaxRoomHeight) {
                reachedEdge = true;
                break;
            }

            // edge of world counts as a wall
            if (x <= 5 || x >= Main.maxTilesX - 5 || y <= 5 || y >= Main.maxTilesY - 5) {
                reachedEdge = true;
                break;
            }

            // check neighbors
            EnqueueNeighbor(x + 1, y, originX, originY, cacheCenterX, cacheCenterY);
            EnqueueNeighbor(x - 1, y, originX, originY, cacheCenterX, cacheCenterY);
            EnqueueNeighbor(x, y + 1, originX, originY, cacheCenterX, cacheCenterY);
            EnqueueNeighbor(x, y - 1, originX, originY, cacheCenterX, cacheCenterY);
        }

        // 4. Final Calculations
        if (!foundMissingWall) {
            room.Width = maxX - minX + 1;
            room.Height = maxY - minY + 1;
            room.Area = areaCount;
            room.MinX = minX;
            room.MinY = minY;
            room.MaxX = maxX;
            room.MaxY = maxY;
            room.WallsSatisfied = true;
        }
        else {
            room.WallsSatisfied = false;
        }

        bool isEnclosed = !reachedEdge && (!requireWalls || !foundMissingWall);

        // DivideByZeroException my beloved
        float denominator = room.NumSolidTiles + room.NumWalls;
        if (denominator == 0) room.PercentEnclosed = 0;
        else {
            float calculation = 1f - (room.NumEmpty / denominator);
            room.PercentEnclosed = MathHelper.Clamp(calculation, 0, 1);
        }

        return isEnclosed;
    }

    static void EnqueueNeighbor(int x, int y, int originX, int originY, int cacheCenterX, int cacheCenterY) {
        if (!WorldGen.InWorld(x, y)) return;

        // check relative to the origin
        int relX = x - originX + cacheCenterX;
        int relY = y - originY + cacheCenterY;

        if (relX < 0 || relX >= _visitedCache.GetLength(0) ||
            relY < 0 || relY >= _visitedCache.GetLength(1)) return;

        // exit if visited
        if (_visitedCache[relX, relY]) return;

        // done
        _visitedCache[relX, relY] = true;
        _queueCache.Enqueue(new Point(x, y));
    }

    // bool _wasInRoom = false;

    public static Room PlayerRoom = new();
    public static bool IsInRoom { get; private set; }

    public override void PostUpdateEverything() {
        if (Main.dedServ) return;
        if (Main.GameUpdateCount % ModContent.GetInstance<AudioConfig>().audioFiltersRefreshTime != 0) return;

        /*var t = (int)(Main.MouseScreen.X / Main.screenWidth * 100);
        MAX_ROOM_WIDTH = t;
        MAX_ROOM_HEIGHT = t;

        MAX_ROOM_AREA = 5000;*/
        IsInRoom = IsWithinRoom(SoundFilterSystem.ScreenListeningPosition, PlayerRoom);

        // Main.NewText(PlayerRoom.Area);

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

    public bool WallsSatisfied { get; set; }
    public float PercentEnclosed { get; set; }
    public List<Point> Tiles = new(1000);
}
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;
using Microsoft.Xna.Framework;
using TerrariaAmbience.Sounds.SFXEffects.FAudioHacks;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace TerrariaAmbience.Sounds.SFXEffects;

public class ReverbAudioSystem : ModSystem
{
    static HashSet<int> _lowReverbWalls = [];
    static HashSet<int> _lowReverbTiles = [];
    static HashSet<int> _noReverbWalls = [];
    static HashSet<int> _noReverbTiles = [];

    // no reverb set is prioritized over the low reverb set
    // so that if something like BambooFence exists, it will be considered as to compute no reverb
    static HashSet<string> _noReverbNames = [
        "silt", "slush", "grass", "mud", "clay",
        "grass", "leaf", "leaves", "flower", "vine", "moss",
        "snow", "ash", "fence"
    ];
    static HashSet<string> _lowReverbNames = [
        "dirt", "sand", "slush", "glass", "plank", "mud",

        "sand", "silt", "dirt", "plank", "bamboo", "glass",
        "ice", "tin", "wood"
    ];
    // support mods soon too...
    public static void PrecomputeReverbProperties() {
        // compute wall reverb properties
        for (int i = 0; i < WallID.Search.Count; i++) {
            string name = WallID.Search.GetName(i).ToLower();

            if (_noReverbNames.Contains(name))
                _noReverbWalls.Add(i);

            // only runs if it doesn't exist in the no reverb set to go in-hand with the comment left above _noReverbNames
            else if (_lowReverbNames.Contains(name))
                _lowReverbWalls.Add(i);
        }

        // compute tile reverb properties
        for (int i = 0; i < TileID.Search.Count; i++) {
            string name = TileID.Search.GetName(i).ToLower();

            if (_noReverbNames.Contains(name))
                _noReverbTiles.Add(i);

            // same here as well
            else if (_lowReverbNames.Contains(name))
                _lowReverbTiles.Add(i);
        }
    }
    public static bool CanFindEscapeRoute(Vector2 position, int dist) {
        Point tilePos = position.ToTileCoordinates();
        int startX = tilePos.X - dist, endX = tilePos.X + dist;
        int startY = tilePos.Y - dist, endY = tilePos.Y + dist;

        for (int i = startX; i <= endX; i++) {
            for (int j = startY; j <= endY; j++) {
                Tile tile = Framing.GetTileSafely(i, j);
                if (!tile.HasTile && tile.WallType == 0) {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool HasWallNextTo(Vector2 fromPosition, Point position) {
        Point[] offsets = { new(0, -1), new(0, 1), new(-1, 0), new(1, 0) };
        foreach (var offset in offsets) {
            Point checkPos = new(position.X + offset.X, position.Y + offset.Y);
            Tile tile = Framing.GetTileSafely(checkPos.X, checkPos.Y);
            if (tile.WallType > 0 && !tile.HasTile && CanRaycastTo(fromPosition, checkPos.ToWorldCoordinates())) {
                return true;
            }
        }
        return false;
    }

    public static bool CanRaycastTo(Vector2 begin, Vector2 destination) =>
        Collision.CanHitLine(begin, 1, 1, destination, 1, 1);

    public static int TilesAround(Vector2 position, Point grid, out List<Point> tileCoords) {
        tileCoords = new();
        Point tilePos = position.ToTileCoordinates();
        int count = 0;

        for (int i = tilePos.X - grid.X; i <= tilePos.X + grid.X; i++) {
            for (int j = tilePos.Y - grid.Y; j <= tilePos.Y + grid.Y; j++) {
                Tile tile = Framing.GetTileSafely(i, j);
                if (tile.HasTile && tile.CollisionType() == 1) {
                    tileCoords.Add(new(i, j));
                    count++;
                }
            }
        }
        return count;
    }

    public static int TilesAround(Vector2 position, Point grid, out List<Tile> tiles) {
        tiles = new();
        Point tilePos = position.ToTileCoordinates();
        int count = 0;

        for (int i = tilePos.X - grid.X; i <= tilePos.X + grid.X; i++) {
            for (int j = tilePos.Y - grid.Y; j <= tilePos.Y + grid.Y; j++) {
                Tile tile = Framing.GetTileSafely(i, j);
                if (tile.HasTile && tile.CollisionType() == 1) {
                    tiles.Add(tile);
                    count++;
                }
            }
        }
        return count;
    }

    public static int TileObjectsAround(Vector2 position, Point grid, out List<Point> blocks, out List<Point> walls) {
        blocks = new();
        walls = new();
        Point tilePos = position.ToTileCoordinates();
        int count = 0;

        for (int i = tilePos.X - grid.X; i <= tilePos.X + grid.X; i++) {
            for (int j = tilePos.Y - grid.Y; j <= tilePos.Y + grid.Y; j++) {
                Tile tile = Framing.GetTileSafely(i, j);
                if (tile.HasTile) {
                    blocks.Add(new(i, j));
                    count++;
                }
                else if (tile.WallType > 0) {
                    walls.Add(new(i, j));
                    count++;
                }
            }
        }
        return count;
    }

    public static int WallsAround(Vector2 position, Point grid, out List<Point> tileCoords) {
        tileCoords = new();
        Point tilePos = position.ToTileCoordinates();
        int count = 0;

        for (int i = tilePos.X - grid.X; i <= tilePos.X + grid.X; i++) {
            for (int j = tilePos.Y - grid.Y; j <= tilePos.Y + grid.Y; j++) {
                Tile tile = Framing.GetTileSafely(i, j);
                if (tile.WallType > 0 && !tile.HasTile) {
                    tileCoords.Add(new(i, j));
                    count++;
                }
            }
        }
        return count;
    }

    public static int EmptyTilesAround(Vector2 position, int dist, bool condition, out List<Point> tileCoords) {
        tileCoords = new();
        if (!condition) return 0;

        Point tilePos = position.ToTileCoordinates();
        int count = 0;

        for (int i = tilePos.X - dist; i <= tilePos.X + dist; i++) {
            for (int j = tilePos.Y - dist; j <= tilePos.Y + dist; j++) {
                if (!Framing.GetTileSafely(i, j).HasTile) {
                    tileCoords.Add(new(i, j));
                    count++;
                }
            }
        }
        return count;
    }
    public static FilterParams CreateAudioFX(Vector2 fromV2, Vector2 offset = default) {
        var cfg = ModContent.GetInstance<AudioAdditionsConfig>();
        var fParam = new FilterParams();
        float reverbActual = 0f;

        if (Main.gameMenu)
            return fParam;

        bool playerUnderwater = Main.LocalPlayer.IsWaterSuffocating();
        bool playerSurface = Main.LocalPlayer.Center.Y < Main.worldSurface * 16; //Main.LocalPlayer.ZoneOverworldHeight;
        bool playerUnderground = !playerSurface; //Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneUnderworldHeight;


        if (!cfg.isReverbEnabled) {
            reverbActual = 0;
            SetFilterValues(fromV2, offset, ref fParam, playerUnderwater);
            return fParam;
        }
        else if (!cfg.advancedReverbCalculation) {
            reverbActual = MathUtils.InverseLerp((float)Main.worldSurface * 16, Main.maxTilesY * 16, Main.LocalPlayer.Center.Y) / 2;
            SetFilterValues(fromV2, offset, ref fParam, playerUnderwater);
            return fParam;
        }


        // start here as a base.
        fParam.Reverb = FAudioReverbController.DefaultFNAReverb;

        List<Point> wallPoints = [];
        List<Point> tilePoints = [];

        int seenWalls = 0;
        int seenTiles = 0;

        if (playerSurface && cfg.surfaceReverbCalculation) {
            WallsAround(fromV2, new Point(15, 15), out wallPoints);
            foreach (var pt in wallPoints) {
                var wall = Framing.GetTileSafely(pt).WallType;

                var isHighReverb = !_lowReverbWalls.Contains(wall) && !_noReverbWalls.Contains(wall);
                if (isHighReverb && CanRaycastTo(fromV2, pt.ToVector2() * 16 + offset)) {
                    reverbActual += 0.002f;
                    seenWalls++;
                }
                else if (_lowReverbWalls.Contains(wall) && CanRaycastTo(fromV2, pt.ToVector2() * 16 + offset)) {
                    reverbActual += 0.0005f;
                    seenWalls++;
                }
            }
            /*foreach (var tilePos in tilePoints) {
                var tileType = Framing.GetTileSafely(tilePos).TileType;
                Vector2 worldCoords = tilePos.ToVector2() * 16;

                // Determine main cardinal direction towards fromV2
                Vector2 direction = fromV2 - worldCoords;
                bool isHorizontal = Math.Abs(direction.X) > Math.Abs(direction.Y);

                // Get adjacent points in two directions facing fromV2
                Point primaryDir = isHorizontal ? new Point(tilePos.X + Math.Sign(direction.X), tilePos.Y)
                                                : new Point(tilePos.X, tilePos.Y + Math.Sign(direction.Y));

                Point secondaryDir = isHorizontal ? new Point(tilePos.X, tilePos.Y + Math.Sign(direction.Y))
                                                  : new Point(tilePos.X + Math.Sign(direction.X), tilePos.Y);

                // Perform two raycasts
                bool raycast1 = CanRaycastTo(fromV2, primaryDir.ToVector2() * 16);
                bool raycast2 = CanRaycastTo(fromV2, secondaryDir.ToVector2() * 16);

                if (!raycast1 && !raycast2)
                    continue;

                // for now, we assume that tiles and walls are "high reverb" if they aren't in the low reverb set or the no reverb set
                if (!_lowReverbTiles.Contains(tileType) && !_noReverbTiles.Contains(tileType)) {
                    highReverbSurfaces++;
                }
                else if (_lowReverbWalls.Contains(tileType)) {
                    lowReverbSurfaces++;
                }
            }*/
        }
        if (playerUnderground && cfg.ugReverbCalculation) {
            int highReverbSurfaces = 0, lowReverbSurfaces = 0;
            TileObjectsAround(fromV2, new Point(15, 15), out tilePoints, out wallPoints);

            foreach (var tilePos in tilePoints) {
                var tileType = Framing.GetTileSafely(tilePos).TileType;
                Vector2 worldCoords = tilePos.ToVector2() * 16;

                // Determine main cardinal direction towards fromV2
                Vector2 direction = fromV2 - worldCoords;
                bool isHorizontal = Math.Abs(direction.X) > Math.Abs(direction.Y);

                // Get adjacent points in two directions facing fromV2
                Point primaryDir = isHorizontal ? new Point(tilePos.X + Math.Sign(direction.X), tilePos.Y)
                                                : new Point(tilePos.X, tilePos.Y + Math.Sign(direction.Y));

                Point secondaryDir = isHorizontal ? new Point(tilePos.X, tilePos.Y + Math.Sign(direction.Y))
                                                  : new Point(tilePos.X + Math.Sign(direction.X), tilePos.Y);

                // Perform two raycasts
                bool raycast1 = CanRaycastTo(fromV2, primaryDir.ToVector2() * 16);
                bool raycast2 = CanRaycastTo(fromV2, secondaryDir.ToVector2() * 16);

                if (!raycast1 && !raycast2)
                    continue;

                seenTiles++;
                // for now, we assume that tiles and walls are "high reverb" if they aren't in the low reverb set or the no reverb set
                if (!_lowReverbTiles.Contains(tileType) && !_noReverbTiles.Contains(tileType)) {
                    highReverbSurfaces++;
                }
                else if (_lowReverbWalls.Contains(tileType)) {
                    lowReverbSurfaces++;
                }
            }

            foreach (var wallPos in wallPoints) {
                var wallType = Framing.GetTileSafely(wallPos).WallType;
                Vector2 worldCoords = wallPos.ToVector2() * 16;

                Vector2 direction = fromV2 - worldCoords;
                bool isHorizontal = Math.Abs(direction.X) > Math.Abs(direction.Y);

                Point primaryDir = isHorizontal ? new Point(wallPos.X + Math.Sign(direction.X), wallPos.Y)
                                                : new Point(wallPos.X, wallPos.Y + Math.Sign(direction.Y));

                Point secondaryDir = isHorizontal ? new Point(wallPos.X, wallPos.Y + Math.Sign(direction.Y))
                                                  : new Point(wallPos.X + Math.Sign(direction.X), wallPos.Y);

                bool raycast1 = CanRaycastTo(fromV2, primaryDir.ToVector2() * 16);
                bool raycast2 = CanRaycastTo(fromV2, secondaryDir.ToVector2() * 16);

                if (!raycast1 && !raycast2)
                    continue;

                seenWalls++;

                if (!_lowReverbWalls.Contains(wallType) && !_noReverbWalls.Contains(wallType)) {
                    highReverbSurfaces++;
                }
                else if (_lowReverbWalls.Contains(wallType)) {
                    lowReverbSurfaces++;
                }
            }

            reverbActual += highReverbSurfaces * 0.02f;
            reverbActual += lowReverbSurfaces * 0.005f;
        }

        fParam.Reverb.DecayTime = seenWalls * 0.008f;
        fParam.Reverb.ReflectionsDelay = (uint)seenWalls / 8;
        fParam.Reverb.EarlyDiffusion = (byte)MathUtils.InverseLerp(0, 15, seenTiles, true);
        fParam.Reverb.RoomFilterFreq = 10000f - (10 * seenWalls);
        fParam.Reverb.RoomSize = seenWalls;

        SetFilterValues(fromV2, offset, ref fParam, playerUnderwater);
        fParam.ReverbGain = MathF.Min(reverbActual, 1f);

        if (!ModContent.GetInstance<AudioAdditionsConfig>().isReverbEnabled)
            fParam.ReverbGain = 0f;
        if (!ModContent.GetInstance<AudioAdditionsConfig>().isSoundOcclusionEnabled)
            fParam.LowPassEnabled = false;
        if (!ModContent.GetInstance<AudioAdditionsConfig>().isSoundDampeningEnabled)
            fParam.BandPassEnabled = false;

        return fParam;
    }

    static void SetFilterValues(Vector2 fromV2, Vector2 offset, ref FilterParams fParam, bool playerUnderwater) {
        var goalPos = Main.screenPosition + Vector2.Transform(new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f - 5f), Main.GameViewMatrix.TransformationMatrix); // Main.LocalPlayer.Top;

        // Dust.NewDustPerfect(goalPos, DustID.SpelunkerGlowstickSparkle);

        float dist = Vector2.Distance(goalPos, fromV2 + offset);
        var numBlockingTiles = CountTilesTouched(goalPos.ToTileCoordinates(), (fromV2 + offset).ToTileCoordinates(), t => t.HasTile && Main.tileSolid[t.TileType]);

        float normalized = Math.Clamp(dist / 1500f, 0f, 1f);
        // p < 1 means approach slows down near 0
        float p = 0.975f; // sqrt curve — fast drop at first, slower and slower near 0
        float curve = 1f - MathF.Pow(normalized, p);

        fParam.LowPassIntensity = Math.Max(1f - numBlockingTiles / 50f / curve, 0f);
        fParam.LowPassEnabled = true;

        bool underWater = Collision.DrownCollision(fromV2, 1, 1);
        fParam.BandPassEnabled = underWater || playerUnderwater;
        fParam.BandPassIntensity = (playerUnderwater && !underWater) ? 0.0175f : (underWater && playerUnderwater) ? 0.01f : 0.025f;
    }

    public static int CountTilesTouched(Point start, Point end, Func<Tile, bool> predicate = null) {
        int count = 0;

        int x0 = start.X;
        int y0 = start.Y;
        int x1 = end.X;
        int y1 = end.Y;

        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true) {
            if (WorldGen.InWorld(x0, y0)) {
                Tile tile = Main.tile[x0, y0];

                if (predicate?.Invoke(tile) == true)
                    count++;
            }

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }

        return count;
    }

}

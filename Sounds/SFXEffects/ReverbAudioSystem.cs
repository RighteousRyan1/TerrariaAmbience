using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;
using Microsoft.Xna.Framework;

namespace TerrariaAmbience.Sounds.SFXEffects;

public class ReverbAudioSystem : ModSystem
{
    private static HashSet<int> _lowReverbWalls = [];
    private static HashSet<int> _lowReverbTiles = [];
    private static HashSet<int> _noReverbWalls = [];
    private static HashSet<int> _noReverbTiles = [];

    private static HashSet<string> _noReverbNames = [
        "silt", "slush", "grass",
        "fence", "leaf", "flower", "leaves", "snow"
    ];
    private static HashSet<string> _lowReverbNames = [
        "dirt", "sand", "slush", "glass", "plank", "mud"
    ];
    // support mods soon too...
    public static void PrecomputeReverbProperties() {
        for (int i = 0; i < WallID.Search.Count; i++) {

            string name = WallID.Search.GetName(i).ToLower();
            if (_noReverbNames.Contains(name))
                _noReverbWalls.Add(i);

            // "planked" if this does not work well
            else if (_lowReverbNames.Contains(name))
                _lowReverbWalls.Add(i);
        }

        for (int i = 0; i < TileID.Search.Count; i++) {
            string name = TileID.Search.GetName(i).ToLower();
            if (_noReverbNames.Contains(name))
                _noReverbTiles.Add(i);
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
    public static void CreateAudioFX(Vector2 fromV2, out float rvGain, out float occlusion, out float dampening, out bool shouldDampen, Vector2 offset = default) {
        var cfg = ModContent.GetInstance<AudioAdditionsConfig>();
        shouldDampen = false;
        dampening = 0f;
        rvGain = 0;
        occlusion = 1f;
        float reverbActual = 0f;

        if (Main.gameMenu)
            return;

        bool playerUnderwater = Main.LocalPlayer.IsWaterSuffocating();
        bool playerSurface = Main.LocalPlayer.Center.Y < Main.worldSurface * 16; //Main.LocalPlayer.ZoneOverworldHeight;
        bool playerUnderground = !playerSurface; //Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneUnderworldHeight;


        if (!cfg.isReverbEnabled) {
            reverbActual = 0;
            SetFilterValues(fromV2, offset, ref occlusion, ref rvGain, ref dampening, ref shouldDampen, reverbActual, playerUnderwater);
            return;
        }
        else if (!cfg.advancedReverbCalculation) {
            reverbActual = MathUtils.InverseLerp((float)Main.worldSurface * 16, Main.maxTilesY * 16, Main.LocalPlayer.Center.Y) / 2;
            SetFilterValues(fromV2, offset, ref occlusion, ref rvGain, ref dampening, ref shouldDampen, reverbActual, playerUnderwater);
            return;
        }


        if (playerSurface && cfg.surfaceReverbCalculation) {
            WallsAround(fromV2, new Point(15, 15), out List<Point> wallPoints);
            foreach (var pt in wallPoints) {
                var wall = Framing.GetTileSafely(pt).WallType;

                var isHighReverb = !_lowReverbWalls.Contains(wall) && !_noReverbWalls.Contains(wall);
                if (isHighReverb && CanRaycastTo(fromV2, pt.ToVector2() * 16 + offset))
                    reverbActual += 0.002f;
                else if (_lowReverbWalls.Contains(wall) && CanRaycastTo(fromV2, pt.ToVector2() * 16 + offset))
                    reverbActual += 0.0005f;
            }
        }
        if (playerUnderground && cfg.ugReverbCalculation) {
            int highReverbSurfaces = 0, lowReverbSurfaces = 0;
            TileObjectsAround(fromV2, new Point(15, 15), out var blockPoints, out var wallPoints);

            foreach (var tilePos in blockPoints) {
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
        SetFilterValues(fromV2, offset, ref occlusion, ref rvGain, ref dampening, ref shouldDampen, reverbActual, playerUnderwater);
    }

    static void SetFilterValues(Vector2 fromV2, Vector2 offset, ref float occlusion, ref float rvGain, ref float dampening, ref bool shouldDampen, float reverbActual, bool playerUnderwater) {
        float dist = Vector2.Distance(Main.LocalPlayer.Top, fromV2 + offset);
        bool hasLOS = CanRaycastTo(Main.LocalPlayer.Top, fromV2 + offset);

        occlusion = hasLOS ? 1f : Math.Max(0.01f, 1f - dist / 1000);
        rvGain = reverbActual;

        bool underWater = Collision.DrownCollision(fromV2, 1, 1);
        shouldDampen = underWater || playerUnderwater;
        dampening = (playerUnderwater && !underWater) ? 0.0175f : (underWater && playerUnderwater) ? 0.01f : 0.025f;
    }
}

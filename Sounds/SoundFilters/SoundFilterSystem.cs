using Microsoft.Win32;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Sounds.SoundFilters.FAudioHacks;

namespace TerrariaAmbience.Sounds.SoundFilters;

public class SoundFilterSystem : ModSystem {
    public static Vector2 ScreenListeningPosition => Vector2.Transform(new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f - 5f), Main.GameViewMatrix.TransformationMatrix);
    public static FilterParams LatestParams { get; set; }

    internal static HashSet<int> lowReverbWalls = [];
    internal static HashSet<int> lowReverbTiles = [];
    internal static HashSet<int> noReverbWalls = [];
    internal static HashSet<int> noReverbTiles = [];

    // no reverb set is prioritized over the low reverb set
    // so that if something like BambooFence exists, it will be considered as to compute no reverb
    static HashSet<string> _noReverbNames = [
        "silt", "slush", "grass", "mud", "clay",
        "grass", "leaf", "leaves", "flower", "vine", "moss",
        "snow", "ash", "fence", "hive"
    ];
    static HashSet<string> _lowReverbNames = [
        "dirt", "sand", "slush", "glass", "plank", "mud",

        "sand", "silt", "dirt", "plank", "bamboo", "glass",
        "ice", "tin", "wood"
    ];
    // support mods soon too...
    public static void PrecomputeReverbProperties() {
        // compute wall reverb properties
        for (int i = 0; i < /*WallID.Search.Count*/WallLoader.WallCount; i++) {
            string name = WallID.Search.GetName(i).ToLower();

            if (_noReverbNames.Any(name.Contains))
                noReverbWalls.Add(i);

            // only runs if it doesn't exist in the no reverb set to go in-hand with the comment left above _noReverbNames
            else if (_lowReverbNames.Any(name.Contains))
                lowReverbWalls.Add(i);
        }

        // compute tile reverb properties
        for (int i = 0; i < /*TileID.Search.Count*/TileLoader.TileCount; i++) {
            string name = TileID.Search.GetName(i).ToLower();

            if (_noReverbNames.Any(name.Contains))
                noReverbTiles.Add(i);

            // same here as well
            else if (_lowReverbNames.Any(name.Contains))
                lowReverbTiles.Add(i);
        }
    }

    public static bool CanRaycastTo(Vector2 begin, Vector2 destination) =>
        Collision.CanHitLine(begin, 1, 1, destination, 1, 1);

    public static int TilesAround(Vector2 position, Point grid, out HashSet<Point> tileCoords) {
        tileCoords = [];
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

    public static int TileObjectsAround(Vector2 position, Point grid, out HashSet<Point> blocks, out HashSet<Point> walls, out HashSet<Point> emptyTiles) {
        blocks = [];
        walls = [];
        emptyTiles = [];
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
                else if (!tile.HasTile && tile.WallType <= 0) {
                    emptyTiles.Add(new(i, j));
                }
            }
        }
        return count;
    }

    public static int WallsAround(Vector2 position, Point grid, out HashSet<Point> tileCoords) {
        tileCoords = [];
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
    public static FilterParams CreateAudioFX(Vector2 fromV2, Vector2 offset = default) {
        var cfg = ModContent.GetInstance<AudioAdditionsConfig>();
        var fParam = new FilterParams();
        float reverbActual = 0f;

        if (Main.gameMenu)
            return fParam;

        bool playerUnderwater = Main.LocalPlayer.IsWaterSuffocating();
        bool playerSurfaceOrHell = Main.LocalPlayer.Center.Y < Main.worldSurface * 16 || Main.LocalPlayer.Center.Y > (Main.maxTilesY - 200) * 16;
        bool playerUnderground = !playerSurfaceOrHell;


        if (!cfg.isReverbEnabled) {
            fParam.ReverbGain = 0f;
            SetFilterValues(fromV2, offset, ref fParam, playerUnderwater);
            return fParam;
        }

        // base values
        fParam.Reverb = FAudioReverbController.DefaultFNAReverb;
        if (!cfg.advancedReverbCalculation) {
            fParam.ReverbGain = MathUtils.InverseLerp((float)Main.worldSurface * 16, Main.maxTilesY * 16, Main.LocalPlayer.Center.Y, true);
            SetFilterValues(fromV2, offset, ref fParam, playerUnderwater);
            return fParam;
        }


        HashSet<Point> wallPoints;
        HashSet<Point> tilePoints;

        int seenWalls = 0;
        int seenTiles = 0;

        int highReverbSurfaces = 0, lowReverbSurfaces = 0;

        // we only get here if advanced reverb calculation is enabled
        if (playerSurfaceOrHell) {
            WallsAround(fromV2, new Point(15, 15), out wallPoints);
            //TileObjectsAround(fromV2, new Point(15, 15), out tilePoints, out wallPoints);
            foreach (var pt in wallPoints) {
                var wall = Framing.GetTileSafely(pt).WallType;

                var isHighReverb = !lowReverbWalls.Contains(wall) && !noReverbWalls.Contains(wall);
                if (isHighReverb && CanRaycastTo(fromV2, pt.ToVector2() * 16 + offset)) {
                    highReverbSurfaces++;
                    seenWalls++;
                }
                else if (lowReverbWalls.Contains(wall) && CanRaycastTo(fromV2, pt.ToVector2() * 16 + offset)) {
                    lowReverbSurfaces++;
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
        if (playerUnderground) {
            TileObjectsAround(fromV2, new Point(15, 15), out tilePoints, out wallPoints, out var emptyTiles);

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
                if (!lowReverbTiles.Contains(tileType) && !noReverbTiles.Contains(tileType)) {
                    highReverbSurfaces++;
                }
                else if (lowReverbWalls.Contains(tileType)) {
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

                if (!lowReverbWalls.Contains(wallType) && !noReverbWalls.Contains(wallType)) {
                    highReverbSurfaces++;
                }
                else if (lowReverbWalls.Contains(wallType)) {
                    lowReverbSurfaces++;
                }
            }

            foreach (var emptyPos in emptyTiles) {
                Vector2 worldCoords = emptyPos.ToVector2() * 16;

                bool primaryCast = CanRaycastTo(fromV2, worldCoords);

                if (!primaryCast) continue;

                highReverbSurfaces++;
                seenWalls++;
            }
        }

        // in the future maybe open areas with no background walls (valleys or things of that nature) should have audio echoing (not reverb)

        reverbActual += highReverbSurfaces * 0.0025f;
        reverbActual += lowReverbSurfaces * 0.00125f;

        fParam.Reverb.DecayTime = (seenWalls + seenTiles) * 0.004f;
        fParam.Reverb.ReflectionsDelay = (uint)(seenWalls + seenTiles) / 8;
        fParam.Reverb.EarlyDiffusion = (byte)MathHelper.Lerp(0, 15, (float)(seenTiles + seenWalls) / 1000);
        // a tile equals 2 "feet".. but maybe not.
        fParam.Reverb.RoomSize = seenWalls + seenTiles;
        // RoomFilterMain seems to create a "distant" echo?
        // fParam.Reverb.RoomFilterHF = 0f;

        SetFilterValues(fromV2, offset, ref fParam, playerUnderwater);
        fParam.ReverbGain = MathF.Min(reverbActual, 1f);

        // doesn't really save on the computation of said things...
        if (!ModContent.GetInstance<AudioAdditionsConfig>().isSoundOcclusionEnabled)
            fParam.LowPassEnabled = false;
        if (!ModContent.GetInstance<AudioAdditionsConfig>().isSoundDampeningEnabled)
            fParam.BandPassEnabled = false;

        return fParam;
    }

    public static void SetFilterValues(Vector2 position, Vector2 offset, ref FilterParams fParam, bool playerUnderwater) {
        fParam.LowPassIntensity = CalculateLowPass(position, offset, out fParam.LowPassEnabled);
        fParam.BandPassIntensity = CalculateBandPass(position, playerUnderwater, out fParam.BandPassEnabled);
    }

    public static float CalculateBandPass(Vector2 position, bool playerUnderwater, out bool enableBand) {
        bool underWater = Collision.DrownCollision(position, 1, 1);
        enableBand = underWater || playerUnderwater;
        return (playerUnderwater && !underWater) ? 0.0175f : (underWater && playerUnderwater) ? 0.01f : 0.04f;
    }
    public static float CalculateLowPass(Vector2 position, Vector2 offset, out bool enabled) {
        var goalPos = Main.screenPosition + ScreenListeningPosition; // Main.LocalPlayer.Top;

        // Dust.NewDustPerfect(goalPos, DustID.SpelunkerGlowstickSparkle);

        // mult by 2 since 2 feet per block
        var numBlockingTiles = CountTilesTouched(goalPos.ToTileCoordinates(), 
            (position + offset).ToTileCoordinates(), 
            t => t.HasTile && Main.tileSolid[t.TileType]);

        float curve = 1f;

        if (numBlockingTiles > 0) {
            float dist = Vector2.Distance(goalPos, position + offset);
            float normalized = Math.Clamp(dist / 2000f, 0f, 1f);
            // p < 1 means approach slows down near 0
            float p = 0.25f; // fast drop at first, slower and slower near 0 (cuz filter semantics)
            curve = 1f - MathF.Pow(normalized, p);
        }

        enabled = true;
        var occlusion = Math.Max(1f - MathF.Pow(numBlockingTiles / 50f, 0.75f), 0f);
        // Debug.WriteLine($"{numBlockingTiles} - {occlusion}, {curve}");

        return occlusion * curve;
    }

    public static float PitchFromPerFrame(
            Vector2 srcPosPx, Vector2 srcVelPxPerFrame,
            Vector2 lisPosPx, Vector2 lisVelPxPerFrame,
            float speedOfSoundTilesPerSec = 100f,              // tweak by ear: 60..125 tiles/s
            float minRatio = 0.5f, float maxRatio = 2.0f)      // safety
        {
        // convert velocities to pixels/second (Terraria runs at 60 updates/sec)
        const float Tps = 60f;
        Vector2 srcVelPxPerSec = srcVelPxPerFrame * Tps;
        Vector2 lisVelPxPerSec = lisVelPxPerFrame * Tps;

        float cPxPerSec = speedOfSoundTilesPerSec * 16f;   // tiles/s -> px/s
        return PitchFromPerSecond(srcPosPx, srcVelPxPerSec, lisPosPx, lisVelPxPerSec, cPxPerSec, minRatio, maxRatio);
    }

    // ---- helper when your velocities are already in pixels/second ----
    public static float PitchFromPerSecond(
        Vector2 srcPosPx, Vector2 srcVelPxPerSec,
        Vector2 lisPosPx, Vector2 lisVelPxPerSec,
        float speedOfSoundPxPerSec,
        float minRatio = 0.5f, float maxRatio = 2.0f) {
        Vector2 d = srcPosPx - lisPosPx;
        float len = d.Length();
        if (len < 1e-4f) return 0f;                // same spot → no shift

        Vector2 n = d / len;                       // listener→source unit vector

        // Radial components (+ toward each other)
        float vSourceToward = -Vector2.Dot(srcVelPxPerSec, n);
        float vListenerToward = Vector2.Dot(lisVelPxPerSec, n);

        float c = MathF.Max(1e-3f, speedOfSoundPxPerSec);

        // Classic Doppler frequency ratio
        float ratio = (c + vListenerToward) / (c - vSourceToward);

        // Keep it sane
        ratio = MathHelper.Clamp(ratio, minRatio, maxRatio);

        // MonoGame SoundEffectInstance.Pitch uses octaves (log2 of ratio)
        float pitchOct = MathF.Log(ratio, 2f);
        return MathHelper.Clamp(pitchOct, -1f, 1f);
    }

    // simple exponential smoothing (call each tick)
    public static float SmoothPitch(float currentPitch, float targetPitch, float lerpFactor = 0.2f)
        => MathHelper.Lerp(currentPitch, targetPitch, lerpFactor);

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
    /*public static bool IsPlayerInEnclosedSpace(Player player, RoomDetails roomDetails = null, bool requireWalls = true) {
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

            // If we require walls, then any *non-solid* interior tile that lacks a wall invalidates the room.
            // (We ignore solid tiles for wall requirement; doors/walls handled in IsTileSolid.)
            if (requireWalls && !curSolid && !curHasWall) {
                foundMissingWall = true;
                // we can bail early to save time; no need to keep filling
                break;
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
            CheckAndEnqueue(x + 1, y, originX, originY, queue, visited);
            CheckAndEnqueue(x - 1, y, originX, originY, queue, visited);
            CheckAndEnqueue(x, y + 1, originX, originY, queue, visited);
            CheckAndEnqueue(x, y - 1, originX, originY, queue, visited);
        }

        bool isEnclosed = !reachedEdge && (!requireWalls || !foundMissingWall);

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
    }*/
}

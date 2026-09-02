using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Common;
using TerrariaAmbience.Common.Systems;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Sounds.SoundFilters.FAudioHacks;

namespace TerrariaAmbience.Sounds.SoundFilters;

public enum Reflectivity {
    None,
    Low,
    Medium,
    High
}
public enum WorldLayer {
    Surface,
    Dirt,
    Cavern,
    Underworld
}
public class SoundFilterSystem : ModSystem {
    public static Vector2 ScreenListeningPosition;
    public static FilterParams LatestParams { get; set; }

    internal readonly static HashSet<int> lowReverbWalls = [];
    internal readonly static HashSet<int> lowReverbTiles = [];
    internal readonly static HashSet<int> noReverbWalls = [];
    internal readonly static HashSet<int> noReverbTiles = [];
    internal readonly static HashSet<int> medReverbTiles = [];
    internal readonly static HashSet<int> medReverbWalls = [];

    // no reverb set is prioritized over the low reverb set
    // so that if something like BambooFence exists, it will be considered as to compute no reverb
    readonly static HashSet<string> _noReverbNames = [
        "silt", "slush", "grass", "mud", "clay",
        "grass", "leaf", "leaves", "flower", "vine", "moss",
        "snow", "ash", "fence", "hive", "mushroom", "dirt", 
        "jungle", "cloud", "paper", "sail",

        // the depths
        "shale",

        // calamity

        // thorium

        // spooky
        "carpet", // but maybe more than just spooky?

        // spirit
        "reach",

        // confection
        "cookie", 
        // why is it "block" and not "snow" or "dirt"? come on
        "creamblock", "cookieblock", "creamwall",
        "floss"
    ];
    readonly static HashSet<string> _lowReverbNames = [
        "sand", "slush", "glass", "mud",

        "sand", "silt", "dirt", "plank", "bamboo", "glass",
        "ice", "tin", "wood", "door",

        // the depths

        // calamity

        // thorium

        // spooky
        "window", // but maybe more than just spooky?

        // spirit
        "reach"
    ];
    readonly static HashSet<string> _medReverbNames = [
        "plank", "shingle", 
        // overrides the "sand" query from the above set
        "sandstone"
    ];

    // so like, whatever. optimizing includes improving readability right?
    struct ReverbCounts {
        public float Walls;
        public float Tiles;
        public int HighReverb;
        public int MedReverb;
        public int LowReverb;

        public static ReverbCounts operator +(ReverbCounts left, ReverbCounts right) {
            ReverbCounts total = new() {
                Walls = left.Walls + right.Walls,
                Tiles = left.Tiles + right.Tiles,
                HighReverb = left.HighReverb + right.HighReverb,
                MedReverb = left.MedReverb + right.MedReverb,
                LowReverb = left.LowReverb + right.LowReverb
            };

            return total;
        }
        /*public void Add(ReverbCounts other) {
            Walls += other.Walls;
            Tiles += other.Tiles;
            HighReverb += other.HighReverb;
            MedReverb += other.MedReverb;
            LowReverb += other.LowReverb;
        }*/
    }
    public override void PostUpdateEverything() {
        if (Main.soundVolume == 0) return;

        ScreenListeningPosition = Main.screenPosition + Vector2.Transform(new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f - 5f), Main.GameViewMatrix.TransformationMatrix);

        UpdateReverbParams();
    }
    public static void UpdateReverbParams() {
        var time = (uint)ModContent.GetInstance<AudioConfig>().audioFiltersRefreshTime;
        if (Main.GameUpdateCount % time != 0) return;

        LatestParams = GenerateAudioFilters(FloodFillSystem.PlayerRoom);
    }

    // compute reverb properties after all mods have loaded their content
    public override void PostAddRecipes() {
        PrecomputeReverbProperties();
    }
    public static void PrecomputeReverbProperties() {
        noReverbTiles.Clear();
        noReverbWalls.Clear();
        lowReverbTiles.Clear();
        lowReverbWalls.Clear();
        // compute wall reverb properties
        for (int i = 0; i < WallLoader.WallCount; i++) {
            string name = WallID.Search.GetName(i).ToLower();

            if (_noReverbNames.Any(name.Contains))
                noReverbWalls.Add(i);
            else if (_medReverbNames.Any(name.Contains))
                medReverbWalls.Add(i);
            // only runs if it doesn't exist in the no reverb set to go in-hand with the comment left above _noReverbNames
            else if (_lowReverbNames.Any(name.Contains))
                lowReverbWalls.Add(i);
        }

        // compute tile reverb properties
        for (int i = 0; i < TileLoader.TileCount; i++) {
            string name = TileID.Search.GetName(i).ToLower();

            // better to check during runtime
            /*if (!Main.tileSolid[i] || Main.tileSolidTop[i]) {
                noReverbTiles.Add(i);
                continue;
            }*/

            if (_noReverbNames.Any(name.Contains))
                noReverbTiles.Add(i);
            else if (_medReverbNames.Any(name.Contains))
                medReverbTiles.Add(i);
            // same here as well
            else if (_lowReverbNames.Any(name.Contains))
                lowReverbTiles.Add(i);
        }
    }

    public const float HIGH_REVERB_MULTIPLIER = 0.00200f;
    public const float MED_REVERB_MULTIPLIER = 0.00100f;
    public const float LOW_REVERB_MULTIPLIER = 0.00050f;
    public const float HIGH_REVERB_EXPONENT = 0.85f;
    public const float MED_REVERB_EXPONENT = 0.9f;
    public const float LOW_REVERB_EXPONENT = 0.95f;

    public const float DECAY_MULTIPLIER = 0.003f;
    public const float ROOM_SIZE_MULTIPLIER = 0.5f;
    public const float WALLS_PARTIAL_COUNT = 0.5f;
    public const float EARLY_DIFF_SCALE = 15f / 1000f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Compress(float value, float exponent) => 1f - MathF.Exp(-exponent * value);

    public static FilterParams GenerateAudioFilters(Room room) {
        var aaCfg = ModContent.GetInstance<AudioConfig>();
        var fParam = new FilterParams();

        if (Main.gameMenu)
            return fParam;

        var pos = ScreenListeningPosition;
        bool playerUnderwater = Main.LocalPlayer.IsWaterSuffocating();

        if (!aaCfg.isReverbEnabled) {
            fParam.ReverbGain = 0f;
            SetFilterValues(pos, Vector2.Zero, ref fParam, playerUnderwater);
            return fParam;
        }

        // base values for reverb :)
        fParam.Reverb = FAudioReverbController.DefaultFAudioReverb;

        if (!aaCfg.advancedReverbCalculation) {
            fParam.ReverbGain = MathUtils.InverseLerp((float)Main.worldSurface * 16, Main.maxTilesY * 16, Main.LocalPlayer.Center.Y, true);
            SetFilterValues(pos, Vector2.Zero, ref fParam, playerUnderwater);
            return fParam;
        }

        // tracks tile stuff
        var counts = new ReverbCounts();

        var tiles = room.Tiles;
        bool isRaycastEnabled = aaCfg.reverbUsingRaycasting;
        Point listenerTileCoords = pos.ToTileCoordinates();

        // only parallelize if we have a large number of tiles to process now
        const int PARALLEL_THRESHOLD = 2000;
        if (!isRaycastEnabled) {
            foreach (var tilePos in tiles) {
                ProcessTile(tilePos, listenerTileCoords, false, ref counts);
            }
        }
        else {
            if (tiles.Count > PARALLEL_THRESHOLD) {
                object lockObj = new();
                Parallel.ForEach(tiles,
                    () => new ReverbCounts(),
                    (tilePos, loopState, localCounts) => {
                        ProcessTile(tilePos, listenerTileCoords, true, ref localCounts);
                        return localCounts;
                    },
                    (finalLocalCounts) => {
                        lock (lockObj) counts += finalLocalCounts;
                    });
            }
            else {
                // sequential is better for smaller sets
                foreach (var tilePos in tiles) {
                    ProcessTile(tilePos, listenerTileCoords, true, ref counts);
                }
            }
        }
        /*if (!isRaycastEnabled) {
            foreach (var tilePos in tiles) {
                ProcessTile(tilePos, listenerTileCoords, false, ref counts);
            }
        }
        else {
            // should this be a member or a variable...?
            object lockObj = new();

            Parallel.ForEach(tiles,
                () => new ReverbCounts(), // Init local storage
                (tilePos, loopState, localCounts) => {
                    // Process tile and update LOCAL counts
                    ProcessTile(tilePos, listenerTileCoords, true, ref localCounts);
                    return localCounts;
                },
                (finalLocalCounts) => {
                    // safely merge local counts into the main 'counts' variable
                    lock (lockObj) {
                        counts.Add(finalLocalCounts);
                    }
                }
            );
        }*/

        // final reverb
        float reverbActual = 0f;
        reverbActual += Compress(counts.HighReverb * HIGH_REVERB_MULTIPLIER, HIGH_REVERB_EXPONENT);
        reverbActual += Compress(counts.MedReverb * MED_REVERB_MULTIPLIER, MED_REVERB_EXPONENT);
        reverbActual += Compress(counts.LowReverb * LOW_REVERB_MULTIPLIER, LOW_REVERB_EXPONENT);

        var gain = MathF.Min(reverbActual, 1f);
        var totalSurfaces = counts.Walls + counts.Tiles;

        // params
        fParam.Reverb.DecayTime = Math.Min(totalSurfaces * gain * 0.003f, 299.9f);
        fParam.Reverb.ReflectionsDelay = Math.Min((uint)totalSurfaces / 16, 299u);
        fParam.Reverb.EarlyDiffusion = (byte)Math.Min(totalSurfaces * EARLY_DIFF_SCALE, 15f);
        fParam.Reverb.RoomSize = totalSurfaces * gain * 0.5f;

        SetFilterValues(pos, Vector2.Zero, ref fParam, playerUnderwater);
        fParam.ReverbGain = gain;

        fParam.LowPassEnabled = aaCfg.isSoundOcclusionEnabled;
        fParam.BandPassEnabled = aaCfg.isSoundDampeningEnabled;

        return fParam;
    }

    static void ProcessTile(Point tilePos, Point listenerCoords, bool doRaycast, ref ReverbCounts counts) {
        var reflectivity = CalculateAcousticReflectivity(tilePos, out bool wasTile, out bool wasWall, out var wl);

        if (reflectivity == Reflectivity.None) return;

        if (doRaycast) {
            // expensive so use sparingly
            if (IsPathBlocked(listenerCoords, tilePos)) return;
        }

        // update counts
        if (wasTile) counts.Tiles++;
        else if (wasWall) counts.Walls++;
        else if (wl == WorldLayer.Cavern || wl == WorldLayer.Dirt) counts.Walls += 0.5f;

        switch (reflectivity) {
            case Reflectivity.Low: counts.LowReverb++; break;
            case Reflectivity.Medium: counts.MedReverb++; break;
            case Reflectivity.High: counts.HighReverb++; break;
        }
    }

    // bresenham might have some inaccuracies that regular delegates don't?
    public static bool IsPathBlocked(Point lpc, Point tilePos) {
        bool isBlocked = false;
        TileLine(lpc, tilePos,
            (x, y, t) => {
                var ts = Main.tileSolid[t.TileType];
                var tst = Main.tileSolidTop[t.TileType];
                if (t.HasTile && !t.IsActuated && ts && !tst) {
                    if (x != tilePos.X || y != tilePos.Y) {
                        isBlocked = true;
                        return true;
                    }
                }
                return false;
            });
        return isBlocked;
        // bresenham line impl
        // something about this impl is just... wrong
        /*if (!WorldGen.InWorld(lpc.X, lpc.Y) || !WorldGen.InWorld(tilePos.X, tilePos.Y)) return true;

        foreach (var point in new BresenhamLine(lpc, tilePos)) {
            // skip the target tile itself
            if (point.X == tilePos.X && point.Y == tilePos.Y) continue;

            var t = Main.tile[point.X, point.Y];
            var ts = Main.tileSolid[t.TileType];
            var tst = Main.tileSolidTop[t.TileType];
            if (t.HasTile && !t.IsActuated && ts && !tst) {
                return true;
            }
        }
        return false;*/
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
        var goalPos = ScreenListeningPosition;

        var occlusionFactor = 50f;

        // experimental pathfinding approach
        /*var start = position.ToTileCoordinates();
        var path = new AStarPath(start, goalPos.ToTileCoordinates());

        foreach (var node in path) {
            Dust.NewDustPerfect(node.ToWorldCoordinates(), DustID.TerraBlade, Vector2.Zero);
        }
        enabled = true;
        var inv = Utils.GetLerpValue(0, 2000, path.Distance, true);
        if (!path.Found) {
            occlusionFactor = 2;
            goto tileline;
        }

        if (path.Distance < 32) return inv;
        Main.NewText(path.Distance + ", " + (1f - inv));
        return 1f - inv;
    // old non-bresenham impl
    // mult by 2 since 2 feet per block
    tileline:*/
        int numBlockingTiles = 0;
        TileLine(goalPos.ToTileCoordinates(),
            (position + offset).ToTileCoordinates(),
            (x, y, t) => {
                var ts = Main.tileSolid[t.TileType];
                var tst = Main.tileSolidTop[t.TileType];
                if (t.HasTile && !t.IsActuated && ts && !tst) {
                    numBlockingTiles++;
                }

                return false;
            });

        float curve = 1f;

        if (numBlockingTiles > 0) {
            float dist = Vector2.Distance(goalPos, position + offset);
            float normalized = Math.Clamp(dist / 2000f, 0f, 1f);
            // p < 1 means approach slows down near 0
            float p = 0.25f; // fast drop at first, slower and slower near 0 (cuz filter semantics)
            curve = 1f - MathF.Pow(normalized, p);
        }

        enabled = true;
        var occlusion = Math.Max(1f - MathF.Pow(numBlockingTiles / occlusionFactor, 0.75f), 0f);
        // Debug.WriteLine($"{numBlockingTiles} - {occlusion}, {curve}");

        return occlusion * curve;
    }

    public static Reflectivity CalculateAcousticReflectivity(Point tilePos, out bool wasTile, out bool wasWall, out WorldLayer wl) {
        wasTile = false;
        wasWall = false;

        Tile t = Framing.GetTileSafely(tilePos);
        int type = t.TileType;
        int wall = t.WallType;
        bool hasTile = t.HasTile;

        var wlRef = CalculateReverbForWorldLayer(tilePos, out wl);

        // has a tile, it's solid, and isn't only solid on the top
        bool isValidSolid = hasTile && Main.tileSolid[type] && !Main.tileSolidTop[type];

        if (isValidSolid) {
            wasTile = true;

            // do by priority
            var refl = Reflectivity.High;

            if (lowReverbTiles.Contains(type))
                refl = Reflectivity.Low;
            else if (medReverbTiles.Contains(type))
                refl = Reflectivity.Medium;
            else if (noReverbTiles.Contains(type))
                refl = Reflectivity.None;

            // strange but decent way of reducing reflectivity of actuated tiles
            if (t.IsActuated) {
                refl = (Reflectivity)Math.Max((int)refl - 1, 0);
            }

            return refl;
        }

        // check walls if there is no tile
        if (wall > 0) {
            wasWall = true;
            return CalculateWall(wall);
        }

        // if not on surface and is an air tile, it's a "tile"
        // because the background looks like stone!
        if (hasTile || wl != WorldLayer.Surface) {
            wasTile = true;
            return wlRef;
        }

        // surface edgecase
        // even if HasTile is false, TileType might not be 0. relogic why
        if (type == 0) {
            return wlRef;
        }

        return Reflectivity.None;
    }

    public static Reflectivity CalculateReverbForWorldLayer(Point tilePos, out WorldLayer wl) {
        // dirt "underground" layer
        // low reverb since it's partially rock and mostly dirt
        if (tilePos.Y >= Main.worldSurface && tilePos.Y < Main.rockLayer) {
            wl = WorldLayer.Dirt;
            return Reflectivity.Low;
        }
        // cavern to top of underworld
        // this is because it's primarily rock in the background
        else if (tilePos.Y >= Main.rockLayer && tilePos.Y < Main.maxTilesY - 200) {
            wl = WorldLayer.Cavern;
            // medium vs high dilemma
            return Reflectivity.Medium;
        }

        wl = tilePos.Y < Main.worldSurface ? WorldLayer.Surface : WorldLayer.Underworld;
        return Reflectivity.None;
    }
    public static Reflectivity CalculateWall(int wall) {
        var isLowReverb = lowReverbWalls.Contains(wall);
        var isMedReverb = medReverbWalls.Contains(wall);
        var isHighReverb = !isLowReverb && !isMedReverb && !noReverbWalls.Contains(wall);

        if (isHighReverb)
            return Reflectivity.High;
        else if (isMedReverb)
            return Reflectivity.Medium;
        else if (isLowReverb)
            return Reflectivity.Low;
        else
            return Reflectivity.None;
    }

    /// <summary>Iterates a line from a start point to an end point.</summary>
    /// <param name="start">The start point of the line.</param>
    /// <param name="end">The end point.</param>
    /// <param name="touchCallback">Return true within the callback to break out of the line.</param>
    public static void TileLine(Point start, Point end, TileTouchCallback touchCallback = null) {
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
            if (!WorldGen.InWorld(x0, y0)) break;

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

            Tile tile = Framing.GetTileSafely(x0, y0);

            bool? cb = touchCallback?.Invoke(x0, y0, tile);
            if (cb == true) break;
        }
    }

    public delegate bool TileTouchCallback(int tilePosX, int tilePosY, Tile tile);
}
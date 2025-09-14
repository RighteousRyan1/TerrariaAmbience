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
    public static Vector2 ScreenListeningPosition => Main.screenPosition + Vector2.Transform(new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f - 5f), Main.GameViewMatrix.TransformationMatrix);
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
        "snow", "ash", "fence", "hive", "mushroom", "dirt"
    ];
    readonly static HashSet<string> _lowReverbNames = [
        "sand", "slush", "glass", "mud",

        "sand", "silt", "dirt", "plank", "bamboo", "glass",
        "ice", "tin", "wood", "door"
    ];
    readonly static HashSet<string> _medReverbNames = [
        "plank", "shingle"
    ];

    /*public static Thread FiltersThread { get; } = new Thread(UpdateReverbParams) {
        Name = "Filter Update Thread",
        IsBackground = true,
        Priority = ThreadPriority.AboveNormal
    };*/
    public override void PostUpdateEverything() {
        if (Main.soundVolume == 0) return;

        var time = ModContent.GetInstance<AudioConfig>().audioFiltersRefreshTime;

        if (Main.GameUpdateCount % time != 0) return;
        // UpdateReverbParams();
        LatestParams = GenerateAudioFilters(FloodFillSystem.PlayerRoom);
    }
    public static void UpdateReverbParams() {
        /*while (true) {
            Thread.Sleep((int)Main.instance.gameTime.ElapsedGameTime.TotalMilliseconds);

            if (Main.gameMenu) continue;

            var time = (uint)ModContent.GetInstance<AudioConfig>().audioFiltersRefreshTime;
            if (Main.GameUpdateCount % time != 0) continue;

            LatestParams = GenerateAudioFilters(FloodFillSystem.PlayerRoom);
        }*/

        /*var time = (uint)ModContent.GetInstance<AudioConfig>().audioFiltersRefreshTime;
        if (Main.GameUpdateCount % time != 0) return;*/

        LatestParams = GenerateAudioFilters(FloodFillSystem.PlayerRoom);
    }

    // compute reverb properties after all mods have loaded their content
    public override void PostAddRecipes() {
        PrecomputeReverbProperties();
        // FiltersThread.Start();
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

    // Make compress method static and inline for better performance
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

        // base values
        fParam.Reverb = FAudioReverbController.DefaultFAudioReverb;
        if (!aaCfg.advancedReverbCalculation) {
            fParam.ReverbGain = MathUtils.InverseLerp((float)Main.worldSurface * 16, Main.maxTilesY * 16, Main.LocalPlayer.Center.Y, true);
            SetFilterValues(pos, Vector2.Zero, ref fParam, playerUnderwater);
            return fParam;
        }

        float numWallsCounts = 0, numTilesCounts = 0;

        int highReverbSurfaces = 0, lowReverbSurfaces = 0, medReverbSurfaces = 0;

        var isRaycastEnabled = aaCfg.reverbUsingRaycasting;

        //var ikd = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.L);

        // we only get here if advanced reverb calculation is enabled
        var lpc = pos.ToTileCoordinates();

        var tiles = room.Tiles;

        Parallel.For(0, tiles.Count, (i) => {
            var tilePos = tiles[i];
            // is this better or worse?
            var reflectivity = CalculateAcousticReflectivity(tilePos, out bool wasTile, out bool wasWall, out var wl);

            var reflectsAny = reflectivity != Reflectivity.None;
            // dont bother performing anything on these tiles
            if (!reflectsAny) return;

            if (isRaycastEnabled) {
                if (IsPathBlocked(lpc, tilePos))
                    return;
            }
            if (wasTile) numTilesCounts++;
            else if (wasWall) numWallsCounts++;
            else if (!wasTile && !wasWall && (wl == WorldLayer.Cavern || wl == WorldLayer.Dirt)) numWallsCounts += 0.5f;

            switch (reflectivity) {
                case Reflectivity.Low:
                    lowReverbSurfaces++;
                    break;
                case Reflectivity.Medium:
                    medReverbSurfaces++;
                    break;
                case Reflectivity.High:
                    highReverbSurfaces++;
                    break;
            }
        });
        /*for (int i = 0; i < tiles.Count; i++) {
            var tilePos = tiles[i];
            // is this better or worse?
            var reflectivity = CalculateAcousticReflectivity(tilePos, out bool wasTile, out bool wasWall, out var wl);

            var reflectsAny = reflectivity != Reflectivity.None;
            // dont bother performing anything on these tiles
            if (!reflectsAny) continue;

            if (isRaycastEnabled) {
                if (IsPathBlocked(lpc, tilePos))
                    continue;
            }
            if (wasTile) numTilesCounts++;
            else if (wasWall) numWallsCounts++;
            else if (!wasTile && !wasWall && (wl == WorldLayer.Cavern || wl == WorldLayer.Dirt)) numWallsCounts += 0.5f;

            switch (reflectivity) {
                case Reflectivity.Low:
                    lowReverbSurfaces++;
                    break;
                case Reflectivity.Medium:
                    medReverbSurfaces++;
                    break;
                case Reflectivity.High:
                    highReverbSurfaces++;
                    break;
            }
        }*/

        // in the future maybe open areas with no background walls (valleys or things of that nature) should have audio echoing (not reverb)

        float reverbActual = 0f;
        reverbActual += Compress(highReverbSurfaces * HIGH_REVERB_MULTIPLIER, HIGH_REVERB_EXPONENT);
        reverbActual += Compress(medReverbSurfaces * MED_REVERB_MULTIPLIER, MED_REVERB_EXPONENT);
        reverbActual += Compress(lowReverbSurfaces * LOW_REVERB_MULTIPLIER, LOW_REVERB_EXPONENT);

        var gain = MathF.Min(reverbActual, 1f);

        var totalSurfaces = numWallsCounts + numTilesCounts;

        // reverb parameters
        var decayTime = Math.Min(totalSurfaces * gain * 0.003f, 299.9f);
        var refDelay = Math.Min((uint)totalSurfaces / 16, 299u);
        var earlyDiff = (byte)Math.Min(totalSurfaces * EARLY_DIFF_SCALE, 15f);
        var roomSize = (numWallsCounts + numTilesCounts) * gain * 0.5f;

        fParam.Reverb.DecayTime = decayTime;
        fParam.Reverb.ReflectionsDelay = refDelay;
        // fParam.Reverb.ReflectionsGain = 0;
        fParam.Reverb.EarlyDiffusion = earlyDiff;
        // a tile equals 2 "feet".. but maybe not.
        fParam.Reverb.RoomSize = roomSize;
        // RoomFilterMain seems to create a "distant" echo?
        // fParam.Reverb.RoomFilterHF = 0f;

        SetFilterValues(pos, Vector2.Zero, ref fParam, playerUnderwater);
        fParam.ReverbGain = gain;

        // doesn't really save on the computation of said things...
        if (!aaCfg.isSoundOcclusionEnabled)
            fParam.LowPassEnabled = false;
        if (!aaCfg.isSoundDampeningEnabled)
            fParam.BandPassEnabled = false;

        return fParam;
    }

    public static int NumTilesBresenham(Point lpc, Point tilePos) {
        int numTiles = 0;
        foreach (var point in new BresenhamLine(lpc, tilePos)) {
            var t = Main.tile[point.X, point.Y];
            int x = point.X, y = point.Y;

            var ts = Main.tileSolid[t.TileType];
            var tst = Main.tileSolidTop[t.TileType];
            if (t.HasTile && !t.IsActuated && ts && !tst)
                if (x != tilePos.X || y != tilePos.Y)
                    numTiles++;
        }
        return numTiles;
    }
    public static int NumTilesTouchMethod(Point lpc, Point tilePos) {
        int numTiles = 0;
        TileLine(tilePos, lpc,
            (x, y, t) => {
                var ts = Main.tileSolid[t.TileType];
                var tst = Main.tileSolidTop[t.TileType];
                if (t.HasTile && !t.IsActuated && ts && !tst) {
                    if (x != tilePos.X || y != tilePos.Y) {
                        numTiles++;
                        return true;
                    }
                }
                return false;
            });
        return numTiles;
    }
    public static bool IsPathBlocked(Point lpc, Point tilePos) {
        bool blocked = false;
        TileLine(lpc, tilePos,
            (x, y, t) => {
                var ts = Main.tileSolid[t.TileType];
                var tst = Main.tileSolidTop[t.TileType];
                if (t.HasTile && !t.IsActuated && ts && !tst) {
                    if (x != tilePos.X || y != tilePos.Y) {
                        blocked = true;
                        return true;
                    }
                }
                return false;
            });
        return blocked;
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

        // Dust.NewDustPerfect(goalPos, DustID.SpelunkerGlowstickSparkle);

        // mult by 2 since 2 feet per block
        int numBlockingTiles = 0;
        TileLine(goalPos.ToTileCoordinates(),
            (position + offset).ToTileCoordinates(),
            (x, y, t) => {
                var ts = Main.tileSolid[t.TileType];
                var tst = Main.tileSolidTop[t.TileType];
                if (t.HasTile && !t.IsActuated && ts && !tst)
                    numBlockingTiles++;

                return false;
            });

        /*foreach (var point in new BresenhamLine(goalPos.ToTileCoordinates(), (position + offset).ToTileCoordinates())) {
            var t = Main.tile[point.X, point.Y];

            var ts = Main.tileSolid[t.TileType];
            var tst = Main.tileSolidTop[t.TileType];
            if (t.HasTile && !t.IsActuated && ts && !tst)
                numBlockingTiles++;
        }*/

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

    public static Reflectivity CalculateAcousticReflectivity(Point tilePos, out bool wasTile, out bool wasWall, out WorldLayer wl) {
        var thisTile = Framing.GetTileSafely(tilePos);
        int wall = thisTile.WallType, tile = thisTile.TileType;
        wasTile = wasWall = false;

        var wlRef = CalculateReverbForWorldLayer(tilePos, out wl);

        if (tile > 0) {
            var isLowReverb = lowReverbTiles.Contains(tile);
            var isMedReverb = medReverbTiles.Contains(tile);
            var isHighReverb = !isLowReverb && !isMedReverb && !noReverbTiles.Contains(tile);
            var isInvalidTile = !Main.tileSolid[tile] || Main.tileSolidTop[tile];

            if (isInvalidTile && wall > 0) {
                wasWall = true;
                return CalculateWall(wall);
            }

            wasTile = true;

            if (isInvalidTile)
                return wlRef;

            var rev = Reflectivity.None;

            if (isHighReverb)
                rev = Reflectivity.High;
            else if (isMedReverb)
                rev = Reflectivity.Medium;
            else if (lowReverbTiles.Contains(tile))
                rev = Reflectivity.Low;

            if (thisTile.IsActuated)
                rev = (Reflectivity)Math.Max((int)(rev - 1), 0);
            return rev;
        }
        else if (wall > 0) {
            wasWall = true;
            return CalculateWall(wall);
        }
        // if we're underground the background is present and it can "count" as a surface
        else if (wall == 0 && tile == 0) {
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
            return Reflectivity.High;
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
            if (!WorldGen.InWorld(x0, y0)) continue;

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

            Tile tile = Main.tile[x0, y0];

            bool? cb = touchCallback?.Invoke(x0, y0, tile);
            if (cb == true) break;
        }
    }

    public delegate bool TileTouchCallback(int tilePosX, int tilePosY, Tile tile);
}
public ref struct BresenhamLine {
    readonly int shortest;
    readonly int longest;
    readonly Point stepA;
    readonly Point stepB;
    int i;
    int numerator;

    public Point Current { get; private set; }

    public BresenhamLine(Point start, Point end) {
        int width = end.X - start.X;
        int height = end.Y - start.Y;

        stepA = new Point(Math.Sign(width), Math.Sign(height));
        stepB = new Point(Math.Sign(width), 0);
        longest = Math.Abs(width);
        shortest = Math.Abs(height);

        if (longest <= shortest) {
            longest = Math.Abs(height);
            shortest = Math.Abs(width);

            stepB.X = 0;
            stepB.Y = Math.Sign(height);
        }

        i = -1;
        numerator = longest >> 1;
        Current = start;
    }

    public bool MoveNext() {
        if (i++ > longest) {
            return false;
        }

        numerator += shortest;

        if (!(numerator < longest)) {
            numerator -= longest;
            Current += stepA;
        }
        else {
            Current += stepB;
        }

        return true;
    }

    public readonly BresenhamLine GetEnumerator() => this;
}

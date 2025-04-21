using Terraria;
using Terraria.ModLoader;
using TerrariaAmbienceAPI.Common;
using TerrariaAmbience.Common.Systems;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Core;
using System.Collections.Generic;
using System.Linq;
using System;
using TerrariaAmbience.Content.Players;
using Microsoft.Xna.Framework.Audio;
using Terraria.ID;
using TerrariaAmbience.Sounds;

namespace TerrariaAmbience.Content.AmbientAndMore;

public class AmbientHandler {
    public static string AmbientPath => $"Sounds/Custom/ambient/";
    public static string AmbientPathWithPrefix => $"{nameof(TerrariaAmbience)}/" + AmbientPath;

    public AmbientHandler() {
        Initialize();
    }

    public float TransitionHarshness;

    public ModAmbience BreezeLight;
    public ModAmbience BreezeHeavy;

    public ModAmbience ForestMorning;
    public ModAmbience ForestDay;
    public ModAmbience ForestEvening;
    public ModAmbience ForestNight;

    public ModAmbience Desert;

    public ModAmbience SnowDay;
    public ModAmbience SnowNight;
    // when it's snowy AND windy!
    public ModAmbience SnowAggro;

    public ModAmbience JungleDay;
    public ModAmbience JungleNight;
    public ModAmbience JungleUndergroundDay; // aka swamp
    public ModAmbience JungleUndergroundNight;

    public ModAmbience BeachCalm;
    // when it's watery AND windy!
    public ModAmbience BeachAggro;

    public ModAmbience EvilsCrimson;
    public ModAmbience EvilsCorruption;

    public ModAmbience CavernLayer;

    public ModAmbience Hell;

    public ModAmbience Underwater;
    public ModAmbience UnderwaterDeep;

    public ModAmbience RainLight;
    public ModAmbience RainMed;
    public ModAmbience RainHeavy;

    public List<ModAmbience> Ambiences { get; private set; }

    // since ModAmbience can't really do campfire stuff. lul.
    // well it technically could...
    public SoundEffectInstance CampfireCrackleInstance;

    // these two fields will be chosen at either the last dusk or the last dawn respectively, aka pre-emptively chosen.
    public int morningAmbienceForTheDay;
    public int eveningAmbienceForTheDay;
    public int nightAmbienceForTheDay;
    public int dayAmbienceForTheDay;

    public const int NUM_EVENING_AMBIENCE = 6;
    public const int NUM_MORNING_AMBIENCE = 6;
    public const int NUM_NIGHT_AMBIENCE = 6;
    public const int NUM_DAY_AMBIENCE = 6;

    // values scraped from decompiled terraria. Main.cs::_minWind,_maxWind,_minRain,_maxRain
    public float MinWind = 0.34f;
    public float MaxWind = 0.4f;
    public float MinRain = 0.4f;
    public float MaxRain = 0.5f;

    public void Initialize() {
        if (Main.dedServ)
            return;

        var mod = ModContent.GetInstance<TerrariaAmbience>();

        BreezeLight = new(mod, AmbientPath + "environment/breeze_light", "BreezeLight", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneOverworldHeight && !player.ZoneSkyHeight;
        });
        BreezeHeavy = new(mod, AmbientPath + "environment/breeze_heavy", "BreezeHeavy", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneOverworldHeight && !player.ZoneSkyHeight;
        });

        ForestMorning = new(mod, AmbientPath + $"biome/forest/morning_{Main.rand.Next(1, NUM_MORNING_AMBIENCE)}", "ForestMorning", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZonePurity || player.ZoneMeteor || (player.ZoneHallow && !player.ZoneDesert);
        });
        ForestDay = new(mod, AmbientPath + $"biome/forest/day_{Main.rand.Next(1, NUM_DAY_AMBIENCE)}", "ForestDay", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return (player.ZonePurity || player.ZoneMeteor || (player.ZoneHallow && !player.ZoneDesert)) &&
                    Main.dayTime;
        });
        ForestEvening = new(mod, AmbientPath + $"biome/forest/evening_{Main.rand.Next(1, NUM_EVENING_AMBIENCE)}", "ForestEvening", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZonePurity || player.ZoneMeteor || (player.ZoneHallow && !player.ZoneDesert);
        });
        ForestNight = new(mod, AmbientPath + $"biome/forest/night_{Main.rand.Next(1, NUM_NIGHT_AMBIENCE)}", "ForestNight", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZonePurity && !player.ZoneDesert && !Main.dayTime;
        });

        CavernLayer = new(mod, AmbientPath + "biome/cavern/wind", "CavernLayer", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.Center.Y >= Main.worldSurface * 16;
        });

        Desert = new(mod, AmbientPath + "biome/desert/critters", "Desert", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneDesert;
        });

        SnowDay = new(mod, AmbientPath + "biome/snow/day", "SnowDay", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneSnow && Main.dayTime && !player.ZoneUnderworldHeight;
        });
        SnowNight = new(mod, AmbientPath + "biome/snow/night", "SnowNight", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneSnow && !Main.dayTime && !player.ZoneUnderworldHeight;
        });
        SnowAggro = new(mod, AmbientPath + "biome/snow/windy", "SnowAggro", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneSnow && !player.ZoneUnderworldHeight && Main.raining;
        });
        EvilsCrimson = new(mod, AmbientPath + "biome/crimson/rumbles", "EvilsCrimson", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneCrimson;
        });
        EvilsCorruption = new(mod, AmbientPath + "biome/corruption/roars", "EvilsCorruption", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneCorrupt;
        });

        JungleDay = new(mod, AmbientPath + "biome/jungle/day", "JungleDay", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneJungle;
        });
        JungleNight = new(mod, AmbientPath + "biome/jungle/night", "JungleNight", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneJungle;
        });
        JungleUndergroundDay = new(mod, AmbientPath + "biome/jungle/underground_day", "JungleUndergroundDay", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneJungle;
        });
        JungleUndergroundNight = new(mod, AmbientPath + "biome/jungle/underground_night", "JungleUndergroundNight", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneJungle;
        });

        BeachCalm = new(mod, AmbientPath + "biome/beach/waves_calm", "BeachCalm", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            // do this when i'm not lazy
            var calLoadedAtSulfSea = false; //TerrariaAmbience.IsPlayerInCalBiome(player, "SulphurousSeaBiome") && TerrariaAmbience.IsCalLoaded;
            return (player.ZoneBeach && !player.ZoneRockLayerHeight) || (calLoadedAtSulfSea);
        });
        BeachAggro = new(mod, AmbientPath + "biome/beach/waves_aggro", "BeachAggro", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneBeach && !player.ZoneRockLayerHeight;
        });
        Hell = new(mod, AmbientPath + "biome/hell/rumbles", "Hell", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            return true; // we return true because we simply control the volume by the player's level of depth in Update()
        });
        
        Underwater = new(mod, AmbientPath + "environment/underwater_loop", "Underwater", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.IsWaterSuffocating();
        });
        UnderwaterDeep = new(mod, AmbientPath + "environment/underwater_loop-deep", "UnderwaterDeep", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.IsWaterSuffocating();
        });
        RainLight = new(mod, AmbientPath + "rain/light", "RainLight", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return !Main.LocalPlayer.ZoneSnow;
        });
        RainMed = new(mod, AmbientPath + "rain/med", "RainMedium", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return !Main.LocalPlayer.ZoneSnow;
        });
        RainHeavy = new(mod, AmbientPath + "rain/heavy", "RainHeavy", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return !Main.LocalPlayer.ZoneSnow;
        });

        // finally, we add all of them via reflection because manual labor is stupid.
        Ambiences = [];
        foreach (var fld in GetType().GetFields().Where(x => x.FieldType.TypeHandle.Equals(typeof(ModAmbience).TypeHandle))) {
            Ambiences.Add((ModAmbience)fld.GetValue(this));
        }
    }

    // FROM HERE: I am creating fields that are used to determine ambient noise changes or differences.
    public float WindThreshold = 20f;
    public bool IsWindTooHarsh;

    public void HandleEnterWorld() {
        if (Main.raining) {
            dayAmbienceForTheDay = AmbienceID.Day_Quiet;
            ForestDay.ChangeTrack(AmbientPath + $"biome/forest/day_{dayAmbienceForTheDay}");
        }
    }
    public static void InitializeAllAmbienceToAvoidRuntimeOverhead() {
        // different for-loops because these values could change.
        // also magic numbers because it's weird to look at an identifier as a loop limit.
        var mod = ModContent.GetInstance<TerrariaAmbience>();
        for (int i = 1; i <= NUM_MORNING_AMBIENCE; i++) {
            mod.Assets.Request<SoundEffect>(AmbientPath + $"biome/forest/morning_{i}");
        }
        for (int i = 1; i <= NUM_DAY_AMBIENCE; i++) {
            mod.Assets.Request<SoundEffect>(AmbientPath + $"biome/forest/day_{i}");
        }
        for (int i = 1; i <= NUM_EVENING_AMBIENCE; i++) {
            mod.Assets.Request<SoundEffect>(AmbientPath + $"biome/forest/evening_{i}");
        }
        for (int i = 1; i <= NUM_NIGHT_AMBIENCE; i++) {
            mod.Assets.Request<SoundEffect>(AmbientPath + $"biome/forest/night_{i}");
        }
    }
    public void RandomizeDuskTracks() {
        eveningAmbienceForTheDay = Main.rand.Next(1, NUM_EVENING_AMBIENCE + 1);
        nightAmbienceForTheDay = Main.rand.Next(1, NUM_NIGHT_AMBIENCE + 1);

        SyncAmbienceSystem.curEveningAmb = eveningAmbienceForTheDay;
        SyncAmbienceSystem.curNightAmb = nightAmbienceForTheDay;

        ForestEvening.ChangeTrack(AmbientPath + $"biome/forest/evening_{eveningAmbienceForTheDay}");
        ForestNight.ChangeTrack(AmbientPath + $"biome/forest/night_{nightAmbienceForTheDay}");
    }
    public void RandomizeDawnTracks() {
        morningAmbienceForTheDay = Main.rand.Next(1, NUM_MORNING_AMBIENCE + 1);
        dayAmbienceForTheDay = Main.rand.Next(1, NUM_DAY_AMBIENCE + 1);
        if (Main.raining)
            dayAmbienceForTheDay = AmbienceID.Day_Quiet;

        SyncAmbienceSystem.curMorningAmb = morningAmbienceForTheDay;
        SyncAmbienceSystem.curDayAmb = dayAmbienceForTheDay;

        ForestDay.ChangeTrack(AmbientPath + $"biome/forest/day_{dayAmbienceForTheDay}");
        ForestMorning.ChangeTrack(AmbientPath + $"biome/forest/morning_{morningAmbienceForTheDay}");
    }
    public void Update() {
        if (Main.dedServ)
            return;

        IsWindTooHarsh = Math.Abs(Main.windSpeedCurrent) >= MaxWind;

        if (Main.gameMenu) {
            Ambiences.ForEach(x => x.MaxVolume = 0);
            return;
        }

        // Make a "master volume" for ambiences.
        if (float.TryParse(ModContent.GetInstance<GeneralConfig>().transitionHarshness, out float harshness)) {
            Ambiences.ForEach(x => x.VolumeStep = TransitionHarshness);
            TransitionHarshness = harshness;
        }
        else
            ModContent.GetInstance<GeneralConfig>().transitionHarshness = "0.01"; // a failsafe in case the player is stupid
        ForestMorning.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.FromMorning * ModContent.GetInstance<GeneralConfig>().forestVolumes[0];
        ForestDay.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.FromNoon * ModContent.GetInstance<GeneralConfig>().forestVolumes[1];
        ForestEvening.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.FromEvening * ModContent.GetInstance<GeneralConfig>().forestVolumes[2];
        ForestNight.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.FromMidnight * ModContent.GetInstance<GeneralConfig>().forestVolumes[3];

        // added AllDayPartNight and AllNightPartDay to JungleDay and JungleNight at 11/22/2023 @ 9:33 AM
        JungleDay.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.AllDayPartNight * ModContent.GetInstance<GeneralConfig>().jungleVolumes[0] * 0.75f; // it's a bit loud.
        JungleNight.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.AllNightPartDay * ModContent.GetInstance<GeneralConfig>().jungleVolumes[1] * 0.4f; // it's very loud by default.
        JungleUndergroundDay.MaxVolume = GradientGlobals.Underground * GradientGlobals.AllDayPartNight * ModContent.GetInstance<GeneralConfig>().jungleVolumes[2];
        JungleUndergroundNight.MaxVolume = GradientGlobals.Underground * GradientGlobals.AllNightPartDay * ModContent.GetInstance<GeneralConfig>().jungleVolumes[3] * 0.6f; // it's a bit louder than the day variant...

        SnowDay.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.WindSpeedsLow * ModContent.GetInstance<GeneralConfig>().snowVolume;
        SnowNight.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.WindSpeedsLow * ModContent.GetInstance<GeneralConfig>().snowVolume;
        SnowAggro.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.RainIntensityForSnow * GradientGlobals.WindSpeedsHigh *  ModContent.GetInstance<GeneralConfig>().snowVolume;
        // make quieter / bandpass filter when inside
        Desert.MaxVolume = GradientGlobals.SkyToUnderground * ModContent.GetInstance<GeneralConfig>().desertVolume;
        if (Desert.SoundInstance != null)
            Desert.SoundInstance.Pitch = Main.dayTime ? 0f : -0.1f;
        if (!Underwater.AreConditionsMet)
            Underwater.VolumeStep = TransitionHarshness * 2.5f;
        else
            Underwater.VolumeStep = TransitionHarshness;

        EvilsCorruption.MaxVolume = 1f;
        EvilsCrimson.MaxVolume = 1f;

        Underwater.MaxVolume = 0f;
        UnderwaterDeep.MaxVolume = 0f;

        // rain volume slider.
        RainLight.MaxVolume = GradientGlobals.RainIntensityForRainLight * GradientGlobals.SkyToUnderground;
        RainMed.MaxVolume = GradientGlobals.RainIntensityForRainMed * GradientGlobals.SkyToUnderground;
        RainHeavy.MaxVolume = GradientGlobals.RainIntensityForRainHeavy * GradientGlobals.SkyToUnderground * 0.6f;

        if (Underwater.AreConditionsMet) {
            // do volume lerp between underwater_loop and underwater_loop-deep
            var player = Main.LocalPlayer;

            // check X tiles from the player's head.
            var tileSize = 16;
            var tilesToCheck = 30;
            var waterDepth = player.GetModPlayer<AmbientPlayer>().GetWaterPressureFloat(tilesToCheck * tileSize);

            float lightMaxThresh = 20;
            float deepMinThresh = 15;
            float deepMaxThresh = 30;

            float lightMinThresh = 2;

            // we start the minValue in the negatives because we want some sort of audio if the player is in very shallow water.
            var underwaterLightGradient = Gradient.CreateFloat(
                waterDepth, -tileSize * lightMinThresh, tileSize * lightMaxThresh);
            var underwaterDeepGradient = Utils.GetLerpValue(tileSize * deepMinThresh, tileSize * deepMaxThresh, waterDepth);

            Underwater.MaxVolume = underwaterLightGradient;
            UnderwaterDeep.MaxVolume = underwaterDeepGradient;
        }

        // Main.NewText(EvilsCorruption.AreConditionsMet + ", " + EvilsCorruption.IsPlaying);

        BeachCalm.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.WindSpeedsLow * ModContent.GetInstance<GeneralConfig>().oceanVolume;
        BeachAggro.MaxVolume = GradientGlobals.SkyToUnderground * GradientGlobals.WindSpeedsHigh * ModContent.GetInstance<GeneralConfig>().oceanVolume;
        
        BreezeLight.MaxVolume = GradientGlobals.WindLight * GradientGlobals.SkyToUnderground * ModContent.GetInstance<GeneralConfig>().breezeVolume;
        BreezeHeavy.MaxVolume = GradientGlobals.WindCapped * GradientGlobals.SkyToUnderground * ModContent.GetInstance<GeneralConfig>().breezeVolume * 0.5f;
        CavernLayer.MaxVolume = GradientGlobals.Underground * ModContent.GetInstance<GeneralConfig>().cavernsVolume * 0.4f;
        
        Hell.MaxVolume = GradientGlobals.Hell * ModContent.GetInstance<GeneralConfig>().hellVolume;
        var pl = Main.LocalPlayer.GetModPlayer<AmbientPlayer>();
        Ambiences.ForEach(x => x.MaxVolume *= pl.InRoomAmbientMultiplier);
    }
}
public static class AmbienceID {
    public const int Day_BirdsAndCrowsLoud = 1;
    public const int Day_BirdsSinging = 2;
    public const int Day_BirdsSingingLoud = 3;
    public const int Day_BirdsSingingQuiet = 4;
    public const int Day_Quiet = 5;
    public const int Day_BirdsSingingOften = 6;

    public const int Morning_CricketsQuiet = 1;
    public const int Morning_CricketsTrillingQuiet = 2;
    public const int Morning_CricketsAndCicadas = 3;
    public const int Morning_CricketsTrillingLoud = 4;
    public const int Morning_CicadasAndBugs = 5;
    public const int Morning_EverythingTrilling = 6;

    public const int Night_CricketsQuiet = 1;
    public const int Night_CricketsPersistent = 2;
    public const int Night_CicadasAndCrickets = 3;
    public const int Night_CicadasConstant = 4;
    // for some reason this was 4 for quite a while...
    public const int Night_LoudEverything = 5;
    public const int Night_TrillingCricketsAndFrogs = 6;

    public const int Evening_SinewaveCicadas = 1;
    public const int Evening_LoudCricketsWithCicadas = 2;
    public const int Evening_HumidSoundingCrickets = 3;
    public const int Evening_CricketsTrilling = 4;
    public const int Evening_CricketsAndQuietCicadas = 5;
    public const int Evening_VariousAnimals = 6;
}
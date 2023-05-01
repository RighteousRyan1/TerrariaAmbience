using Terraria;
using System.Security.Permissions;
using Terraria.ModLoader;
using TerrariaAmbienceAPI.Common;
using TerrariaAmbience.Common.Systems;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Core;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace TerrariaAmbience.Content.AmbientAndMore;

public class AmbientHandler {
    #region Metadata
    public string AmbientPath => "Sounds/Custom/ambient/";

    #endregion

    public AmbientHandler() {
        Initialize();
    }

    public float TransitionHarshness;

    public ModAmbience Breeze;

    public ModAmbience ForestMorning;
    public ModAmbience ForestDay;
    public ModAmbience ForestEvening;
    public ModAmbience ForestNight;

    public ModAmbience Desert;

    public ModAmbience SnowDay;
    public ModAmbience SnowNight;

    public ModAmbience JungleDay;
    public ModAmbience JungleNight;
    public ModAmbience JungleUndergroundDay; // aka swamp
    public ModAmbience JungleUndergroundNight;

    public ModAmbience Beach;

    public ModAmbience EvilsCrimson;
    public ModAmbience EvilsCorruption;

    public ModAmbience CavernLayer;

    public ModAmbience Hell;

    public ModAmbience Underwater;

    public List<ModAmbience> Ambiences;

    // these two fields will be chosen at either the last dusk or the last dawn respectively, aka pre-emptively chosen.
    private int _chosenMorningAmbience;
    private int _chosenEveningAmbience;

    public const int NUM_EVENING_AMBIENCE = 3;
    public const int NUM_MORNING_AMBIENCE = 3;
    public const int NUM_NIGHT_AMBIENCE = 2;

    public void Initialize() {
        var mod = ModContent.GetInstance<TerrariaAmbience>();

        Breeze = new(mod, AmbientPath + "environment/breeze", "Breeze", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneOverworldHeight && !player.ZoneSkyHeight;
        });

        ForestMorning = new(mod, AmbientPath + $"biome/forest/morning_{Main.rand.Next(1, NUM_MORNING_AMBIENCE)}", "ForestMorning", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return !player.ZoneDesert && !player.ZoneSkyHeight;
        });
        ForestDay = new(mod, AmbientPath + "biome/forest/day", "ForestDay", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return (player.ZonePurity || player.ZoneMeteor || (player.ZoneHallow && !player.ZoneDesert)) &&
                    Main.dayTime;
        });
        ForestEvening = new(mod, AmbientPath + $"biome/forest/evening_{Main.rand.Next(1, NUM_EVENING_AMBIENCE)}", "ForestEvening", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return !player.ZoneDesert && !player.ZoneSkyHeight;
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
            return player.ZoneJungle && Main.dayTime;
        });
        JungleNight = new(mod, AmbientPath + "biome/jungle/night", "JungleNight", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneJungle && !Main.dayTime;
        });
        JungleUndergroundDay = new(mod, AmbientPath + "biome/jungle/underground_day", "JungleUndergroundDay", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneJungle;
        });
        JungleUndergroundNight = new(mod, AmbientPath + "biome/jungle/underground_night", "JungleUndergroundNight", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneJungle;
        });

        Beach = new(mod, AmbientPath + "biome/beach/waves", "Ocean", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            return player.ZoneBeach && !player.ZoneRockLayerHeight;
        });

        Hell = new(mod, AmbientPath + "biome/hell/rumbles", "Hell", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            return true; // we return true because we simply control the volume by the player's level of depth in Update()
        });
        Underwater = new(mod, AmbientPath + "environment/underwater_loop", "Underwater", maxVolume: 1f, volumeStep: TransitionHarshness, (a) => {
            var player = Main.LocalPlayer;
            // in the original code, the volumeStep would be TransitionHarshness / 2 when going into the water, and divided by 4 when coming out.
            // TBA whether or not that will still happen. Ryan, remember.
            return player.Underwater();
        });

        // finally, we add all of them via reflection because manual labor is stupid.
        Ambiences = new();
        foreach (var fld in GetType().GetFields().Where(x => x.FieldType.TypeHandle.Equals(typeof(ModAmbience).TypeHandle))) {
            Ambiences.Add((ModAmbience)fld.GetValue(this));
        }
    }

    public void Update() {
        if (TerrariaAmbience.JustTurnedDay) {
            _chosenEveningAmbience = Main.rand.Next(1, NUM_EVENING_AMBIENCE + 1);
            ForestEvening.ChangeTrack(AmbientPath + $"biome/forest/evening_{_chosenEveningAmbience}");
        }
        else if (TerrariaAmbience.JustTurnedNight) {
            _chosenMorningAmbience = Main.rand.Next(1, NUM_MORNING_AMBIENCE + 1);
            ForestMorning.ChangeTrack(AmbientPath + $"biome/forest/morning_{_chosenMorningAmbience}");
        }
        if (Main.gameMenu) {
            Ambiences.ForEach(x => x.MaxVolume = 0);
            return;
        }
        if (float.TryParse(ModContent.GetInstance<GeneralConfig>().transitionHarshness, out float harshness)) {
            Ambiences.ForEach(x => x.VolumeStep = TransitionHarshness);
            TransitionHarshness = harshness;
        }
        else
            ModContent.GetInstance<GeneralConfig>().transitionHarshness = "0.01"; // a failsafe in case the player is stupid
        ForestMorning.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * GradualValueSystem.Gradient_FromMorning * ModContent.GetInstance<GeneralConfig>().forestVolumes[0];
        ForestDay.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * GradualValueSystem.Gradient_FromNoon * ModContent.GetInstance<GeneralConfig>().forestVolumes[1];
        ForestEvening.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * GradualValueSystem.Gradient_FromEvening * ModContent.GetInstance<GeneralConfig>().forestVolumes[2];
        ForestNight.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * GradualValueSystem.Gradient_FromNight * ModContent.GetInstance<GeneralConfig>().forestVolumes[3];

        JungleDay.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * ModContent.GetInstance<GeneralConfig>().jungleVolumes[0];
        JungleNight.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * ModContent.GetInstance<GeneralConfig>().jungleVolumes[1];
        JungleUndergroundDay.MaxVolume = GradualValueSystem.Gradient_Underground * GradualValueSystem.Gradient_AllDayPartNight * ModContent.GetInstance<GeneralConfig>().jungleVolumes[2];
        JungleUndergroundNight.MaxVolume = GradualValueSystem.Gradient_Underground * GradualValueSystem.Gradient_AllNightPartDay * ModContent.GetInstance<GeneralConfig>().jungleVolumes[3] * 0.6f; // it's a bit louder than the day variant...

        SnowDay.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * ModContent.GetInstance<GeneralConfig>().snowVolumes[0];
        SnowNight.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * ModContent.GetInstance<GeneralConfig>().snowVolumes[1];

        Desert.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * ModContent.GetInstance<GeneralConfig>().desertVolume;
        if (Desert.SoundInstance != null)
            Desert.SoundInstance.Pitch = Main.dayTime ? 0f : -0.1f;

        Beach.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * ModContent.GetInstance<GeneralConfig>().oceanVolume;

        Breeze.MaxVolume = GradualValueSystem.Gradient_SkyToUnderground * ModContent.GetInstance<GeneralConfig>().breezeVolume;

        CavernLayer.MaxVolume = GradualValueSystem.Gradient_Underground * ModContent.GetInstance<GeneralConfig>().cavernsVolume * 0.4f;

        Hell.MaxVolume = GradualValueSystem.Gradient_Hell * ModContent.GetInstance<GeneralConfig>().hellVolume;
    }
}
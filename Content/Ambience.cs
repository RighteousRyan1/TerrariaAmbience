using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using TerrariaAmbience.Core;
using TerrariaAmbience.Content.Players;
using System.Reflection;
using Terraria.ID;
using System.IO;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Sounds;
using ReLogic.Content;
using TerrariaAmbience.Common.Systems;

namespace TerrariaAmbience.Content
{
    public class Ambience
    {
        // TODO: Pending/Potential removal of the TAAmbient slider.
        public enum AmbienceTracks
        {
            ForestMorning,
            ForestDay,
            ForestEvening,
            ForestNight,
            Desert,
            SnowDay,
            SnowNight,
            JungleDay,
            JungleNight,
            Ocean,
            EvilsCrimson,
            EvilsCorruption,
            CavernLayer,
            Hell,
            Winds
        }
        public static float TAAmbient;
        #region Forest SFX

        public static SoundEffect MorningCrickets { get; set; }
        public static SoundEffectInstance MorningCricketsInstance { get; set; }
        public static SoundEffect DayCrickets { get; set; }
        public static SoundEffectInstance DayCricketsInstance { get; set; }
        public static SoundEffect EveningCrickets { get; set; }
        public static SoundEffectInstance EveningCricketsInstance { get; set; }
        public static SoundEffect NightCrickets { get; set; }
        public static SoundEffectInstance NightCricketsInstance { get; set; }

        #endregion
        #region Jungle SFX

        public static SoundEffect DayJungle { get; set; }
        public static SoundEffectInstance DaytimeJungleInstance { get; set; }
        public static SoundEffect NightJungle { get; set; }
        public static SoundEffectInstance NightJungleInstance { get; set; }

        #endregion
        #region Beach SFX

        public static SoundEffect BeachWaves { get; set; }
        public static SoundEffectInstance BeachWavesInstance { get; set; }

        #endregion
        #region Desert SFX

        public static SoundEffect DesertAmbience { get; set; }
        public static SoundEffectInstance DesertAmbienceInstance { get; set; }

        #endregion
        #region Caves/Underground SFX

        public static SoundEffect CavesAmbience { get; set; }
        public static SoundEffectInstance CavesAmbienceInstance { get; set; }

        #endregion
        #region Snow SFX

        public static SoundEffect SnowBreezeDay { get; set; }
        public static SoundEffectInstance SnowBreezeDayInstance { get; set; }
        public static SoundEffect SnowBreezeNight { get; set; }
        public static SoundEffectInstance SnowBreezeNightInstance { get; set; }

        #endregion
        #region Hell SFX

        public static SoundEffect HellRumble { get; set; }
        public static SoundEffectInstance HellRumbleInstance { get; set; }

        #endregion
        #region World Evils SFX

        public static SoundEffect CrimsonRumbles { get; set; }
        public static SoundEffectInstance CrimsonRumblesInstance { get; set; }
        public static SoundEffect CorruptionRoars { get; set; }
        public static SoundEffectInstance CorruptionRoarsInstance { get; set; }

        #endregion
        #region Other Ambience

        public static SoundEffect CampfireCrackle { get; set; }
        public static SoundEffectInstance CampfireCrackleInstance { get; set; }

        public static SoundEffect Drip1 { get; set; }
        public static SoundEffect Drip2 { get; set; }
        public static SoundEffect Drip3 { get; set; }
        public static SoundEffectInstance Drip1Instance { get; set; }
        public static SoundEffectInstance Drip2Instance { get; set; }
        public static SoundEffectInstance Drip3Instance { get; set; }

        public static SoundEffect LavaStream { get; set; }
        public static SoundEffect WaterStream { get; set; }
        public static SoundEffectInstance LavaStreamInstance { get; set; }
        public static SoundEffectInstance WaterStreamInstance { get; set; }
        public static SoundEffect SplashNew { get; set; }
        public static SoundEffectInstance SplashNewInstance { get; set; }

        public static SoundEffect Breeze { get; set; }
        public static SoundEffectInstance BreezeInstance { get; set; }

        public static SoundEffect UnderwaterLoop { get; set; }
        public static SoundEffectInstance UnderwaterLoopInstance { get; set; }

        #endregion
        #region Volumes
        public float dayCricketsVolume;
        public float eveningCricketsVolume;
        public float nightCricketsVolume;
        public float crackleVolume;
        public float desertCricketsVolume;
        public float ugAmbienceVolume;
        public float snowDayVolume;
        public float snowNightVolume;
        public float crimsonRumblesVolume;
        public float corruptionRoarsVolume;
        public float dayJungleVolume;
        public float nightJungleVolume;
        public float beachWavesVolume;
        public float hellRumbleVolume;
        public float rainVolume;
        public float morningCricketsVolume;
        public float breezeVolume;
        public float underwaterLoopVolume;
        #endregion

        public static GeneralConfig CFG => ModContent.GetInstance<GeneralConfig>();
        public static Ambience Instance => ModContent.GetInstance<Ambience>();
        public static string AmbientPath => "Sounds/Custom/ambient";
        internal static string INTERNAL_CombinedPath => Path.Combine(ModLoader.ModPath, "TASoundReplace");
        public static string SFXReplacePath { get; internal set; } = INTERNAL_CombinedPath;
        public static SoundEffect GetSoundFromPath(string path)
        {
            if (File.Exists(path))
                return SoundEffect.FromStream(new FileStream(path, FileMode.Open));
            else
                return null;
        }
        public struct Directories
        {
            public static bool ExistsInSFXReplace(string sfx)
            {
                return File.Exists(Path.Combine(SFXReplacePath, sfx.AppendFileExtension(GeneralHelpers.AudioFileExtension.WAV)));
            }

            public static string ForestMorning => !ExistsInSFXReplace("forest_morning") ? 
                $"{AmbientPath}/biome/forest_morning" : Path.Combine(SFXReplacePath, "forest_morning.wav");
            public static string ForestDay => !ExistsInSFXReplace("forest_day") ? 
                $"{AmbientPath}/biome/forest_day" : Path.Combine(SFXReplacePath, "forest_day.wav");
            public static string ForestEvening => !ExistsInSFXReplace("forest_evening") ?
                $"{AmbientPath}/biome/forest_evening" : Path.Combine(SFXReplacePath, "forest_evening.wav");
            public static string ForestNight => !ExistsInSFXReplace("forest_night") ?
                $"{AmbientPath}/biome/forest_night" : Path.Combine(SFXReplacePath, "forest_night.wav");

            public static string Desert => !ExistsInSFXReplace("desert_crickets") ?
                $"{AmbientPath}/biome/desert_crickets" : Path.Combine(SFXReplacePath, "desert_crickets.wav");

            public static string SnowDay => !ExistsInSFXReplace("snowfall_day") ?
                $"{AmbientPath}/biome/snowfall_day" : Path.Combine(SFXReplacePath, "snowfall_day.wav");
            public static string SnowNight => !ExistsInSFXReplace("snowfall_night") ? 
                $"{AmbientPath}/biome/snowfall_night" : Path.Combine(SFXReplacePath, "snowfall_night.wav");

            public static string JungleDay => !ExistsInSFXReplace("jungle_day") ?
                $"{AmbientPath}/biome/jungle_day" : Path.Combine(SFXReplacePath, "jungle_day.wav");
            public static string JungleNight => !ExistsInSFXReplace("jungle_night") ? 
                $"{AmbientPath}/biome/jungle_night" : Path.Combine(SFXReplacePath, "jungle_night.wav");

            public static string Corruption => !ExistsInSFXReplace("corruption_roars") ? 
                $"{AmbientPath}/biome/corruption_roars" : Path.Combine(SFXReplacePath, "corruption_roars.wav");
            public static string Crimson => !ExistsInSFXReplace("crimson_rumbles") ? 
                $"{AmbientPath}/biome/crimson_rumbles" : Path.Combine(SFXReplacePath, "crimson_rumbles.wav");

            public static string CavernLayer => !ExistsInSFXReplace("ug_drip") ? 
                $"{AmbientPath}/biome/ug_drip" : Path.Combine(SFXReplacePath, "ug_drip.wav");
            public static string HellLayer => !ExistsInSFXReplace("hell_rumble") ?
                $"{AmbientPath}/biome/hell_rumble" : Path.Combine(SFXReplacePath, "hell_rumble.wav");

            public static string Beach => !ExistsInSFXReplace("beach_waves") ? 
                $"{AmbientPath}/biome/beach_waves" : Path.Combine(SFXReplacePath, "beach_waves.wav");

            public static string Breeze => !ExistsInSFXReplace("breeze") ? 
                $"{AmbientPath}/environment/breeze" : Path.Combine(SFXReplacePath, "breeze.wav");

            public static string Campfire => $"{AmbientPath}/environment/campfire_crackle";
        }
        public static void Initialize()
        {
            var mod = ModContent.GetInstance<TerrariaAmbience>();
            // var loader = Instance;

            if (!Main.dedServ)
            {
                MorningCrickets = !Directories.ExistsInSFXReplace("forest_morning") ? mod.Assets.Request<SoundEffect>(Directories.ForestMorning, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/forest_morning.wav");
                MorningCricketsInstance = MorningCrickets.CreateInstance();
                MorningCrickets.Name = "Morning Crickets";
                DayCrickets = !Directories.ExistsInSFXReplace("forest_day") ? mod.Assets.Request<SoundEffect>(Directories.ForestDay, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/forest_day.wav");
                DayCricketsInstance = DayCrickets.CreateInstance();
                DayCrickets.Name = "Daytime Crickets";
                EveningCrickets = !Directories.ExistsInSFXReplace("forest_evening") ? mod.Assets.Request<SoundEffect>(Directories.ForestEvening, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/forest_evening.wav");
                EveningCricketsInstance = EveningCrickets.CreateInstance();
                EveningCrickets.Name = "Evening Crickets";
                NightCrickets = !Directories.ExistsInSFXReplace("forest_night") ? mod.Assets.Request<SoundEffect>(Directories.ForestNight, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/forest_night.wav");
                NightCricketsInstance = NightCrickets.CreateInstance();
                NightCrickets.Name = "Night Crickets";

                DesertAmbience = !Directories.ExistsInSFXReplace("desert_crickets") ? mod.Assets.Request<SoundEffect>(Directories.Desert, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/desert_crickets.wav");
                DesertAmbienceInstance = DesertAmbience.CreateInstance();
				DesertAmbience.Name = "Desert Animals";

                CampfireCrackle = mod.Assets.Request<SoundEffect>(Directories.Campfire, AssetRequestMode.ImmediateLoad).Value;
                CampfireCrackleInstance = CampfireCrackle.CreateInstance();
                CampfireCrackle.Name = "Campfire Embers";

                CavesAmbience = !Directories.ExistsInSFXReplace("ug_drip") ? mod.Assets.Request<SoundEffect>(Directories.CavernLayer, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/ug_drip.wav");
                CavesAmbienceInstance = CavesAmbience.CreateInstance();
                CavesAmbience.Name = "Caves";

                SnowBreezeDay = !Directories.ExistsInSFXReplace("snowfall_day") ? mod.Assets.Request<SoundEffect>(Directories.SnowDay, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/snowfall_day.wav");
                SnowBreezeDayInstance = SnowBreezeDay.CreateInstance();
                SnowBreezeDay.Name = "Daytime Snowfall";

                SnowBreezeNight = !Directories.ExistsInSFXReplace("snowfall_night") ? mod.Assets.Request<SoundEffect>(Directories.SnowNight, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/snowfall_night.wav");
                SnowBreezeNightInstance = SnowBreezeNight.CreateInstance();
                SnowBreezeNight.Name = "Night Snowfall";

                CrimsonRumbles = !Directories.ExistsInSFXReplace("crimson_rumbles") ? mod.Assets.Request<SoundEffect>(Directories.Crimson, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/crimson_rumbles.wav");
                CrimsonRumblesInstance = CrimsonRumbles.CreateInstance();
                CrimsonRumbles.Name = "Crimson Rumbles";

                CorruptionRoars = !Directories.ExistsInSFXReplace("corruption_roars") ? mod.Assets.Request<SoundEffect>(Directories.Corruption, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/corruption_roars.wav");
                CorruptionRoarsInstance = CorruptionRoars.CreateInstance();
                CorruptionRoars.Name = "Corruption Roars";

                DayJungle = !Directories.ExistsInSFXReplace("jungle_day") ? mod.Assets.Request<SoundEffect>(Directories.JungleDay, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/jungle_day.wav");
                DaytimeJungleInstance = DayJungle.CreateInstance();
                DayJungle.Name = "Daytime Jungle";
                NightJungle = !Directories.ExistsInSFXReplace("jungle_night") ? mod.Assets.Request<SoundEffect>(Directories.JungleNight, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/jungle_night.wav");
                NightJungleInstance = NightJungle.CreateInstance();
                NightJungle.Name = "Night Jungle";

                BeachWaves = !Directories.ExistsInSFXReplace("beach_waves") ? mod.Assets.Request<SoundEffect>(Directories.Beach, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/beach_waves.wav");
                BeachWavesInstance = BeachWaves.CreateInstance();
                BeachWaves.Name = "Beach Waves";
                HellRumble = !Directories.ExistsInSFXReplace("hell_rumble") ? mod.Assets.Request<SoundEffect>(Directories.HellLayer, AssetRequestMode.ImmediateLoad).Value : GetSoundFromPath(SFXReplacePath + "/hell_rumble.wav");
                HellRumbleInstance = HellRumble.CreateInstance();
                HellRumble.Name = "Hell Rumbling";

                SplashNew = mod.Assets.Request<SoundEffect>($"{AmbientPath}/environment/liquid/entity_splash_light", AssetRequestMode.ImmediateLoad).Value;
                SplashNewInstance = SplashNew.CreateInstance();
                SplashNew.Name = "Water Splash (Light)";

                LavaStream = mod.Assets.Request<SoundEffect>($"{AmbientPath}/environment/liquid/stream_lava", AssetRequestMode.ImmediateLoad).Value;
                WaterStream = mod.Assets.Request<SoundEffect>($"{AmbientPath}/environment/liquid/stream_water", AssetRequestMode.ImmediateLoad).Value;
                LavaStreamInstance = LavaStream.CreateInstance();
                WaterStreamInstance = WaterStream.CreateInstance();

                UnderwaterLoop = mod.Assets.Request<SoundEffect>($"Sounds/Custom/underwater_loop", AssetRequestMode.ImmediateLoad).Value;
                UnderwaterLoopInstance = UnderwaterLoop.CreateInstance();

                Breeze = mod.Assets.Request<SoundEffect>(Directories.Breeze, AssetRequestMode.ImmediateLoad).Value;
                BreezeInstance = Breeze.CreateInstance();
                Breeze.Name = "Wind Breeze";

                DayCricketsInstance.IsLooped = true;
                EveningCricketsInstance.IsLooped = true;
                NightCricketsInstance.IsLooped = true;
                CampfireCrackleInstance.IsLooped = true;
                DesertAmbienceInstance.IsLooped = true;
                CavesAmbienceInstance.IsLooped = true;
                SnowBreezeDayInstance.IsLooped = true;
                SnowBreezeNightInstance.IsLooped = true;
                CrimsonRumblesInstance.IsLooped = true;
                CorruptionRoarsInstance.IsLooped = true;
                DaytimeJungleInstance.IsLooped = true;
                NightJungleInstance.IsLooped = true;
                BeachWavesInstance.IsLooped = true;
                HellRumbleInstance.IsLooped = true;
                LavaStreamInstance.IsLooped = true;
                WaterStreamInstance.IsLooped = true;
                MorningCricketsInstance.IsLooped = true;
                BreezeInstance.IsLooped = true;
                UnderwaterLoopInstance.IsLooped = true;

                UnderwaterLoopInstance.Volume = 0f;
            }
        }
        public static float transitionHarshness = 0.0075f;
        public static void UpdateVolume()
        {
            if (float.TryParse(ModContent.GetInstance<GeneralConfig>().transitionHarshness, out float given) && given != 0f)
            {
                transitionHarshness = float.Parse(ModContent.GetInstance<GeneralConfig>().transitionHarshness);
            }
            else
            {
                ModContent.GetInstance<GeneralConfig>().transitionHarshness = "0.01";
                // decOrIncRate = 0.01f;
            }
            if (Main.gameMenu || Main.dedServ) return;
            AmbientPlayer ambiencePlayer = Main.player[Main.myPlayer].GetModPlayer<AmbientPlayer>();

            Player player = ambiencePlayer.Player;
            var aLoader = Instance;
            UnderwaterLoopInstance.Volume = aLoader.underwaterLoopVolume;
            if (Main.hasFocus)
            {
                #region GeneralVolumeUpdate
                aLoader.underwaterLoopVolume += player.Underwater() ? transitionHarshness / 2 : -transitionHarshness * 4;
                aLoader.dayCricketsVolume +=
                    (player.ZonePurity || player.ZoneMeteor || (player.ZoneHallow && !player.ZoneDesert)) &&
                    Main.dayTime ?
                    transitionHarshness : -transitionHarshness;

                if (player.ZoneOverworldHeight && !player.ZoneSkyHeight)
                {
                    aLoader.breezeVolume += transitionHarshness;
                }
                else
                {
                    aLoader.breezeVolume -= transitionHarshness;
                }
                if (!player.ZoneDesert && !player.ZoneSkyHeight)
                {
                    aLoader.eveningCricketsVolume += transitionHarshness;
                }
                else
                {
                    aLoader.eveningCricketsVolume -= transitionHarshness;
                }
                if (!player.ZoneDesert && !player.ZoneSkyHeight)
                {
                    aLoader.morningCricketsVolume += transitionHarshness;
                }
                else
                {
                    aLoader.morningCricketsVolume -= transitionHarshness;
                }
                if (player.ZonePurity && !player.ZoneDesert && !Main.dayTime)
                {
                    aLoader.nightCricketsVolume += transitionHarshness;
                }
                else
                {
                    aLoader.nightCricketsVolume -= transitionHarshness;
                }

                if (player.Center.Y >= Main.worldSurface * 16)
                {
                    aLoader.ugAmbienceVolume += transitionHarshness;
                }
                else
                {
                    aLoader.ugAmbienceVolume -= transitionHarshness;
                }

                if (player.ZoneDesert)
                {
                    aLoader.desertCricketsVolume += transitionHarshness;
                }
                else
                {
                    aLoader.desertCricketsVolume -= transitionHarshness;
                }
                if (!Main.dayTime)
                {
                    DesertAmbienceInstance.Pitch = -0.1f;
                }
                else
                {
                    DesertAmbienceInstance.Pitch = 0f;
                }

                if (player.ZoneSnow && Main.dayTime && !player.ZoneUnderworldHeight)
                {
                    aLoader.snowDayVolume += transitionHarshness;
                }
                else
                {
                    aLoader.snowDayVolume -= transitionHarshness;
                }

                if (player.ZoneSnow && !Main.dayTime && !player.ZoneUnderworldHeight)
                {
                    aLoader.snowNightVolume += transitionHarshness;
                }
                else
                {
                    aLoader.snowNightVolume -= transitionHarshness;
                }
                if (player.ZoneCrimson)
                {
                    aLoader.crimsonRumblesVolume += transitionHarshness;
                }
                else
                {
                    aLoader.crimsonRumblesVolume -= transitionHarshness;
                }
                if (player.ZoneCorrupt)
                {
                    aLoader.corruptionRoarsVolume += transitionHarshness;
                }
                else
                {
                    aLoader.corruptionRoarsVolume -= transitionHarshness;
                }

                if (player.ZoneJungle && Main.dayTime)
                {
                    aLoader.dayJungleVolume += transitionHarshness;
                }
                else
                {
                    aLoader.dayJungleVolume -= transitionHarshness;
                }

                if (player.ZoneJungle && !Main.dayTime)
                {
                    aLoader.nightJungleVolume += transitionHarshness;
                }
                else
                {
                    aLoader.nightJungleVolume -= transitionHarshness;
                }

                if (player.ZoneBeach && !player.ZoneRockLayerHeight)
                {
                    aLoader.beachWavesVolume += transitionHarshness;
                }
                else
                {
                    aLoader.beachWavesVolume -= transitionHarshness;
                }
                aLoader.hellRumbleVolume += transitionHarshness;
                #endregion
            }
            if (!Main.hasFocus || Main.gameMenu)
            {
                aLoader.dayCricketsVolume -= transitionHarshness;
                aLoader.eveningCricketsVolume -= transitionHarshness;
                aLoader.nightCricketsVolume -= transitionHarshness;
                aLoader.crackleVolume -= transitionHarshness;
                aLoader.desertCricketsVolume -= transitionHarshness;
                aLoader.ugAmbienceVolume -= transitionHarshness;
                aLoader.snowDayVolume -= transitionHarshness;
                aLoader.snowNightVolume -= transitionHarshness;
                aLoader.crimsonRumblesVolume -= transitionHarshness;
                aLoader.corruptionRoarsVolume -= transitionHarshness;
                aLoader.dayJungleVolume -= transitionHarshness;
                aLoader.nightJungleVolume -= transitionHarshness;
                aLoader.beachWavesVolume -= transitionHarshness;
                aLoader.hellRumbleVolume -= transitionHarshness;
                aLoader.rainVolume -= transitionHarshness;
                aLoader.morningCricketsVolume -= transitionHarshness;
                aLoader.breezeVolume -= transitionHarshness;
                aLoader.underwaterLoopVolume -= transitionHarshness;
            }
        }
        /// <summary>
        /// Useable only in this assembly for many reasons. Plays all sounds once they are initialized.
        /// </summary>
        internal static void PlayAllAmbience()
        {
            DayCricketsInstance?.Play();
            EveningCricketsInstance?.Play();
            NightCricketsInstance?.Play();
            CampfireCrackleInstance?.Play();
            DesertAmbienceInstance?.Play();
            CavesAmbienceInstance?.Play();
            SnowBreezeDayInstance?.Play();
            SnowBreezeNightInstance?.Play();
            CrimsonRumblesInstance?.Play();
            CorruptionRoarsInstance?.Play();
            DaytimeJungleInstance?.Play();
            NightJungleInstance?.Play();
            BeachWavesInstance?.Play();
            HellRumbleInstance?.Play();
            MorningCricketsInstance?.Play();
            BreezeInstance?.Play();
            UnderwaterLoopInstance?.Play();
        }
        private bool playerBehindWall;
        private bool playerInLiquid;
        /// <summary>
        /// Updates everything that has to do with ambiences.
        /// </summary>
        internal static void DoUpdate_Ambience()
        {
            var cfgCl = ModContent.GetInstance<GeneralConfig>();
            var inst = Instance;
            float ambVol = TAAmbient / 100;
            if (!Main.dedServ)
            {
                /*if (Main.gameMenu)
                {
                    DayCricketsInstance.Volume = Instance.dayCricketsVolume * 0.75f * ambVol;
                    EveningCricketsInstance.Volume = Instance.eveningCricketsVolume * 0.75f;
                    NightCricketsInstance.Volume = Instance.nightCricketsVolume * 0.75f * ambVol;
                    DesertAmbienceInstance.Volume = Instance.desertCricketsVolume * 0.75f * ambVol;
                    CavesAmbienceInstance.Volume = Instance.ugAmbienceVolume * 0.75f * ambVol;
                    CrimsonRumblesInstance.Volume = Instance.crimsonRumblesVolume * 0.7f * ambVol;
                    CorruptionRoarsInstance.Volume = Instance.corruptionRoarsVolume * 0.7f * ambVol;
                    MorningCricketsInstance.Volume = Instance.morningCricketsVolume * ambVol;
                    BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeedCurrent) * ambVol;

                    HellRumbleInstance.Volume = Instance.hellRumbleVolume * 0.75f * ambVol;

                    BeachWavesInstance.Volume = Instance.beachWavesVolume * 0.8f * ambVol;

                    DaytimeJungleInstance.Volume = Instance.dayJungleVolume * 0.5f * ambVol;
                    NightJungleInstance.Volume = Instance.nightJungleVolume * 0.5f * ambVol;

                    SnowBreezeDayInstance.Volume = Instance.snowDayVolume * 0.65f * ambVol;
                    SnowBreezeNightInstance.Volume = Instance.snowNightVolume * 0.7f * ambVol;

                    CampfireCrackleInstance.Volume = Instance.crackleVolume * 0.95f * Main.soundVolume;

                    UnderwaterLoopInstance.Volume = Instance.underwaterLoopVolume * 0.8f * ambVol;
                }*/
                if (!Instance.playerBehindWall)
                {
                    DayCricketsInstance.Volume = Instance.dayCricketsVolume * 0.9f * ambVol;
                    EveningCricketsInstance.Volume = Instance.eveningCricketsVolume * 0.85f * ambVol;
                    NightCricketsInstance.Volume = Instance.nightCricketsVolume * ambVol;
                    DesertAmbienceInstance.Volume = Instance.desertCricketsVolume * 0.75f * ambVol;
                    CavesAmbienceInstance.Volume = Instance.ugAmbienceVolume * 0.35f * ambVol;
                    CrimsonRumblesInstance.Volume = Instance.crimsonRumblesVolume * 0.7f * ambVol;
                    CorruptionRoarsInstance.Volume = Instance.corruptionRoarsVolume * 0.7f * ambVol;
                    MorningCricketsInstance.Volume = Instance.morningCricketsVolume * 0.9f * ambVol;
                    BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeedCurrent) * ambVol;

                    HellRumbleInstance.Volume = Instance.hellRumbleVolume * 0.75f * ambVol;

                    BeachWavesInstance.Volume = Instance.beachWavesVolume * 0.8f * ambVol;

                    DaytimeJungleInstance.Volume = Instance.dayJungleVolume * 0.4f * ambVol;
                    NightJungleInstance.Volume = Instance.nightJungleVolume * 0.4f * ambVol;

                    SnowBreezeDayInstance.Volume = Instance.snowDayVolume * 0.65f * ambVol;
                    SnowBreezeNightInstance.Volume = Instance.snowNightVolume * 0.7f * ambVol;

                    if (Instance.crackleVolume >= 0f && Instance.crackleVolume <= 1f)
                        CampfireCrackleInstance.Volume = Instance.crackleVolume * 0.95f * Main.soundVolume;
                }
                else
                {
                    BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeedCurrent) * ambVol * 0.8f;
                    MorningCricketsInstance.Volume = Instance.morningCricketsVolume * 0.8f * ambVol;
                    DayCricketsInstance.Volume = Instance.dayCricketsVolume * 0.7f * ambVol;
                    EveningCricketsInstance.Volume = Instance.eveningCricketsVolume * 0.7f * ambVol;
                    NightCricketsInstance.Volume = Instance.nightCricketsVolume * 0.7f * ambVol;
                    DesertAmbienceInstance.Volume = Instance.desertCricketsVolume * 0.7f * ambVol;
                    CavesAmbienceInstance.Volume = Instance.ugAmbienceVolume * 0.25f * ambVol;
                    CrimsonRumblesInstance.Volume = Instance.crimsonRumblesVolume * 0.3f * ambVol;
                    CorruptionRoarsInstance.Volume = Instance.corruptionRoarsVolume * 0.3f * ambVol;

                    HellRumbleInstance.Volume = Instance.hellRumbleVolume * 0.5f * ambVol;

                    BeachWavesInstance.Volume = Instance.beachWavesVolume * 0.5f * ambVol;

                    DaytimeJungleInstance.Volume = Instance.dayJungleVolume * 0.35f * ambVol;
                    NightJungleInstance.Volume = Instance.nightJungleVolume * 0.35f * ambVol;

                    SnowBreezeDayInstance.Volume = Instance.snowDayVolume * 0.45f * ambVol;
                    SnowBreezeNightInstance.Volume = Instance.snowNightVolume * 0.45f * ambVol;

                    if (Instance.crackleVolume >= 0f && Instance.crackleVolume <= 1f)
                        CampfireCrackleInstance.Volume = Instance.crackleVolume * 0.8f * Main.soundVolume;
                }
                if (Instance.playerBehindWall && !Instance.playerInLiquid)
                {
                    BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeedCurrent) * ambVol * 0.6f;
                }
                if (Instance.playerBehindWall && Instance.playerInLiquid)
                {
                    BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeedCurrent) * ambVol * 0.4f;
                }

                UnderwaterLoopInstance.Volume = Instance.underwaterLoopVolume * ambVol;
            }

            if (Main.gameMenu) return;
            Player player = Main.player[Main.myPlayer]?.GetModPlayer<AmbientPlayer>().Player;

            if (player.Underwater())
            {
                float defaultBand = 0.075f;
                Instance.playerInLiquid = true;
                Instance.dayCricketsVolume *= 0.85f;
                Instance.eveningCricketsVolume *= 0.85f;
                Instance.nightCricketsVolume *= 0.85f;
                Instance.desertCricketsVolume *= 0.85f;
                Instance.ugAmbienceVolume *= 0.85f;
                Instance.crackleVolume *= 0.85f;
                Instance.snowNightVolume *= 0f;
                Instance.snowDayVolume *= 0f;
                Instance.crimsonRumblesVolume *= 0.9f;
                Instance.corruptionRoarsVolume *= 0.9f;
                Instance.dayJungleVolume *= 0.9f;
                Instance.nightJungleVolume *= 0.9f;
                Instance.hellRumbleVolume *= 0.95f;
                Instance.morningCricketsVolume *= 0.95f;
                Instance.breezeVolume *= 0.7f;

                NightCricketsInstance.ApplyBandPassFilter(defaultBand);
                DayCricketsInstance.ApplyBandPassFilter(defaultBand);
                EveningCricketsInstance.ApplyBandPassFilter(defaultBand);
                CampfireCrackleInstance.ApplyBandPassFilter(defaultBand);
                DesertAmbienceInstance.ApplyBandPassFilter(defaultBand);
                CavesAmbienceInstance.ApplyBandPassFilter(defaultBand);
                CrimsonRumblesInstance.ApplyBandPassFilter(defaultBand);
                CorruptionRoarsInstance.ApplyBandPassFilter(defaultBand);
                DaytimeJungleInstance.ApplyBandPassFilter(defaultBand);
                NightJungleInstance.ApplyBandPassFilter(defaultBand);
                BeachWavesInstance.ApplyBandPassFilter(defaultBand);
                HellRumbleInstance.ApplyBandPassFilter(defaultBand);
                MorningCricketsInstance.ApplyBandPassFilter(defaultBand);
                BreezeInstance.ApplyBandPassFilter(defaultBand);
            }

            var wNear = player.GetModPlayer<AmbientPlayer>().WallsAround;

            Vector2 newTop = player.Top + new Vector2(0, 7.5f);
            if (!WallID.Search.GetName(Main.tile[(int)newTop.X / 16, (int)newTop.Y / 16].WallType).ToLower().Contains("fence")
                && wNear >= 16 )
            {
                Instance.playerBehindWall = true;
            }
            else
                Instance.playerBehindWall = false;
            // https://i.kym-cdn.com/entries/icons/facebook/000/001/576/Screen_Shot_2021-03-22_at_3.45.30_PM.jpg
            // this code == the link
            if (!cfgCl.enabledForest[0])
                inst.morningCricketsVolume = 0f;
            if (!cfgCl.enabledForest[1])
                inst.dayCricketsVolume = 0f;
            if (!cfgCl.enabledForest[2])
                inst.eveningCricketsVolume = 0f;
            if (!cfgCl.enabledForest[3])
                inst.nightCricketsVolume = 0f;

            if (!cfgCl.enabledSnow[0])
                inst.snowDayVolume = 0f;
            if (!cfgCl.enabledSnow[1])
                inst.snowNightVolume = 0f;
            if (!cfgCl.enabledJungle[0])
                inst.dayJungleVolume = 0f;
            if (!cfgCl.enabledJungle[1])
                inst.nightJungleVolume = 0f;
            if (!cfgCl.enabledEvils[0])
                inst.corruptionRoarsVolume = 0f;
            if (!cfgCl.enabledEvils[1])
                inst.crimsonRumblesVolume = 0f;
            if (!cfgCl.enabledHell)
                inst.hellRumbleVolume = 0f;
            if (!cfgCl.enabledCavern)
                inst.ugAmbienceVolume = 0f;
            if (!cfgCl.enabledBreeze)
                inst.breezeVolume = 0f;
            if (!cfgCl.enabledOcean)
                inst.beachWavesVolume = 0f;
            if (!cfgCl.enabledDesert)
                inst.desertCricketsVolume = 0f;
        }
        /// <summary>
        /// Oddly enough, MathHelper.Clamp is completely useless in this case since it works every 2 frames or something akin to that.
        /// </summary>
        public static void ClampAll()
        {
            if (!Main.dedServ)
            {
                var aLoader = Instance;
                if (aLoader.underwaterLoopVolume > 1)
                    aLoader.underwaterLoopVolume = 1;
                if (aLoader.underwaterLoopVolume < 0)
                    aLoader.underwaterLoopVolume = 0;
                if (aLoader.breezeVolume > 1)
                    aLoader.breezeVolume = 1;
                if (aLoader.breezeVolume < 0)
                    aLoader.breezeVolume = 0;
                if (BreezeInstance.Volume > 1)
                    BreezeInstance.Volume = 1;
                if (BreezeInstance.Volume < 0)
                    BreezeInstance.Volume = 0;
                if (aLoader.morningCricketsVolume > (float)GradualValueSystem.Gradient_FromMorning)
                    aLoader.morningCricketsVolume = (float)GradualValueSystem.Gradient_FromMorning;
                if (aLoader.morningCricketsVolume < 0)
                    aLoader.morningCricketsVolume = 0;
                if (aLoader.dayJungleVolume > (float)GradualValueSystem.Gradient_FromNoon)
                    aLoader.dayJungleVolume = (float)GradualValueSystem.Gradient_FromNoon;
                if (aLoader.dayJungleVolume < 0)
                    aLoader.dayJungleVolume = 0;
                if (aLoader.nightJungleVolume > (float)GradualValueSystem.Gradient_FromNight)
                    aLoader.nightJungleVolume = (float)GradualValueSystem.Gradient_FromNight;
                if (aLoader.nightJungleVolume < 0)
                    aLoader.nightJungleVolume = 0;

                if (aLoader.dayCricketsVolume > (float)GradualValueSystem.Gradient_FromNoon)
                    aLoader.dayCricketsVolume = (float)GradualValueSystem.Gradient_FromNoon;
                if (aLoader.dayCricketsVolume < 0)
                    aLoader.dayCricketsVolume = 0;

                if (aLoader.eveningCricketsVolume > (float)GradualValueSystem.Gradient_FromEvening)
                    aLoader.eveningCricketsVolume = (float)GradualValueSystem.Gradient_FromEvening;
                if (aLoader.eveningCricketsVolume < 0)
                    aLoader.eveningCricketsVolume = 0;

                if (aLoader.nightCricketsVolume > (float)GradualValueSystem.Gradient_FromNight)
                    aLoader.nightCricketsVolume = (float)GradualValueSystem.Gradient_FromNight;
                if (aLoader.nightCricketsVolume < 0)
                    aLoader.nightCricketsVolume = 0;
                if (aLoader.crackleVolume > 1)
                    aLoader.crackleVolume = 1;
                if (aLoader.crackleVolume < 0)
                    aLoader.crackleVolume = 0;

                if (aLoader.desertCricketsVolume > (float)GradualValueSystem.Gradient_SkyToUnderground)
                    aLoader.desertCricketsVolume = (float)GradualValueSystem.Gradient_SkyToUnderground;
                if (aLoader.desertCricketsVolume < 0)
                    aLoader.desertCricketsVolume = 0;

                if (aLoader.ugAmbienceVolume > (float)GradualValueSystem.Gradient_Underground)
                    aLoader.ugAmbienceVolume = (float)GradualValueSystem.Gradient_Underground;
                if (aLoader.ugAmbienceVolume < 0)
                    aLoader.ugAmbienceVolume = 0;

                if (aLoader.snowNightVolume > (float)GradualValueSystem.Gradient_SkyToUnderground)
                    aLoader.snowNightVolume = (float)GradualValueSystem.Gradient_SkyToUnderground;
                if (aLoader.snowNightVolume < 0)
                    aLoader.snowNightVolume = 0;
                if (aLoader.snowDayVolume > (float)GradualValueSystem.Gradient_SkyToUnderground)
                    aLoader.snowDayVolume = (float)GradualValueSystem.Gradient_SkyToUnderground;
                if (aLoader.snowDayVolume < 0)
                    aLoader.snowDayVolume = 0;

                if (aLoader.crimsonRumblesVolume > 1)
                    aLoader.crimsonRumblesVolume = 1;
                if (aLoader.crimsonRumblesVolume < 0)
                    aLoader.crimsonRumblesVolume = 0;

                if (aLoader.corruptionRoarsVolume > 1)
                    aLoader.corruptionRoarsVolume = 1;
                if (aLoader.corruptionRoarsVolume < 0)
                    aLoader.corruptionRoarsVolume = 0;

                if (aLoader.beachWavesVolume > 1)
                    aLoader.beachWavesVolume = 1;
                if (aLoader.beachWavesVolume < 0)
                    aLoader.beachWavesVolume = 0;

                if (aLoader.hellRumbleVolume > (float)GradualValueSystem.Gradient_Hell)
                    aLoader.hellRumbleVolume = (float)GradualValueSystem.Gradient_Hell;
                if (aLoader.hellRumbleVolume < 0)
                    aLoader.hellRumbleVolume = 0;
            }
        }
    }
}
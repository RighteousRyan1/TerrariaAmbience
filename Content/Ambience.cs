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

namespace TerrariaAmbience.Content
{
    public class Ambience
    {
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
        public static float fStepsVol;
        // I really wished that using a list or an array for these values could affect them, through experimentation, I realized..
        // No, it's not gonna work. :/

        // In hindsight, I realize that this code could be ultra-optimised, but I'll do that when I have the time to, since I am/was on
        // spring break as of writing this code.

        // If someone wants to PR to this repository, it is welcome as long as you don't ruin anything :)

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

        public static SoundEffect Rain { get; set; }
        public static SoundEffectInstance RainInstance { get; set; }

        public static SoundEffect SplashNew { get; set; }
        public static SoundEffectInstance SplashNewInstance { get; set; }

        public static SoundEffect Breeze { get; set; }
        public static SoundEffectInstance BreezeInstance { get; set; }

        #endregion

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


        public static AmbientConfigClient CFG => ModContent.GetInstance<AmbientConfigClient>();
        public static Ambience Instance => ModContent.GetInstance<Ambience>();
        public static string AmbientPath  => "Sounds/Custom/ambient";
        public static string AmbientPathWithModName => "TerrariaAmbience/Sounds/Custom/ambient";
        public static string SFXReplacePath => Path.Combine(ModLoader.ModPath, "TASoundReplace");

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
                return File.Exists(Path.Combine(SFXReplacePath, sfx.AppendFileExtension(Helpers.GeneralHelpers.AudioFileExtension.WAV)));
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

            public static string Rain => !ExistsInSFXReplace("rain_new") ? 
                $"{AmbientPath}/rain/rain_new" : Path.Combine(SFXReplacePath, "rain_new.wav");
        }

        public static void Initialize()
        {
            var mod = ModContent.GetInstance<TerrariaAmbience>();
            // var loader = Instance;

            if (!Main.dedServ)
            {
                MorningCrickets = !Directories.ExistsInSFXReplace("forest_morning") ? mod.GetSound(Directories.ForestMorning) : GetSoundFromPath(SFXReplacePath + "/forest_morning.wav");
                MorningCricketsInstance = MorningCrickets.CreateInstance();
                MorningCrickets.Name = "Morning Crickets";
                DayCrickets = !Directories.ExistsInSFXReplace("forest_day") ? mod.GetSound(Directories.ForestDay) : GetSoundFromPath(SFXReplacePath + "/forest_day.wav");
                DayCricketsInstance = DayCrickets.CreateInstance();
                DayCrickets.Name = "Daytime Crickets";
                EveningCrickets = !Directories.ExistsInSFXReplace("forest_evening") ? mod.GetSound(Directories.ForestEvening) : GetSoundFromPath(SFXReplacePath + "/forest_evening.wav");
                EveningCricketsInstance = EveningCrickets.CreateInstance();
                EveningCrickets.Name = "Evening Crickets";
                NightCrickets = !Directories.ExistsInSFXReplace("forest_night") ? mod.GetSound(Directories.ForestNight) : GetSoundFromPath(SFXReplacePath + "/forest_night.wav");
                NightCricketsInstance = NightCrickets.CreateInstance();
                NightCrickets.Name = "Night Crickets";

                DesertAmbience = !Directories.ExistsInSFXReplace("desert_crickets") ? mod.GetSound(Directories.Desert) : GetSoundFromPath(SFXReplacePath + "/desert_crickets.wav");
                DesertAmbienceInstance = DesertAmbience.CreateInstance();
				DesertAmbience.Name = "Desert Animals";

                CampfireCrackle = mod.GetSound(Directories.Campfire);
                CampfireCrackleInstance = CampfireCrackle.CreateInstance();
                CampfireCrackle.Name = "Campfire Embers";

                CavesAmbience = !Directories.ExistsInSFXReplace("ug_drip") ? mod.GetSound(Directories.CavernLayer) : GetSoundFromPath(SFXReplacePath + "/ug_drip.wav");
                CavesAmbienceInstance = CavesAmbience.CreateInstance();
                CavesAmbience.Name = "Caves";

                SnowBreezeDay = !Directories.ExistsInSFXReplace("snowfall_day") ? mod.GetSound(Directories.SnowDay) : GetSoundFromPath(SFXReplacePath + "/snowfall_day.wav");
                SnowBreezeDayInstance = SnowBreezeDay.CreateInstance();
                SnowBreezeDay.Name = "Daytime Snowfall";

                SnowBreezeNight = !Directories.ExistsInSFXReplace("snowfall_night") ? mod.GetSound(Directories.SnowNight) : GetSoundFromPath(SFXReplacePath + "/snowfall_night.wav");
                SnowBreezeNightInstance = SnowBreezeNight.CreateInstance();
                SnowBreezeNight.Name = "Night Snowfall";

                CrimsonRumbles = !Directories.ExistsInSFXReplace("crimson_rumbles") ? mod.GetSound(Directories.Crimson) : GetSoundFromPath(SFXReplacePath + "/crimson_rumbles.wav");
                CrimsonRumblesInstance = CrimsonRumbles.CreateInstance();
                CrimsonRumbles.Name = "Crimson Rumbles";

                CorruptionRoars = !Directories.ExistsInSFXReplace("corruption_roars") ? mod.GetSound(Directories.Corruption) : GetSoundFromPath(SFXReplacePath + "/corruption_roars.wav");
                CorruptionRoarsInstance = CorruptionRoars.CreateInstance();
                CorruptionRoars.Name = "Corruption Roars";

                DayJungle = !Directories.ExistsInSFXReplace("jungle_day") ? mod.GetSound(Directories.JungleDay) : GetSoundFromPath(SFXReplacePath + "/jungle_day.wav");
                DaytimeJungleInstance = DayJungle.CreateInstance();
                DayJungle.Name = "Daytime Jungle";
                NightJungle = !Directories.ExistsInSFXReplace("jungle_night") ? mod.GetSound(Directories.JungleNight) : GetSoundFromPath(SFXReplacePath + "/jungle_night.wav");
                NightJungleInstance = NightJungle.CreateInstance();
                NightJungle.Name = "Night Jungle";

                BeachWaves = !Directories.ExistsInSFXReplace("beach_waves") ? mod.GetSound(Directories.Beach) : GetSoundFromPath(SFXReplacePath + "/beach_waves.wav");
                BeachWavesInstance = BeachWaves.CreateInstance();
                BeachWaves.Name = "Beach Waves";

                HellRumble = !Directories.ExistsInSFXReplace("hell_rumble") ? mod.GetSound(Directories.HellLayer) : GetSoundFromPath(SFXReplacePath + "/hell_rumble.wav");
                HellRumbleInstance = HellRumble.CreateInstance();
                HellRumble.Name = "Hell Rumbling";

                Drip1 = mod.GetSound($"{AmbientPath}/environment/liquid/liquid_drip1");
                Drip2 = mod.GetSound($"{AmbientPath}/environment/liquid/liquid_drip2");
                Drip3 = mod.GetSound($"{AmbientPath}/environment/liquid/liquid_drip3");
                Drip1Instance = Drip1.CreateInstance();
                Drip2Instance = Drip2.CreateInstance();
                Drip3Instance = Drip3.CreateInstance();

                SplashNew = mod.GetSound($"{AmbientPath}/environment/liquid/entity_splash_light");
                SplashNewInstance = SplashNew.CreateInstance();
                SplashNew.Name = "Water Splash (Light)";

                LavaStream = mod.GetSound($"{AmbientPath}/environment/liquid/stream_lava");
                WaterStream = mod.GetSound($"{AmbientPath}/environment/liquid/stream_water");
                LavaStreamInstance = LavaStream.CreateInstance();
                WaterStreamInstance = WaterStream.CreateInstance();

                Rain = !Directories.ExistsInSFXReplace("rain_new") ? mod.GetSound(Directories.Rain) : GetSoundFromPath(SFXReplacePath + "/rain_new.wav");
                RainInstance = Rain.CreateInstance();
                Rain.Name = "Rain Outside";

                Breeze = mod.GetSound(Directories.Breeze);
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
                RainInstance.IsLooped = true;
                LavaStreamInstance.IsLooped = true;
                WaterStreamInstance.IsLooped = true;
                MorningCricketsInstance.IsLooped = true;
                BreezeInstance.IsLooped = true;
            }
        }
        public static float decOrIncRate = 0.01f;
        public static void UpdateVolume()
        {
            if (float.TryParse(ModContent.GetInstance<AmbientConfigClient>().transitionHarshness, out float given) && given != 0f)
            {
                decOrIncRate = float.Parse(ModContent.GetInstance<AmbientConfigClient>().transitionHarshness);
            }
            else
            {
                ModContent.GetInstance<AmbientConfigClient>().transitionHarshness = "0.01";
                // decOrIncRate = 0.01f;
            }
            if (Main.gameMenu) return;
            FootstepsPlayer ambiencePlayer = Main.player[Main.myPlayer].GetModPlayer<FootstepsPlayer>();

            Player player = ambiencePlayer.player;
            var aLoader = Instance;
            if (Main.hasFocus)
            {
                    aLoader.dayCricketsVolume +=
                        (player.ZoneForest() || (player.ZoneHoly && player.ZoneOverworldHeight && !player.ZoneDesert)) &&
                        (Main.dayTime && Main.time > 7000) && (Main.dayTime && Main.time < 46800) ?
                        decOrIncRate : -decOrIncRate;

                    if ((player.ZoneForest() || (player.ZoneDesert && !player.ZoneUndergroundDesert) || player.ZoneJungle || player.ZoneBeach || player.ZoneCorrupt || player.ZoneCrimson) && !player.ZoneDirtLayerHeight && !player.ZoneRockLayerHeight && !player.ZoneUnderworldHeight)
                    {
                        aLoader.breezeVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.breezeVolume -= decOrIncRate;
                    }
                    if ((player.ZoneForest() || (player.ZoneHoly && player.ZoneOverworldHeight && !player.ZoneDesert)) && (Main.time >= 46800 && Main.dayTime))
                    {
                        aLoader.eveningCricketsVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.eveningCricketsVolume -= decOrIncRate;
                    }
                    if (player.ZoneForest() && Main.dayTime && Main.time <= 7000)
                    {
                        aLoader.morningCricketsVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.morningCricketsVolume -= decOrIncRate;
                    }

                    if ((player.ZoneForest() || (player.ZoneHoly && player.ZoneOverworldHeight && !player.ZoneDesert)) && !Main.dayTime)
                    {
                        aLoader.nightCricketsVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.nightCricketsVolume -= decOrIncRate;
                    }

                    if (player.ZoneUnderground())
                    {
                        aLoader.ugAmbienceVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.ugAmbienceVolume -= decOrIncRate;
                    }

                    if (player.ZoneDesert)
                    {
                        aLoader.desertCricketsVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.desertCricketsVolume -= decOrIncRate;
                    }

                    if (player.ZoneDesert && Main.dayTime)
                    {
                        DesertAmbienceInstance.Pitch = -0.075f;
                    }
                    else
                    {
                        DesertAmbienceInstance.Pitch = 0f;
                    }

                    if (player.ZoneSnow && Main.dayTime && !player.ZoneUnderworldHeight)
                    {
                        aLoader.snowDayVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.snowDayVolume -= decOrIncRate;
                    }

                    if (player.ZoneSnow && !Main.dayTime && !player.ZoneUnderworldHeight)
                    {
                        aLoader.snowNightVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.snowNightVolume -= decOrIncRate;
                    }

                    if (player.ZoneCrimson)
                    {
                        aLoader.crimsonRumblesVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.crimsonRumblesVolume -= decOrIncRate;
                    }
                    if (player.ZoneCorrupt)
                    {
                        aLoader.corruptionRoarsVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.corruptionRoarsVolume -= decOrIncRate;
                    }

                    if (player.ZoneJungle && !player.ZoneRockLayerHeight && !player.ZoneUnderworldHeight && Main.dayTime)
                    {
                        aLoader.dayJungleVolume += decOrIncRate;
                    }
                    else if (!player.ZoneRockLayerHeight)
                    {
                        aLoader.dayJungleVolume -= decOrIncRate;
                    }
                    else if (player.ZoneRockLayerHeight && player.ZoneJungle)
                    {
                        aLoader.dayJungleVolume = 0.05f;
                    }

                    if (player.ZoneJungle && !player.ZoneRockLayerHeight && !player.ZoneUnderworldHeight && !Main.dayTime)
                    {
                        aLoader.nightJungleVolume += decOrIncRate;
                    }
                    else if (!player.ZoneRockLayerHeight)
                    {
                        aLoader.nightJungleVolume -= decOrIncRate;
                    }
                    else if (player.ZoneRockLayerHeight && player.ZoneJungle)
                    {
                        aLoader.nightJungleVolume = 0.05f;
                    }

                    if (player.ZoneBeach && !player.ZoneRockLayerHeight)
                    {
                        aLoader.beachWavesVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.beachWavesVolume -= decOrIncRate;
                    }

                    if (player.ZoneUnderworldHeight)
                    {
                        aLoader.hellRumbleVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.hellRumbleVolume -= decOrIncRate;
                    }

                    foreach (Rain rain in Main.rain)
                    {
                        rain.rotation = rain.velocity.ToRotation() + MathHelper.PiOver2;
                    }
                    if (Main.raining && (player.ZoneOverworldHeight || player.ZoneSkyHeight) && !player.ZoneSnow)
                    {
                        aLoader.rainVolume += decOrIncRate;
                    }
                    else
                    {
                        aLoader.rainVolume -= decOrIncRate;
                    }
                }
                else
                {
                    aLoader.dayCricketsVolume -= decOrIncRate;
                    aLoader.eveningCricketsVolume -= decOrIncRate;
                    aLoader.nightCricketsVolume -= decOrIncRate;
                    aLoader.crackleVolume -= decOrIncRate;
                    aLoader.desertCricketsVolume -= decOrIncRate;
                    aLoader.ugAmbienceVolume -= decOrIncRate;
                    aLoader.snowDayVolume -= decOrIncRate;
                    aLoader.snowNightVolume -= decOrIncRate;
                    aLoader.crimsonRumblesVolume -= decOrIncRate;
                    aLoader.corruptionRoarsVolume -= decOrIncRate;
                    aLoader.dayJungleVolume -= decOrIncRate;
                    aLoader.nightJungleVolume -= decOrIncRate;
                    aLoader.beachWavesVolume -= decOrIncRate;
                    aLoader.hellRumbleVolume -= decOrIncRate;
                    aLoader.rainVolume -= decOrIncRate;
                    aLoader.morningCricketsVolume -= decOrIncRate;
                    aLoader.breezeVolume -= decOrIncRate;
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
            RainInstance?.Play();
            MorningCricketsInstance?.Play();
            BreezeInstance?.Play();
        }
        private bool playerBehindWall;
        private bool playerInLiquid;

        private static bool rainSFXStopped;
        /// <summary>
        /// Updates everything that has to do with ambiences.
        /// </summary>
        internal static void DoUpdate_Ambience()
        {
            var cfgCl = ModContent.GetInstance<AmbientConfigClient>();
            var inst = Instance;
            // https://i.kym-cdn.com/entries/icons/facebook/000/001/576/Screen_Shot_2021-03-22_at_3.45.30_PM.jpg
            // this code be link
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
            float ambVol = TAAmbient / 100;
            Terraria.ModLoader.Audio.Music rainSFX = Main.music[MusicID.RainSoundEffect];
            if (!Main.dedServ)
            {
                if (Main.gameMenu)
                {
                    DayCricketsInstance.Volume = Instance.dayCricketsVolume * 0.75f * ambVol;
                    EveningCricketsInstance.Volume = Instance.eveningCricketsVolume * 0.75f;
                    NightCricketsInstance.Volume = Instance.nightCricketsVolume * 0.75f * ambVol;
                    DesertAmbienceInstance.Volume = Instance.desertCricketsVolume * 0.75f * ambVol;
                    CavesAmbienceInstance.Volume = Instance.ugAmbienceVolume * 0.75f * ambVol;
                    CrimsonRumblesInstance.Volume = Instance.crimsonRumblesVolume * 0.7f * ambVol;
                    CorruptionRoarsInstance.Volume = Instance.corruptionRoarsVolume * 0.7f * ambVol;
                    MorningCricketsInstance.Volume = Instance.morningCricketsVolume * 0.9f * ambVol;
                    BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeed) * ambVol;

                    HellRumbleInstance.Volume = Instance.hellRumbleVolume * 0.75f * ambVol;

                    BeachWavesInstance.Volume = Instance.beachWavesVolume * 0.8f * ambVol;

                    DaytimeJungleInstance.Volume = Instance.dayJungleVolume * 0.5f * ambVol;
                    NightJungleInstance.Volume = Instance.nightJungleVolume * 0.5f * ambVol;

                    SnowBreezeDayInstance.Volume = Instance.snowDayVolume * 0.65f * ambVol;
                    SnowBreezeNightInstance.Volume = Instance.snowNightVolume * 0.7f * ambVol;

                    CampfireCrackleInstance.Volume = Instance.crackleVolume * 0.95f * Main.soundVolume;

                    RainInstance.Volume = Instance.rainVolume * 0.6f * ambVol;
                }
                // TODO: Fix?
                rainSFX?.Stop(AudioStopOptions.Immediate);
                if (!Main.gameMenu)
                {
                    if (!rainSFXStopped)
                    {
                        rainSFX?.Pause();
                        rainSFXStopped = true;
                    }
                    if (!Main.raining)
                    {
                        rainSFXStopped = false;
                    }
                    else
                    {
                        rainSFX?.SetVariable("Volume", 0f);
                    }
                    if (!Instance.playerBehindWall)
                    {
                        DayCricketsInstance.Volume = Instance.dayCricketsVolume * 0.75f * ambVol;
                        EveningCricketsInstance.Volume = Instance.eveningCricketsVolume * 0.75f * ambVol;
                        NightCricketsInstance.Volume = Instance.nightCricketsVolume * 0.75f * ambVol;
                        DesertAmbienceInstance.Volume = Instance.desertCricketsVolume * 0.75f * ambVol;
                        CavesAmbienceInstance.Volume = Instance.ugAmbienceVolume * 0.75f * ambVol;
                        CrimsonRumblesInstance.Volume = Instance.crimsonRumblesVolume * 0.7f * ambVol;
                        CorruptionRoarsInstance.Volume = Instance.corruptionRoarsVolume * 0.7f * ambVol;
                        MorningCricketsInstance.Volume = Instance.morningCricketsVolume * 0.9f * ambVol;
                        BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeed) * ambVol;

                        HellRumbleInstance.Volume = Instance.hellRumbleVolume * 0.75f * ambVol;

                        BeachWavesInstance.Volume = Instance.beachWavesVolume * 0.8f * ambVol;

                        DaytimeJungleInstance.Volume = Instance.dayJungleVolume * 0.5f * ambVol;
                        NightJungleInstance.Volume = Instance.nightJungleVolume * 0.5f * ambVol;

                        SnowBreezeDayInstance.Volume = Instance.snowDayVolume * 0.65f * ambVol;
                        SnowBreezeNightInstance.Volume = Instance.snowNightVolume * 0.7f * ambVol;

                        if (Instance.crackleVolume >= 0f && Instance.crackleVolume <= 1f)
                            CampfireCrackleInstance.Volume = Instance.crackleVolume * 0.95f * Main.soundVolume;
                    }
                    else
                    {
                        BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeed) * ambVol * 0.8f;
                        MorningCricketsInstance.Volume = Instance.morningCricketsVolume * 0.5f * ambVol;
                        DayCricketsInstance.Volume = Instance.dayCricketsVolume * 0.4f * ambVol;
                        EveningCricketsInstance.Volume = Instance.eveningCricketsVolume * 0.4f * ambVol;
                        NightCricketsInstance.Volume = Instance.nightCricketsVolume * 0.4f * ambVol;
                        DesertAmbienceInstance.Volume = Instance.desertCricketsVolume * 0.4f * ambVol;
                        CavesAmbienceInstance.Volume = Instance.ugAmbienceVolume * 0.4f * ambVol;
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
                    if (!Instance.playerBehindWall && !Instance.playerInLiquid)
                    {
                        RainInstance.Volume = Instance.rainVolume * 0.6f * ambVol;
                    }
                    else if (Instance.playerInLiquid && !Instance.playerBehindWall)
                    {
                        RainInstance.Volume = Instance.rainVolume * 0.2f * ambVol;
                    }
                    else if (Instance.playerBehindWall && !Instance.playerInLiquid)
                    {
                        BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeed) * ambVol * 0.6f;
                        RainInstance.Volume = Instance.rainVolume * 0.4f * ambVol;
                    }
                    else if (Instance.playerBehindWall && Instance.playerInLiquid)
                    {
                        BreezeInstance.Volume = Instance.breezeVolume * Math.Abs(Main.windSpeed) * ambVol * 0.4f;
                        RainInstance.Volume = Instance.rainVolume * 0.05f * ambVol;
                    }
                }
                // Main.NewText(Instance.crackleVolume * 0.95f * Main.soundVolume);

                if (Main.gameMenu) return;
                Player player = Main.player[Main.myPlayer]?.GetModPlayer<FootstepsPlayer>().player;

                if (player.HeadWet())
                {
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
                    Main.soundInstanceDrip[0].Volume = 0.5f;
                    Main.soundInstanceDrip[1].Volume = 0.5f;
                    Main.soundInstanceDrip[2].Volume = 0.5f;

                    NightCricketsInstance.Pitch = -0.25f;
                    DayCricketsInstance.Pitch = -0.25f;
                    EveningCricketsInstance.Pitch = -0.25f;
                    CampfireCrackleInstance.Pitch = -0.15f;
                    DesertAmbienceInstance.Pitch = -0.25f;
                    CavesAmbienceInstance.Pitch = -0.25f;
                    CrimsonRumblesInstance.Pitch = -0.25f;
                    CorruptionRoarsInstance.Pitch = -0.25f;
                    DaytimeJungleInstance.Pitch = -0.25f;
                    NightJungleInstance.Pitch = -0.25f;
                    BeachWavesInstance.Pitch = 0.1f;
                    HellRumbleInstance.Pitch = -0.3f;
                    MorningCricketsInstance.Pitch = -0.25f;
                    BreezeInstance.Pitch = -0.25f;

                    Main.soundInstanceDrip[0].Pitch = -0.25f;
                    Main.soundInstanceDrip[1].Pitch = -0.25f;
                    Main.soundInstanceDrip[2].Pitch = -0.25f;
                }
                else
                {
                    Instance.playerInLiquid = false;
                    BreezeInstance.Pitch = 0f;
                    NightCricketsInstance.Pitch = 0f;
                    DayCricketsInstance.Pitch = 0f;
                    EveningCricketsInstance.Pitch = 0f;
                    CampfireCrackleInstance.Pitch = 0f;
                    DesertAmbienceInstance.Pitch = 0f;
                    CavesAmbienceInstance.Pitch = 0f;
                    CrimsonRumblesInstance.Pitch = 0f;
                    CorruptionRoarsInstance.Pitch = -0f;
                    DaytimeJungleInstance.Pitch = 0f;
                    NightJungleInstance.Pitch = 0f;
                    BeachWavesInstance.Pitch = 0f;
                    HellRumbleInstance.Pitch = 0f;
                    MorningCricketsInstance.Pitch = 0f;
                    Main.soundInstanceDrip[0].Pitch = 0f;
                    Main.soundInstanceDrip[1].Pitch = 0f;
                    Main.soundInstanceDrip[2].Pitch = 0f;
                }

                Vector2 newTop = player.Top + new Vector2(0, 7.5f);
                if (Main.tile[(int)newTop.X / 16, (int)newTop.Y / 16].wall > 0)
                {
                    Instance.playerBehindWall = true;
                }
                else
                {
                    Instance.playerBehindWall = false;
                }
            }
        }
        /// <summary>
        /// Oddly enough, MathHelper.Clamp is completely useless in this case since it works every 2 frames or something akin to that.
        /// </summary>
        public static void ClampAll()
        {
            var aLoader = Instance;

            // TODO: Change this to an iterator
            if (aLoader.breezeVolume > 1)
            {
                aLoader.breezeVolume = 1;
            }
            if (aLoader.breezeVolume < 0)
            {
                aLoader.breezeVolume = 0;
            }
            if (BreezeInstance.Volume > 1)
            {
                BreezeInstance.Volume = 1;
            }
            if (BreezeInstance.Volume < 0)
            {
                BreezeInstance.Volume = 0;
            }
            if (aLoader.morningCricketsVolume > 1)
            {
                aLoader.morningCricketsVolume = 1;
            }
            if (aLoader.morningCricketsVolume < 0)
            {
                aLoader.morningCricketsVolume = 0;
            }
            if (aLoader.rainVolume > 1)
            {
                aLoader.rainVolume = 1;
            }
            if (aLoader.rainVolume < 0)
            {
                aLoader.rainVolume = 0;
            }
            if (aLoader.dayJungleVolume > 1)
            {
                aLoader.dayJungleVolume = 1;
            }
            if (aLoader.dayJungleVolume < 0)
            {
                aLoader.dayJungleVolume = 0;
            }
            if (aLoader.nightJungleVolume > 1)
            {
                aLoader.nightJungleVolume = 1;
            }
            if (aLoader.nightJungleVolume < 0)
            {
                aLoader.nightJungleVolume = 0;
            }

            if (aLoader.dayCricketsVolume > 1)
            {
                aLoader.dayCricketsVolume = 1;
            }
            if (aLoader.dayCricketsVolume < 0)
            {
                aLoader.dayCricketsVolume = 0;
            }

            if (aLoader.eveningCricketsVolume > 1)
            {
                aLoader.eveningCricketsVolume = 1;
            }
            if (aLoader.eveningCricketsVolume < 0)
            {
                aLoader.eveningCricketsVolume = 0;
            }

            if (aLoader.nightCricketsVolume > 1)
            {
                aLoader.nightCricketsVolume = 1;
            }
            if (aLoader.nightCricketsVolume < 0)
            {
                aLoader.nightCricketsVolume = 0;
            }
            if (aLoader.crackleVolume > 1)
            {
                aLoader.crackleVolume = 1;
            }
            if (aLoader.crackleVolume < 0)
            {
                aLoader.crackleVolume = 0;
            }

            if (aLoader.desertCricketsVolume > 1)
            {
                aLoader.desertCricketsVolume = 1;
            }
            if (aLoader.desertCricketsVolume < 0)
            {
                aLoader.desertCricketsVolume = 0;
            }

            if (aLoader.ugAmbienceVolume > 1)
            {
                aLoader.ugAmbienceVolume = 1;
            }
            if (aLoader.ugAmbienceVolume < 0)
            {
                aLoader.ugAmbienceVolume = 0;
            }

            if (aLoader.snowNightVolume > 1)
            {
                aLoader.snowNightVolume = 1;
            }
            if (aLoader.snowNightVolume < 0)
            {
                aLoader.snowNightVolume = 0;
            }
            if (aLoader.snowDayVolume > 1)
            {
                aLoader.snowDayVolume = 1;
            }
            if (aLoader.snowDayVolume < 0)
            {
                aLoader.snowDayVolume = 0;
            }

            if (aLoader.crimsonRumblesVolume > 1)
            {
                aLoader.crimsonRumblesVolume = 1;
            }
            if (aLoader.crimsonRumblesVolume < 0)
            {
                aLoader.crimsonRumblesVolume = 0;
            }

            if (aLoader.corruptionRoarsVolume > 1)
            {
                aLoader.corruptionRoarsVolume = 1;
            }
            if (aLoader.corruptionRoarsVolume < 0)
            {
                aLoader.corruptionRoarsVolume = 0;
            }

            if (aLoader.beachWavesVolume > 1)
            {
                aLoader.beachWavesVolume = 1;
            }
            if (aLoader.beachWavesVolume < 0)
            {
                aLoader.beachWavesVolume = 0;
            }

            if (aLoader.hellRumbleVolume > 1)
            {
                aLoader.hellRumbleVolume = 1;
            }
            if (aLoader.hellRumbleVolume < 0)
            {
                aLoader.hellRumbleVolume = 0;
            }
        }
    }
}

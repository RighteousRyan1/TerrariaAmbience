using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Collections.Generic;

namespace TerrariaAmbience
{
	public class TerrariaAmbience : Mod
	{
        public override void Load()
        {
            Ambience.Initialize();
            On.Terraria.Main.DoUpdate += Main_DoUpdate;

            SoundInstancing.InitializeAll();
        }

        public override void Unload()
        {
            Ambience.Unload();
        }

        public static float decOrIncRate = 0.01f;
        // SoundInstancing soundInstancer = new SoundInstancing(ModContent.GetInstance<TerrariaAmbience>(), "Sounds/Custom/ambient/animals/rattling_bones", "Bones Do Be Rattling", true, true);

        private void Main_DoUpdate(On.Terraria.Main.orig_DoUpdate orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);
            decOrIncRate = 0.01f;
            var aLoader = Ambience.Instance;

            Ambience.DoUpdate_Ambience();

            if (Main.gameMenu)
            {
                aLoader.dayCricketsVolume = 0f;
                aLoader.nightCricketsVolume = 0f;
                aLoader.eveningCricketsVolume = 0f;
                aLoader.desertCricketsVolume = 0f;
                aLoader.crackleVolume = 0f;
                aLoader.ugAmbienceVolume = 0f;
                aLoader.crimsonRumblesVolume = 0f;
                aLoader.snowDayVolume = 0f;
                aLoader.snowNightVolume = 0f;
                aLoader.corruptionRoarsVolume = 0f;
                aLoader.dayJungleVolume = 0f;
                aLoader.nightJungleVolume = 0f;
                aLoader.beachWavesVolume = 0f;
                aLoader.hellRumbleVolume = 0f;
                aLoader.rainVolume = 0f;
            }
            if (Main.gameMenu) return;
            Player player = Main.player[Main.myPlayer]?.GetModPlayer<AmbiencePlayer>().player;
            // soundInstancer.Condition = player.ZoneSkyHeight;
            Main.NewText(SoundInstancing.allSounds.Count);
            if (Main.hasFocus)
            {
                if ((player.ZoneForest() || player.ZoneHoly) && (Main.dayTime && Main.time > 14400) && (Main.dayTime && Main.time < 46800))
                {
                    aLoader.dayCricketsVolume += decOrIncRate;
                }
                else
                {
                    aLoader.dayCricketsVolume -= decOrIncRate;
                }

                if ((player.ZoneForest() || player.ZoneHoly) && aLoader.DayCricketsInstance.Volume == 0 && Main.dayTime)
                {
                    aLoader.eveningCricketsVolume += decOrIncRate;
                }
                else
                {

                    aLoader.eveningCricketsVolume -= decOrIncRate;
                }

                if ((player.ZoneForest() || player.ZoneHoly) && !Main.dayTime)
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
                    aLoader.DesertAmbienceInstance.Pitch = -0.075f;
                }
                else
                {
                    aLoader.DesertAmbienceInstance.Pitch = 0f;
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
                else if(!player.ZoneRockLayerHeight)
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
                if (Main.raining && (player.ZoneOverworldHeight || player.ZoneSkyHeight))
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
            }
            Ambience.ClampAll();
        }
    }
    public class AmbiencePlayer : ModPlayer
    {
        public bool isOnWoodTile;
        public bool isOnStoneTile;
        public bool isOnGrassyTile;
        public bool isOnSandyTile;
        public bool isOnSnowyTile;

        public bool isNearCampfire;

        public SoundEffectInstance soundInstanceWoodStep;
        public SoundEffectInstance soundInstanceSandStep;
        public SoundEffectInstance soundInstanceStoneStep;
        public SoundEffectInstance soundInstanceSnowStep;
        public SoundEffectInstance soundInstanceGrassStep;

        public override void OnEnterWorld(Player player)
        {
            Ambience.PlayAllAmbience();

            soundInstanceGrassStep = Main.soundInstanceRun;
            soundInstanceWoodStep = Main.soundInstanceRun;
            soundInstanceSandStep = Main.soundInstanceRun;
            soundInstanceSnowStep = Main.soundInstanceRun;
            soundInstanceStoneStep = Main.soundInstanceRun;
        }

        private int legFrameSnapShotNew;
        private int legFrameSnapShotOld;
        public override void PreUpdate()
        {
            legFrameSnapShotNew = player.legFrame.Y / 20;

            // Good Values -> 25 (first), 44 (second), 14 = midair

            int randWood = Main.rand.Next(1, 8);
            int randSand = Main.rand.Next(1, 4);
            int randStone = Main.rand.Next(1, 9);
            int randGrass = Main.rand.Next(1, 9);
            int randSnow = Main.rand.Next(1, 12);

            string pathWood = $"Sounds/Custom/steps/wood/step{randWood}";
            string pathSand = $"Sounds/Custom/steps/sand/step{randSand}";
            string pathStone = $"Sounds/Custom/steps/stone/step{randStone}";
            string pathGrass = $"Sounds/Custom/steps/grass/step{randGrass}";
            string pathSnow = $"Sounds/Custom/steps/snow/step{randSnow}";
            // Main.NewText($"{legFrameSnapShotNew}, {legFrameSnapShotOld}, {player.legFrame.Y == player.legFrame.Height * 16 }");

            if (ModContent.GetInstance<AmbientConfig>().footsteps)
            {
                if (Main.soundVolume != 0f)
                {
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnSandyTile)
                    {
                        soundInstanceSandStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathSand), player.Bottom);
                        soundInstanceSandStep.Volume = player.wet ? Main.soundVolume * 0.6f : Main.soundVolume * 0.9f;
                        soundInstanceSandStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnGrassyTile)
                    {
                        soundInstanceGrassStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathGrass), player.Bottom);
                        soundInstanceGrassStep.Volume = player.wet ? Main.soundVolume * 0.45f : Main.soundVolume * 0.65f;
                        soundInstanceGrassStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnStoneTile)
                    {
                        soundInstanceStoneStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathStone), player.Bottom);
                        soundInstanceStoneStep.Volume = player.wet ? Main.soundVolume / 8 : Main.soundVolume / 4;
                        soundInstanceStoneStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnSnowyTile)
                    {
                        soundInstanceSnowStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathSnow), player.Bottom);
                        soundInstanceSnowStep.Volume = player.wet ? Main.soundVolume / 6 : Main.soundVolume / 2;
                        soundInstanceSnowStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnWoodTile)
                    {
                        soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWood), player.Bottom);
                        soundInstanceWoodStep.Volume = player.wet ? Main.soundVolume / 2 : Main.soundVolume / 4;
                        soundInstanceWoodStep.Pitch = player.wet ? -0.5f : 0f;
                    }

                    // Top: landing sounds
                    // Bottom: walking sounds

                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnSandyTile)
                    {
                        soundInstanceSandStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathSand), player.Bottom);
                        soundInstanceSandStep.Volume = player.wet ? Main.soundVolume * 0.25f : Main.soundVolume * 0.75f;
                        soundInstanceSandStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnGrassyTile)
                    {
                        soundInstanceGrassStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathGrass), player.Bottom);
                        soundInstanceGrassStep.Volume = player.wet ? Main.soundVolume / 5 : Main.soundVolume / 2;
                        soundInstanceGrassStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnStoneTile)
                    {
                        soundInstanceStoneStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathStone), player.Bottom);
                        soundInstanceStoneStep.Volume = player.wet ? Main.soundVolume / 12 : Main.soundVolume / 8;
                        soundInstanceStoneStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnWoodTile)
                    {
                        soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWood), player.Bottom);
                        soundInstanceWoodStep.Volume = player.wet ? Main.soundVolume / 12 : Main.soundVolume / 8;
                        soundInstanceWoodStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnSnowyTile)
                    {
                        soundInstanceSnowStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathSnow), player.Bottom);
                        soundInstanceSnowStep.Volume = player.wet ? Main.soundVolume / 8 : Main.soundVolume / 4;
                        soundInstanceSnowStep.Pitch = player.wet ? -0.2f : 0.3f;
                    }
                }
            }
            legFrameSnapShotOld = legFrameSnapShotNew;
        }
        public override void PostUpdate()
        {


            if (!Main.dayTime)
            {
                int rand = Main.rand.Next(1, 3);
                string pathToHoot = $"Sounds/Custom/ambient/animals/hoot{rand}";
                string pathToHowl = $"Sounds/Custom/ambient/animals/howl";

                int randX = Main.rand.Next(-1750, 1750);
                int randY = Main.rand.Next(-1750, 1750);
                int chance1 = Main.rand.Next(750);
                int chance2 = Main.rand.Next(1500);

                // Main.NewText(chance);
                if (Main.soundVolume > 0f)
                {
                    if (chance1 == 0)
                    {
                        if (player.ZoneForest())
                        {
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathToHoot), player.Center + new Vector2(randX, randY)).Volume = Main.ambientVolume * 0.9f;
                        }
                    }
                    if (player.ZoneSnow && !player.ZoneUnderworldHeight && !player.ZoneRockLayerHeight)
                    {
                        if (chance2 == 1)
                        {
                            var SFE = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathToHowl), player.Center + new Vector2(randX, randY));
                            SFE.Volume = Main.ambientVolume * 0.1f;
                            SFE.Pitch = Main.rand.NextFloat(-0.4f, -0.1f);
                        }
                    }
                }
            }
            if (player.ZoneDungeon)
            {
                int possibleChance = Main.rand.Next(1000);
                int randX = Main.rand.Next(-2250, 2250);
                int randY = Main.rand.Next(-2250, 2250);

                if (possibleChance == 0)
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/ambient/animals/rattling_bones"), player.Center + new Vector2(randX, randY)).Volume = Main.ambientVolume * 0.05f;
                }
            }
            // Main.NewText(Main.tile[(int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16]);
            var aLoader = Ambience.Instance;
            if (aLoader.crackleVolume > 1)
            {
                aLoader.crackleVolume = 1;
            }
            if (aLoader.crackleVolume < 0)
            {
                aLoader.crackleVolume = 0;
            }
            if (isNearCampfire && ModContent.GetInstance<AmbientConfig>().campfireSounds)
            {
                Ambience.Instance.crackleVolume = 1f - ModContent.GetInstance<CampfireDetection>().distanceOf / 780;
            }
            else
            {
                Ambience.Instance.crackleVolume = 0f;
            }
        }
    }
    public static class ExtensionMethods
    {
        public static bool ZoneForest(this Player player)
        {
            return !player.ZoneJungle
                   && !player.ZoneDungeon
                   && !player.ZoneCorrupt
                   && !player.ZoneCrimson
                   && !player.ZoneHoly
                   && !player.ZoneSnow
                   && !player.ZoneUndergroundDesert
                   && !player.ZoneGlowshroom
                   && !player.ZoneMeteor
                   && !player.ZoneBeach
                   && !player.ZoneDesert
                   && player.ZoneOverworldHeight;
        }
        public static bool ZoneUnderground(this Player player)
        {
            return player.Center.Y >= Main.rockLayer * 16 && !player.ZoneUnderworldHeight;
        }
    }
    public class CampfireDetection : GlobalTile
    {
        public Vector2 originOfCampfire;

        public float distanceOf;

        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            originOfCampfire.X = i * 16;
            originOfCampfire.Y = j * 16;
            Player player = Main.player[Main.myPlayer].GetModPlayer<AmbiencePlayer>().player;

            if (type == TileID.Campfire && closer && player.HasBuff(BuffID.Campfire))
            {
                distanceOf = Vector2.Distance(originOfCampfire, player.Center);
                player.GetModPlayer<AmbiencePlayer>().isNearCampfire = true;
            }
            if ((type == TileID.Campfire && !closer) || !player.HasBuff(BuffID.Campfire))
            {
                Ambience.Instance.CampfireCrackleInstance.Volume = 0f;
                player.GetModPlayer<AmbiencePlayer>().isNearCampfire = false;
            }
        }
    }
    public class SoundInstancing
    {
        public SoundEffect soundEffect;
        public SoundEffectInstance soundEffectInstance;
        public bool Condition
        {
            get;
            set;
        }
        public string Name
        {
            get;
            private set;
        }
        public string Path
        {
            get;
            private set;
        }
        public bool IsLooped
        {
            get;
            private set;
        }

        public float Volume
        {
            get;
            private set;
        }
        private Mod Mod
        {
            get;
            set;
        }

        private SoundInstancing validSound;

        private static int count;

        // Change back to private
        public static List<SoundInstancing> allSounds = new List<SoundInstancing> { };
        /// <summary>
        /// Creates a completely new ambience sound. This will do all of the work for you.
        /// </summary>
        /// <param name="mod">The mod to default a path from.</param>
        /// <param name="pathForSound">The path to the ambient after your mod's direcotry.</param>
        /// <param name="name">The name for the ambience effect created.</param>
        /// <param name="conditionToPlayUnder">When or when to not play this ambience.</param>
        /// <param name="isLooped">Whether or not to loop the ambient.</param>
        public SoundInstancing(Mod mod, string pathForSound, string name, bool conditionToPlayUnder, bool isLooped = true)
        {

            validSound = this;
            allSounds.Add(this);

            Name = name;

            Path = pathForSound;

            IsLooped = isLooped;

            Mod = mod;
            soundEffect = mod.GetSound(pathForSound);
            soundEffectInstance = soundEffect.CreateInstance();

            soundEffect.Name = name;
            Condition = conditionToPlayUnder;

            Volume = soundEffectInstance.Volume;

            if (isLooped)
            {
                soundEffectInstance.IsLooped = true;
            }

            soundEffectInstance.Volume = conditionToPlayUnder ? 1f : 0f;
        }
        public override string ToString()
        {
            return "{ isLooped: " + IsLooped + " | path: " + Path + " | name: " + Name + " | isPlaying: " + Condition    + " }";
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            validSound?.soundEffectInstance?.Play();
        }
        /// <summary>
        /// 
        /// </summary>
        public static void InitializeAll()
        {
            foreach (SoundInstancing instancing in allSounds)
            {
                instancing.Initialize();

                count = allSounds.Count;
            }
        }
    }
}
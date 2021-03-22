using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;

using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content
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

        public float lastPlayerPositionOnGround;
        public float delta_lastPos_playerBottom;
        // SoundInstancing soundInstancer = new SoundInstancing(ModContent.GetInstance<TerrariaAmbience>(), "Sounds/Custom/ambient/animals/rattling_bones", "Bones Do Be Rattling", true, true);

        private void Main_DoUpdate(On.Terraria.Main.orig_DoUpdate orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);
            var aLoader = Ambience.Instance;

            Ambience.DoUpdate_Ambience();
            Ambience.UpdateVolume();
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
            if (Main.hasFocus)
            {
                delta_lastPos_playerBottom = lastPlayerPositionOnGround - player.Bottom.Y;

                // Main.NewText(delta_lastPos_playerBottom);
                if (player.velocity.Y <= 0 || player.teleporting)
                {
                    lastPlayerPositionOnGround = player.Bottom.Y;
                }
                // Main.NewText(player.fallStart - player.Bottom.Y);
                if (delta_lastPos_playerBottom > -300)
                {
                    Main.soundSplash[0] = Ambience.Instance.SplashNew;
                }
                else if (delta_lastPos_playerBottom <= -300)
                {
                    Main.soundSplash[0] = GetSound($"{Ambience.ambienceDirectory}/environment/liquid/entity_splash_heavy");
                }
                /*if (Main.mouseRight)
                {
                    Main.soundInstanceSplash[0]?.Play(); // entity splash
                }
                if (Main.mouseLeft)
                {
                    Main.soundInstanceSplash[1]?.Play(); // hook bob?
                // Finish mini-spla
                }*/
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

        private bool hasTilesAbove;
        public override void PreUpdate()
        {
            Tile playerTile = Main.tile[(int)player.Center.X / 16, (int)player.Center.Y / 16];
            for (int i = (int)player.Top.X - 1; i < player.Top.X + 1; i++)
            {
                for (int j = (int)player.Top.Y - 1; playerTile.wall <= 0 ? j > player.Top.Y - 350 : j > player.Top.Y - 600; j--)
                {
                    Tile tile = Main.tile[i / 16, j / 16];
                    if (tile.active() && tile.collisionType == 1)
                    {
                        hasTilesAbove = true;
                        break;
                    }
                    else
                    {
                        hasTilesAbove = false;
                    }
                }
            }

            legFrameSnapShotNew = player.legFrame.Y / 20;

            // Good Values -> 25 (first), 44 (second), 14 = midair

            int randWood = Main.rand.Next(1, 8);
            int randSand = Main.rand.Next(1, 4);
            int randStone = Main.rand.Next(1, 9);
            int randGrass = Main.rand.Next(1, 9);
            int randSnow = Main.rand.Next(1, 12);
            int randWet = Main.rand.Next(1, 4);

            string pathWood = $"Sounds/Custom/steps/wood/step{randWood}";
            string pathSand = $"Sounds/Custom/steps/sand/step{randSand}";
            string pathStone = $"Sounds/Custom/steps/stone/step{randStone}";
            string pathGrass = $"Sounds/Custom/steps/grass/step{randGrass}";
            string pathSnow = $"Sounds/Custom/steps/snow/step{randSnow}";
            string pathWet = $"Sounds/Custom/steps/wet/step{randWet}";
            // Main.NewText($"{legFrameSnapShotNew}, {legFrameSnapShotOld}, {player.legFrame.Y == player.legFrame.Height * 16 }");

            if (ModContent.GetInstance<AmbientConfig>().footsteps)
            {
                // Main.NewText(Main.tile[(int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16].wall);
                // Main.NewText(hasTilesAbove);
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
                        if (!Main.raining || hasTilesAbove)
                        {
                            soundInstanceGrassStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathGrass), player.Bottom);
                            soundInstanceGrassStep.Volume = player.wet ? Main.soundVolume * 0.45f : Main.soundVolume * 0.65f;
                            soundInstanceGrassStep.Pitch = player.wet ? -0.5f : 0f;
                        }
                        if (!hasTilesAbove && Main.raining && !player.wet)
                        {
                            soundInstanceGrassStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWet), player.Bottom);
                            soundInstanceGrassStep.Volume = player.wet ? Main.soundVolume / 2 : Main.soundVolume / 4;
                            soundInstanceGrassStep.Pitch = Main.rand.NextFloat(-0.3f, 0.3f);
                        }
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
                        if (!Main.raining || hasTilesAbove) // rain and no tiles covering said player
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWood), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? Main.soundVolume / 2 : Main.soundVolume / 4;
                            soundInstanceWoodStep.Pitch = player.wet ? -0.5f : 0f;
                        }
                        if (!hasTilesAbove && Main.raining && !player.wet)
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWet), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? Main.soundVolume / 2 : Main.soundVolume / 4;
                            soundInstanceWoodStep.Pitch = Main.rand.NextFloat(-0.3f, 0.3f);
                        }
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
                        if (!Main.raining || hasTilesAbove) // rain and no tiles covering said player
                        {
                            soundInstanceGrassStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathGrass), player.Bottom);
                            soundInstanceGrassStep.Volume = player.wet ? Main.soundVolume / 5 : Main.soundVolume / 2;
                            soundInstanceGrassStep.Pitch = player.wet ? -0.5f : 0f;
                        }
                        if (!hasTilesAbove && Main.raining && !player.wet)
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWet), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? Main.soundVolume / 8 : Main.soundVolume / 4;
                            soundInstanceWoodStep.Pitch = Main.rand.NextFloat(-0.4f, 0.4f);
                        }
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnStoneTile)
                    {
                        soundInstanceStoneStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathStone), player.Bottom);
                        soundInstanceStoneStep.Volume = player.wet ? Main.soundVolume / 12 : Main.soundVolume / 8;
                        soundInstanceStoneStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnWoodTile)
                    {
                        if (!Main.raining || hasTilesAbove) // rain and no tiles covering said player
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWood), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? Main.soundVolume / 12 : Main.soundVolume / 8;
                            soundInstanceWoodStep.Pitch = player.wet ? -0.5f : 0f;
                        }
                        if (!hasTilesAbove && Main.raining && !player.wet)
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWet), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? Main.soundVolume / 8 : Main.soundVolume / 4;
                            soundInstanceWoodStep.Pitch = Main.rand.NextFloat(-0.3f, 0.3f);
                        }
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
        int a;
        public override void PostUpdate()
        {
            // Main.NewText(Main.soundZombie.Length);
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
        public static bool HeadWet(this Player player)
        {
            return Main.tile[(int)player.Top.X / 16, (int)(player.Top.Y - 1.25) / 16].liquid > 0;
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

    public class x:GlobalNPC
    {
        public override void AI(NPC npc)
        {
            if (npc.aiStyle == NPCID.Zombie)
            {
                Tile tileToLeft = Main.tile[(int)npc.Left.X / 16 - 1, (int)npc.Left.Y / 16];
                Tile tileToRight = Main.tile[(int)npc.Right.X / 16 + 1, (int)npc.Right.Y / 16];
                if (TileLoader.IsClosedDoor(tileToLeft) || TileLoader.IsClosedDoor(tileToRight) && npc.ai[0] == 20)
                {
                    // God damn, wtf do i do
                }
            }
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content.Players
{
    public class FootstepsAndAmbiencePlayer : ModPlayer
    {
        // This class serves the purpose of playing sounds and/or playing footsteps.

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

        public SoundEffectInstance thunderInstance;
        public SoundEffectInstance hootInstance;
        public SoundEffectInstance howlInstance;

        internal int timerUntilValidChestStateChange;

        public override void OnEnterWorld(Player player)
        {
            Ambience.PlayAllAmbience();
            timerUntilValidChestStateChange = 0;
            soundInstanceGrassStep = Main.soundInstanceRun;
            soundInstanceWoodStep = Main.soundInstanceRun;
            soundInstanceSandStep = Main.soundInstanceRun;
            soundInstanceSnowStep = Main.soundInstanceRun;
            soundInstanceStoneStep = Main.soundInstanceRun;
        }

        private int legFrameSnapShotNew; // These 2 variables should never be modified. Ever.
        private int legFrameSnapShotOld;

        private int chestStateNew;
        private int chestStateOld; // Same as above.

        /// <summary>
        /// For the use of rain. Not modifyable publically due to potential anomalies happening otherwise.
        /// <para>
        /// Checks above the player for about 8 tiles for a ceiling. Used for wet-sounding footsteps.
        /// </para>
        /// </summary>
        private bool hasTilesAbove;

        /// <summary>
        /// Updates based on what station you craft at.
        /// </summary>
        public int playerCraftType;
        public override void ResetEffects()
        {
            playerCraftType = 0;
        }
        public override void PreUpdate()
        {
            timerUntilValidChestStateChange++;
            // This is a pretty niche finding for tiles above said player
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

            bool hasLegArmor = Utils.CheckPlayerArmorSlot(player, Utils.IDs.ArmorSlotID.LegSlot);

            // Main.NewText(hasLegArmor);

            legFrameSnapShotNew = player.legFrame.Y / 20;

            chestStateNew = player.chest;

            // Good Values -> 25 (first), 44 (second), 14 = midair
            // Main.NewText($"{chestStateNew}, {chestStateOld}");

            if (Main.soundVolume > 0f)
            {
                if (ModContent.GetInstance<AmbientConfigServer>().chestSounds)
                {
                    if (timerUntilValidChestStateChange > 10)
                    {
                        if ((chestStateNew != chestStateOld) && chestStateNew != -1)
                        {
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/ambient/player/chest_open"), player.Center).Volume = 0.35f;
                        }
                        if ((chestStateNew != chestStateOld) && chestStateNew == -1)
                        {
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/ambient/player/chest_close"), player.Center);
                        }
                        else if ((chestStateNew != chestStateOld) && chestStateOld > -1)
                        {
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/ambient/player/chest_close"), player.Center);
                        }
                    }
                }
            }

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

            if (ModContent.GetInstance<AmbientConfigClient>().footsteps)
            {
                // TODO: if (!hasLegArmor)
                // Main.NewText(Main.tile[(int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16].wall);
                if (Main.soundVolume != 0f && !player.mount.Active)
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
                        if (!hasTilesAbove && Main.raining && !player.wet && !player.ZoneSnow && player.ZoneOverworldHeight)
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
                        if (!hasTilesAbove && Main.raining && !player.wet && !player.ZoneSnow && player.ZoneOverworldHeight)
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
                            if (soundInstanceGrassStep != null)
                            {
                                soundInstanceGrassStep.Volume = player.wet ? Main.soundVolume / 5 : Main.soundVolume / 2;
                            }
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
            chestStateOld = chestStateNew;
        }
        public override void PostUpdate()
        {
            // Main.NewText(hootInstance == null);
            if (!Main.dayTime)
            {
                int rand = Main.rand.Next(1, 3);
                string pathToHoot = $"Sounds/Custom/ambient/animals/hoot{rand}";
                string pathToHowl = $"Sounds/Custom/ambient/animals/howl";
                string pathToThunder = $"Sounds/Custom/ambient/rain/thunder";

                int randX = Main.rand.Next(-1750, 1750);
                int randY = Main.rand.Next(-1750, 1750);
                int chance1 = Main.rand.Next(750 * Main.ActivePlayersCount);
                int chance2 = Main.rand.Next(1500 * Main.ActivePlayersCount);
                int chance3 = Main.rand.Next(3500 * Main.ActivePlayersCount);
                if (Main.soundVolume > 0f)
                {
                    //if (hootInstance.IsStopped())
                    {
                        if (chance1 == 0)
                        {
                            if (player.ZoneForest())
                            {
                                hootInstance = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathToHoot), player.Center + new Vector2(randX, randY));
                                hootInstance.Volume = Main.ambientVolume * 0.9f;
                            }
                        }
                    }
                    //if (howlInstance.IsStopped())
                    {
                        if (player.ZoneSnow && !player.ZoneUnderworldHeight && !player.ZoneRockLayerHeight)
                        {
                            if (chance2 == 1)
                            {
                                howlInstance = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathToHowl), player.Center + new Vector2(randX, randY));
                                howlInstance.Volume = Main.ambientVolume * 0.1f;
                                howlInstance.Pitch = Main.rand.NextFloat(-0.4f, -0.1f);
                            }
                        }
                    }
                    //if (thunderInstance.IsStopped())
                    {
                        if (Main.raining && !player.ZoneSnow && !player.ZoneUnderworldHeight && !player.ZoneRockLayerHeight && !player.ZoneDirtLayerHeight)
                        {
                            if (chance3 == 2)
                            {
                                thunderInstance = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathToThunder), player.Center + new Vector2(randX, randY));
                                thunderInstance.Volume = Main.ambientVolume * 0.9f;
                                thunderInstance.Pitch = Main.rand.NextFloat(-0.2f, -0.1f);
                            }
                        }
                        else if (player.ZoneDirtLayerHeight)
                        {
                            if (chance3 == 2)
                            {
                                thunderInstance = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathToThunder), player.Center + new Vector2(randX, randY));
                                thunderInstance.Volume = Main.ambientVolume * 0.45f;
                                thunderInstance.Pitch = Main.rand.NextFloat(-0.2f, -0.1f);
                            }
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
            if (isNearCampfire && ModContent.GetInstance<AmbientConfigClient>().campfireSounds)
            {
                Ambience.Instance.crackleVolume = 1f - ModContent.GetInstance<CampfireDetection>().distanceOf / 780;
            }
            else
            {
                Ambience.Instance.crackleVolume = 0f;
            }
        }
    }
}

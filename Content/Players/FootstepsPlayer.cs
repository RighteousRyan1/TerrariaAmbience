using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Content.Players
{
    public class FootstepsPlayer : ModPlayer
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
            string Between(string text, string start, string end)
            {
                if (text.Contains(start) && text.Contains(end))
                {
                    int stringStart;
                    int stringEnd;
                    stringStart = text.IndexOf(start, 0) + start.Length;
                    stringEnd = text.IndexOf(end, stringStart);
                    return text.Substring(stringStart, stringEnd - stringStart);
                }
                return "";
            }
            Version GetBrowserVersion()
            {
                Version vers;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                WebClient client = new WebClient();
                string URL = "https://mirror.sgkoi.dev/Mods/Details/TerrariaAmbience";
                string htmlCode = client.DownloadString(URL);

                htmlCode = Between(htmlCode, "Version", "</dd>");
                htmlCode = htmlCode.Remove(0, 50).Trim().Remove(0, 1);
                vers = new Version(htmlCode);

                return vers;
            }
            if (mod.Version < GetBrowserVersion())
            {
                Main.NewTextMultiline($"[c/FabcdF:Terraria Ambience] >> [c/FFFF00:Hey]! You are using an outdated version of [c/FabcdF:Terraria Ambience].\nYour current version is [c/FFdd00:v{mod.Version}], the browser version is [c/00dd00:v{GetBrowserVersion()}]. Go to the mod browser and install the latest version!");
            }
            if (mod.Version > GetBrowserVersion())
            {
                Main.NewTextMultiline($"[c/FabcdF:Terraria Ambience] >> [c/FFFF00:Hey]! You are using an early release of [c/FabcdF:Terraria Ambience]. Do not spread this anywhere!");
            }
            MethodDetours.attemptingToPlayTracks = false;
            // Ambience.PlayAllAmbience();
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
        public bool reverbRequirement;
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

            bool hasLegArmor = GeneralHelpers.CheckPlayerArmorSlot(player, GeneralHelpers.IDs.ArmorSlotID.LegSlot);

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
            int randSand = Main.rand.Next(1, 7);
            int randStone = Main.rand.Next(1, 9);
            int randGrass = Main.rand.Next(1, 9);
            int randSnow = Main.rand.Next(1, 12);
            int randWet = Main.rand.Next(1, 4);

            int randReverb = Main.rand.Next(1, 7);

            string pathWood = $"Sounds/Custom/steps/wood/step{randWood}";
            string pathSand = $"Sounds/Custom/steps/sand/step{randSand}";
            string pathStone = $"Sounds/Custom/steps/stone/step{randStone}";
            string pathGrass = $"Sounds/Custom/steps/grass/step{randGrass}";
            string pathSnow = $"Sounds/Custom/steps/snow/step{randSnow}";
            string pathWet = $"Sounds/Custom/steps/wet/step{randWet}";

            string pathReverbSnow = $"Sounds/Custom/steps/snow/reverb/step{randReverb}";
            string pathReverbStone = $"Sounds/Custom/steps/stone/reverb/step{randReverb}";
            string pathReverbSand = $"Sounds/Custom/steps/sand/reverb/step{randReverb}";
            string pathReverbWood = $"Sounds/Custom/steps/wood/reverb/step{randReverb}";
            string pathReverbGrass = $"Sounds/Custom/steps/grass/reverb/step{randReverb}";
            if (ModContent.GetInstance<AmbientConfigClient>().footsteps)
            {
                int tileReq = player.TilesAround(20, player.ZoneRockLayerHeight || player.ZoneDirtLayerHeight);
                reverbRequirement = !ModContent.GetInstance<AmbientConfigServer>().noReverbMath ? (player.ZoneRockLayerHeight && tileReq > 100) || (player.ZoneDirtLayerHeight && tileReq > 650) : player.ZoneRockLayerHeight || player.ZoneDirtLayerHeight;
                var actualVol = Ambience.fStepsVol / 100;
                if (Ambience.fStepsVol != 0 && !player.mount.Active && player.velocity.Y == 0 && Main.soundVolume != 0f)
                {
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnSandyTile)
                    {
                        soundInstanceSandStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathSand : pathReverbSand), player.Bottom);
                        soundInstanceSandStep.Volume = player.wet ? 0.8f * actualVol : actualVol;
                        soundInstanceSandStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnGrassyTile)
                    {
                        if (!Main.raining || hasTilesAbove)
                        {
                            soundInstanceGrassStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathGrass : pathReverbGrass), player.Bottom);
                            soundInstanceGrassStep.Volume = player.wet ? 0.6f * actualVol : 0.8f * actualVol;
                            soundInstanceGrassStep.Pitch = player.wet ? -0.5f : 0f;
                        }
                        if (!hasTilesAbove && Main.raining && !player.wet && !player.ZoneSnow && player.ZoneOverworldHeight)
                        {
                            soundInstanceGrassStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWet), player.Bottom);
                            soundInstanceGrassStep.Volume = player.wet ? 0.3f * actualVol : 0.6f * actualVol;
                            soundInstanceGrassStep.Pitch = Main.rand.NextFloat(-0.3f, 0.3f);
                        }
                    }
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnStoneTile)
                    {
                        soundInstanceStoneStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathStone : pathReverbStone), player.Bottom);
                        soundInstanceStoneStep.Volume = player.wet ? 0.2f * actualVol : 0.3f * actualVol;
                        soundInstanceStoneStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnSnowyTile)
                    {
                        soundInstanceSnowStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathSnow : pathReverbSnow), player.Bottom);
                        soundInstanceSnowStep.Volume = player.wet ? 0.16666f * actualVol : 0.55f * actualVol;
                        soundInstanceSnowStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if ((legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14) && isOnWoodTile)
                    {
                        if (!Main.raining || hasTilesAbove) // no rain and no tiles covering said player
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathWood : pathReverbWood), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? actualVol * 0.3f : 0.45f * actualVol;
                            soundInstanceWoodStep.Pitch = player.wet ? -0.5f : 0f;
                        }
                        if (!hasTilesAbove && Main.raining && !player.wet && !player.ZoneSnow && player.ZoneOverworldHeight)
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWet), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? 0.5f * actualVol : actualVol / 4;
                            soundInstanceWoodStep.Pitch = Main.rand.NextFloat(-0.3f, 0.3f);
                        }
                    }

                    // Top: landing sounds
                    // Bottom: walking sounds

                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnSandyTile)
                    {
                        soundInstanceSandStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathSand : pathReverbSand), player.Bottom);
                        soundInstanceSandStep.Volume = player.wet ? actualVol * 0.25f : actualVol * 0.75f;
                        soundInstanceSandStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnGrassyTile)
                    {
                        if (!Main.raining || hasTilesAbove) // rain and no tiles covering said player
                        {
                            soundInstanceGrassStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathGrass : pathReverbGrass), player.Bottom);
                            if (soundInstanceGrassStep != null)
                            {
                                soundInstanceGrassStep.Volume = player.wet ? actualVol / 5 : actualVol / 2;
                            }
                            soundInstanceGrassStep.Pitch = player.wet ? -0.5f : 0f;
                        }
                        if (!hasTilesAbove && Main.raining && !player.wet)
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWet), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? actualVol / 8 : actualVol / 4;
                            soundInstanceWoodStep.Pitch = Main.rand.NextFloat(-0.4f, 0.4f);
                        }
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnStoneTile)
                    {
                        soundInstanceStoneStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathStone : pathReverbStone), player.Bottom);
                        soundInstanceStoneStep.Volume = player.wet ? actualVol / 12 : actualVol / 8;
                        soundInstanceStoneStep.Pitch = player.wet ? -0.5f : 0f;
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnWoodTile)
                    {
                        if (!Main.raining || hasTilesAbove) // rain and no tiles covering said player
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathWood : pathReverbWood), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? actualVol / 12 : actualVol / 8;
                            soundInstanceWoodStep.Pitch = player.wet ? -0.5f : 0f;
                        }
                        if (!hasTilesAbove && Main.raining && !player.wet)
                        {
                            soundInstanceWoodStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWet), player.Bottom);
                            soundInstanceWoodStep.Volume = player.wet ? actualVol / 8 : actualVol / 4;
                            soundInstanceWoodStep.Pitch = Main.rand.NextFloat(-0.3f, 0.3f);
                        }
                    }
                    if (((legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25)) && isOnSnowyTile)
                    {
                        soundInstanceSnowStep = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, !reverbRequirement ? pathSnow : pathReverbSnow), player.Bottom);
                        soundInstanceSnowStep.Volume = player.wet ? actualVol / 8 : actualVol / 4;
                        soundInstanceSnowStep.Pitch = player.wet ? -0.2f : 0.3f;
                    }
                }
            }
            legFrameSnapShotOld = legFrameSnapShotNew;
            chestStateOld = chestStateNew;
        }
        public override void PostUpdate()
        {
            // if (player.ZoneRockLayerHeight) Main.NewText(QuickMath());

            // Main.NewText(hootInstance == null);
            int randX = Main.rand.Next(-1750, 1750);
            int randY = Main.rand.Next(-1750, 1750);
            int randC = Main.rand.Next(1, 6);
            int randD = Main.rand.Next(1, 7);
            int randF = Main.rand.Next(1, 3);
            int randM = Main.rand.Next(1, 10);

            string pick = GeneralHelpers.Pick($"close/{randC}", $"distant/{randD}", $"far/{randF}", $"medial/{randM}");
            string pathToThunder = $"Sounds/Custom/ambient/rain/thunder/{pick}";

            float mafs = 2000 / (Main.maxRaining * 2);// 500 * Main.ActivePlayersCount / (Main.maxRaining / 2);
            float chance3 = Main.rand.NextFloat(mafs);
            // Main.NewText($"{mafs} -> rand({chance3}) ({Main.maxRaining})");
            if (Main.raining && !player.ZoneSnow && player.ZoneOverworldHeight)
            {
                if (chance3 < 2)
                {
                    // Main.NewText(pathToThunder);
                    thunderInstance = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathToThunder), player.Center + new Vector2(randX, randY));
                    thunderInstance.Volume = Main.ambientVolume * (player.ZoneDirtLayerHeight ? 0.35f : 0.9f);
                    thunderInstance.Pitch = Main.rand.NextFloat(-0.2f, -0.1f);
                }
            }
            if (!Main.dayTime)
            {
                int rand = Main.rand.Next(1, 3);
                string pathToHoot = $"Sounds/Custom/ambient/animals/hoot{rand}";
                string pathToHowl = $"Sounds/Custom/ambient/animals/howl";

                int chance1 = Main.rand.Next(750 * Main.ActivePlayersCount * 4);
                int chance2 = Main.rand.Next(1500 * Main.ActivePlayersCount * 4);
                if (Main.soundVolume > 0f)
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
            if (player.ZoneDungeon)
            {
                int possibleChance = Main.rand.Next(1000);
                int randX1 = Main.rand.Next(-2250, 2250);
                int randY1 = Main.rand.Next(-2250, 2250);
                if (possibleChance == 0)
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/ambient/animals/rattling_bones"), player.Center + new Vector2(randX1, randY1)).Volume = Main.ambientVolume * 0.05f;
            }
            var aLoader = Ambience.Instance;
            if (aLoader.crackleVolume > 1)
                aLoader.crackleVolume = 1;
            if (aLoader.crackleVolume < 0)
                aLoader.crackleVolume = 0;
            if (isNearCampfire && ModContent.GetInstance<AmbientConfigClient>().campfireSounds)
                Ambience.Instance.crackleVolume = 1f - ModContent.GetInstance<CampfireDetection>().distanceOf / 780;
            else
                Ambience.Instance.crackleVolume = 0f;
        }
    }
}

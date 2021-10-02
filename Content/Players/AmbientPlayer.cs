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
using Terraria.Audio;
using TerrariaAmbience.Sounds;
using TerrariaAmbience.Common.Enums;

namespace TerrariaAmbience.Content.Players
{
    public class AmbientPlayer : ModPlayer
    {
        // This class serves the purpose of playing sounds and/or playing footsteps.

        public int WallsAround => ReverbAudioSystem.WallsAround(Player.Center, new(4, 4), out var coords);

        public bool isOnWoodTile;
        public bool isOnStoneTile;
        public bool isOnGrassyTile;
        public bool isOnSandyTile;
        public bool isOnSnowyTile;
        public bool isOnDirtyTile;
        public bool isOnIcyTile;
        public bool isOnSmoothTile;
        public bool isOnMetalTile;
        public bool isOnMarbleOrGraniteTile;
        public bool isOnGlassTile;
        public bool isOnLeafTile;

        public bool isNearCampfire;

        public SoundEffectInstance soundInstanceWoodStep;
        public SoundEffectInstance soundInstanceSandStep;
        public SoundEffectInstance soundInstanceStoneStep;
        public SoundEffectInstance soundInstanceSnowStep;
        public SoundEffectInstance soundInstanceGrassStep;
        public SoundEffectInstance soundInstanceDirtStep;

        public SoundEffectInstance soundInstanceIceStep;
        public SoundEffectInstance soundInstanceMetalStep;
        public SoundEffectInstance soundInstanceSmoothTileStep;
        public SoundEffectInstance soundInstanceMarbleGraniteStep;
        public SoundEffectInstance soundInstanceLeafStep;
        public SoundEffectInstance soundInstanceGlassStep;
        public SoundEffectInstance soundInstanceArmorStep;
        public SoundEffectInstance soundInstanceVanityStep;

        public SoundEffectInstance thunderInstance;
        public SoundEffectInstance hootInstance;
        public SoundEffectInstance howlInstance;

        internal int timerUntilValidChestStateChange;

        public override void OnEnterWorld(Player player)
        {
            /*string Between(string text, string start, string end)
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
            if (Mod.Version < GetBrowserVersion())
            {
                if (!Environment.Is64BitProcess)
                    Main.NewTextMultiline($"[c/FabcdF:Terraria Ambience] >> [c/FFFF00:Hey]! You are using an outdated version of [c/FabcdF:Terraria Ambience].\nYour current version is [c/FFdd00:v{Mod.Version}], the browser version is [c/00dd00:v{GetBrowserVersion()}]. Go to the mod browser and install the latest version!", false, Color.White);
            }
            if (Mod.Version > GetBrowserVersion())
            {
                Main.NewTextMultiline($"[c/FabcdF:Terraria Ambience] >> [c/FFFF00:Hey]! You are using an early release of [c/FabcdF:Terraria Ambience]. Do not spread this anywhere!", false, Color.White);
            }*/
            MethodDetours.attemptingToPlayTracks = false;
            timerUntilValidChestStateChange = 0;
            soundInstanceGrassStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceWoodStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceSandStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceSnowStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceStoneStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceDirtStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceIceStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceLeafStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceSmoothTileStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceGlassStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceMetalStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
            soundInstanceMarbleGraniteStep = SoundEngine.LegacySoundPlayer.SoundInstanceRun;
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

        public static string pathSand;
        public static string pathDirt;
        public static string pathGrass;
        public static string pathSnow;
        public static string pathStone;
        public static string pathWet;
        public static string pathWood;
        public static string pathIce;
        public static string pathLeaf;
        public static string pathSmooth;
        public static string pathMetal;
        public static string pathMarbleGranite;
        public static string pathGlass;
        public static string pathArmor;
        public static string pathVanity;
        public override void PreUpdate()
        {
            var cfg = ModContent.GetInstance<GeneralConfig>();
            var cfg2 = ModContent.GetInstance<AmbientConfigServer>();

            #region ShowReverbTiles
            if (cfg.debugInterface && cfg2.ugReverbCalculation && cfg2.advancedReverbCalculation && !cfg2.noReverbMath)
            {
                int wallCount = 0;
                int tileCount = 0;
                if (Main.GameUpdateCount % 10 == 0)
                {
                    if (Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneUnderworldHeight || Main.LocalPlayer.ZoneDirtLayerHeight)
                    {
                        int wallsNear = ReverbAudioSystem.WallsAround(Player.Center, new Point(15, 15), out List<Point> wallPoints);
                        int tilesNear = ReverbAudioSystem.TilesAround(Player.Center, new Point(15, 15), out List<Point> tilePoints);
                        foreach (var tilePos in tilePoints)
                        {
                            var worldCoords = tilePos.ToVector2() * 16;
                            var left = new Point(tilePos.X - 1, tilePos.Y);
                            var right = new Point(tilePos.X + 1, tilePos.Y);
                            var up = new Point(tilePos.X, tilePos.Y - 1);
                            var down = new Point(tilePos.X, tilePos.Y + 1);
                            if (ReverbAudioSystem.CanRaycastTo(Player.Center, left.ToVector2() * 16) || ReverbAudioSystem.CanRaycastTo(Player.Center, right.ToVector2() * 16)
                                || ReverbAudioSystem.CanRaycastTo(Player.Center, up.ToVector2() * 16) || ReverbAudioSystem.CanRaycastTo(Player.Center, down.ToVector2() * 16))
                            {
                                tileCount++;
                                if (Main.GameUpdateCount % 10 == 0)
                                {
                                    Dust.QuickBox(worldCoords, worldCoords + new Vector2(16), 0, Color.Green, null);
                                }
                            }
                        }
                        foreach (var wallPos in wallPoints)
                        {
                            if (ReverbAudioSystem.CanRaycastTo(Player.Center, wallPos.ToVector2() * 16))
                            {
                                wallCount++;
                                if (Main.GameUpdateCount % 10 == 0)
                                {
                                    Dust.QuickBox(wallPos.ToVector2() * 16, wallPos.ToVector2() * 16 + new Vector2(16), 0, Color.Red, null);
                                }
                            }
                        }
                        Console.WriteLine($"wall: {wallCount} | tile: {tileCount}");
                    }
                }
            }
            #endregion
            #region Sound Paths / StepRands / Whatever
            timerUntilValidChestStateChange++;
            // This is a pretty niche finding for tiles above said player
            Tile playerTile = Main.tile[(int)Player.Center.X / 16, (int)Player.Center.Y / 16];
            for (int i = (int)Player.Top.X - 1; i < Player.Top.X + 1; i++)
            {
                for (int j = (int)Player.Top.Y - 1; playerTile.wall <= 0 ? j > Player.Top.Y - 350 : j > Player.Top.Y - 600; j--)
                {
                    if (WorldGen.InWorld(i / 16, j / 16))
                    {
                        Tile tile = Main.tile[i / 16, j / 16];
                        if (tile.IsActive && tile.CollisionType == 1)
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
            }
            legFrameSnapShotNew = Player.legFrame.Y / 20;
            int randWood = Main.rand.Next(1, 8);
            int randSand = Main.rand.Next(1, 7);
            int randStone = Main.rand.Next(1, 9);
            int randGrass = Main.rand.Next(1, 9);
            int randSnow = Main.rand.Next(1, 12);
            int randWet = Main.rand.Next(1, 4);
            int randDirt = Main.rand.Next(1, 7);
            int randLeaf = Main.rand.Next(1, 8);
            int randMarbleGranite = Main.rand.Next(1, 8);
            int randIce = Main.rand.Next(1, 8);
            int randGlass = Main.rand.Next(1, 7);
            int randSmooth = Main.rand.Next(1, 8);
            int randMetal = Main.rand.Next(1, 7);
            int randArmor = Main.rand.Next(1, 10);
            int randVanity = Main.rand.Next(1, 7);

            pathWood = $"TerrariaAmbience/Sounds/Custom/steps/wood/step{randWood}";
            pathSand = $"TerrariaAmbience/Sounds/Custom/steps/sand/step{randSand}";
            pathStone = $"TerrariaAmbience/Sounds/Custom/steps/stone/step{randStone}";
            pathGrass = $"TerrariaAmbience/Sounds/Custom/steps/grass/step{randGrass}";
            pathSnow = $"TerrariaAmbience/Sounds/Custom/steps/snow/step{randSnow}";
            pathWet = $"TerrariaAmbience/Sounds/Custom/steps/wet/step{randWet}";
            pathDirt = $"TerrariaAmbience/Sounds/Custom/steps/dirt/step{randDirt}";
            pathLeaf = $"TerrariaAmbience/Sounds/Custom/steps/leaf/step{randLeaf}";
            pathMetal = $"TerrariaAmbience/Sounds/Custom/steps/metal/step{randMetal}";
            pathMarbleGranite = $"TerrariaAmbience/Sounds/Custom/steps/marblegranite/step{randMarbleGranite}";
            pathSmooth = $"TerrariaAmbience/Sounds/Custom/steps/smoothstones/step{randSmooth}";
            pathGlass = $"TerrariaAmbience/Sounds/Custom/steps/glass/step{randGlass}";
            pathIce = $"TerrariaAmbience/Sounds/Custom/steps/ice/step{randIce}";
            pathArmor = $"TerrariaAmbience/Sounds/Custom/steps/armor/heavy{randArmor}";
            pathVanity = $"TerrariaAmbience/Sounds/Custom/steps/armor/light{randVanity}";

            if (Player.velocity.Y != 0f)
            {
                isOnWoodTile = false;
                isOnDirtyTile = false;
                isOnSnowyTile = false;
                isOnStoneTile = false;
                isOnGrassyTile = false;
                isOnSandyTile = false;
                isOnIcyTile = false;
                isOnLeafTile = false;
                isOnMetalTile = false;
                isOnMarbleOrGraniteTile = false;
                isOnGlassTile = false;
                isOnSmoothTile = false;
            }

            ReverbAudioSystem.CreateAudioFX(Player.Center, out var reverbActual, out float occ, out float dampening, out bool sOcclude);
            Player.GetModPlayer<ReverbPlayer>().ReverbFactor = reverbActual;

            // reverbActual = ReverbAudioSystem.CanFindEscapeRoute(Player.Bottom - new Vector2(0, 3), 5) ? 0f : reverbActual;
            if (!Player.wet)
            {
                soundInstanceDirtStep.ApplyReverb(reverbActual);
                soundInstanceStoneStep.ApplyReverb(reverbActual);
                soundInstanceWoodStep.ApplyReverb(reverbActual);
                soundInstanceGrassStep.ApplyReverb(reverbActual);
                soundInstanceSnowStep.ApplyReverb(reverbActual);
                soundInstanceSandStep.ApplyReverb(reverbActual);

                soundInstanceIceStep.ApplyReverb(reverbActual);
                soundInstanceSmoothTileStep.ApplyReverb(reverbActual);
                soundInstanceMetalStep.ApplyReverb(reverbActual);
                soundInstanceGlassStep.ApplyReverb(reverbActual);
                soundInstanceMarbleGraniteStep.ApplyReverb(reverbActual);
                soundInstanceLeafStep.ApplyReverb(reverbActual);
                soundInstanceArmorStep.ApplyReverb(reverbActual);
                soundInstanceVanityStep.ApplyReverb(reverbActual);
            }
            else
            {
                float standardBand = 0.1f;
                soundInstanceDirtStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceStoneStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceWoodStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceGrassStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceSnowStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceSandStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);

                soundInstanceIceStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceSmoothTileStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceMetalStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceGlassStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceMarbleGraniteStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceLeafStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceArmorStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
                soundInstanceVanityStep.ApplyReverbReturnInstance(reverbActual).ApplyBandPassFilter(standardBand);
            }

            // this is left in because my reverb system thinks that the sound originates from within a tile.
            #endregion
            if (ModContent.GetInstance<GeneralConfig>().footsteps)
            {
                bool stepping = (legFrameSnapShotNew == 44 && legFrameSnapShotOld != 44) || (legFrameSnapShotNew == 25 && legFrameSnapShotOld != 25);
                bool landing = legFrameSnapShotNew != 14 && legFrameSnapShotOld == 14;
                if (stepping)
                    GetFootstepSound(false);
                else if (landing)
                    GetFootstepSound(true);
            }
            // HandleContainerOpenings(ContainerContext.Chest, out var opened);
            chestStateNew = Player.chest;
            if (Main.soundVolume > 0f)
            {
                if (ModContent.GetInstance<AmbientConfigServer>().chestSounds)
                {
                    if (timerUntilValidChestStateChange > 10)
                    {
                        if (chestStateNew >= -1)
                        {
                            if ((chestStateNew != chestStateOld) && chestStateNew > -1)
                            {
                                SoundEngine.PlaySound(new ModSoundStyle("TerrariaAmbience/Sounds/Custom/ambient/player/chest_open", 0, default, 0.25f), Player.Center);
                            }
                            if ((chestStateNew != chestStateOld) && chestStateNew == -1 && chestStateOld > -1)
                            {
                                SoundEngine.PlaySound(new ModSoundStyle("TerrariaAmbience/Sounds/Custom/ambient/player/chest_close", 0, default, 0.55f), Player.Center);
                            }
                            else if ((chestStateNew != chestStateOld) && chestStateOld > -1)
                            {
                                SoundEngine.PlaySound(new ModSoundStyle("TerrariaAmbience/Sounds/Custom/ambient/player/chest_close", 0, default, 0.55f), Player.Center);
                            }
                        }
                    }
                }
            }
            chestStateOld = chestStateNew;
            legFrameSnapShotOld = legFrameSnapShotNew;
            Player.runSoundDelay = 100;
        }
        public override void PostUpdate()
        {
            HandleIceScraping();
            int randX = Main.rand.Next(-1750, 1750);
            int randY = Main.rand.Next(-1750, 1750);
            int randC = Main.rand.Next(1, 6);
            int randD = Main.rand.Next(1, 7);
            int randF = Main.rand.Next(1, 3);
            int randM = Main.rand.Next(1, 10);

            string pick = GeneralHelpers.Pick($"close/{randC}", $"distant/{randD}", $"far/{randF}", $"medial/{randM}");
            string pathToThunder = $"TerrariaAmbience/Sounds/Custom/ambient/rain/thunder/{pick}";

            float mafs = 2000 / (Main.maxRaining * 2);
            float chance3 = Main.rand.NextFloat(mafs);
            int activePlayers = 0;
            foreach (Player p in Main.player)
            {
                if (p.active)
                    activePlayers++;
            }
            if (Main.raining && !Player.ZoneSnow && Player.ZoneOverworldHeight)
            {
                if (chance3 < 2)
                {
                    thunderInstance = SoundEngine.PlaySound(new ModSoundStyle(pathToThunder, 0, SoundType.Ambient), Player.Center + new Vector2(randX, randY));
                    thunderInstance.Volume = Main.ambientVolume * (Player.ZoneDirtLayerHeight ? 0.35f : 0.9f);
                    thunderInstance.Pitch = Main.rand.NextFloat(-0.2f, -0.1f);
                }
            }
            if (!Main.dayTime)
            {
                string pathToHowl = $"TerrariaAmbience/Sounds/Custom/ambient/animals/howl";

                int chance = Main.rand.Next(1500 * activePlayers * 4);
                if (Player.ZoneSnow && !Player.ZoneUnderworldHeight && !Player.ZoneRockLayerHeight)
                {
                    if (chance == 1)
                    {
                        howlInstance = SoundEngine.PlaySound(new ModSoundStyle(pathToHowl), Player.Center + new Vector2(randX, randY));
                        howlInstance.Volume = Main.ambientVolume * 0.1f;
                        howlInstance.Pitch = Main.rand.NextFloat(-0.4f, -0.1f);
                    }
                }
            }
            if (Player.ZoneDungeon)
            {
                int possibleChance = Main.rand.Next(1000);
                int randX1 = Main.rand.Next(-2250, 2250);
                int randY1 = Main.rand.Next(-2250, 2250);
                if (possibleChance == 0)
                    SoundEngine.PlaySound(new ModSoundStyle("TerrariaAmbience/Sounds/Custom/ambient/animals/rattling_bones"), Player.Center + new Vector2(randX1, randY1)).Volume = Main.ambientVolume * 0.05f; ;
            }
            var aLoader = Ambience.Instance;
            if (aLoader.crackleVolume > 1)
                aLoader.crackleVolume = 1;
            if (aLoader.crackleVolume < 0)
                aLoader.crackleVolume = 0;
            if (isNearCampfire && ModContent.GetInstance<GeneralConfig>().campfireSounds)
                Ambience.Instance.crackleVolume = 1f - ModContent.GetInstance<CampfireDetection>().distanceToCampfire / 780;
            else
                Ambience.Instance.crackleVolume = 0f;
        }

        /// <summary>
        /// Get the player's footstep sound.
        /// </summary>
        /// <param name="landing">If the player is landing. If false, the player is stepping.</param>
        /// <returns></returns>
        public SoundEffectInstance GetFootstepSound(bool landing)
        {
            SoundEffectInstance sound = null;
            Player.Bottom.TilesAround(20, Player.ZoneRockLayerHeight || Player.ZoneDirtLayerHeight);
            if (!Player.mount.Active && Player.velocity.Y == 0 && Main.soundVolume != 0f && !Player.pulley)
            {
                if (ModContent.GetInstance<GeneralConfig>().areArmorAndVanitySoundsEnabled)
                {
                    soundInstanceArmorStep = SoundEngine.PlaySound(new ModSoundStyle(pathArmor, 0, SoundType.Sound, GetArmorStepVolume() * 1.5f), Player.Bottom);
                    soundInstanceVanityStep = SoundEngine.PlaySound(new ModSoundStyle(pathVanity, 0, SoundType.Sound, GetVanityStepVolume() * 1.5f), Player.Bottom);
                    soundInstanceArmorStep = SoundEngine.PlaySound(new ModSoundStyle(pathArmor, 0, SoundType.Sound, GetArmorStepVolume()), Player.Bottom);
                    soundInstanceVanityStep = SoundEngine.PlaySound(new ModSoundStyle(pathVanity, 0, SoundType.Sound, GetVanityStepVolume()), Player.Bottom);
                }

                #region Steps Per-tile
                if (isOnSandyTile && landing)
                {
                    sound = soundInstanceSandStep = SoundEngine.PlaySound(new ModSoundStyle(pathSand, 0, SoundType.Sound, 1f), Player.Bottom);
                }
                if (isOnGrassyTile && landing)
                {
                    if (!Main.raining || hasTilesAbove)
                    {
                        sound = soundInstanceGrassStep = SoundEngine.PlaySound(new ModSoundStyle(pathGrass, 0, SoundType.Sound, 0.35f), Player.Bottom);
                    }
                    if (!hasTilesAbove && Main.raining && !Player.wet && !Player.ZoneSnow && Player.ZoneOverworldHeight)
                    {
                        sound = soundInstanceGrassStep = SoundEngine.PlaySound(new ModSoundStyle(pathWet, 0, SoundType.Sound, 0.55f), Player.Bottom);
                    }
                }
                if (isOnDirtyTile && landing)
                {
                    if (!Main.raining || hasTilesAbove)
                    {
                        sound = soundInstanceDirtStep = SoundEngine.PlaySound(new ModSoundStyle(pathDirt, 0, SoundType.Sound, 0.45f), Player.Bottom);
                    }
                    if (!hasTilesAbove && Main.raining && !Player.wet && !Player.ZoneSnow && Player.ZoneOverworldHeight)
                    {
                        sound = soundInstanceDirtStep = SoundEngine.PlaySound(new ModSoundStyle(pathWet, 0, SoundType.Sound, 0.45f), Player.Bottom);
                    }
                }
                if (isOnStoneTile && landing)
                {
                    sound = soundInstanceStoneStep = SoundEngine.PlaySound(new ModSoundStyle(pathStone, 0, SoundType.Sound, 0.35f), Player.Bottom);
                }
                if (isOnSnowyTile && landing)
                {
                    sound = soundInstanceSnowStep = SoundEngine.PlaySound(new ModSoundStyle(pathSnow, 0, SoundType.Sound, 0.45f), Player.Bottom);
                }
                if (isOnWoodTile && landing)
                {
                    sound = soundInstanceWoodStep = SoundEngine.PlaySound(new ModSoundStyle(pathWood, 0, SoundType.Sound, 0.35f), Player.Bottom);
                }
                if (isOnMetalTile && landing)
                {
                    sound = soundInstanceMetalStep = SoundEngine.PlaySound(new ModSoundStyle(pathMetal, 0, SoundType.Sound, 0.45f), Player.Bottom);
                }
                if (isOnSmoothTile && landing)
                {
                    sound = soundInstanceSmoothTileStep = SoundEngine.PlaySound(new ModSoundStyle(pathSmooth, 0, SoundType.Sound, 0.55f), Player.Bottom);
                }
                if (isOnMarbleOrGraniteTile && landing)
                {
                    sound = soundInstanceMarbleGraniteStep = SoundEngine.PlaySound(new ModSoundStyle(pathMarbleGranite, 0, SoundType.Sound, 0.45f), Player.Bottom);
                }
                if (isOnIcyTile && landing)
                {
                    sound = soundInstanceIceStep = SoundEngine.PlaySound(new ModSoundStyle(pathIce, 0, SoundType.Sound, 0.175f), Player.Bottom);
                }
                if (isOnLeafTile && landing)
                {
                    sound = soundInstanceLeafStep = SoundEngine.PlaySound(new ModSoundStyle(pathLeaf, 0, SoundType.Sound, 0.45f), Player.Bottom);
                }
                if (isOnGlassTile && landing)
                {
                    sound = soundInstanceGlassStep = SoundEngine.PlaySound(new ModSoundStyle(pathGlass, 0, SoundType.Sound, 0.45f), Player.Bottom);
                }
                // Top: landing sounds
                // Bottom: walking sounds
                if (isOnSandyTile && !landing)
                {
                    sound = soundInstanceSandStep = SoundEngine.PlaySound(new ModSoundStyle(pathSand, 0, SoundType.Sound, 0.9f), Player.Bottom);
                }
                if (isOnGrassyTile && !landing)
                {
                    if (!Main.raining || hasTilesAbove) // rain and no tiles covering said player
                    {
                        sound = soundInstanceGrassStep = SoundEngine.PlaySound(new ModSoundStyle(pathGrass, 0, SoundType.Sound, 0.1f), Player.Bottom);
                    }
                    if (!hasTilesAbove && Main.raining && !Player.wet)
                    {
                        sound = soundInstanceGrassStep = SoundEngine.PlaySound(new ModSoundStyle(pathWet, 0, SoundType.Sound, 0.65f), Player.Bottom);
                    }
                }
                if (isOnDirtyTile && !landing)
                {
                    if (!Main.raining || hasTilesAbove) // rain and no tiles covering said player
                    {
                        sound = soundInstanceDirtStep = SoundEngine.PlaySound(new ModSoundStyle(pathDirt, 0, SoundType.Sound, 0.15f), Player.Bottom);
                    }
                    if (!hasTilesAbove && Main.raining && !Player.wet)
                    {
                        sound = soundInstanceDirtStep = SoundEngine.PlaySound(new ModSoundStyle(pathWet, 0, SoundType.Sound, 0.65f), Player.Bottom);
                    }
                }
                if (isOnStoneTile && !landing)
                {
                    sound = soundInstanceStoneStep = SoundEngine.PlaySound(new ModSoundStyle(pathStone, 0, SoundType.Sound, 0.15f), Player.Bottom);
                }
                if (isOnWoodTile && !landing)
                {
                    sound = soundInstanceWoodStep = SoundEngine.PlaySound(new ModSoundStyle(pathWood, 0, SoundType.Sound, 0.2f), Player.Bottom);
                }
                if (isOnSnowyTile && !landing)
                {
                    sound = soundInstanceSnowStep = SoundEngine.PlaySound(new ModSoundStyle(pathSnow, 0, SoundType.Sound, 0.25f), Player.Bottom);
                }
                if (isOnMetalTile && !landing)
                {
                    sound = soundInstanceMetalStep = SoundEngine.PlaySound(new ModSoundStyle(pathMetal, 0, SoundType.Sound, 0.25f), Player.Bottom);
                }
                if (isOnSmoothTile && !landing)
                {
                    sound = soundInstanceSmoothTileStep = SoundEngine.PlaySound(new ModSoundStyle(pathSmooth, 0, SoundType.Sound, 0.25f), Player.Bottom);
                }
                if (isOnMarbleOrGraniteTile && !landing)
                {
                    sound = soundInstanceMarbleGraniteStep = SoundEngine.PlaySound(new ModSoundStyle(pathMarbleGranite, 0, SoundType.Sound, 0.25f), Player.Bottom);
                }
                if (isOnIcyTile && !landing)
                {
                    sound = soundInstanceIceStep = SoundEngine.PlaySound(new ModSoundStyle(pathIce, 0, SoundType.Sound, 0.075f), Player.Bottom);
                }
                if (isOnLeafTile && !landing)
                {
                    sound = soundInstanceLeafStep = SoundEngine.PlaySound(new ModSoundStyle(pathLeaf, 0, SoundType.Sound, 0.25f), Player.Bottom);
                }
                if (isOnGlassTile && !landing)
                {
                    sound = soundInstanceGlassStep = SoundEngine.PlaySound(new ModSoundStyle(pathGlass, 0, SoundType.Sound, 0.25f), Player.Bottom);
                }
                #endregion
            }
            return sound;
        }

        public float GetArmorStepVolume()
        {
            bool hasHeadArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.HeadSlot, out var head);
            bool hasChestArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.ChestSlot, out var chest);
            bool hasLegArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.LegSlot, out var legs);

            float determinedValue = 0f;

            // 10 <= x <= 12 == vanity armor
            bool isWood(Item item)
            {
                if (ItemID.Search.TryGetName(item.type, out var name))
                {
                    return name.ToLower().Contains("wood");
                }
                return false;
            }
            bool hasVanityCovering(int context)
            {
                return !Player.armor[context + 10].IsAir;
            }

            if (hasHeadArmor)
            {
                if (!head.vanity)
                {
                    if (!isWood(head))
                    {
                        if (!hasVanityCovering(GeneralHelpers.IDs.ArmorSlotID.HeadSlot))
                        {
                            determinedValue *= 1.25f + 0.0025f;
                        }
                    }
                }
            }
            if (hasChestArmor)
            {
                if (!chest.vanity)
                {
                    if (!isWood(chest))
                    {
                        if (!hasVanityCovering(GeneralHelpers.IDs.ArmorSlotID.ChestSlot))
                        {
                            determinedValue += 0.005f;
                        }
                    }
                }
            }
            if (hasLegArmor)
            {
                if (!legs.vanity)
                {
                    if (!isWood(legs))
                    {
                        if (!hasVanityCovering(GeneralHelpers.IDs.ArmorSlotID.LegSlot))
                        {
                            determinedValue += 0.01f;
                        }
                    }
                }
            }

            return determinedValue;
        }

        public float GetVanityStepVolume()
        {
            bool hasHeadArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.VanityHeadSlot, out var head);
            bool hasChestArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.VanityChestSlot, out var chest);
            bool hasLegArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.VanityLegSlot, out var legs);

            float determinedValue = 0f;

            // 10 <= x <= 12 == vanity armor
            if (hasHeadArmor)
            {
                if (head.vanity)
                {
                    determinedValue *= 1.25f + 0.01f;
                }
            }
            if (hasChestArmor)
            {
                if (chest.vanity)
                {
                    determinedValue += 0.02f;
                }
            }
            if (hasLegArmor)
            {
                if (legs.vanity)
                {
                     determinedValue += 0.03f;
                }
            }

            return determinedValue;
        }

        public void HandleContainerOpenings(ContainerContext context, out bool opened)
        {
            opened = false;
            chestStateNew = Player.chest;
            if (timerUntilValidChestStateChange > 10)
            {
                // Main.NewText($"{chestStateNew} : {chestStateOld}");
                switch (context)
                {
                    case ContainerContext.Chest:
                        // > -1
                        if (chestStateNew >= -1)
                        {
                            if ((chestStateNew != chestStateOld) && chestStateNew > -1)
                            {
                                opened = true;
                            }
                            // orignal closed, none other opened
                            if ((chestStateNew != chestStateOld) && chestStateNew == -1 && chestStateOld > -1)
                            {
                                opened = false;
                            }
                            // original closed, new opened
                            else if ((chestStateNew != chestStateOld) && chestStateOld > -1)
                            {
                                opened = false;
                            }
                        }
                        break;
                    case ContainerContext.PiggyBank:
                        // -2
                        break;
                    case ContainerContext.Safe:
                        // -3
                        break;
                }
            }
            chestStateOld = chestStateNew;
        }

        public SoundEffect soundSlippyRough;
        public SoundEffectInstance soundSlippyRoughInst;

        public SoundEffect soundSlippySmooth;
        public SoundEffectInstance soundSlippySmoothInst;

        private bool _areSoundsInitialized;
        public void HandleIceScraping()
        {
            if (!Main.dedServ)
            {
                if (!_areSoundsInitialized)
                {
                    soundSlippyRough = Mod.Assets.Request<SoundEffect>("Sounds/Custom/ambient/player/ice_slide_rough", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    soundSlippyRoughInst = soundSlippyRough.CreateInstance();
                    soundSlippyRoughInst.IsLooped = true;
                    soundSlippyRoughInst?.Play();
                    soundSlippyRoughInst.Volume = 0f;
                    soundSlippySmooth = Mod.Assets.Request<SoundEffect>("Sounds/Custom/ambient/player/ice_slide_smooth", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    soundSlippySmoothInst = soundSlippySmooth.CreateInstance();
                    soundSlippySmoothInst.IsLooped = true;
                    soundSlippySmoothInst?.Play();
                    soundSlippySmoothInst.Volume = 0f;
                    _areSoundsInitialized = true;
                }
                bool slippyRough = Player.slippy;
                bool slippySmooth = Player.slippy2;

                if (slippyRough)
                    soundSlippyRoughInst.Volume = Math.Abs(Player.velocity.X / 50f);
                else
                    soundSlippyRoughInst.Volume -= 0.01f;
                if (slippySmooth)
                    soundSlippySmoothInst.Volume = Math.Abs(Player.velocity.X / 50f);
                else
                    soundSlippySmoothInst.Volume -= 0.01f;
                soundSlippyRoughInst.Volume = MathHelper.Clamp(soundSlippyRoughInst.Volume, 0f, 0.5f);
                soundSlippySmoothInst.Volume = MathHelper.Clamp(soundSlippySmoothInst.Volume, 0f, 0.5f);
            }
        }

        public override void ResetEffects()
        {
            TileDetection.curTileType = -1;
        }
    }
    public class CampfireDetection : GlobalTile
    {
        public Vector2 originOfCampfire;

        public float distanceToCampfire;

        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            originOfCampfire.X = i * 16;
            originOfCampfire.Y = j * 16;
            Player player = Main.player[Main.myPlayer].GetModPlayer<AmbientPlayer>().Player;

            if (ModContent.GetInstance<GeneralConfig>().campfireSounds)
            {
                if (type == TileID.Campfire && closer && player.HasBuff(BuffID.Campfire))
                {
                    /*var t = Main.tile[i, j];
                    var orig = new Vector2(i * 16, j * 16);
                    Main.NewText(t.frameY);*/
                    //if (t.frameY <= 18 && t.frameY >= 0)
                    {
                        distanceToCampfire = Vector2.Distance(originOfCampfire, player.Center);
                        player.GetModPlayer<AmbientPlayer>().isNearCampfire = true;
                    }
                }
                if ((type == TileID.Campfire && !closer) || !player.HasBuff(BuffID.Campfire))
                {
                    /*var t = Main.tile[i, j];
                    var orig = new Vector2(i * 16, j * 16);
                    Main.NewText(t.frameY);*/
                    //if (t.frameY <= 54 && t.frameY >= 36)
                    {
                        Ambience.CampfireCrackleInstance.Volume = 0f;
                        player.GetModPlayer<AmbientPlayer>().isNearCampfire = false;
                    }
                }
            }
        }
    }
}

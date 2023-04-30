using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaAmbience.Content;
using ReLogic.Graphics;
using Microsoft.Xna.Framework.Audio;
using Terraria.ID;
using Terraria.UI.Chat;
using TerrariaAmbience.Content.Players;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using TerrariaAmbience.Helpers;
using Terraria.Audio;
using Terraria.GameContent;
using System.Diagnostics;
using System.Data;
using NAudio.CoreAudioApi;

namespace TerrariaAmbience.Core
{
    internal class MethodDetours
    {
        public static void DetourAll()
        {
            ContentInstance.Register(new GeneralHelpers());

            On_Main.DrawMenu += Main_DrawMenu;
            On_Main.DrawInterface_30_Hotbar += Main_DrawInterface_30_Hotbar;
            On_IngameOptions.DrawRightSide += DrawVolumeValues;
            On_IngameOptions.Draw += DrawTAVolume;
            // MenuDetours.On_AddMenuButtons += MenuDetours_On_AddMenuButtons;

            active = true;
            posY = 4;
        }

        private static void DrawTAVolume(On_IngameOptions.orig_Draw orig, Main mainInstance, SpriteBatch sb)
        {
            sb.DrawString(FontAssets.DeathText.Value, $"Hold Add or Subtract (or period or comma) to change the volume of Terraria Ambience!", new Vector2(8, 8), Color.White, 0f, Vector2.Zero, 0.3f, SpriteEffects.None, 1f);
            orig(mainInstance, sb);
        }

        public static void MenuDetours_On_AddMenuButtons(MenuDetours.Orig_AddMenuButtons orig, Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
        {
            GeneralHelpers.AddMainMenuButton("Ambience Menu", delegate { Main.menuMode = 999; }, selectedMenu, buttonNames, ref buttonIndex, ref numButtons);
            orig(main, selectedMenu, buttonNames, buttonScales, ref offY, ref spacing, ref buttonIndex, ref numButtons);
        }
        private static bool oldHover;
        private static bool hovering;
        private static bool DrawVolumeValues(On_IngameOptions.orig_DrawRightSide orig, SpriteBatch sb, string txt, int i, Vector2 anchor, Vector2 offset, float scale, float colorScale, Color over)
        {
            Rectangle hoverPos = new((int)anchor.X - 65, (int)anchor.Y + 119, 275, 15);
            if (i == 14)
            {
                hovering = hoverPos.Contains(Main.MouseScreen.ToPoint());
            }

            if (!oldHover && hovering)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            oldHover = hovering;
            if (IngameOptions.category == 2)
            {
                if (i == 3)
                {
                    if (Main.FrameSkipMode == Terraria.Enums.FrameSkipMode.Subtle)
                    {
                        txt = "Frame Skip Subtle (WARNING)";
                    }
                }
            }
            return orig(sb, txt, i, anchor, offset, scale, colorScale, over);
        }
        #region Shitcode i will never rework
        private static Vector2 drawPos;
        private static string displayable;
        private static void Main_DrawInterface_30_Hotbar(On_Main.orig_DrawInterface_30_Hotbar orig, Main self)
        {
            orig(self);
            var loader = Ambience.Instance;

            var aPlayer = Main.player[Main.myPlayer].GetModPlayer<AmbientPlayer>();
            bool isVanillaTile = TileID.Search.TryGetName(TileDetection.curTileType, out string name);
            displayable =
                $"{Ambience.BeachWaves.Name}: {Ambience.BeachWavesInstance.Volume}"
                + $"\n{Ambience.CampfireCrackle.Name}: {loader.crackleVolume}"
                + $"\n{Ambience.SnowBreezeDay.Name}: {Ambience.SnowBreezeDayInstance.Volume}"
                + $"\n{Ambience.SnowBreezeNight.Name}: {Ambience.SnowBreezeNightInstance.Volume}"
                + $"\n{Ambience.MorningCrickets.Name}: {Ambience.MorningCricketsInstance.Volume}"
                + $"\n{Ambience.DayCrickets.Name}: {Ambience.DayCricketsInstance.Volume}"
                + $"\n{Ambience.NightCrickets.Name}: {Ambience.NightCricketsInstance.Volume}"
                + $"\n{Ambience.EveningCrickets.Name}: {Ambience.EveningCricketsInstance.Volume}"
                + $"\n{Ambience.CavesAmbience.Name}: {Ambience.CavesAmbienceInstance.Volume}"
                + $"\n{Ambience.CrimsonRumbles.Name}: {Ambience.CrimsonRumblesInstance.Volume}"
                + $"\n{Ambience.CorruptionRoars.Name}: {Ambience.CorruptionRoarsInstance.Volume}"
                + $"\n{Ambience.DayJungle.Name}: {Ambience.DaytimeJungleInstance.Volume}"
                + $"\n{Ambience.NightJungle.Name}: {Ambience.NightJungleInstance.Volume}"
                + $"\n{Ambience.DesertAmbience.Name}: {Ambience.DesertAmbienceInstance.Volume}"
                + $"\n{Ambience.HellRumble.Name}: {Ambience.HellRumbleInstance.Volume}"
                + $"\nFootsteps:" +
                (aPlayer.soundInstanceGrassStep != null ? $"\nGrass: {aPlayer.soundInstanceGrassStep.State} (isOn: {aPlayer.isOnGrassyTile})" : "\nGrass: Stopped (isOn: False)")
                + (aPlayer.soundInstanceStoneStep != null ? $"\nStone: {aPlayer.soundInstanceStoneStep.State} (isOn: {aPlayer.isOnStoneTile})" : "\nStone: Stopped (isOn: False)")
                + (aPlayer.soundInstanceWoodStep != null ? $"\nWood: {aPlayer.soundInstanceWoodStep.State} (isOn: {aPlayer.isOnWoodTile})" : "\nWood: Stopped (isOn: False)")
                + (aPlayer.soundInstanceSnowStep != null ? $"\nSnow: {aPlayer.soundInstanceSnowStep.State} (isOn: {aPlayer.isOnSnowyTile})" : "\nSnow: Stopped (isOn: False)")
                + (aPlayer.soundInstanceSandStep != null ? $"\nSand: {aPlayer.soundInstanceSandStep.State} (isOn: {aPlayer.isOnSandyTile})" : "\nSand: Stopped (isOn: False)")
                + (aPlayer.soundInstanceDirtStep != null ? $"\nDirt: {aPlayer.soundInstanceDirtStep.State} (isOn: {aPlayer.isOnDirtyTile})" : "\nDirt: Stopped (isOn: False)")
                + (aPlayer.soundInstanceIceStep != null ? $"\nIce: {aPlayer.soundInstanceIceStep.State} (isOn: {aPlayer.isOnIcyTile})" : "\nIce: Stopped (isOn: False)")
                + (aPlayer.soundInstanceLeafStep != null ? $"\nLeaf: {aPlayer.soundInstanceLeafStep.State} (isOn: {aPlayer.isOnLeafTile})" : "\nLeaf: Stopped (isOn: False)")
                + (aPlayer.soundInstanceMetalStep != null ? $"\nMetal: {aPlayer.soundInstanceMetalStep.State} (isOn: {aPlayer.isOnMetalTile})" : "\nMetal: Stopped (isOn: False)")
                + (aPlayer.soundInstanceMarbleGraniteStep != null ? $"\nGranMarb: {aPlayer.soundInstanceMarbleGraniteStep.State} (isOn: {aPlayer.isOnMarbleOrGraniteTile})" : "\nGranMarb: Stopped (isOn: False)")
                + (aPlayer.soundInstanceSmoothTileStep != null ? $"\nSmoothStones: {aPlayer.soundInstanceSmoothTileStep.State} (isOn: {aPlayer.isOnSmoothTile})" : "\nSmoothStones: Stopped (isOn: False)")
                + (aPlayer.soundInstanceGlassStep != null ? $"\nGlass: {aPlayer.soundInstanceGlassStep.State} (isOn: {aPlayer.isOnGlassTile})" : "\nGlass: Stopped (isOn: False)")
                + $"\nCurTile: " + (TileDetection.curTileType >= 0 ? (isVanillaTile ? name + $" | ID: {TileDetection.curTileType}" : TileLoader.GetTile(TileDetection.curTileType).Name + $" ({TileLoader.GetTile(TileDetection.curTileType).Mod.Name})") : "None")
                + $"\nPlayer Reverb Gain: {Main.LocalPlayer.GetModPlayer<Sounds.ReverbPlayer>().ReverbFactor}"
                + $"\nisUnderground: {Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneDirtLayerHeight}";

            if (ModContent.GetInstance<GeneralConfig>().debugInterface)
            {
                #region DrawVolume
                if (Main.playerInventory && (Main.mapStyle == 0 || Main.mapStyle == 2))
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 2, 175);
                if (Main.mapStyle == 1 && Main.playerInventory)
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 2, 375);
                if (Main.mapStyle == 1 && !Main.playerInventory)
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 3, 375);
                if ((Main.mapStyle == 0 || Main.mapStyle == 2) && !Main.playerInventory)
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 3, 175);
                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, 
                    new Rectangle(
                        (int)drawPos.X - 6,
                        (int)drawPos.Y - 6,
                        (int)(FontAssets.DeathText.Value.MeasureString(displayable).X * 0.24f),
                        (int)(FontAssets.DeathText.Value.MeasureString(displayable).Y * 0.23f)), 
                    Color.SkyBlue * 0.6f);

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch,
                    FontAssets.DeathText.Value, 
                    displayable,
                    position: drawPos, 
                    Color.LightGray, 
                    0f, 
                    origin: Vector2.Zero, 
                    baseScale: new Vector2(0.225f), 
                    -1, 
                    1);
                #endregion

                #region DrawDebuggingKeybinds

                string txt = $"Press LeftAlt to toggle reverb info" +
                    $"\n\nMouseWorld: ({(int)Main.MouseWorld.X}, {(int)Main.MouseWorld.Y})" +
                    $"\nL: Play sound at mouse" +
                    "\nK: Spawn Positional Audio Sound at mouse" +
                    "\nOemOpenBrackets: Play CurTile footstep sound";
                Vector2 offset = new(-300, 0);
                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, 
                    new Rectangle((int)(drawPos.X + offset.X - 6), 
                    (int)(drawPos.Y + offset.Y - 6), 
                    (int)(FontAssets.DeathText.Value.MeasureString(txt).X * 0.24f), 
                    (int)(FontAssets.DeathText.Value.MeasureString(txt).Y * 0.23f)), 
                    Color.DarkOrange * 0.6f);

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch,
                    FontAssets.DeathText.Value,
                    txt,
                    drawPos - new Vector2(3, 3) + offset,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    new Vector2(0.225f), -1, 1);

                if (GeneralHelpers.KeyPress(Keys.L))
                    new ActiveSound(SoundID.ZombieMoan, Main.MouseWorld);
                if (GeneralHelpers.KeyPress(Keys.K))
                {
                    int choice = Main.rand.Next(0, 2);
                    if (choice == 1)
                        Common.Systems.AudioLoopsSystem.grassCritters[Main.rand.Next(Common.Systems.AudioLoopsSystem.grassCritters.Length)]?.Play(Main.MouseWorld, 0.75f);
                    if (choice == 2)
                        Common.Systems.AudioLoopsSystem.owls[Main.rand.Next(Common.Systems.AudioLoopsSystem.owls.Length)]?.Play(Main.MouseWorld, 0.75f);
                }
                if (GeneralHelpers.KeyPress(Keys.OemOpenBrackets))
                    Main.LocalPlayer.GetModPlayer<AmbientPlayer>().GetFootstepSound(false)?.Play();
                #endregion
            }
        }
        #region Shameless Variable Naming
        private static float posX;
        private static float posY;
        private static bool active;

        private static float backButtonScale;
        private static float buttonScale1;
        private static float buttonScale2;
        private static float buttonScale3;
        private static float buttonScale4;
        private static float buttonScale5;
        private static float buttonScale6;
        private static float buttonScale7;
        private static float buttonScale8;
        private static float buttonScale9;
        private static float buttonScale10;
        private static float buttonScale11;
        private static float buttonScale12;

        private static float[] ButtonScalesPerDir
        {
            get
            {
                int count = 0;
                foreach (var dir in GetSubFolders(Ambience.INTERNAL_CombinedPath))
                {
                    count++;
                }
                return new float[count];
            }
        }

        private static float subButtonScale1;
        private static float subButtonScale2;
        private static float subButtonScale3;
        private static float subButtonScale4;
        private static float subButtonScale5;
        private static float subButtonScale6;
        private static float subButtonScale7;
        private static float subButtonScale8;
        private static float subButtonScale9;
        private static float subButtonScale10;
        public static bool attemptingToPlayTracks;
        #endregion
        public static void HaltAllMenuAmbient()
        {
            var aLoader = Ambience.Instance;
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
            aLoader.morningCricketsVolume = 0f;
            aLoader.breezeVolume = 0f;
        }
        private static bool fOpen;
        private static bool jOpen;
        public static bool sOpen;
        public static bool eOpen;
        internal static int soundPack_whoAmI = -1;
        private static float supposedMousePosY;
        public static void DrawAmbienceMenu()
        {
            string genericError = "recievedFileBytes was null or a custom ambience track is loaded and/or replaced a normal one. Could not save file!";
            string info = "<< INFORMATION >>\n\nThese sounds are under the PUBLIC DOMAIN / CREATIVE COMMONS.\n" +
                                "Since these sounds were made with various other creative commons sounds, they cannot be used under the Attribution license.\n" +
                                "But please, if you use them, be sure to give credit to the mod and Tika, the person behind the sounds of this mod.\n\n<< This file was generated by Terraria Ambience, a Terraria mod supported by tModLoader >>";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\TA Ambient";
            Mod mod = ModContent.GetInstance<TerrariaAmbience>();

            int offY = 42;
            float defaultAlpha = 0.9f;
            float goodScreenFloat = Main.screenHeight * 0.3f;
            if (Main.menuMode == 999)
            {
                Rectangle toPlay = new Rectangle(208, 74, 26, 16);
                Rectangle toOpen = new Rectangle(40, 96, 26, 16);
                Rectangle toOpen2 = new Rectangle(290, 118, 26, 16);
                Utils.DrawBorderString(Main.spriteBatch, $"Left click the buttons to listen to the various ambience tracks from this mod!\nSome have different sounds based on the time of day.\nRight click the buttons to download the tracks to your desktop!\nIf it cannot save, you will hear\nClick      to view the folder the sounds export to.\nFor sound replacements (soundpacks), click\nTo reload all sounds, press the R key.", new Vector2(6, 6), Color.White, 0.8f);
                Utils.DrawBorderString(Main.spriteBatch, $"[c/FFFF00:this].", new Vector2(208, 74), Color.White, 0.8f);
                for (int u = 0; u < 3; u++)
                {
                    var pos = u == 1 ? new Vector2(40, 96) : new Vector2(290, 118);
                    Utils.DrawBorderString(Main.spriteBatch, u == 2 ? $"[c/FFFF00:here]." : "[c/FFFF00:here]", pos, Color.White, 0.8f);
                }

                if (toPlay.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    SoundEngine.PlaySound(SoundID.Unlock);
                }
                if (toOpen2.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    Process.Start(Ambience.SFXReplacePath);
                }
                if (toOpen.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    if (Directory.Exists(path))
                        Process.Start(path);
                }
                if (GeneralHelpers.KeyPress(Keys.Down) || GeneralHelpers.KeyPress(Keys.S))
                {
                    if (supposedMousePosY == 0)
                    {
                        supposedMousePosY = Main.screenHeight * 0.25f;
                    }
                    supposedMousePosY += 42;
                }
                if (GeneralHelpers.KeyPress(Keys.Up) || GeneralHelpers.KeyPress(Keys.W))
                {
                    supposedMousePosY -= 42;
                }
                if (GeneralHelpers.KeyPress(Keys.R))
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    GeneralHelpers.ReloadMods();
                }
                var aLoader = Ambience.Instance;
                #region Not My Proudest Code
                void DrawOpens()
                {
                    if (eOpen)
                    {
                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 180, (goodScreenFloat) + offY * 5),
                        Ambience.Instance.crimsonRumblesVolume == 1f ? "Crimson (On)" : "Crimson",
                        delegate
                        {
                            aLoader.dayCricketsVolume = 0f;
                            aLoader.nightCricketsVolume = 0f;
                            aLoader.eveningCricketsVolume = 0f;
                            aLoader.desertCricketsVolume = 0f;
                            aLoader.snowDayVolume = 0f;
                            aLoader.snowNightVolume = 0f;
                            aLoader.corruptionRoarsVolume = 0f;
                            aLoader.dayJungleVolume = 0f;
                            aLoader.nightJungleVolume = 0f;
                            aLoader.hellRumbleVolume = 0f;
                            aLoader.morningCricketsVolume = 0f;
                            SoundEngine.PlaySound(Ambience.Instance.crimsonRumblesVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);

                            Ambience.Instance.crimsonRumblesVolume = Ambience.Instance.crimsonRumblesVolume == 1f ? 0f : 1f;
                        },
                        ref subButtonScale9, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.Crimson + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "crimson_rumbles.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        });

                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 180, (goodScreenFloat) + offY * 5),
                        Ambience.Instance.corruptionRoarsVolume == 1f ? "Corruption (On)" : "Corruption",
                        delegate
                        {
                            aLoader.dayCricketsVolume = 0f;
                            aLoader.nightCricketsVolume = 0f;
                            aLoader.eveningCricketsVolume = 0f;
                            aLoader.desertCricketsVolume = 0f;
                            aLoader.snowDayVolume = 0f;
                            aLoader.snowNightVolume = 0f;
                            aLoader.crimsonRumblesVolume = 0f;
                            aLoader.dayJungleVolume = 0f;
                            aLoader.nightJungleVolume = 0f;
                            aLoader.hellRumbleVolume = 0f;
                            aLoader.morningCricketsVolume = 0f;
                            SoundEngine.PlaySound(Ambience.Instance.corruptionRoarsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);

                            Ambience.Instance.corruptionRoarsVolume = Ambience.Instance.corruptionRoarsVolume == 1f ? 0f : 1f;

                        },
                        ref subButtonScale10, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.Corruption + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "corruption_roars.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                    if (jOpen)
                    {
                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 140, (goodScreenFloat) + offY * 3),
                        Ambience.Instance.dayJungleVolume == 1f ? "Day (On)" : "Day",
                        delegate
                        {
                            Ambience.Instance.nightJungleVolume = 0f;
                            SoundEngine.PlaySound(Ambience.Instance.nightJungleVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);

                            Ambience.Instance.dayJungleVolume = Ambience.Instance.dayJungleVolume == 1f ? 0f : 1f;

                            aLoader.dayCricketsVolume = 0f;
                            aLoader.nightCricketsVolume = 0f;
                            aLoader.eveningCricketsVolume = 0f;
                            aLoader.desertCricketsVolume = 0f;
                            aLoader.ugAmbienceVolume = 0f;
                            aLoader.crimsonRumblesVolume = 0f;
                            aLoader.snowDayVolume = 0f;
                            aLoader.snowNightVolume = 0f;
                            aLoader.corruptionRoarsVolume = 0f;
                            aLoader.nightJungleVolume = 0f;
                            aLoader.beachWavesVolume = 0f;
                            aLoader.morningCricketsVolume = 0f;
                        },
                        ref subButtonScale1, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.JungleDay + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "jungle_day.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        });

                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 140, (goodScreenFloat) + offY * 3),
                        Ambience.Instance.nightJungleVolume == 1f ? "Night (On)" : "Night",
                        delegate
                        {
                            Ambience.Instance.dayJungleVolume = 0f;
                            SoundEngine.PlaySound(Ambience.Instance.nightJungleVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);

                            Ambience.Instance.nightJungleVolume = Ambience.Instance.nightJungleVolume == 1f ? 0f : 1f;

                            aLoader.dayCricketsVolume = 0f;
                            aLoader.nightCricketsVolume = 0f;
                            aLoader.eveningCricketsVolume = 0f;
                            aLoader.desertCricketsVolume = 0f;
                            aLoader.ugAmbienceVolume = 0f;
                            aLoader.crimsonRumblesVolume = 0f;
                            aLoader.snowDayVolume = 0f;
                            aLoader.snowNightVolume = 0f;
                            aLoader.corruptionRoarsVolume = 0f;
                            aLoader.dayJungleVolume = 0f;
                            aLoader.morningCricketsVolume = 0f;
                        },
                        ref subButtonScale2, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.JungleNight + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "jungle_night.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                    if (sOpen)
                    {
                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 130, goodScreenFloat + offY * 2),
                        Ambience.Instance.snowDayVolume == 1f ? "Day (On)" : "Day",
                        delegate
                        {
                            aLoader.dayCricketsVolume = 0f;
                            aLoader.nightCricketsVolume = 0f;
                            aLoader.eveningCricketsVolume = 0f;
                            aLoader.desertCricketsVolume = 0f;
                            aLoader.ugAmbienceVolume = 0f;
                            aLoader.crimsonRumblesVolume = 0f;
                            aLoader.snowNightVolume = 0f;
                            aLoader.corruptionRoarsVolume = 0f;
                            aLoader.dayJungleVolume = 0f;
                            aLoader.nightJungleVolume = 0f;
                            aLoader.beachWavesVolume = 0f;
                            aLoader.hellRumbleVolume = 0f;
                            aLoader.rainVolume = 0f;
                            aLoader.morningCricketsVolume = 0f;
                            aLoader.breezeVolume = 0f;

                            Ambience.Instance.snowDayVolume = Ambience.Instance.snowDayVolume == 1f ? 0f : 1f;
                            SoundEngine.PlaySound(Ambience.Instance.snowDayVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        },
                        ref subButtonScale3, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.SnowDay + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "snowfall_day.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );

                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 130, goodScreenFloat + offY * 2),
                        Ambience.Instance.snowNightVolume == 1f ? "Night (On)" : "Night",
                        delegate
                        {
                            aLoader.dayCricketsVolume = 0f;
                            aLoader.nightCricketsVolume = 0f;
                            aLoader.eveningCricketsVolume = 0f;
                            aLoader.desertCricketsVolume = 0f;
                            aLoader.ugAmbienceVolume = 0f;
                            aLoader.crimsonRumblesVolume = 0f;
                            aLoader.snowDayVolume = 0f;
                            aLoader.corruptionRoarsVolume = 0f;
                            aLoader.dayJungleVolume = 0f;
                            aLoader.nightJungleVolume = 0f;
                            aLoader.beachWavesVolume = 0f;
                            aLoader.hellRumbleVolume = 0f;
                            aLoader.rainVolume = 0f;
                            aLoader.morningCricketsVolume = 0f;
                            aLoader.breezeVolume = 0f;

                            Ambience.Instance.snowNightVolume = Ambience.Instance.snowNightVolume == 1f ? 0f : 1f;

                            SoundEngine.PlaySound(Ambience.Instance.snowNightVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        },
                        ref subButtonScale4, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.JungleNight + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "jungle_night.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                    if (fOpen)
                    {
                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 320, goodScreenFloat),
                        Ambience.Instance.morningCricketsVolume == 1f ? "Morning (On)" : "Morning",
                        delegate
                        {
                            aLoader.dayCricketsVolume = 0f;
                            aLoader.nightCricketsVolume = 0f;
                            aLoader.eveningCricketsVolume = 0f;
                            aLoader.desertCricketsVolume = 0f;
                            aLoader.ugAmbienceVolume = 0f;
                            aLoader.crimsonRumblesVolume = 0f;
                            aLoader.snowDayVolume = 0f;
                            aLoader.snowNightVolume = 0f;
                            aLoader.corruptionRoarsVolume = 0f;
                            aLoader.dayJungleVolume = 0f;
                            aLoader.nightJungleVolume = 0f;
                            aLoader.hellRumbleVolume = 0f;
                            aLoader.breezeVolume = 0f;
                            aLoader.morningCricketsVolume = Ambience.Instance.morningCricketsVolume == 1f ? 0f : 1f;
                            SoundEngine.PlaySound(Ambience.Instance.morningCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        },
                        ref subButtonScale5, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.ForestMorning + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "forest_morning.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );

                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 150, goodScreenFloat + 2),
                        Ambience.Instance.dayCricketsVolume == 1f ? "Midday (On)" : "Midday",
                        delegate
                        {
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
                            aLoader.hellRumbleVolume = 0f;
                            aLoader.morningCricketsVolume = 0f;

                            aLoader.dayCricketsVolume = Ambience.Instance.dayCricketsVolume == 1f ? 0f : 1f;
                            SoundEngine.PlaySound(Ambience.Instance.dayCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        },
                        ref subButtonScale6, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.ForestDay + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "forest_day.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 150, goodScreenFloat + 3),
                        Ambience.Instance.eveningCricketsVolume == 1f ? "Evening (On)" : "Evening",
                        delegate
                        {
                            aLoader.dayCricketsVolume = 0f;
                            aLoader.nightCricketsVolume = 0f;
                            aLoader.desertCricketsVolume = 0f;
                            aLoader.ugAmbienceVolume = 0f;
                            aLoader.crimsonRumblesVolume = 0f;
                            aLoader.snowDayVolume = 0f;
                            aLoader.snowNightVolume = 0f;
                            aLoader.corruptionRoarsVolume = 0f;
                            aLoader.dayJungleVolume = 0f;
                            aLoader.nightJungleVolume = 0f;
                            aLoader.hellRumbleVolume = 0f;
                            aLoader.morningCricketsVolume = 0f;

                            aLoader.eveningCricketsVolume = Ambience.Instance.eveningCricketsVolume == 1f ? 0f : 1f;
                            SoundEngine.PlaySound(Ambience.Instance.eveningCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        },
                        ref subButtonScale7, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.ForestEvening + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "forest_evening.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );

                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 320, goodScreenFloat + 4),
                        Ambience.Instance.nightCricketsVolume == 1f ? "Night (On)" : "Night",
                        delegate
                        {
                            aLoader.dayCricketsVolume = 0f;
                            aLoader.eveningCricketsVolume = 0f;
                            aLoader.desertCricketsVolume = 0f;
                            aLoader.ugAmbienceVolume = 0f;
                            aLoader.crimsonRumblesVolume = 0f;
                            aLoader.snowDayVolume = 0f;
                            aLoader.snowNightVolume = 0f;
                            aLoader.corruptionRoarsVolume = 0f;
                            aLoader.dayJungleVolume = 0f;
                            aLoader.nightJungleVolume = 0f;
                            aLoader.hellRumbleVolume = 0f;
                            aLoader.morningCricketsVolume = 0f;

                            aLoader.nightCricketsVolume = Ambience.Instance.nightCricketsVolume == 1f ? 0f : 1f;
                            SoundEngine.PlaySound(Ambience.Instance.nightCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        },
                        ref subButtonScale8, default, 0.015f, delegate { }, default, defaultAlpha, false,
                        delegate
                        {
                            byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.ForestNight + ".ogg");

                            Directory.CreateDirectory(path);
                            if (recievedFileBytes != null)
                                File.WriteAllBytes(Path.Combine(path, "forest_night.ogg"), recievedFileBytes);
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                }
                DrawOpens();
                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat),
                    "Forest",
                    delegate
                    {
                        SoundEngine.PlaySound(fOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        fOpen = !fOpen;
                    },
                    ref buttonScale1, default, 0.015f, delegate { }, fOpen ? Main.highVersionColor : default, defaultAlpha, false);

                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY),
                    Ambience.Instance.desertCricketsVolume == 1f ? "Desert (On)" : "Desert",
                    delegate
                    {
                        aLoader.dayCricketsVolume = 0f;
                        aLoader.nightCricketsVolume = 0f;
                        aLoader.eveningCricketsVolume = 0f;
                        aLoader.ugAmbienceVolume = 0f;
                        aLoader.crimsonRumblesVolume = 0f;
                        aLoader.snowDayVolume = 0f;
                        aLoader.snowNightVolume = 0f;
                        aLoader.corruptionRoarsVolume = 0f;
                        aLoader.dayJungleVolume = 0f;
                        aLoader.nightJungleVolume = 0f;
                        aLoader.hellRumbleVolume = 0f;
                        aLoader.rainVolume = 0f;
                        aLoader.morningCricketsVolume = 0f;

                        Ambience.Instance.desertCricketsVolume = Ambience.Instance.desertCricketsVolume == 1f ? 0f : 1f;
                        SoundEngine.PlaySound(Ambience.Instance.desertCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                    },
                    ref buttonScale2, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.Desert + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "desert_crickets.ogg"), recievedFileBytes);
                        else
                        {
                            SoundEngine.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 2),
                    "Snow",
                    delegate
                    {
                        SoundEngine.PlaySound(sOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        sOpen = !sOpen;
                    },
                    ref buttonScale3, default, 0.015f, delegate { }, sOpen ? Main.highVersionColor : default, defaultAlpha, false);

                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 3),
                    "Jungle",
                    delegate
                    {
                        SoundEngine.PlaySound(jOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        jOpen = !jOpen;
                    },
                    ref buttonScale4, default, 0.015f, delegate { }, jOpen ? Main.highVersionColor : default, defaultAlpha, false);

                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 4),
                    Ambience.Instance.beachWavesVolume == 1f ? "Ocean (On)" : "Ocean",
                    delegate
                    {
                        aLoader.ugAmbienceVolume = 0f;
                        aLoader.crimsonRumblesVolume = 0f;
                        aLoader.corruptionRoarsVolume = 0f;
                        aLoader.dayJungleVolume = 0f;
                        aLoader.nightJungleVolume = 0f;
                        aLoader.rainVolume = 0f;
                        aLoader.morningCricketsVolume = 0f;
                        SoundEngine.PlaySound(Ambience.Instance.beachWavesVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        Ambience.Instance.beachWavesVolume = Ambience.Instance.beachWavesVolume == 1f ? 0f : 1f;
                    },
                    ref buttonScale5, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.Beach + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "beach_waves.ogg"), recievedFileBytes);
                        else
                        {
                            SoundEngine.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );
                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 5),
                    "Evils",
                    delegate
                    {
                        SoundEngine.PlaySound(eOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        eOpen = !eOpen;
                    },
                    ref buttonScale6, default, 0.015f, delegate { }, eOpen ? Main.highVersionColor : default, defaultAlpha, false);

                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 6),
                    Ambience.Instance.crackleVolume == 1f ? "Campfire (On)" : "Campfire",
                    delegate
                    {
                        SoundEngine.PlaySound(Ambience.Instance.crackleVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        Ambience.Instance.crackleVolume = Ambience.Instance.crackleVolume == 1f ? 0f : 1f;
                    },
                    ref buttonScale7, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.Campfire + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "campfire_crackle.ogg"), recievedFileBytes);
                        else
                        {
                            SoundEngine.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 7),
                    Ambience.Instance.ugAmbienceVolume == 1f ? "Cavern Layer (On)" : "Cavern Layer",
                    delegate
                    {
                        aLoader.dayCricketsVolume = 0f;
                        aLoader.nightCricketsVolume = 0f;
                        aLoader.eveningCricketsVolume = 0f;
                        aLoader.desertCricketsVolume = 0f;
                        aLoader.snowDayVolume = 0f;
                        aLoader.snowNightVolume = 0f;
                        aLoader.dayJungleVolume = 0f;
                        aLoader.nightJungleVolume = 0f;
                        aLoader.beachWavesVolume = 0f;
                        aLoader.hellRumbleVolume = 0f;
                        aLoader.rainVolume = 0f;
                        aLoader.morningCricketsVolume = 0f;
                        aLoader.breezeVolume = 0f;
                        SoundEngine.PlaySound(Ambience.Instance.ugAmbienceVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        Ambience.Instance.ugAmbienceVolume = Ambience.Instance.ugAmbienceVolume == 1f ? 0f : 1f;
                    },
                    ref buttonScale8, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.CavernLayer + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "ug_drip.ogg"), recievedFileBytes);
                        else
                        {
                            SoundEngine.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 8),
                    Ambience.Instance.hellRumbleVolume == 1f ? "The Underworld (On)" : "The Underworld",
                    delegate
                    {
                        aLoader.dayCricketsVolume = 0f;
                        aLoader.nightCricketsVolume = 0f;
                        aLoader.eveningCricketsVolume = 0f;
                        aLoader.desertCricketsVolume = 0f;
                        aLoader.ugAmbienceVolume = 0f;
                        aLoader.crimsonRumblesVolume = 0f;
                        aLoader.snowDayVolume = 0f;
                        aLoader.snowNightVolume = 0f;
                        aLoader.corruptionRoarsVolume = 0f;
                        aLoader.dayJungleVolume = 0f;
                        aLoader.nightJungleVolume = 0f;
                        aLoader.beachWavesVolume = 0f;
                        aLoader.rainVolume = 0f;
                        aLoader.morningCricketsVolume = 0f;
                        aLoader.breezeVolume = 0f;
                        SoundEngine.PlaySound(Ambience.Instance.hellRumbleVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        Ambience.Instance.hellRumbleVolume = Ambience.Instance.hellRumbleVolume == 1f ? 0f : 1f;
                    },
                    ref buttonScale9, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.HellLayer + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "hell_rumbles.ogg"), recievedFileBytes);
                        else
                        {
                            SoundEngine.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 9),
                    Ambience.Instance.breezeVolume == 1f ? "Breeze (On)" : "Breeze",
                    delegate
                    {
                        aLoader.ugAmbienceVolume = 0f;
                        aLoader.hellRumbleVolume = 0f;
                        SoundEngine.PlaySound(Ambience.Instance.breezeVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        Ambience.Instance.breezeVolume = Ambience.Instance.breezeVolume == 1f ? 0f : 1f;
                    },
                    ref buttonScale10, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.Breeze + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "breeze.ogg"), recievedFileBytes);
                        else
                        {
                            SoundEngine.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 10), "Stop All",
                    delegate
                    {
                        SoundEngine.PlaySound(SoundID.MenuClose);
                        HaltAllMenuAmbient();
                    },
                    ref buttonScale11, default, 0.015f, delegate { }, default, defaultAlpha, false);
                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 12), "Back",
                    delegate
                    {
                        Main.menuMode = 0;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    },
                    ref backButtonScale, default, 0.015f, delegate { }, default, defaultAlpha, false);
                #endregion
            }
            if (Main.menuMode == 999 || Main.menuMode == 1001)
            {
                string pathToPack = Path.Combine(ModLoader.ModPath, "TAConfig", "ta_secretconfig.txt");
                bool reloadNeeded = false;
                if (!Directory.Exists(Path.Combine(ModLoader.ModPath, "TAConfig")))
                    Directory.CreateDirectory(Path.Combine(ModLoader.ModPath, "TAConfig"));
                if (File.Exists(pathToPack))
                {
                    string idkWhatToNameThis = soundPack_whoAmI > -1 ? GetSubFolders(Ambience.INTERNAL_CombinedPath)[soundPack_whoAmI] : Ambience.INTERNAL_CombinedPath;
                    var lines = File.ReadAllLines(pathToPack);

                    var path1 = lines.First(line => line.Contains('/') || line.Contains('\\'));
                    if (path1 != idkWhatToNameThis)
                        reloadNeeded = true;
                }
                string displayPack = $"Current Sound Pack: {(soundPack_whoAmI > -1 ? GetSubFolders(Ambience.INTERNAL_CombinedPath, true)[soundPack_whoAmI] : "Default")}" + (reloadNeeded ? " (Reload Needed!)" : string.Empty);

                Vector2 orig = FontAssets.MouseText.Value.MeasureString(displayPack) / 2;

                Utils.DrawBorderString(Main.spriteBatch, displayPack, new Vector2(Main.screenWidth / 2, 225) - orig, Color.White);
            }
            // separated into cleaner cuts
            if (Main.menuMode == 999)
            {
                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + offY * 11), "Choose Sound Pack",
                    delegate { SoundEngine.PlaySound(SoundID.MenuOpen); Main.menuMode = 1001; },
                    ref buttonScale12, default, 0.015f, delegate { }, default, defaultAlpha, false);
            }
            if (Main.menuMode == 1001)
            {
                int count = 0;
                foreach (var name in GetSubFolders(Ambience.INTERNAL_CombinedPath, true)) {
                    try {
                        var flt = ButtonScalesPerDir[count];
                        GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + (offY * count)), name,
                            delegate {
                                SoundEngine.PlaySound(SoundID.MenuOpen);
                                mod.Logger.Info($"Swapped or tried swapping soundpack. {GetSubFolders(Path.Combine(ModLoader.ModPath, "TASoundReplace"))[count] + $" ({GetSubFolders(Path.Combine(ModLoader.ModPath, "TASoundReplace"), true)[count]})" ?? "Was null, failed."}");
                                Ambience.SFXReplacePath = GetSubFolders(Path.Combine(ModLoader.ModPath, "TASoundReplace"))[count];
                                soundPack_whoAmI = count;
                            },
                            ref flt, default, 0.015f, delegate { }, default, defaultAlpha, false);

                        count++;
                    }
                    catch { }
                }
                var flot = 0.5f;
                var flot2 = 0.5f;
                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + (offY * (count))), "Default",
                    delegate 
                    { 
                        SoundEngine.PlaySound(SoundID.MenuClose); Ambience.SFXReplacePath = Ambience.INTERNAL_CombinedPath;
                        soundPack_whoAmI = -1;
                        mod.Logger.Info($"Swapped soundpack to default.");
                    },
                    ref flot, default, 0.015f, delegate { }, default, defaultAlpha, false);
                GeneralHelpers.Instance.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, goodScreenFloat + (offY * (count + 1))), "Back",
                    delegate { SoundEngine.PlaySound(SoundID.MenuClose); Main.menuMode = 999; },
                    ref flot2, default, 0.015f, delegate { }, default, defaultAlpha, false);
            }
        }

        public static string[] GetSubFolders(string root, bool getName = false)
        {
            var rootGotten = Directory.GetDirectories(root);

            string[] names = new string[rootGotten.Length];

            int curTotal = 0;
            if (getName)
            {
                foreach (var dir in rootGotten)
                {
                    var dirInfo = new DirectoryInfo(dir);

                    names[curTotal] = dirInfo.Name;
                    curTotal++;
                }
            }
            return !getName ? rootGotten : names;
        }

        private static void Main_DrawMenu(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            // orig(self, gameTime);
            //Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            DrawAmbienceMenu();
            Mod mod = ModContent.GetInstance<TerrariaAmbience>();

            posX = MathHelper.Clamp(posX, -325, -16);
            var sb = Main.spriteBatch;
            string viewPost = "Visit the Terraria Ambience";
            string server = $"Discord Server";

            var click2Activate = new Rectangle((int)posX + 330, (int)posY, 12, 20);
            if (Main.menuMode == 0)
            {
                var texture = mod.Assets.Request<Texture2D>("Content/UI/UIButtonRight", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Main.spriteBatch.SafeDraw(texture,
                    new Vector2(posX + 330, posY), null, Color.White, 0f, Vector2.Zero, 0.6f,
                    !active ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1f);
            }
            var rect = new Rectangle((int)posX + 223, (int)posY, (int)(FontAssets.DeathText.Value.MeasureString(server).X * 0.35f), 
                (int)(FontAssets.DeathText.Value.MeasureString(server).Y * 0.25f));
            // Main.spriteBatch.Draw(Main.magicPixel, click2Activate, Color.White * 0.35f);
            bool hovering = rect.Contains(Main.MouseScreen.ToPoint());
            bool hoverAct = click2Activate.Contains(Main.MouseScreen.ToPoint());

            if (Main.menuMode == 0)
            {
                if (hoverAct)
                {
                    if (Main.mouseRight)
                    {
                        if (Main.MouseScreen.Y < Main.screenHeight && Main.MouseScreen.Y > 0)
                        {
                            posY = Main.MouseScreen.Y - 10;
                        }
                    }
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        active = !active;
                    }
                }
                if (hovering)
                {
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        if (active)
                        {
                            Process.Start(new ProcessStartInfo("https://discord.gg/pT2BzSG")
                            {
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            posX += active ? 20f : -20f;

            if (Main.menuMode == 0) {
                if (TerrariaAmbience.UserHasSteelSeries) {
                    Vector2 scale = new Vector2(0.5f) * new Vector2(Main.screenWidth / 1920f, Main.screenHeight / 1080f);
                    var warning = "Warning! You have a SteelSeries headset.\nIf you are using the [c/00FF00:Sonar] audio device, [c/FF0000:DISABLE IT] and [c/FF0000:RESTART].\nOtherwise, it will crash the game due to audio issues.";
                    ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, warning, new Vector2(10, 60), Color.LightGray, 0f, Vector2.Zero, scale, 0, 1);
                    ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, viewPost, new Vector2(posX, posY), Color.LightGray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);
                    ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, server, new Vector2(posX + (int)(FontAssets.DeathText.Value.MeasureString(viewPost).X * 0.35f) + 10, posY), hovering ? Color.White : Color.Gray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);
                }
            }

            GeneralHelpers.MSOld = GeneralHelpers.MSNew;
            orig(self, gameTime);
            //Main.spriteBatch.End();
        }
        #endregion
    }
}

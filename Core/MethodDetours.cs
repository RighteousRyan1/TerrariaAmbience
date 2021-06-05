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


namespace TerrariaAmbience.Core
{
    internal class MethodDetours
    {
        public static void DetourAll()
        {
            ContentInstance.Register(new GeneralHelpers());

            On.Terraria.Main.DrawMenu += Main_DrawMenu;
            On.Terraria.Main.DrawInterface_30_Hotbar += Main_DrawInterface_30_Hotbar;
            On.Terraria.IngameOptions.Draw += IngameOptions_Draw;
            On.Terraria.IngameOptions.DrawRightSide += IngameOptions_DrawRightSide;
            On.Terraria.IngameOptions.DrawLeftSide += IngameOptions_DrawLeftSide;
            MenuDetours.On_AddMenuButtons += MenuDetours_On_AddMenuButtons;

            active = true;
            posY = 4;
        }

        private static void MenuDetours_On_AddMenuButtons(MenuDetours.Orig_AddMenuButtons orig, Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
        {
            Helpers.GeneralHelpers.AddMainMenuButton("Ambience Menu", delegate { Main.menuMode = 999; }, selectedMenu, buttonNames, ref buttonIndex, ref numButtons);
            orig(main, selectedMenu, buttonNames, buttonScales, ref offY, ref spacing, ref buttonIndex, ref numButtons);
        }

        private static bool IngameOptions_DrawLeftSide(On.Terraria.IngameOptions.orig_DrawLeftSide orig, SpriteBatch sb, string txt, int i, Vector2 anchor, Vector2 offset, float[] scales, float minscale, float maxscale, float scalespeed)
        {
            if (i == 0)
            {
                sb.DrawString(Main.fontMouseText, $"Hold Add or Subtract (or period or comma) to change the volume of Terraria Ambience!\nPress the left or right arrow to change what you modify.", new Vector2(8, 8), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1f);
            }
            return orig(sb, txt, i, anchor, offset, scales, minscale, maxscale, scalespeed);
        }

        private static bool oldHover;
        private static bool hovering;
        private static bool _mode;
        private static bool IngameOptions_DrawRightSide(On.Terraria.IngameOptions.orig_DrawRightSide orig, SpriteBatch sb, string txt, int i, Vector2 anchor, Vector2 offset, float scale, float colorScale, Color over)
        {
            if (Main.keyState.IsKeyDown(Keys.Add) || Main.keyState.IsKeyDown(Keys.OemPeriod))
            {
                if (!_mode)
                    Ambience.TAAmbient += 0.025f;
                else
                    Ambience.fStepsVol += 0.025f;
            }
            if (Main.keyState.IsKeyDown(Keys.Subtract) || Main.keyState.IsKeyDown(Keys.OemComma))
            {
                if (!_mode)
                    Ambience.TAAmbient -= 0.025f;
                else
                    Ambience.fStepsVol -= 0.025f;
            }

            if (Main.keyState.IsKeyDown(Keys.Right) && Main.oldKeyState.IsKeyUp(Keys.Right))
            {
                Main.PlaySound(SoundID.MenuTick);
                _mode = true;
            }
            else if (Main.keyState.IsKeyDown(Keys.Left) && Main.oldKeyState.IsKeyUp(Keys.Left))
            {
                Main.PlaySound(SoundID.MenuTick);
                _mode = false;
            }

            Rectangle hoverPos = new Rectangle((int)anchor.X - 65, (int)anchor.Y + 119, 275, 15);
            if (i == 14)
            {
                hovering = hoverPos.Contains(Main.MouseScreen.ToPoint());
            }

            var svol = !_mode ? (float)Math.Round(Ambience.TAAmbient) : (float)Math.Round(Ambience.fStepsVol);
            svol = MathHelper.Clamp(svol, 0f, 100f);
            Ambience.TAAmbient = MathHelper.Clamp(Ambience.TAAmbient, 0f, 100f);
            Ambience.fStepsVol = MathHelper.Clamp(Ambience.fStepsVol, 0f, 100f);
            string percent = !_mode ? $"TA Ambient: {svol}%" : $"Footsteps: {svol}%";
            if (!oldHover && hovering)
            {
                Main.PlaySound(SoundID.MenuTick);
            }
            var center = (Main.fontMouseText.MeasureString(percent) * Main.UIScale) / 2; 
            if (i == 14 && IngameOptions.category == 0)
            {
                orig(sb, percent, i, anchor, offset, scale, colorScale, over);
            }

            oldHover = hovering;

            if (IngameOptions.category == 2)
            {
                if (i == 3)
                {
                    if (Main.FrameSkipMode == 2)
                    {
                        txt = "Frame Skip Subtle (WARNING)";
                    }
                }
            }
            return orig(sb, txt, i, anchor, offset, scale, colorScale, over);
        }

        private static void IngameOptions_Draw(On.Terraria.IngameOptions.orig_Draw orig, Main mainInstance, SpriteBatch sb)
        {
            orig(mainInstance, sb);
        }

        private static Vector2 drawPos;
        private static string displayable;
        private static void Main_DrawInterface_30_Hotbar(On.Terraria.Main.orig_DrawInterface_30_Hotbar orig, Main self)
        {
            orig(self);

            // Main.spriteBatch.Begin();
            var loader = Ambience.Instance;

            var aPlayer = Main.player[Main.myPlayer].GetModPlayer<FootstepsPlayer>();
            if (aPlayer.soundInstanceSnowStep != null && aPlayer.soundInstanceWoodStep != null && aPlayer.soundInstanceStoneStep != null && aPlayer.soundInstanceGrassStep != null && aPlayer.soundInstanceSandStep != null)
            {
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
                    + $"\n{Ambience.Rain.Name}: {Ambience.RainInstance.Volume}"
                    + $"\n{Ambience.Breeze.Name}: {Ambience.BreezeInstance.Volume}"
                    + $"\nFootsteps:\nGrass: {aPlayer.soundInstanceGrassStep.State}"
                    + $"\nStone: {aPlayer.soundInstanceStoneStep.State}"
                    + $"\nWood: {aPlayer.soundInstanceWoodStep.State}"
                    + $"\nSnow: {aPlayer.soundInstanceSnowStep.State}"
                    + $"\nSand: {aPlayer.soundInstanceSandStep.State}"
                    + $"\nTiles Near -> {Main.LocalPlayer.TilesAround(20, true)}";
            }

            if (ModContent.GetInstance<AmbientConfigClient>().volVals)
            {
                if (Main.playerInventory && (Main.mapStyle == 0 || Main.mapStyle == 2))
                {
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 2, 175);
                }
                if (Main.mapStyle == 1 && Main.playerInventory)
                {
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 2, 375);
                }
                if (Main.mapStyle == 1 && !Main.playerInventory)
                {
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 3, 375);
                }
                if ((Main.mapStyle == 0 || Main.mapStyle == 2) && !Main.playerInventory)
                {
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 3, 175);
                }

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, 
                    Main.fontDeathText, 
                    displayable != default ? displayable : "Sounds not valid", 
                    position: drawPos, 
                    Color.LightGray, 
                    0f, 
                    origin: Vector2.Zero, 
                    baseScale: new Vector2(0.25f, 0.25f), 
                    -1, 
                    1);
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

        private static int unequalDisplayTimer;
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

        private static float supposedMousePosY;
        public static void DrawAmbienceMenu()
        {
            string genericError = "recievedFileBytes was null or a custom ambience track is loaded and/or replaced a normal one. Could not save file!";
            string info = "<< INFORMATION >>\n\nThese sounds are under the PUBLIC DOMAIN / CREATIVE COMMONS.\n" +
                                "Since these sounds were made with various other creative commons sounds, they cannot be used under the Attribution license.\n" +
                                "But please, if you use them, be sure to give credit to the mod and Tika, the person behind the sounds of this mod.\n\n<< This file was generated by Terraria Ambience, a Terraria mod supported by tModLoader >>";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\TA Ambient";
            Mod mod = ModContent.GetInstance<TerrariaAmbience>();
            // string thing = $"help";
            // Terraria.Utils.DrawBorderString(Main.spriteBatch, $"{thing}", Main.MouseScreen + new Vector2(25, 25), Color.White);
            if (Main.menuMode == 999)
            {
                Rectangle toPlay = new Rectangle(208, 74, 26, 16);
                Rectangle toOpen = new Rectangle(40, 96, 26, 16);
                Rectangle toOpen2 = new Rectangle(290, 118, 26, 16);
                Terraria.Utils.DrawBorderString(Main.spriteBatch, $"Left click the buttons to listen to the various ambience tracks from this mod!\nSome have different sounds based on the time of day.\nRight click the buttons to download the tracks to your desktop!\nIf it cannot save, you will hear\nClick      to view the folder the sounds export to.\nFor sound replacements (soundpacks), click\nTo reload all sounds, press the R key.", new Vector2(6, 6), Color.White, 0.8f);
                Terraria.Utils.DrawBorderString(Main.spriteBatch, $"[c/FFFF00:this].", new Vector2(208, 74), Color.White, 0.8f);
                for (int u = 0; u < 3; u++)
                {
                    var pos = u == 1 ? new Vector2(40, 96) : new Vector2(290, 118);
                    Terraria.Utils.DrawBorderString(Main.spriteBatch, u == 2 ? $"[c/FFFF00:here]." : "[c/FFFF00:here]", pos, Color.White, 0.8f);
                }

                if (toPlay.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.PlaySound(SoundID.Unlock);
                }
                if (toOpen2.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.PlaySound(SoundID.MenuOpen);
                    System.Diagnostics.Process.Start(Ambience.SFXReplacePath);
                }
                if (toOpen.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.PlaySound(SoundID.MenuOpen);
					if (Directory.Exists(path))
						System.Diagnostics.Process.Start(path);
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
                    Main.PlaySound(SoundID.MenuOpen);
                    GeneralHelpers.ReloadMods();
                }

                int offY = 42;
                float defaultAlpha = 0.9f;
                var aLoader = Ambience.Instance;
                void DrawOpens()
                {
                    if (eOpen)
                    {
                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 180, (Main.screenHeight * 0.25f) + offY * 5),
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
                            Main.PlaySound(Ambience.Instance.crimsonRumblesVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);

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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        });

                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 180, (Main.screenHeight * 0.25f) + offY * 5),
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
                            Main.PlaySound(Ambience.Instance.corruptionRoarsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);

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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                    if (jOpen)
                    {
                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 140, (Main.screenHeight * 0.25f) + offY * 3),
                        Ambience.Instance.dayJungleVolume == 1f ? "Day (On)" : "Day",
                        delegate 
                        {
                            Ambience.Instance.nightJungleVolume = 0f;
                            Main.PlaySound(Ambience.Instance.nightJungleVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);

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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        });

                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 140, (Main.screenHeight * 0.25f) + offY * 3),
                        Ambience.Instance.nightJungleVolume == 1f ? "Night (On)" : "Night",
                        delegate
                        {
                            Ambience.Instance.dayJungleVolume = 0f;
                            Main.PlaySound(Ambience.Instance.nightJungleVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);

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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                    if (sOpen)
                    {
                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 130, Main.screenHeight * 0.25f + offY * 2),
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
                            Main.PlaySound(Ambience.Instance.snowDayVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );

                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 130, Main.screenHeight * 0.25f + offY * 2),
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

                            Main.PlaySound(Ambience.Instance.snowNightVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                    if (fOpen)
                    {
                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 320, Main.screenHeight * 0.25f),
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
                            Main.PlaySound(Ambience.Instance.morningCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );

                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 150, Main.screenHeight * 0.25f + 2),
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
                            Main.PlaySound(Ambience.Instance.dayCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 150, Main.screenHeight * 0.25f + 3),
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
                            Main.PlaySound(Ambience.Instance.eveningCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );

                        GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 320, Main.screenHeight * 0.25f + 4),
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
                            Main.PlaySound(Ambience.Instance.nightCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
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
                                Main.PlaySound(SoundID.Unlock);
                                mod.Logger.Error(genericError);
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                }

                DrawOpens();

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f), 
                    "Forest",
                    delegate 
                    {
                        Main.PlaySound(fOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        fOpen = !fOpen;
                    }, 
                    ref buttonScale1, default, 0.015f, delegate { }, fOpen ? Main.highVersionColor : default, defaultAlpha, false);

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY),
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
                        Main.PlaySound(Ambience.Instance.desertCricketsVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
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
                            Main.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 2), 
                    "Snow",
                    delegate 
                    {
                        Main.PlaySound(sOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        sOpen = !sOpen;
                    }, 
                    ref buttonScale3, default, 0.015f, delegate { }, sOpen ? Main.highVersionColor : default, defaultAlpha, false);

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 3), 
                    "Jungle",
                    delegate 
                    {
                        Main.PlaySound(jOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        jOpen = !jOpen;
                    }, 
                    ref buttonScale4, default, 0.015f, delegate { }, jOpen ? Main.highVersionColor : default, defaultAlpha, false);

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 4),
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
                        Main.PlaySound(Ambience.Instance.beachWavesVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
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
                            Main.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );
                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 5),
                    "Evils",
                    delegate
                    {
                        Main.PlaySound(eOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        eOpen = !eOpen;
                    },
                    ref buttonScale11, default, 0.015f, delegate { }, eOpen ? Main.highVersionColor : default, defaultAlpha, false);

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 6), 
                    Ambience.Instance.crackleVolume == 1f ? "Campfire (On)" : "Campfire",
                    delegate 
                    { 
                        Main.PlaySound(Ambience.Instance.crackleVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        Ambience.Instance.crackleVolume = Ambience.Instance.crackleVolume == 1f ? 0f : 1f;
                    }, 
                    ref buttonScale6, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.Campfire + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "campfire_crackle.ogg"), recievedFileBytes);
                        else
                        {
                            Main.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 7), 
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
                        Main.PlaySound(Ambience.Instance.ugAmbienceVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen); 
                        Ambience.Instance.ugAmbienceVolume = Ambience.Instance.ugAmbienceVolume == 1f ? 0f : 1f; 
                    }, 
                    ref buttonScale7, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.CavernLayer + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "ug_drip.ogg"), recievedFileBytes);
                        else
                        {
                            Main.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 8), 
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
                        Main.PlaySound(Ambience.Instance.hellRumbleVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen); 
                        Ambience.Instance.hellRumbleVolume = Ambience.Instance.hellRumbleVolume == 1f ? 0f : 1f; 
                    }, 
                    ref buttonScale8, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.HellLayer + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "hell_rumbles.ogg"), recievedFileBytes);
                        else
                        {
                            Main.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 9), 
                    Ambience.Instance.breezeVolume == 1f ? "Breeze (On)" : "Breeze",
                    delegate 
                    {
                        aLoader.ugAmbienceVolume = 0f;
                        aLoader.hellRumbleVolume = 0f;
                        Main.PlaySound(Ambience.Instance.breezeVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen); 
                        Ambience.Instance.breezeVolume = Ambience.Instance.breezeVolume == 1f ? 0f : 1f; 
                    }, 
                    ref buttonScale9, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.Breeze + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "breeze.ogg"), recievedFileBytes);
                        else
                        {
                            Main.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );
                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 10),
                    Ambience.Instance.rainVolume == 1f ? "Rain (On)" : "Rain",
                    delegate
                    {
                        aLoader.ugAmbienceVolume = 0f;
                        aLoader.hellRumbleVolume = 0f;
                        aLoader.snowDayVolume = 0f;
                        aLoader.snowNightVolume = 0f;
                        aLoader.desertCricketsVolume = 0f;
                        Main.PlaySound(Ambience.Instance.rainVolume == 1f ? SoundID.MenuClose : SoundID.MenuOpen);
                        Ambience.Instance.rainVolume = Ambience.Instance.rainVolume == 1f ? 0f : 1f;
                    },
                    ref buttonScale12, default, 0.015f, delegate { }, default, defaultAlpha, false,
                    delegate
                    {
                        byte[] recievedFileBytes = mod.GetFileBytes(Ambience.Directories.Rain + ".ogg");

                        Directory.CreateDirectory(path);
                        if (recievedFileBytes != null)
                            File.WriteAllBytes(Path.Combine(path, "rain_new.ogg"), recievedFileBytes);
                        else
                        {
                            Main.PlaySound(SoundID.Unlock);
                            mod.Logger.Error(genericError);
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 11), "Stop All",
                    delegate 
                    { 
                        Main.PlaySound(SoundID.MenuClose); 
                        HaltAllMenuAmbient(); 
                    }, 
                    ref buttonScale10, default, 0.015f, delegate { }, default, defaultAlpha, false);
                GeneralHelpers.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 12), "Back",
                    delegate 
                    { 
                        Main.menuMode = 0; 
                        Main.PlaySound(SoundID.MenuClose); 
                    }, 
                    ref backButtonScale, default, 0.015f, delegate { }, default, defaultAlpha, false);

                if (unequalDisplayTimer == 1)
                {
                    Main.PlaySound(SoundID.MenuTick);
                }
                unequalDisplayTimer--;
            }
        }
        private static void Main_DrawMenu(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            DrawAmbienceMenu();
            Mod mod = ModContent.GetInstance<TerrariaAmbience>();

            posX = MathHelper.Clamp(posX, -450, -16);
            var sb = Main.spriteBatch;
            string viewPost = "View the Terraria Ambience forums post here:";
            string forums = $"Forums Post";

            var click2Activate = new Rectangle((int)posX + 455, (int)posY, 12, 20);

            if (Main.menuMode == 0 && mod.TextureExists("Content/UI/UIButtonRight")) Main.spriteBatch.SafeDraw(mod.GetTexture("Content/UI/UIButtonRight"), new Vector2(posX + 455, posY), null, Color.White, 0f, Vector2.Zero, 0.6f, !active ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1f);
            var rect = new Rectangle((int)posX + 350, (int)posY, (int)(Main.fontDeathText.MeasureString(forums).X * 0.35f), (int)(Main.fontDeathText.MeasureString(forums).Y * 0.25f));
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
                        Main.PlaySound(SoundID.MenuTick);
                        active = !active;
                    }
                }
            }
            posX += active ? 20f : -20f;

            if (Main.menuMode == 0) ChatManager.DrawColorCodedStringWithShadow(sb, Main.fontDeathText, viewPost, new Vector2(posX, posY), Color.LightGray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);
            if (Main.menuMode == 0) ChatManager.DrawColorCodedStringWithShadow(sb, Main.fontDeathText, forums, new Vector2(posX + (int)(Main.fontDeathText.MeasureString(viewPost).X * 0.35f) + 10, posY), hovering ? Color.White : Color.Gray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);

            var svol = (float)Math.Round(Ambience.TAAmbient);
            svol = MathHelper.Clamp(svol, 0f, 100f);
            Ambience.TAAmbient = MathHelper.Clamp(Ambience.TAAmbient, 0f, 100f);
            string percent = $"TA Ambient: {svol}%";

            var pos = new Vector2(Main.screenWidth / 2, 435);
            if (Main.menuMode == 26)
            {
                sb.DrawString(Main.fontMouseText, $"Hold Add or Subtract (or period or comma) to change the volume of Terraria Ambience or footsteps volume!", new Vector2(6, 22), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1f);

                ChatManager.DrawColorCodedStringWithShadow(sb, Main.fontDeathText, percent, pos, Color.LightGray, 0f, Main.fontDeathText.MeasureString(percent) / 2, new Vector2(0.6f, 0.6f), 0, 2);

                if (Main.keyState.IsKeyDown(Keys.Add) || Main.keyState.IsKeyDown(Keys.OemPeriod))
                {
                    Ambience.TAAmbient += 0.5f;
                }
                if (Main.keyState.IsKeyDown(Keys.Subtract) || Main.keyState.IsKeyDown(Keys.OemComma))
                {
                    Ambience.TAAmbient -= 0.5f;
                }
            }
            if (hovering)
            {
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (active)
                    {
                        System.Diagnostics.Process.Start("https://forums.terraria.org/index.php?threads/terraria-ambience-mod.104161/");
                    }
                }
            }
            GeneralHelpers.MSOld = GeneralHelpers.MSNew;
            orig(self, gameTime);
        }
    }
}

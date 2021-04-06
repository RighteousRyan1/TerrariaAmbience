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

namespace TerrariaAmbience.Core
{
    internal class MethodDetours
    {
        public static void DetourAll()
        {
            ContentInstance.Register(new Utils());

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
            Utils.AddMainMenuButton("Ambience Menu", delegate { Main.menuMode = 999; }, selectedMenu, buttonNames, ref buttonIndex, ref numButtons);
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
                displayable = ModAmbience.modAmbienceList.Count <= 0 && ModAmbience.allAmbiences.Count <= 0 ?
                    $"{loader.BeachWaves.Name}: {loader.BeachWavesInstance.Volume}"
                    + $"\n{loader.CampfireCrackle.Name}: {loader.crackleVolume}"
                    + $"\n{loader.SnowBreezeDay.Name}: {loader.SnowBreezeDayInstance.Volume}"
                    + $"\n{loader.SnowBreezeNight.Name}: {loader.SnowBreezeNightInstance.Volume}"
                    + $"\n{loader.MorningCrickets.Name}: {loader.MorningCricketsInstance.Volume}"
                    + $"\n{loader.DayCrickets.Name}: {loader.DayCricketsInstance.Volume}"
                    + $"\n{loader.NightCrickets.Name}: {loader.NightCricketsInstance.Volume}"
                    + $"\n{loader.EveningCrickets.Name}: {loader.EveningCricketsInstance.Volume}"
                    + $"\n{loader.CavesAmbience.Name}: {loader.CavesAmbienceInstance.Volume}"
                    + $"\n{loader.CrimsonRumbles.Name}: {loader.CrimsonRumblesInstance.Volume}"
                    + $"\n{loader.CorruptionRoars.Name}: {loader.CorruptionRoarsInstance.Volume}"
                    + $"\n{loader.DayJungle.Name}: {loader.DaytimeJungleInstance.Volume}"
                    + $"\n{loader.NightJungle.Name}: {loader.NightJungleInstance.Volume}"
                    + $"\n{loader.DesertAmbience.Name}: {loader.DesertAmbienceInstance.Volume}"
                    + $"\n{loader.HellRumble.Name}: {loader.HellRumbleInstance.Volume}"
                    + $"\n{loader.Rain.Name}: {loader.RainInstance.Volume}"
                    + $"\n{loader.Breeze.Name}: {loader.BreezeInstance.Volume}"
                    + $"\nFootsteps:\nGrass: {aPlayer.soundInstanceGrassStep.State}"
                    + $"\nStone: {aPlayer.soundInstanceStoneStep.State}"
                    + $"\nWood: {aPlayer.soundInstanceWoodStep.State}"
                    + $"\nSnow: {aPlayer.soundInstanceSnowStep.State}"
                    + $"\nSand: {aPlayer.soundInstanceSandStep.State}"
                    + $"\nTiles Near -> {Main.LocalPlayer.TilesAround(20, true)}"
                    :
               displayable = $"{loader.BeachWaves.Name}: {loader.BeachWavesInstance.Volume}"
                    + $"\n{loader.CampfireCrackle.Name}: {loader.crackleVolume}"
                    + $"\n{loader.SnowBreezeDay.Name}: {loader.SnowBreezeDayInstance.Volume}"
                    + $"\n{loader.SnowBreezeNight.Name}: {loader.SnowBreezeNightInstance.Volume}"
                    + $"\n{loader.MorningCrickets.Name}: {loader.MorningCricketsInstance.Volume}"
                    + $"\n{loader.DayCrickets.Name}: {loader.DayCricketsInstance.Volume}"
                    + $"\n{loader.NightCrickets.Name}: {loader.NightCricketsInstance.Volume}"
                    + $"\n{loader.EveningCrickets.Name}: {loader.EveningCricketsInstance.Volume}"
                    + $"\n{loader.CavesAmbience.Name}: {loader.CavesAmbienceInstance.Volume}"
                    + $"\n{loader.CrimsonRumbles.Name}: {loader.CrimsonRumblesInstance.Volume}"
                    + $"\n{loader.CorruptionRoars.Name}: {loader.CorruptionRoarsInstance.Volume}"
                    + $"\n{loader.DayJungle.Name}: {loader.DaytimeJungleInstance}"
                    + $"\n{loader.NightJungle.Name}: {loader.NightJungleInstance.Volume}"
                    + $"\n{loader.DesertAmbience.Name}: {loader.DesertAmbienceInstance.Volume}"
                    + $"\n{loader.HellRumble.Name}: {loader.HellRumbleInstance.Volume}"
                    + $"\n{loader.Rain.Name}: {loader.RainInstance.Volume}"
                    + $"\n{loader.Breeze.Name}: {loader.BreezeInstance.Volume}"
                    + $"\nFootsteps:\nGrass: {aPlayer.soundInstanceGrassStep.State}"
                    + $"\nStone: {aPlayer.soundInstanceStoneStep.State}"
                    + $"\nWood: {aPlayer.soundInstanceWoodStep.State}"
                    + $"\nSnow: {aPlayer.soundInstanceSnowStep.State}"
                    + $"\nTiles Near -> {Main.LocalPlayer.TilesAround(20, true)}"
                    + $"\nSand: {aPlayer.soundInstanceSandStep.State}\nModded Sounds:";


                int index = 0;
                foreach (ModAmbience ambient in ModAmbience.modAmbienceList)
                {
                    if (ambient != null)
                    {
                        if (ModAmbience.modAmbienceList.Count > 0)
                        {
                            index = ModAmbience.allAmbiences.Count;
                            displayable += $"\n{ambient.Name}: {ambient.Volume}";
                        }
                    }
                }
                foreach (ModAmbience ambient in ModAmbience.allAmbiences)
                {
                    if (ambient != null)
                    {
                        if (ModAmbience.allAmbiences.Count > 0)
                        {
                            index = ModAmbience.allAmbiences.Count;
                            displayable = string.Concat(displayable, $"\n{ambient.Name}: {ambient.Volume}");
                        }
                    }
                }
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
        private static float buttonScale13;
        private static float buttonScale14;
        private static float buttonScale15;
        private static float buttonScale16;
        private static float buttonScale17;
        private static float buttonScale18;

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
            string info = "<< INFORMATION >>\n\nThese sounds are under the PUBLIC DOMAIN / CREATIVE COMMONS.\n" +
                                "Since these sounds were made with various other creative commons sounds, they cannot be used under the Attribution license.\n" +
                                "But please, if you use them, be sure to give credit to the mod and Tika, the person behind the sounds of this mod.\n\n<< This file was generated by Terraria Ambience, a Terraria mod supported by tModLoader >>";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\TA Ambient";
            Mod mod = ModContent.GetInstance<TerrariaAmbience>();
            bool KeyPress(Keys key)
            {
                return Main.keyState.IsKeyDown(key) && Main.oldKeyState.IsKeyUp(key);
            }
            float y = 0.1f;
            // Terraria.Utils.DrawBorderString(Main.spriteBatch, $"{Path.Combine(path, "crimson_rumbles.ogg")}", Main.MouseScreen + new Vector2(25, 25), Color.White);
            if (Main.menuMode == 999)
            {
                Rectangle toPlay = new Rectangle(208, 74, 26, 16);
                Rectangle toOpen = new Rectangle(40, 96, 26, 16);
                Terraria.Utils.DrawBorderString(Main.spriteBatch, $"Left click the buttons to listen to the various ambience tracks from this mod!\nSome have different sounds based on the time of day.\nRight click the buttons to download the tracks to your desktop!\nIf it cannot save, you will hear\nClick      to view the folder the sounds export to.", new Vector2(6, 6), Color.White, 0.8f);
                Terraria.Utils.DrawBorderString(Main.spriteBatch, $"[c/FFFF00:this].", new Vector2(208, 74), Color.White, 0.8f);
                Terraria.Utils.DrawBorderString(Main.spriteBatch, $"[c/FFFF00:here]", new Vector2(40, 96), Color.White, 0.8f);

                if (toPlay.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.PlaySound(SoundID.Unlock);
                }
                if (toOpen.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.PlaySound(SoundID.MenuOpen);
                    System.Diagnostics.Process.Start(path);
                }
                if (KeyPress(Keys.Down) || KeyPress(Keys.S))
                {
                    if (supposedMousePosY == 0)
                    {
                        supposedMousePosY = Main.screenHeight * 0.25f;
                    }
                    supposedMousePosY += 42;
                }
                if (KeyPress(Keys.Up) || KeyPress(Keys.W))
                {
                    if (supposedMousePosY == 0)
                    {

                    }
                    supposedMousePosY -= 42;
                }


                int offY = 42;
                float defaultAlpha = 0.9f;
                var aLoader = Ambience.Instance;
                void DrawOpens()
                {
                    if (eOpen)
                    {
                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 180, (Main.screenHeight * 0.25f) + offY * 5),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        });

                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 180, (Main.screenHeight * 0.25f) + offY * 5),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                    if (jOpen)
                    {
                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 140, (Main.screenHeight * 0.25f) + offY * 3),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        });

                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 140, (Main.screenHeight * 0.25f) + offY * 3),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                    if (sOpen)
                    {
                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 130, Main.screenHeight * 0.25f + offY * 2),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );

                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 130, Main.screenHeight * 0.25f + offY * 2),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                    if (fOpen)
                    {
                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 320, Main.screenHeight * 0.25f),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );

                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) - 150, Main.screenHeight * 0.25f + 2),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 150, Main.screenHeight * 0.25f + 3),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );

                        Utils.Utility.CreateSimpleUIButton(new Vector2((Main.screenWidth / 2) + 320, Main.screenHeight * 0.25f + 4),
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
                                mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                            }
                            File.WriteAllText(Path.Combine(path, "README.info"), info);
                        }
                        );
                    }
                }

                DrawOpens();

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f), 
                    "Forest",
                    delegate 
                    {
                        Main.PlaySound(fOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        fOpen = !fOpen;
                    }, 
                    ref buttonScale1, default, 0.015f, delegate { }, fOpen ? Main.highVersionColor : default, defaultAlpha, false);

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY),
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
                            mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 2), 
                    "Snow",
                    delegate 
                    {
                        Main.PlaySound(sOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        sOpen = !sOpen;
                    }, 
                    ref buttonScale3, default, 0.015f, delegate { }, sOpen ? Main.highVersionColor : default, defaultAlpha, false);

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 3), 
                    "Jungle",
                    delegate 
                    {
                        Main.PlaySound(jOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        jOpen = !jOpen;
                    }, 
                    ref buttonScale4, default, 0.015f, delegate { }, jOpen ? Main.highVersionColor : default, defaultAlpha, false);

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 4),
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
                            mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );
                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 5),
                    "Evils",
                    delegate
                    {
                        Main.PlaySound(eOpen ? SoundID.MenuClose : SoundID.MenuOpen);
                        eOpen = !eOpen;
                    },
                    ref buttonScale11, default, 0.015f, delegate { }, eOpen ? Main.highVersionColor : default, defaultAlpha, false);

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 6), 
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
                            mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 7), 
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
                            mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 8), 
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
                            mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 9), 
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
                            mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );
                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 10),
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
                            mod.Logger.Error("recievedFileBytes was null. Could not save file!");
                        }
                        File.WriteAllText(Path.Combine(path, "README.info"), info);
                    }
                    );

                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 11), "Stop All",
                    delegate 
                    { 
                        Main.PlaySound(SoundID.MenuClose); 
                        HaltAllMenuAmbient(); 
                    }, 
                    ref buttonScale10, default, 0.015f, delegate { }, default, defaultAlpha, false);
                Utils.Utility.CreateSimpleUIButton(new Vector2(Main.screenWidth / 2, Main.screenHeight * 0.25f + offY * 12), "Back",
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

            // Echcode.tm
            /*int i = 0;
            int j = 1;
            int z = 0;
            foreach (Mod mod in ModLoader.Mods)
            {
                if (mod != null && mod.TextureExists("icon"))
                {
                    i++;
                    if (((100 * i) + 95) < Main.screenHeight)
                    {
                        Main.spriteBatch.SafeDraw(mod.GetTexture("icon"), new Vector2(80, 100 * i), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontDeathText, mod.DisplayName, new Vector2(120, (100 * i) + 95), Color.LightGray, 0f, Main.fontDeathText.MeasureString(mod.DisplayName) / 2, new Vector2(0.35f, 0.35f), 0, 1);
                    }
                    else
                    {
                        j++;
                        z++;
                        Main.spriteBatch.Draw(mod.GetTexture("icon"), new Vector2(100 * j, (100 * i) - (z * 900)), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontDeathText, mod.DisplayName, new Vector2(120 * j, ((100 * i) + 95) - (z * 900)), Color.LightGray, 0f, Main.fontDeathText.MeasureString(mod.DisplayName) / 2, new Vector2(0.35f, 0.35f), 0, 1);
                    }
                }
            }*/

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
                if (Main.keyState.IsKeyDown(Keys.Subtract) || Main.keyState.IsKeyDown(Keys.OemPeriod))
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
            Utils.MSOld = Utils.MSNew;
            orig(self, gameTime);
        }
    }
}

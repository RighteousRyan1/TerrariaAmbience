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

namespace TerrariaAmbience.Core
{
    internal class MethodDetours
    {
        public static void DetourAll()
        {
            On.Terraria.Main.DrawMenu += Main_DrawMenu;
            On.Terraria.Main.DrawInterface_30_Hotbar += Main_DrawInterface_30_Hotbar;
            On.Terraria.IngameOptions.Draw += IngameOptions_Draw;
            On.Terraria.IngameOptions.DrawRightSide += IngameOptions_DrawRightSide;
            On.Terraria.IngameOptions.DrawLeftSide += IngameOptions_DrawLeftSide;
        }
        private static bool IngameOptions_DrawLeftSide(On.Terraria.IngameOptions.orig_DrawLeftSide orig, SpriteBatch sb, string txt, int i, Vector2 anchor, Vector2 offset, float[] scales, float minscale, float maxscale, float scalespeed)
        {
            string path = $"C://Users//{Environment.UserName}//Documents//My Games//Terraria//ModLoader//Mods//Cache//ta_secretconfig.txt";
            string[] lines = File.ReadAllLines(path);

            lines[0] = "WHAT";


            if (i == 0)
            {
                sb.DrawString(Main.fontMouseText, $"Hold Add or Subtract to change the volume of Terraria Ambience!", new Vector2(8, 8), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1f);
            }
            return orig(sb, txt, i, anchor, offset, scales, minscale, maxscale, scalespeed);
        }

        private static bool oldHover;
        private static bool hovering;
        private static bool IngameOptions_DrawRightSide(On.Terraria.IngameOptions.orig_DrawRightSide orig, SpriteBatch sb, string txt, int i, Vector2 anchor, Vector2 offset, float scale, float colorScale, Color over)
        {
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Add))
            {
                Ambience.TAAmbient += 0.025f;
            }
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Subtract))
            {
                Ambience.TAAmbient -= 0.025f;
            }

            Rectangle hoverPos = new Rectangle((int)anchor.X - 65, (int)anchor.Y + 119, 275, 15);
            if (i == 14)
            {
                hovering = hoverPos.Contains(Main.MouseScreen.ToPoint());
            }

            var svol = (float)System.Math.Round(Ambience.TAAmbient);
            svol = MathHelper.Clamp(svol, 0f, 100f);
            Ambience.TAAmbient = MathHelper.Clamp(Ambience.TAAmbient, 0f, 100f);
            string percent = $"TA Ambient: {svol}%";
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

            var aPlayer = Main.player[Main.myPlayer].GetModPlayer<FootstepsAndAmbiencePlayer>();
            if (aPlayer.soundInstanceSnowStep != null && aPlayer.soundInstanceWoodStep != null && aPlayer.soundInstanceStoneStep != null && aPlayer.soundInstanceGrassStep != null && aPlayer.soundInstanceSandStep != null)
            {
                displayable = ModAmbience.modAmbienceList.Count <= 0 && ModAmbience.allAmbiences.Count <= 0 ? 
                    $"{loader.BeachWaves.Name}: {loader.beachWavesVolume}"
                    + $"\n{loader.CampfireCrackle.Name}: {loader.crackleVolume}"
                    + $"\n{loader.SnowBreezeDay.Name}: {loader.snowDayVolume}"
                    + $"\n{loader.SnowBreezeNight.Name}: {loader.snowNightVolume}"
                    + $"\n{loader.MorningCrickets.Name}: {loader.morningCricketsVolume}"
                    + $"\n{loader.DayCrickets.Name}: {loader.dayCricketsVolume}"
                    + $"\n{loader.NightCrickets.Name}: {loader.nightCricketsVolume}"
                    + $"\n{loader.EveningCrickets.Name}: {loader.eveningCricketsVolume}"
                    + $"\n{loader.CavesAmbience.Name}: {loader.ugAmbienceVolume}"
                    + $"\n{loader.CrimsonRumbles.Name}: {loader.crimsonRumblesVolume}"
                    + $"\n{loader.CorruptionRoars.Name}: {loader.corruptionRoarsVolume}"
                    + $"\n{loader.DayJungle.Name}: {loader.dayJungleVolume}"
                    + $"\n{loader.NightJungle.Name}: {loader.nightJungleVolume}"
                    + $"\n{loader.DesertAmbience.Name}: {loader.desertCricketsVolume}"
                    + $"\n{loader.HellRumble.Name}: {loader.hellRumbleVolume}"
                    + $"\n{loader.Rain.Name}: {loader.rainVolume}"
                    + $"\nFootsteps:\nGrass: {aPlayer.soundInstanceGrassStep.State}"
                    + $"\nStone: {aPlayer.soundInstanceStoneStep.State}"
                    + $"\nWood: {aPlayer.soundInstanceWoodStep.State}"
                    + $"\nSnow: {aPlayer.soundInstanceSnowStep.State}"
                    + $"\nSand: {aPlayer.soundInstanceSandStep.State}"
                    :
               displayable = $"{loader.BeachWaves.Name}: {loader.beachWavesVolume}"
                    + $"\n{loader.CampfireCrackle.Name}: {loader.crackleVolume}"
                    + $"\n{loader.SnowBreezeDay.Name}: {loader.snowDayVolume}"
                    + $"\n{loader.SnowBreezeNight.Name}: {loader.snowNightVolume}"
                    + $"\n{loader.MorningCrickets.Name}: {loader.morningCricketsVolume}"
                    + $"\n{loader.DayCrickets.Name}: {loader.dayCricketsVolume}"
                    + $"\n{loader.NightCrickets.Name}: {loader.nightCricketsVolume}"
                    + $"\n{loader.EveningCrickets.Name}: {loader.eveningCricketsVolume}"
                    + $"\n{loader.CavesAmbience.Name}: {loader.ugAmbienceVolume}"
                    + $"\n{loader.CrimsonRumbles.Name}: {loader.crimsonRumblesVolume}"
                    + $"\n{loader.CorruptionRoars.Name}: {loader.corruptionRoarsVolume}"
                    + $"\n{loader.DayJungle.Name}: {loader.dayJungleVolume}"
                    + $"\n{loader.NightJungle.Name}: {loader.nightJungleVolume}"
                    + $"\n{loader.DesertAmbience.Name}: {loader.desertCricketsVolume}"
                    + $"\n{loader.HellRumble.Name}: {loader.hellRumbleVolume}"
                    + $"\n{loader.Rain.Name}: {loader.rainVolume}"
                    + $"\nFootsteps:\nGrass: {aPlayer.soundInstanceGrassStep.State}"
                    + $"\nStone: {aPlayer.soundInstanceStoneStep.State}"
                    + $"\nWood: {aPlayer.soundInstanceWoodStep.State}"
                    + $"\nSnow: {aPlayer.soundInstanceSnowStep.State}"
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

        private static void Main_DrawMenu(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            var sb = Main.spriteBatch;
            string viewPost = "View the Terraria Ambience forums post here:";
            string forums = "Forums Post";
            var rect = new Rectangle((int)(Main.fontDeathText.MeasureString(viewPost).X * 0.35f) + 10, 4, (int)(Main.fontDeathText.MeasureString(forums).X * 0.35f), (int)(Main.fontDeathText.MeasureString(forums).Y * 0.25f));

            bool hovering = rect.Contains(Main.MouseScreen.ToPoint());


            ChatManager.DrawColorCodedStringWithShadow(sb, Main.fontDeathText, viewPost, new Vector2(4, 4), Color.LightGray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);
            ChatManager.DrawColorCodedStringWithShadow(sb, Main.fontDeathText, forums, new Vector2((int)(Main.fontDeathText.MeasureString(viewPost).X * 0.35f) + 10, 4), hovering ? Color.White : Color.Gray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);

            var svol = (float)System.Math.Round(Ambience.TAAmbient);
            svol = MathHelper.Clamp(svol, 0f, 100f);
            Ambience.TAAmbient = MathHelper.Clamp(Ambience.TAAmbient, 0f, 100f);
            string percent = $"TA Ambient: {svol}%";

            var pos = new Vector2(Main.screenWidth / 2, 435);
            if (Main.menuMode == 26)
            {
                sb.DrawString(Main.fontMouseText, $"Hold Add or Subtract to change the volume of Terraria Ambience!", new Vector2(6, 22), Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1f);

                ChatManager.DrawColorCodedStringWithShadow(sb, Main.fontDeathText, percent, pos, Color.LightGray, 0f, Main.fontDeathText.MeasureString(percent) / 2, new Vector2(0.6f, 0.6f), 0, 2);

                if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Add))
                {
                    Ambience.TAAmbient += 0.5f;
                }
                if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Subtract))
                {
                    Ambience.TAAmbient -= 0.5f;
                }
            }
            if (hovering)
            {
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    System.Diagnostics.Process.Start("https://forums.terraria.org/index.php?threads/terraria-ambience-mod.104161/");
                }
            }
            orig(self, gameTime);
        }
    }
}

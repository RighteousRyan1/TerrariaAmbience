using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Graphics;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework.Input;
using MonoMod.RuntimeDetour.HookGen;
using System.Reflection;
using Terraria.ID;

namespace TerrariaAmbience
{
    public static class MenuDetours
    {
        public delegate void Orig_AddMenuButtons(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons);

        public delegate void Hook_AddMenuButtons(Orig_AddMenuButtons orig, Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons);

        public static event Hook_AddMenuButtons On_AddMenuButtons
        {
            add
            {
                HookEndpointManager.Add<Hook_AddMenuButtons>(typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface").GetMethod("AddMenuButtons", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic), value);
            }
            remove
            {
                HookEndpointManager.Remove<Hook_AddMenuButtons>(typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface").GetMethod("AddMenuButtons", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic), value);
            }
        }
    }
    public class Utils
    {
        public static Utils Utility => ModContent.GetInstance<Utils>();
        public enum AudioFileExtension
        {
            MP3,
            WAV,
            OGG,
            M4A,
            FLAC,
            VOX
        }
        public static void AddMainMenuButton(string text, Action act, int selectedMenu, string[] buttonNames, ref int buttonIndex, ref int numButtons)
        {
            buttonNames[buttonIndex] = text;

            if (selectedMenu == buttonIndex)
            {
                Main.PlaySound(SoundID.MenuOpen);
                act();
            }

            buttonIndex++;
            numButtons++;
        }
        public static MouseState MSNew
        {
            get;
            internal set;
        }
        public static MouseState MSOld
        {
            get;
            internal set;
        }
        public static void ClickHandling()
        {
            MSNew = Mouse.GetState();

        }
        public static void UpdateButtons()
        {
            if (Main.dayTime)
            {
                if (def < 210)
                    def++;
            }
            else
            {
                if (def > 86)
                    def--;
            }
        }
        public static bool ClickEnded(bool leftClick)
        {
            if (leftClick)
            {
                return MSOld.LeftButton == ButtonState.Pressed && MSNew.LeftButton == ButtonState.Released;
            }
            else
            {
                return MSOld.RightButton == ButtonState.Pressed && MSNew.RightButton == ButtonState.Released;
            }
        }
        private static byte def;
        public bool hovering;
        public Rectangle CreateSimpleUIButton(
            Vector2 position,
            string text,
            Action whatToDo,
            ref float useScale,
            Color colorWhenHovered = default,
            float scaleMultiplier = 0.015f,
            Action hoveringToDo = null,
            Color nonHoverColor = default,
            float alpha = 1f,
            bool waitUntilClickEnd = false,
            Action rightClickAction = null)
        {
            if (colorWhenHovered == default)
            {
                colorWhenHovered = Main.highVersionColor;
            }
            if (nonHoverColor == default)
            {
                // nonHoverColor = Main.dayTime ? new Color(200, 200, 200) : new Color(86, 86, 86);
                nonHoverColor = new Color(def, def, def);
            }
            useScale = MathHelper.Clamp(useScale, 0.65f, 0.85f);

            var bounds = Main.fontDeathText.MeasureString(text);
            var rectHoverable = new Rectangle((int)position.X - (int)(bounds.X / 2 * useScale), (int)(position.Y - bounds.Y / 2 + 10), (int)(bounds.X * useScale), (int)bounds.Y - 30);

            hovering = rectHoverable.Contains(Main.MouseScreen.ToPoint());
            // hoverNewNew = !hovering && !hoverOld; // False False
            /*if (!hoverNewNew && hoverOldOld) //  true true, false true, or true false >> false false
            {
                Main.PlaySound(SoundID.MenuTick);
            }*/

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontDeathText, text, position, hovering ? colorWhenHovered * alpha : nonHoverColor * alpha, 0f, bounds / 2, new Vector2(useScale), -1, 2);
            // Main.spriteBatch.Draw(mod.GetTexture("Assets/Debug/WhitePixel"), rectHoverable, Color.White * 0.1f);
            if (!waitUntilClickEnd)
            {
                if (Main.mouseLeft && Main.mouseLeftRelease && hovering && whatToDo != null)
                {
                    whatToDo();
                }
                if (Main.mouseRight && Main.mouseRightRelease && hovering && rightClickAction != null)
                {
                    rightClickAction();
                }
            }
            else
            {
                if (ClickEnded(true) && hovering && whatToDo != null)
                {
                    whatToDo();
                }
            }
            if (hoveringToDo != null)
            {
                if (hovering)
                {
                    hoveringToDo();
                }
            }
            useScale += hovering ? scaleMultiplier : -scaleMultiplier;

            // hoverOld = hovering;
            // hoverOldOld = hoverNewNew;
            return rectHoverable;
        }
        public static bool CheckPlayerArmorSlot(Player player, int slot)
        {
            return !player.armor[slot].IsAir;
        }
        public static Item GetItemFromArmorSlot(Player player, int slot)
        {
            return player.armor[slot];
        }

        public static int GetAllUsedAccSlots(Player player)
        {
            int x = 0;
            foreach (Item item in player.armor)
            {
                if (!item.IsAir)
                {
                    x++;
                }
            }
            return x;
        }
        public static string DisplayAllUsedAccSlots(Player player)
        {
            int i = 0;
            string s = "";
            foreach (Item item in player.armor)
            {
                if (!item.IsAir)
                {
                    i++;
                    s+= $" | {i}:{item.Name}";
                }
            }
            return s == "" ? "No valid slots" : s;  
        }

        public class IDs
        {
            public struct ArmorSlotID
            {
                public const short HeadSlot = 0;
                public const short ChestSlot = 1;
                public const short LegSlot = 2;
                public const short AccSlot1 = 3;
                public const short AccSlot2 = 4;
                public const short AccSlot3 = 4;
                public const short AccSlot4 = 5;
                public const short AccSlot5 = 6;
                
            }
        }
    }
    public static class ExtensionMethods
    {
        public static Mod mod => ModContent.GetInstance<TerrariaAmbience>();
        public static string AppendFileExtension(this string str, Utils.AudioFileExtension extension)
        {
            return $"{str}.{extension.ToString().ToLower()}";
        }
        public static void InitializeSoundEffect(this SoundEffect toInit, ref SoundEffectInstance toBindTo, string path, string name)
        {
            var mod = ModContent.GetInstance<TerrariaAmbience>();
            toInit = mod.GetSound(path);
            toBindTo = toInit.CreateInstance();

            toInit.Name = name;
        }
        public static void ModifyRGB(this Color color, byte amount, bool subract = false)
        {
            void Solve()
            {
                if (!subract)
                {
                    color.R += amount;
                    color.G += amount;
                    color.B += amount;
                }
                else
                {
                    color.R -= amount;
                    color.G -= amount;
                    color.B -= amount;
                }
            }
            Solve();
        }
        public static void SafeDraw(this SpriteBatch spriteBatch, Texture2D tex, Rectangle rect, Color color)
        {
            if (tex != null && !tex.IsDisposed)
            {
                spriteBatch.Draw(tex, rect, color);
            }
        }
        public static void SafeDraw(this SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, Color color)
        {
            if (tex != null && !tex.IsDisposed)
            {
                spriteBatch.Draw(tex, pos, color);
            }
        }
        public static void SafeDraw(this SpriteBatch spriteBatch, Texture2D tex, Rectangle rect, Rectangle? sourceRect, Color color)
        {
            if (tex != null && !tex.IsDisposed)
            {
                spriteBatch.Draw(tex, rect, sourceRect, color);
            }
        }
        public static void SafeDraw(this SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, Rectangle? sourceRect, Color color)
        {
            if (tex != null && !tex.IsDisposed)
            {
                spriteBatch.Draw(tex, pos, sourceRect, color);
            }
        }
        public static void SafeDraw(this SpriteBatch spriteBatch, Texture2D tex, Rectangle rect, Rectangle? sourceRect, Color color, float rot, Vector2 orig, float scale, SpriteEffects effects, float layerDepth)
        {
            if (tex != null && !tex.IsDisposed)
            {
                spriteBatch.Draw(tex, rect, sourceRect, color, rot, orig, effects, layerDepth);
            }
        }
        public static void SafeDraw(this SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, Rectangle? sourceRect, Color color, float rot, Vector2 orig, float scale, SpriteEffects effects, float layerDepth)
        {
            if (tex != null && !tex.IsDisposed)
            {
                spriteBatch.Draw(tex, pos, sourceRect, color, rot, orig, scale, effects, layerDepth);
            }
        }
        public static void SafeDraw(this SpriteBatch spriteBatch, Texture2D tex, Vector2 pos, Rectangle? sourceRect, Color color, float rot, Vector2 orig, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            if (tex != null && !tex.IsDisposed)
            {
                spriteBatch.Draw(tex, pos, sourceRect, color, rot, orig, scale, effects, layerDepth);
            }
        }
        /// <summary>
        /// Checks if this SoundEffectInstance is Playing.
        /// </summary>
        /// <returns>A boolean determining if this sound is Paused.</returns>
        public static bool IsPlaying(this SoundEffectInstance sfx)
        {
            bool isNull = sfx == null;

            if (!isNull)
                return sfx.State == SoundState.Playing;

            return false;
        }
        /// <summary>
        /// Checks if this SoundEffectInstance is Stopped.
        /// </summary>
        /// <returns>A boolean determining if this sound is Stopped.</returns>
        public static bool IsStopped(this SoundEffectInstance sfx)
        {
            bool isNull = sfx == null;

            if (!isNull)
                return sfx.State == SoundState.Stopped;

            return false;
        }
        /// <summary>
        /// Checks if this SoundEffectInstance is Paused.
        /// </summary>
        /// <returns>A boolean determining if this sound is Paused.</returns>
        public static bool IsPaused(this SoundEffectInstance sfx)
        {
            bool isNull = sfx == null;

            if (!isNull)
                return sfx.State == SoundState.Paused;

            return false;
        }
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
            return Main.tile[(int)player.Top.X / 16, (int)(player.Top.Y - 1.4) / 16].liquid > 0;
        }
        /// <summary>
        /// Finds the tiles around the player in a square. Do not put the value too high otherwise FPS will tank.
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="dist">The distance from the player</param>
        /// <returns></returns>
        public static int TilesAround(this Player player, int dist, bool condition)
        {
            int index = 0;
            if (condition)
            {
                for (int i = (int)player.Bottom.X / 16 - dist; i < (int)player.Bottom.X / 16 + dist; i++)
                {
                    for (int j = (int)player.Bottom.Y / 16 - dist; j < (int)player.Bottom.Y / 16 + dist; j++)
                    {
                        Tile tile = Main.tile[i, j];

                        if (tile.active() && tile.collisionType == 1)
                        {
                            index++;
                        }
                    }
                }
            }
            return index;
        }

        /// <summary>
        /// Finds the tiles around the player in a square. Do not put the value too high otherwise FPS will tank.
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="dist">The distance from the player</param>
        /// <param name="tile">An tile to do something with the tiles inside <paramref name="dist"/>.</param>
        /// <returns></returns>
        public static int TilesAround(this Player player, int dist, out Tile tile, bool condition)
        {
            Tile leTile = Main.tile[1, 1];
            int index = 0;
            if (condition)
            {
                for (int i = (int)player.Bottom.X / 16 - dist; i < (int)player.Bottom.X / 16 + dist; i++)
                {
                    for (int j = (int)player.Bottom.Y / 16 - dist; j < (int)player.Bottom.Y / 16 + dist; j++)
                    {
                        leTile = Main.tile[i, j];

                        if (leTile.active() && leTile.collisionType == 1)
                        {
                            index++;
                        }
                    }
                }
            }
            tile = leTile;
            return index;
        }
    }
}

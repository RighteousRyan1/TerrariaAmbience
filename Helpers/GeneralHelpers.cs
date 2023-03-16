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
using TerrariaAmbience.Core;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;

namespace TerrariaAmbience.Helpers
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
    public class GeneralHelpers
    {
        public static SoundStyle SimpleSoundStyle(string path, int variants, SoundType type = SoundType.Sound, float volume = 1f, float pitch = 0f)
        {
            return new(path, variants, type)
            {
                Volume = volume,
                Pitch = pitch,
            };
        }
        public static SoundEffectInstance PlaySound(in SoundStyle style, Vector2? position = null)
        {
            var id = SoundEngine.PlaySound(style, position);
            if (SoundEngine.TryGetActiveSound(id, out var sound))
                return sound.Sound;
            else
                return null;
        }
        public static Rectangle GetRectOf(Vector2 position) => new((int)position.X, (int)position.Y, 1, 1);
        public static Rectangle GetRectOf(Point position) => new(position.X, position.Y, 1, 1);
        public static T GetAssetValue<T>(Mod mod, string path) where T : class
        {
            return mod.Assets.Request<T>(path, AssetRequestMode.ImmediateLoad).Value;
        }
        public static void SavePlayer()
        {
            Player.SavePlayer(Main.ActivePlayerFileData);
            Main.NewText("[c/00FF00:Console] >> Player saved. You are free to exit.");
        }
        public static T Pick<T>(params T[] values)
        {
            return Main.rand.Next(values);
        }
        public static void ReloadMods()
        {
            typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.ModLoader").GetMethod("Reload", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        }
        public static bool KeyPress(Keys key)
        {
            return Main.keyState.IsKeyDown(key) && Main.oldKeyState.IsKeyUp(key);
        }
        public static GeneralHelpers Instance => ModContent.GetInstance<GeneralHelpers>();
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
                SoundEngine.PlaySound(SoundID.MenuOpen);
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
                if (def < 150)
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
                nonHoverColor = new Color(def, def, def);
            }
            useScale = MathHelper.Clamp(useScale, 0.65f, 0.85f);

            var bounds = FontAssets.DeathText.Value.MeasureString(text);
            var rectHoverable = new Rectangle((int)position.X - (int)(bounds.X / 2 * useScale), (int)(position.Y - bounds.Y / 2 + 10), (int)(bounds.X * useScale), (int)bounds.Y - 30);

            hovering = rectHoverable.Contains(Main.MouseScreen.ToPoint());

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.DeathText.Value, text, position, hovering ? colorWhenHovered * alpha : nonHoverColor * alpha, 0f, bounds / 2, new Vector2(useScale), -1, 2);
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
            return rectHoverable;
        }
        public static bool CheckPlayerArmorSlot(Player player, int slot, out Item item)
        {
            item = new Item();
            if (!player.armor[slot].IsAir)
                item = player.armor[slot];

            return !player.armor[slot].IsAir;
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
                    s+= $" | player.armor[{i - 1}]:{item.Name}";
                }
            }
            return s == "" ? "No valid slots" : s;  
        }
        public static float TrueTime => (float)(Main.time + (Main.dayTime ? 0d : Main.dayLength));
        public const float FullDayLength = (float)(Main.nightLength + Main.dayLength);
        public class IDs
        {
            public struct ArmorSlotID
            {
                public static ReLogic.Reflection.IdDictionary Search = ReLogic.Reflection.IdDictionary.Create<short, short>();
                public const short HeadSlot = 0;
                public const short ChestSlot = 1;
                public const short LegSlot = 2;
                public const short AccSlot1 = 3;
                public const short AccSlot2 = 4;
                public const short AccSlot3 = 4;
                public const short AccSlot4 = 5;
                public const short AccSlot5 = 6;

                public const short VanityHeadSlot = 10;
                public const short VanityChestSlot = 11;
                public const short VanityLegSlot = 12;

                public const short VanityAccSlot1 = 3;
                public const short VanityAccSlot2 = 4;
                public const short VanityAccSlot3 = 4;
                public const short VanityAccSlot4 = 5;
                public const short VanityAccSlot5 = 6;
            }
        }
        public static List<Tile> GetTileSquare(int i, int j, int width, int height)
        {
            var tilesInSquare = new List<Tile>();
            for (int n = i - width / 2; n < i + width / 2; n++)
            {
                for (int m = j - height / 2; m < j + height / 2; m++)
                {
                    Tile tile = Framing.GetTileSafely(n, m);
                    tilesInSquare.Add(tile);
                }
            }
            return tilesInSquare;
        }
        public static List<Point> GetTileSquareCoordinates(int i, int j, int width, int height)
        {
            var coordsPerTile = new List<Point>();
            for (int n = i - width / 2; n < i + width / 2; n++)
            {
                for (int m = j - height / 2; m < j + height / 2; m++)
                {
                    coordsPerTile.Add(new Point(n, m));
                }
            }

            return coordsPerTile;
        }

        public static Tile GetTileAt(Vector2 coordinates)
        {
            return WorldGen.InWorld((int)coordinates.X, (int)coordinates.Y) ? Framing.GetTileSafely(coordinates.ToPoint()) : new Tile();
        }
        public static string GetTileNameAt(Vector2 coordinates)
        {
            return WorldGen.InWorld((int)coordinates.X, (int)coordinates.Y) ? TileID.Search.GetName(Framing.GetTileSafely(coordinates.ToPoint()).TileType) : "Empty";
        }
        public static string GetWallNameAt(Vector2 coordinates)
        {
            return WorldGen.InWorld((int)coordinates.X, (int)coordinates.Y) ? WallID.Search.GetName(Framing.GetTileSafely(coordinates.ToPoint()).WallType) : "Empty";
        }
    }
    public static class MathUtils
    {
        public static float InverseLerp(float begin, float end, float value, bool clamped = false)
        {
            if (clamped)
            {
                if (begin < end)
                {
                    if (value < begin)
                        return 0f;
                    if (value > end)
                        return 1f;
                }
                else
                {
                    if (value < end)
                        return 1f;
                    if (value > begin)
                        return 0f;
                }
            }
            return (value - begin) / (end - begin);
        }
    }
    public static class ExtensionMethods
    {
        public static Mod mod => ModContent.GetInstance<TerrariaAmbience>();
        public static bool Underwater(this Vector2 drowningPosition) => Collision.DrownCollision(drowningPosition, 1, 1);
        public static bool Underwater(this Player player) => Collision.DrownCollision(player.position, player.width, player.height, player.gravDir); // how did i not know this existed before
        public static string AppendFileExtension(this string str, GeneralHelpers.AudioFileExtension extension)
        {
            return $"{str}.{extension.ToString().ToLower()}";
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
        public static bool HeadWet(this Player player)
        {
            Vector2 v = new Vector2(player.Top.X, player.Top.Y - 1.4f).RotatedBy(player.fullRotation, player.Center);
            return Main.tile[(int)v.X / 16, (int)v.Y / 16].LiquidAmount > 0;
        }
        /// <summary>
        /// Finds the tiles around the player in a square. Do not put the value too high otherwise FPS will tank.
        /// </summary>
        /// <param name="position">The position to check around.</param>
        /// <param name="dist">The distance from the player</param>
        /// <returns>The amount of tiles in dist around the player.</returns>
        public static int TilesAround(this Vector2 position, int dist, bool condition)
        {
            int index = 0;
            if (condition)
            {
                for (int i = (int)position.X / 16 - dist; i < (int)position.X / 16 + dist; i++)
                {
                    for (int j = (int)position.Y / 16 - dist; j < (int)position.Y / 16 + dist; j++)
                    {
                        Tile tile = Main.tile[i, j];

                        if (tile.HasTile && tile.CollisionType() == 1)
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

                        if (leTile.HasTile && leTile.CollisionType() == 1)
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
    public static class TileUtils
    {
        public static int CollisionType(this Tile tile)
        {
            if (!tile.HasTile)
                return 0;
            if (tile.BlockType == BlockType.HalfBlock)
                return 2;
            if (tile.Slope != SlopeType.Solid)
                return 2 * (int)tile.Slope;
            if (Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                return 1;
            return -1;
        }
    }
}

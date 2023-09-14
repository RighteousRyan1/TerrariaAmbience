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
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbienceAPI.Common;
using TerrariaAmbience.Common.Systems;

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
            // MenuDetours.On_AddMenuButtons += MenuDetours_On_AddMenuButtons;

            active = true;
            posY = 4;
        }

        public static void MenuDetours_On_AddMenuButtons(MenuDetours.Orig_AddMenuButtons orig, Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
        {
            // GeneralHelpers.AddMainMenuButton("Ambience Menu", delegate { Main.menuMode = 999; }, selectedMenu, buttonNames, ref buttonIndex, ref numButtons);
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
            displayable = string.Empty;

            bool isVanillaTile = TileID.Search.TryGetName(PlayerTileChecker.TileId, out string name);
            foreach (var amb in TerrariaAmbience.DefaultAmbientHandler.Ambiences) {
                displayable += $"{amb.Name}: {amb.volume}\n";
            }
            displayable += $"\n\nTile Registry (Is player on?):\n";
            foreach (var step in TerrariaAmbience.DefaultFootstepHandler.AllSounds) {
                displayable += $"{step.Name}: {(step.IsPlayerOnAnyTile 
                    && Main.LocalPlayer.velocity.Y == 0 ? "Yes" : "No")}\n";
                // it doesnt update unless player is on ground... hmmm fix?
            }

            displayable += $"CurTile: " + (PlayerTileChecker.TileId >= 0 ? (isVanillaTile ? name + $" | ID: {PlayerTileChecker.TileId}" : TileLoader.GetTile(PlayerTileChecker.TileId).Name + $" ({TileLoader.GetTile(PlayerTileChecker.TileId).Mod.Name})") : "None")
                + $"\nPlayer Reverb Gain: {Main.LocalPlayer.GetModPlayer<Sounds.ReverbPlayer>().ReverbFactor}"
                + $"\nIsUnderground: {Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneDirtLayerHeight}";

            if (ModContent.GetInstance<GeneralConfig>().debugInterface) {
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

                string txt = $"Press RightAlt to toggle reverb info" +
                    $"\n\nMouseWorld: ({(int)Main.MouseWorld.X}, {(int)Main.MouseWorld.Y})" +
                    $"\nL: Play sound at mouse" +
                    "\nK: Spawn Positional Audio Sound at mouse" +
                    "\nOemOpenBrackets: Play CurTile footstep sound" +
                    "\n\nGradients:" +
                    $"\nAllNightPartDay: {GradualValueSystem.Gradient_AllNightPartDay}" +
                    $"\nAllDayPartNight: {GradualValueSystem.Gradient_AllDayPartNight}";
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
                        AudioLoopsSystem.grassCritters[Main.rand.Next(AudioLoopsSystem.grassCritters.Length)]?.Play(Main.MouseWorld, 0.75f);
                    if (choice == 2)
                        AudioLoopsSystem.owls[Main.rand.Next(AudioLoopsSystem.owls.Length)]?.Play(Main.MouseWorld, 0.75f);
                }
                if (GeneralHelpers.KeyPress(Keys.OemOpenBrackets)) {
                    foreach (var step in TerrariaAmbience.DefaultFootstepHandler.AllSounds) {
                        if (step.IsPlayerOnAnyTile) {
                            step.PlayAny(1f, Main.LocalPlayer.Center);
                        }
                    }
                }
                    // Main.LocalPlayer.GetModPlayer<AmbientPlayer>().GetFootstepSound(false)?.Play();
                #endregion
            }
        }
        #region Shameless Variable Naming
        private static float posX;
        private static float posY;
        private static bool active;
        #endregion
        public static bool sOpen;
        public static bool eOpen;
        private static void Main_DrawMenu(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            // orig(self, gameTime);
            //Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            // DrawAmbienceMenu();
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
                }
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, viewPost, new Vector2(posX, posY), Color.LightGray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);
                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, server, new Vector2(posX + (int)(FontAssets.DeathText.Value.MeasureString(viewPost).X * 0.35f) + 10, posY), hovering ? Color.White : Color.Gray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);
            }

            GeneralHelpers.MSOld = GeneralHelpers.MSNew;
            orig(self, gameTime);
            //Main.spriteBatch.End();
        }
        #endregion
    }
}

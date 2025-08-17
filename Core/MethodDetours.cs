using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using TerrariaAmbience.Common.Systems;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Sounds.SoundFilters;
using TerrariaAmbienceAPI.Common;

namespace TerrariaAmbience.Core;

internal class MethodDetours {
    public static void DetourAll() {
        ContentInstance.Register(new GeneralHelpers());

        On_Main.DrawMenu += Main_DrawMenu;
        On_Main.DrawInterface_30_Hotbar += Main_DrawInterface_30_Hotbar;
        On_IngameOptions.DrawRightSide += DrawVolumeValues;
        // MenuDetours.On_AddMenuButtons += MenuDetours_On_AddMenuButtons;
        active = true;
        posY = 4;
    }

    public static void MenuDetours_On_AddMenuButtons(MenuDetours.Orig_AddMenuButtons orig, Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons) {
        // GeneralHelpers.AddMainMenuButton("Ambience Menu", delegate { Main.menuMode = 999; }, selectedMenu, buttonNames, ref buttonIndex, ref numButtons);
        orig(main, selectedMenu, buttonNames, buttonScales, ref offY, ref spacing, ref buttonIndex, ref numButtons);
    }
    static bool oldHover;
    static bool hovering;
    // disposing sounds in draw code?
    static bool DrawVolumeValues(On_IngameOptions.orig_DrawRightSide orig, SpriteBatch sb, string txt, int i, Vector2 anchor, Vector2 offset, float scale, float colorScale, Color over) {
        Rectangle hoverPos = new((int)anchor.X - 65, (int)anchor.Y + 119, 275, 15);
        if (i == 14) {
            hovering = hoverPos.Contains(Main.MouseScreen.ToPoint());
        }

        if (!oldHover && hovering) {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        oldHover = hovering;
        if (IngameOptions.category == 2) {
            if (i == 3) {
                if (Main.FrameSkipMode == Terraria.Enums.FrameSkipMode.Subtle) {
                    txt = "Frame Skip Subtle (WARNING)";
                }
            }
        }
        return orig(sb, txt, i, anchor, offset, scale, colorScale, over);
    }

    static Vector2 drawPos;
    static string column1;
    static string column2;
    internal static byte viewMode;

    private static void Main_DrawInterface_30_Hotbar(On_Main.orig_DrawInterface_30_Hotbar orig, Main self) {
        orig(self);
        column1 = string.Empty;
        column2 = string.Empty;

        if (!ModContent.GetInstance<GeneralConfig>().debugInterface) return;

        #region DrawVolume
        string txt = string.Empty;
        var fontToUse = FontAssets.DeathText.Value;
        var ambPlayer = Main.LocalPlayer.GetModPlayer<AmbientPlayer>();
        if (viewMode >= 0) {
            column1 += $"Ambience Name/Volume:";
            foreach (var amb in TerrariaAmbience.DefaultAmbientHandler.Ambiences) {
                column1 += $"\n{amb.Name}: {amb.Volume:0.###}";
            }

            if (Main.playerInventory && (Main.mapStyle == 0 || Main.mapStyle == 2))
                drawPos = new Vector2(Main.screenWidth - Main.screenWidth / 7, 80);
            if (Main.mapStyle == 1 && Main.playerInventory)
                drawPos = new Vector2(Main.screenWidth - Main.screenWidth / 5, 80);
            if (Main.mapStyle == 1 && !Main.playerInventory)
                drawPos = new Vector2(Main.screenWidth - Main.screenWidth / 5, 80);
            if ((Main.mapStyle == 0 || Main.mapStyle == 2) && !Main.playerInventory)
                drawPos = new Vector2(Main.screenWidth - Main.screenWidth / 20, 80);

            drawPos.X -= FontAssets.DeathText.Value.MeasureString(column1).X * 0.24f;
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(
                    (int)drawPos.X - 6,
                    (int)drawPos.Y - 6,
                    (int)(fontToUse.MeasureString(column1).X * 0.24f),
                    (int)(fontToUse.MeasureString(column1).Y * 0.23f)),
                Color.SkyBlue * 0.6f);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch,
                fontToUse,
                column1,
                position: drawPos,
                Color.LightGray, 0f,
                origin: Vector2.Zero,
                baseScale: new Vector2(0.225f), -1, 1);
        }
        if (viewMode >= 1) {
            bool isVanillaTile = TileID.Search.TryGetName(PlayerTileChecker.TileId, out string name);
            column2 += $"Tile Registry (Is player on?):\n";
            foreach (var step in TerrariaAmbience.DefaultFootstepHandler.AllSounds) {
                var meetsConditions = (step.FootstepConditions is not null && step.FootstepConditions.Invoke(Main.LocalPlayer)) || step.FootstepConditions is null;
                column2 += $"{step.Name}: {(step.IsPlayerOnAnyTile && meetsConditions
                    && Main.LocalPlayer.velocity.Y == 0 ? "Yes" : "No")}\n";
                // it doesnt update unless player is on ground... hmmm fix?
            }
            var latest = SoundFilterSystem.LatestParams;
            column2 += $"CurTile: " + (PlayerTileChecker.TileId >= 0 ? (isVanillaTile ? name + $" | ID: {PlayerTileChecker.TileId}" : TileLoader.GetTile(PlayerTileChecker.TileId).Name + $" ({TileLoader.GetTile(PlayerTileChecker.TileId).Mod.Name})") : "None")
                + $"\nPlayer Filters: " +
                $"\n    ReverbGain: {latest.ReverbGain}" +
                $"\n    DecayTime: {latest.Reverb.DecayTime}" +
                $"\n    RoomSize: {latest.Reverb.RoomSize}" +
                $"\n    RefDelay: {latest.Reverb.ReflectionsDelay}" +
                $"\n    EarlyDiff: {latest.Reverb.EarlyDiffusion}" +
                $"\nIsUnderground: {Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneDirtLayerHeight}";

            #region DrawColumn2

            drawPos.X -= 300;

            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(
                   (int)drawPos.X - 6,
                   (int)drawPos.Y - 6,
                    (int)(fontToUse.MeasureString(column2).X * 0.24f),
                    (int)(fontToUse.MeasureString(column2).Y * 0.23f)),
                Color.SkyBlue * 0.6f);

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch,
                fontToUse,
                column2,
                position: drawPos,
                Color.LightGray,
                0f,
                origin: Vector2.Zero,
                baseScale: new Vector2(0.225f),
                -1,
                1);

            #endregion
        }

        // draws the little debug text at the top
        var stepsDetected = TerrariaAmbience.DefaultFootstepHandler.AllSounds.Where(x => x.IsPlayerOnAnyTile).Select(x => x.Name);
        var text = string.Join(", ", stepsDetected);
        text += $"\nPress RightShift to change debug view mode. (current={viewMode})";
        float scale = 0.25f;
        var measure = fontToUse.MeasureString(text) * scale;
        var infoPos = new Vector2(Main.screenWidth / 2, 8);

        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch,
            fontToUse,
            text,
            position: infoPos,
            Color.LightGray, 0f,
            origin: new Vector2(measure.X / 2, 0),
            baseScale: new Vector2(scale), -1, 1);
        #endregion

        #region DrawDebuggingKeybinds

        if (viewMode >= 2) {
            var b = TerrariaAmbience.dbg_modBiomes.Where(Main.LocalPlayer.InModBiome).ToList();

            txt = $"Press RightAlt to toggle reverb info" +
                $"\n\nMouseWorld: ({(int)Main.MouseWorld.X}, {(int)Main.MouseWorld.Y})" +
                $"\nL: Play sound at mouse" +
                "\nK: Spawn Positional Audio Sound at mouse" +
                "\nOemOpenBrackets: Play CurTile footstep sound" +
                "\n\nGradients:" +
                $"\nAllNightPartDay: {GradientGlobals.AllNightPartDay}" +
                $"\nAllDayPartNight: {GradientGlobals.AllDayPartNight}" +
                $"\nBehindWallMultiplier: {ambPlayer.InRoomAmbientMultiplier}" +
                $"\nSkyToUnderground: {GradientGlobals.SkyToUnderground}" +
                $"\nIsInRoom: {RoomDetectionPlayer.IsInRoom}" +
                $"\nModBiome(s): {(b.Count > 0 ? string.Join(", ", b.Select(x => x.Name)) : "N/A")}";

            drawPos.X -= 300;
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle((int)(drawPos.X - 6),
                (int)(drawPos.Y - 6),
                (int)(fontToUse.MeasureString(txt).X * 0.24f),
                (int)(fontToUse.MeasureString(txt).Y * 0.23f)),
                Color.DarkOrange * 0.6f);

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch,
                fontToUse,
                txt,
                drawPos - new Vector2(3, 3),
                Color.White,
                0f,
                Vector2.Zero,
                new Vector2(0.225f), -1, 1);

            if (GeneralHelpers.KeyPress(Keys.L))
                new ActiveSound(SoundID.ZombieMoan, Main.MouseWorld);
            if (GeneralHelpers.KeyPress(Keys.K)) {
                int choice = Main.rand.Next(0, 2);
                if (choice == 1)
                    AudioLoopsSystem.grassCritters[Main.rand.Next(AudioLoopsSystem.grassCritters.Length)]?.Play(Main.MouseWorld, 0.75f);
                if (choice == 2)
                    AudioLoopsSystem.owls[Main.rand.Next(AudioLoopsSystem.owls.Length)]?.Play(Main.MouseWorld, 0.75f);
            }
            if (GeneralHelpers.KeyPress(Keys.OemOpenBrackets)) {
                foreach (var step in TerrariaAmbience.DefaultFootstepHandler.AllSounds) {
                    var meetsConditions = (step.FootstepConditions is not null && step.FootstepConditions.Invoke(Main.LocalPlayer)) || step.FootstepConditions is null;
                    if (step.IsPlayerOnAnyTile && meetsConditions && Main.LocalPlayer.velocity.Y == 0) {
                        step.PlayAny(step.StepVolume, Main.LocalPlayer.Center);
                    }
                }
            }
        }
        #endregion

        if (viewMode >= 3) {
            int numPerRow = 5;
            // draw reverb material registry
            txt = "Tile absorption" + (viewMode == 3 ? " (Vanilla)" : " (Vanilla + Modded)") +
                "\n\nWall Insulators (low sound deflection):";
            drawPos.X -= 500;

            for (int i = 0; i < SoundFilterSystem.lowReverbWalls.Count; i++) {
                var lowR = SoundFilterSystem.lowReverbWalls.ElementAt(i);
                var wallName = WallID.Search.GetName(lowR);

                if (viewMode < 4) if (lowR >= WallID.Count) break;

                if (i % numPerRow == 0) txt += Environment.NewLine;
                txt += $"{wallName}, ";
            }
            txt += "\n\n";
            txt += "Tile insulators (low sound deflection):";
            for (int i = 0; i < SoundFilterSystem.lowReverbTiles.Count; i++) {
                var lowR = SoundFilterSystem.lowReverbTiles.ElementAt(i);

                if (viewMode < 4) if (lowR >= TileID.Count) break;

                var tileName = TileID.Search.GetName(lowR);

                if (i % numPerRow == 0) txt += Environment.NewLine;
                txt += $"{tileName}, ";
            }
            txt += "\n\n";
            txt += "Wall inhibitors (no sound deflection):";
            for (int i = 0; i < SoundFilterSystem.noReverbWalls.Count; i++) {
                var lowR = SoundFilterSystem.noReverbWalls.ElementAt(i);
                var wallName = WallID.Search.GetName(lowR);

                if (viewMode < 4) if (lowR >= WallID.Count) break;

                if (i % numPerRow == 0) txt += Environment.NewLine;
                txt += $"{wallName}, ";
            }
            txt += "\n\n";
            txt += "Tile inhibitors (no sound deflection):";
            for (int i = 0; i < SoundFilterSystem.noReverbTiles.Count; i++) {
                var lowR = SoundFilterSystem.noReverbTiles.ElementAt(i);
                var tileName = TileID.Search.GetName(lowR);

                if (viewMode < 4) if (lowR >= TileID.Count) break;

                if (i % numPerRow == 0) txt += Environment.NewLine;
                txt += $"{tileName}, ";
            }

            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle((int)(drawPos.X - 6),
                (int)(drawPos.Y - 6),
                (int)(fontToUse.MeasureString(txt).X * 0.175f),
                (int)(fontToUse.MeasureString(txt).Y * 0.175f)),
                Color.DarkMagenta * 0.6f
            );

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch,
                fontToUse,
                txt,
                drawPos - new Vector2(3, 3),
                Color.White,
                0f,
                Vector2.Zero,
                new Vector2(0.175f), -1, 1
            );
        }
    }
    static float posX;
    static float posY;
    static bool active;
    static void Main_DrawMenu(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime) {
        Mod mod = ModContent.GetInstance<TerrariaAmbience>();

        posX = MathHelper.Clamp(posX, -325, -16);
        var sb = Main.spriteBatch;
        string viewPost = "Visit the Terraria Ambience";
        string server = $"Discord Server";

        var click2Activate = new Rectangle((int)posX + 330, (int)posY, 12, 20);
        if (Main.menuMode == 0 && AmbientDisplaySystem.Arrow is not null) {
            Main.spriteBatch.SafeDraw(AmbientDisplaySystem.Arrow,
                new Vector2(posX + 330, posY), null, Color.White, 0f, Vector2.Zero, 0.6f,
                !active ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1f);
        }
        var rect = new Rectangle((int)posX + 223, (int)posY, (int)(FontAssets.DeathText.Value.MeasureString(server).X * 0.35f),
            (int)(FontAssets.DeathText.Value.MeasureString(server).Y * 0.25f));
        // Main.spriteBatch.Draw(Main.magicPixel, click2Activate, Color.White * 0.35f);
        bool hovering = rect.Contains(Main.MouseScreen.ToPoint());
        bool hoverAct = click2Activate.Contains(Main.MouseScreen.ToPoint());

        if (Main.menuMode == 0) {
            if (hoverAct) {
                if (Main.mouseRight) {
                    if (Main.MouseScreen.Y < Main.screenHeight && Main.MouseScreen.Y > 0) {
                        posY = Main.MouseScreen.Y - 10;
                    }
                }
                if (Main.mouseLeft && Main.mouseLeftRelease) {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    active = !active;
                }
            }
            if (hovering) {
                if (Main.mouseLeft && Main.mouseLeftRelease) {
                    if (active) {
                        Process.Start(new ProcessStartInfo("https://discord.gg/pT2BzSG") {
                            UseShellExecute = true
                        });
                    }
                }
            }
        }
        posX += active ? 20f : -20f;

        if (Main.menuMode == 0 && ModContent.GetInstance<UIConfig>().showMainMenuUi) {
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, viewPost, new Vector2(posX, posY), Color.LightGray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, server, new Vector2(posX + (int)(FontAssets.DeathText.Value.MeasureString(viewPost).X * 0.35f) + 10, posY), hovering ? Color.White : Color.Gray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);

            var alert = "!!! WARNING !!!";
            var disclaimer1 = "Ensure your audio device is set to 48000hz or less.";
            var disclaimer2 = "Otherwise, your game will crash soon after entering a world.";

            var alertScale = 0.4f;
            var txtScale = 0.3f;

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, alert,
                new Vector2(Main.screenWidth - 10, 10),
                Color.Yellow, 0f, new Vector2(FontAssets.DeathText.Value.MeasureString(alert).X, 0), new Vector2(alertScale), 0, 1);

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, disclaimer1,
                new Vector2(Main.screenWidth - 10, 10 + FontAssets.DeathText.Value.MeasureString(alert).Y * alertScale),
                Color.White, 0f, new Vector2(FontAssets.DeathText.Value.MeasureString(disclaimer1).X, 0), new Vector2(txtScale), 0, 1);

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.DeathText.Value, disclaimer2,
                new Vector2(Main.screenWidth - 10, 10 + FontAssets.DeathText.Value.MeasureString(alert).Y * alertScale
                + FontAssets.DeathText.Value.MeasureString(disclaimer1).Y * txtScale),
                Color.White, 0f, new Vector2(FontAssets.DeathText.Value.MeasureString(disclaimer2).X, 0), new Vector2(txtScale), 0, 1);
        }

        GeneralHelpers.MSOld = GeneralHelpers.MSNew;
        orig(self, gameTime);
        //Main.spriteBatch.End();
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Core;
using TerrariaAmbience.Sounds.SoundFilters;

namespace TerrariaAmbience.Helpers; 
public class TileVisualizationHelpers : ModSystem {
    public static Dictionary<Vector2, Color> TileVisualizations { get; private set; } = [];

    public static Texture2D DebugPixel;

    static byte _visVis;

    static Color _colorNoReverb = Color.Red;
    static Color _colorLowReverb = Color.DarkOrange;
    static Color _colorHighReverb = Color.Lime;
    static Color _colorMedReverb = Color.DarkSeaGreen;
    public override void PostDrawTiles() {
        if (_visVis == 0) return;

        if (DebugPixel == null) {
            DebugPixel = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
            DebugPixel.SetData([Color.White]);
        }

        Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

        foreach (var elem in TileVisualizations) {
            var pos = elem.Key - Main.screenPosition;
            var color = elem.Value;
            // Draw a rectangle at the tile position with the specified color
            var light = _visVis == 1 ? Lighting.GetSubLight(elem.Key).X : 1f;
            DrawBox(Main.spriteBatch, pos, pos + new Vector2(16, 16f), color * light);
        }
        Main.spriteBatch.End();
        TileVisualizations.Clear();
    }
    public static Color ToColor(Vector3 vec) => new((int)Math.Round(vec.X * 255), (int)Math.Round(vec.Y * 255), (int)Math.Round(vec.Z * 255));
    public override void PostUpdateEverything() {
        if (Main.mapFullscreen)
            TileVisualizations.Clear();

        if (Main.keyState.IsKeyDown(Keys.RightShift) && Main.oldKeyState.IsKeyUp(Keys.RightShift)) {
            MethodDetours.viewMode++;
            if (MethodDetours.viewMode >= 6)
                MethodDetours.viewMode = 0;
        }

        if (Main.keyState.IsKeyDown(Keys.RightAlt) && Main.oldKeyState.IsKeyUp(Keys.RightAlt)) {
            _visVis++;
            if (_visVis >= 3)
                _visVis = 0;
        }

        var genCfg = ModContent.GetInstance<AmbientConfig>();
        var aaCfg = ModContent.GetInstance<AudioConfig>();

        if (!genCfg.debugInterface || !aaCfg.advancedReverbCalculation || !aaCfg.isReverbEnabled) return;

        var tilePosList = RoomDetectionPlayer.PlayerRoom.Tiles;

        bool playerSurfaceOrHell = Main.LocalPlayer.Center.Y < Main.worldSurface * 16 || Main.LocalPlayer.Center.Y > (Main.maxTilesY - 200) * 16;
        bool playerUnderground = !playerSurfaceOrHell;

        bool isRaycastEnabled = aaCfg.reverbUsingRaycasting;

        foreach (var tilePos in tilePosList) {
            var tile = Framing.GetTileSafely(tilePos);
            Vector2 worldCoords = tilePos.ToVector2() * 16;

            var reflectivity = SoundFilterSystem.CalculateAcousticReflectivity(tilePos, out bool wt, out bool ww, out var wl);

            if (isRaycastEnabled) {
                var numTiles = 0;

                var lpc = SoundFilterSystem.ScreenListeningPosition.ToTileCoordinates();

                SoundFilterSystem.TileLine(tilePos, lpc,
                    (x, y, t) => {
                        var ts = Main.tileSolid[t.TileType];
                        var tst = Main.tileSolidTop[t.TileType];
                        if (t.HasTile && ts && !tst)
                            if (x != tilePos.X || y != tilePos.Y)
                                numTiles++;
                    });
                if (numTiles > 0)
                    continue;
            }

            switch (reflectivity) {
                case Reflectivity.Low:
                    TryAdd(worldCoords, _colorLowReverb);
                    break;
                case Reflectivity.High:
                    TryAdd(worldCoords, _colorHighReverb);
                    break;
                case Reflectivity.Medium:
                    TryAdd(worldCoords, _colorMedReverb);
                    break;
                case Reflectivity.None:
                    // don't display empty tiles on parts of the world without a cavern background
                    if ((ww || wt) && (wl != WorldLayer.Cavern || wl != WorldLayer.Dirt))
                    TryAdd(worldCoords, _colorNoReverb);
                    break;
            }
        }
    }

    public static void TryAdd(Vector2 pos, Color color) {
        TileVisualizations.TryAdd(pos, color);
    }

    static void DrawBox(SpriteBatch sb, Vector2 start, Vector2 end, Color boxColor, Vector2 origin = default) {
        var tex = DebugPixel;
        start -= origin;
        end -= origin;

        // top line
        sb.Draw(tex, start, null, boxColor, 0f, Vector2.Zero, new Vector2(end.X - start.X, 1), default, 0);
        // bottom line
        sb.Draw(tex, start + Vector2.UnitY * (end.Y - start.Y), null, boxColor, 0f, Vector2.Zero, new Vector2(end.X - start.X, 1), default, 0);
        // left line
        sb.Draw(tex, start, null, boxColor, 0f, Vector2.Zero, new Vector2(1, end.Y - start.Y), default, 0);
        // right line
        sb.Draw(tex, start + Vector2.UnitX * (end.X - start.X), null, boxColor, 0f, Vector2.Zero, new Vector2(1, end.Y - start.Y), default, 0);
    }
}

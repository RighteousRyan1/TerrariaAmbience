using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Helpers; 
public class TileVisualizationHelpers : ModSystem {
    public static Dictionary<Vector2, Color> TileVisualizations { get; private set; } = [];

    public static Texture2D DebugPixel;
    public override void PostDrawTiles() {
        if (DebugPixel == null) {
            DebugPixel = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
            DebugPixel.SetData([Color.White]);
        }

        Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

        foreach (var elem in TileVisualizations) {
            var pos = elem.Key - Main.screenPosition;
            var color = elem.Value;
            // Draw a rectangle at the tile position with the specified color
            DrawBox(Main.spriteBatch, pos, pos + new Vector2(16, 16f), color);
        }
        Main.spriteBatch.End();
        TileVisualizations.Clear();
    }
    public override void PostUpdateEverything() {
        if (Main.mapFullscreen)
            TileVisualizations.Clear();

        if (Main.keyState.IsKeyDown(Keys.RightShift) && Main.oldKeyState.IsKeyUp(Keys.RightShift)) {
            MethodDetours.viewMode++;
            if (MethodDetours.viewMode >= 5)
                MethodDetours.viewMode = 0;
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

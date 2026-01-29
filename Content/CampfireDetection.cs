using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;

namespace TerrariaAmbience.Content;

public class CampfireDetection {
    static readonly HashSet<Point> _tileBuffer = [];
    public static HashSet<Point> GetNearbyCampfires(Vector2 position, int radius = 30) {
        var pos = position.ToTileCoordinates();
        _tileBuffer.Clear();
        for (int i = pos.X - radius; i < pos.X + radius; i++) {
            for (int j = pos.Y - radius; j < pos.Y + radius; j++) {
                if (!WorldGen.InWorld(i, j)) continue;

                var tile = Main.tile[i, j];
                if (tile.TileType != TileID.Campfire) continue;

                if (tile.TileFrameY % 72 < 36) {
                    int originX = i - tile.TileFrameX % 54 / 18;
                    int originY = j - tile.TileFrameY % 36 / 18;
                    _tileBuffer.Add(new(originX, originY));
                }
            }
        }
        return _tileBuffer;
    }
}
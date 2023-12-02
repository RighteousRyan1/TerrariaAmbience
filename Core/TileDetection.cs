using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Content;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Content.Players;

namespace TerrariaAmbience.Core;

public class TileDetection {
    /// <summary>
    /// Access this for when you want to add a modded tile to the valid tiles list. Use the ID for the tile.
    /// </summary>
    /// <param name="listToAddTo"></param>
    /// <param name="tiles"></param>
    public static void AddTilesToList(List<int> listToAddTo, params int[] tiles) {
        foreach (int tileType in tiles) {
            if (tileType > TileID.Search.Count)
                listToAddTo.Add(tileType);
        }
    }
    /// <summary>
    /// Access this for when you want to add a modded tile to the valid tiles list, use the tile's internal name for the tile.
    /// </summary>
    /// <param name="listToAddTo"></param>
    /// <param name="tiles"></param>
    public static void AddTilesToList(Mod mod, List<int> listToAddTo, params string[] name) {
        foreach (string tile in name) {
            if (mod.TryFind<ModTile>(tile, out var found))
                listToAddTo.Add(found.Type);
            else
                ModContent.GetInstance<TerrariaAmbience>().Logger.Error($"Mod Tile '{tile}' not found. Skipping over...");
        }
    }
}

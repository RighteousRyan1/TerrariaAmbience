using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content.ModSupport; 
internal class TheDepthsModSupport : ModSystem {
    public static IEnumerable<ModBiome> Biomes;

    public override void PostSetupContent() {
        if (ModLoader.TryGetMod("TheDepths", out var mod)) {
            Biomes = [.. mod.GetContent<ModBiome>()];

            TileDetection.AddTilesToList(mod, FootstepHandler.StoneBlocks,
                "Shalestone", "OnyxShalestone",
                "ShalestoneDiamond", "ShalestoneEmerald", "ShalestoneAmethyst", "ShalestoneTopaz", "ShalestoneSapphire",
                "ShalestoneRuby");
            TileDetection.AddTilesToList(mod, FootstepHandler.DirtBlocks,
                "ShaleBlock");
            // "SpookyWood" is "Old Wood"
            TileDetection.AddTilesToList(mod, FootstepHandler.DeckWood,
                "QuartzPlatform");

            TileDetection.AddTilesToList(mod, FootstepHandler.SmoothStones,
                "QuartzBricks");
            Console.WriteLine($"{mod.Name}: {string.Join("\n", Biomes.Select(x => x.Name))}");
            Console.WriteLine();

            // the depths is just hell anyway. hell biome is determined by depth not biome
            // var dah = TerrariaAmbience.DefaultAmbientHandler;
            //AmbientHandler.AddModBiomeTo(mod, dah.modde, "SpookyBiome");
            //AmbientHandler.AddModBiomeTo(mod, dah.ModdedEvilCorruptions, "CemeteryBiome");
        }
    }
}

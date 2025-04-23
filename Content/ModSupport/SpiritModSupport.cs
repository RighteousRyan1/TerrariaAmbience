using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content.ModSupport; 
public class SpiritModSupport : ModSystem {
    public static IEnumerable<ModBiome> Biomes;
    public override void PostSetupContent() {
        if (ModLoader.TryGetMod("SpiritMod", out var mod)) {
            Biomes = mod.GetContent<ModBiome>();
            TileDetection.AddTilesToList(mod, FootstepHandler.StoneBlocks,
                "BlastStone");
            TileDetection.AddTilesToList(mod, FootstepHandler.GrassBlocks,
                "BriarGrass");

            Console.WriteLine($"{mod.Name}: {string.Join("\n", Biomes.Select(x => x.Name))}");
            Console.WriteLine();

            var dah = TerrariaAmbience.DefaultAmbientHandler;
            dah.AddModBiomeTo(mod, dah.ModdedForests, "BriarSurfaceBiome", "BriarUndergroundBiome");
            dah.AddModBiomeTo(mod, dah.ModdedEvilCorruptions, "CemeteryBiome");
        }
    }
}

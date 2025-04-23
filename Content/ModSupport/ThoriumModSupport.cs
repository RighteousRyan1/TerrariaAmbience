using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content.ModSupport; 
public class ThoriumModSupport : ModSystem {
    public static IEnumerable<ModBiome> Biomes;
    public override void PostSetupContent() {
        if (ModLoader.TryGetMod("ThoriumMod", out var mod)) {
            Biomes = mod.GetContent<ModBiome>().ToList();
            // multiple sounds for things like CloudSlab because snowy + stone-y sounds would probably fit!
            TileDetection.AddTilesToList(mod, FootstepHandler.StoneBlocks,
                "ThoriumOre", "LifeQuartz", "MarineRock", "MarineRockMoss", "DepthsAmber", "PearlStone", "Aquaite", "DepthsOpal",
                "SynthPlatinum", "DepthsOnyx", "DepthsSapphire", "DepthsEmerald", "DepthsTopaz", "DepthsAmethyst", "ScarletChestPlatform",
                "SugarCookieBlockNew", "BloodstainedBlock", "Aquamarine",
                "CursedBlockNew", "OrnateBlock", "SynthGold", "ShadyBlock", "OrnatePlatform");
            TileDetection.AddTilesToList(mod, FootstepHandler.SmoothStones,
                "CelestialBrick", "CloudSlab", "CutStoneBlock", "CutSandstoneBlock",
                "CutStoneBlockSlab", "CutSandstoneBlockSlab", "NagaBlockNew", "CelestialPlatform", "NagaPlatform");
            TileDetection.AddTilesToList(mod, FootstepHandler.GrassBlocks,
                "SpookyAstroturf", "CherryAstroturf");
            TileDetection.AddTilesToList(mod, FootstepHandler.Marbles,
                "CheckeredBrickTile", "RefinedMarineBlock");
            TileDetection.AddTilesToList(mod, FootstepHandler.SnowBlocks,
                "SnowyAstroturf", "CloudSlab");
            TileDetection.AddTilesToList(mod, FootstepHandler.GemBlocks,
                "AquamarineGemsparkNew", "ThoriumBrick", "ThoriumBrickBlock", "ThoriumPlatform");
            TileDetection.AddTilesToList(mod, FootstepHandler.SandBlocks, "Brack", "BrackBare");

            Console.WriteLine($"{mod.Name}: {string.Join("\n", Biomes.Select(x => x.Name))}");
            Console.WriteLine();
        }
    }
}

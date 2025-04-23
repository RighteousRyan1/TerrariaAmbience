using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content.ModSupport; 
public class CalamityModSupport : ModSystem {

    public static IEnumerable<ModBiome> Biomes;
    public override void PostSetupContent() {
        if (ModLoader.TryGetMod("CalamityMod", out var mod)) {
            Biomes = mod.GetContent<ModBiome>().ToList();
            TileDetection.AddTilesToList(mod, FootstepHandler.SnowBlocks, "AstralSnow");
            TileDetection.AddTilesToList(mod, FootstepHandler.SandBlocks,
                "SulphurousSand",
                "AstralSand",
                "EutrophicSand",
                "AstralClay",
                "SulphurousSandNoWater",
                "CelestialRemains");

            TileDetection.AddTilesToList(mod, FootstepHandler.GrassBlocks,
                "AstralGrass",
                "VernalSoil");

            TileDetection.AddTilesToList(mod, FootstepHandler.MetalBlocks,
                "ExoPrismPanelTile",
                "ExoPlatingTile",
                "ExoPrismPlatformTile",
                "ExoPlatformTile",
                "CosmilitePlatform", "CosmiliteBrick",
                "PlaguedPlatePlatform",
                "LaboratoryPlating",
                "LaboratoryPipePlating",
                "RustedShelf", "RustedPlating", "PlaguedPlate");
            TileDetection.AddTilesToList(mod, FootstepHandler.GemBlocks,
                "SilvaCrystal");
            TileDetection.AddTilesToList(mod, FootstepHandler.LeafBlocks,
                "PlantyMush");
            TileDetection.AddTilesToList(mod, FootstepHandler.GlassBlocks,
                "SeaPrismBrick", "EutrophicGlass", "CryonicBrick");
            TileDetection.AddTilesToList(mod, FootstepHandler.DirtBlocks,
                "PlantyMush",
                "AstralDirt");
            TileDetection.AddTilesToList(mod, FootstepHandler.WeakIceBlocks,
                "AstralIce");
            TileDetection.AddTilesToList(mod, FootstepHandler.Marbles,
                "StatigelBlock", "StatigelPlatform",
                "SmoothNavystone",
                "SmoothBrimstoneSlag");
            TileDetection.AddTilesToList(mod, FootstepHandler.SmoothStones,
                "AshenSlab", "AshenAccentSlab", "AshenPlatform", "SmoothAbyssGravel",
                "OccultBrickTile", "SilvaPlatform", "OccultPlatformTile",
                "RunicProfanedBrick", "VoidstoneSlab", "ProfanedSlab", "ScoriaBrick",
                "PerennialBrick", "AerialiteBrick", "BrimstoneSlab", "AstralBrick");
            TileDetection.AddTilesToList(mod, FootstepHandler.StoneBlocks,
                "OtherworldlyStone", "OtherworldlyPlatform",
                "AstralOre",
                "AstralStone",
                "AstralMonolith",
                "AstralMonolith", "AbyssGravel", "Tenebris", "ChaoticOre", "Voidstone", "Navystone",
                "TableCoral", "SeaPrism", "HazardChevronPanels", "Navyplate", "RustedPlating",
                "HardenedSulphurousSandstone", "ChaoticOre",
                "HardenedAstralSand",
                "SulphurousSandstone",
                "HardenedSulphurousSandstone",
                "BrimstoneSlag", "CharredOre",
                "LaboratoryShelf",
                "LaboratoryPanels", "Cinderplate", "SmoothVoidstone", "Onyxplate", "Elumplate",
                "Havocplate", "CryonicOre", "ProfanedRock",
                "PerennialOre", "HallowedOre", "NovaeSlag", "AuricOre", "AstralSandstone",
                "UelibloomOre", "AerialiteOre", "AerialiteOreDisenchanted", "ExodiumOre",
                "PlagueContainmentCells", "SulphurousShale");

            Console.WriteLine($"{mod.Name}: {string.Join("\n", Biomes.Select(x => x.Name))}");
            Console.WriteLine();

            var dah = TerrariaAmbience.DefaultAmbientHandler;
            dah.AddModBiomeTo(mod, dah.ModdedBeaches, "SulphurousSeaBiome", "AcidRainBiome");
            dah.AddModBiomeTo(mod, dah.ModdedTundras, "AstralIce");
            dah.AddModBiomeTo(mod, dah.ModdedDeserts, "AstralDesert", "AstralCaveDesert");
        }
    }
}

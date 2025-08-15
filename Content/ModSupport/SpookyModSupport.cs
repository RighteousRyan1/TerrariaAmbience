using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content.ModSupport; 
public class SpookyModSupport : ModSystem {
    public static IEnumerable<ModBiome> Biomes;
    public override void PostSetupContent() {
        if (ModLoader.TryGetMod("Spooky", out var mod)) {
            Biomes = mod.GetContent<ModBiome>().ToList();
            TileDetection.AddTilesToList(mod, FootstepHandler.GrassBlocks,
                "SpookyGrass", "SpookyGrassGreen", "CemeteryGrass", "MushroomMoss");
            TileDetection.AddTilesToList(mod, FootstepHandler.StoneBlocks,
                "CemeteryStone");
            TileDetection.AddTilesToList(mod, FootstepHandler.DirtBlocks,
                "SpookyDirt");
            // "SpookyWood" is "Old Wood"
            TileDetection.AddTilesToList(mod, FootstepHandler.BluntWood,
                "SpookyWood");
            TileDetection.AddTilesToList(mod, FootstepHandler.DeckWood,
                "OldWoodPlatform");
            TileDetection.AddTilesToList(mod, FootstepHandler.StickyBlocks,
                "GourdBlockLimeOrange", "GourdBlockRed", "GourdBlockOrange", "LivingFlesh");
            TileDetection.AddTilesToList(mod, FootstepHandler.SmoothStones,
                "NoseTempleBrickGraySafe", "NoseTempleBrickPurpleSafe", "NoseTempleBrickRedSafe", "NoseTempleBrickGreenSafe", "CatacombBrick1Safe", "CatacombBrick2Safe");
            TileDetection.AddTilesToList(mod, FootstepHandler.MetalPlatingBlocks,
                "NoseTempleFancyBrickPurpleSafe", "NoseTempleFancyBrickGreenSafe", "NoseTempleFancyBrickRedSafe", "NoseTempleFancyBrickGraySafe");
            TileDetection.AddTilesToList(mod, FootstepHandler.GravelBlocks,
                "ValleyStone");

            Console.WriteLine($"{mod.Name}: {string.Join("\n", Biomes.Select(x => x.Name))}");
            Console.WriteLine();

            var dah = TerrariaAmbience.DefaultAmbientHandler;
            dah.AddModBiomeTo(mod, dah.ModdedForests, "SpookyBiome");
            dah.AddModBiomeTo(mod, dah.ModdedEvilCorruptions, "CemeteryBiome");
        }
    }
}

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
            Biomes = [.. mod.GetContent<ModBiome>()];
            TileDetection.AddTilesToList(mod, FootstepHandler.GrassBlocks,
                "SpookyGrass", "SpookyGrassGreen", "CemeteryGrass", "MushroomMoss", 
                "DampGrass", "SpookyMushGrass", "ChristmasCarpet",
                "CatacombBrick2Grass", "CatacombBrick2GrassArena", "JungleMoss", "JungleSoilGrass");
            TileDetection.AddTilesToList(mod, FootstepHandler.StoneBlocks,
                "CemeteryStone", "SpookyStone", "OceanRock", "DesertSandstone", "PlantFossil");
            TileDetection.AddTilesToList(mod, FootstepHandler.DirtBlocks,
                "SpookyDirt", "JungleSoil", "BloomSoil");
            // "SpookyWood" is "Old Wood"
            TileDetection.AddTilesToList(mod, FootstepHandler.BluntWood,
                "SpookyWood", "ChristmasWood");
            TileDetection.AddTilesToList(mod, FootstepHandler.DeckWood,
                "OldWoodPlatform", "CatacombBrickPlatform1", "ChristmasPlatform");
            TileDetection.AddTilesToList(mod, FootstepHandler.SandBlocks,
                "OceanSand", "DesertSand");
            TileDetection.AddTilesToList(mod, FootstepHandler.StickyBlocks,
                "GourdBlockLimeOrange", "GourdBlockRed", "GourdBlockOrange", "LivingFlesh", "EyeBlock", "EyeballBlock");
            TileDetection.AddTilesToList(mod, FootstepHandler.SmoothStones,
                "NoseTempleBrickGraySafe", "NoseTempleBrickPurpleSafe", "NoseTempleBrickRedSafe", 
                "NoseTempleBrickGreenSafe", "CatacombBrick1Safe", "CatacombBrick2Safe", 
                "CatacombBrick1", "CatacombBrick1Arena", "CatacombBrick2Arena",
                "SpookyStoneBricks", "CatacombFlooring", 
                "NoseTempleFancyBrickGreen", "NoseTempleFancyBrickPurple", "NoseTempleFancyBrickRed",
                "ChristmasBrickGreen", "ChristmasBrickRed", "ChristmasBrickBlue");
            TileDetection.AddTilesToList(mod, FootstepHandler.Marbles,
                "ChristmasSlabBlue", "ChristmasSlabGreen", "ChristmasSlabRed");
            TileDetection.AddTilesToList(mod, FootstepHandler.MetalPlatingBlocks,
                "NoseTempleFancyBrickPurpleSafe", "NoseTempleFancyBrickGreenSafe", "NoseTempleFancyBrickRedSafe", "NoseTempleFancyBrickGraySafe",
                "LabMetalPlate", "GildedBrick", "CatacombTrapdoor1", "CatacombTrapdoor2", "CatacombTrapdoor3");
            TileDetection.AddTilesToList(mod, FootstepHandler.GravelBlocks,
                "ValleyStone");
            TileDetection.AddTilesToList(mod, FootstepHandler.LeafBlocks,
                "WebBlock");

            Console.WriteLine($"{mod.Name}: {string.Join("\n", Biomes.Select(x => x.Name))}");
            Console.WriteLine();

            var dah = TerrariaAmbience.DefaultAmbientHandler;
            AmbientHandler.AddModBiomeTo(mod, dah.ModdedForests, "SpookyBiome");
            AmbientHandler.AddModBiomeTo(mod, dah.ModdedEvilCrimsons, "CemeteryBiome");
        }
    }
}

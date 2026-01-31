using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content.ModSupport;
public class TheConfectionModSupport : ModSystem {
    public static IEnumerable<ModBiome> Biomes;
    public override void PostSetupContent() {
        if (ModLoader.TryGetMod("TheConfectionRebirth", out var mod)) {
            Biomes = mod.GetContent<ModBiome>();

            TileDetection.AddTilesToList(mod, FootstepHandler.GrassBlocks,
                "CreamGrass");
            TileDetection.AddTilesToList(mod, FootstepHandler.StoneBlocks,
                "Creamstone");
            TileDetection.AddTilesToList(mod, FootstepHandler.DirtBlocks,
                "CookieBlock");

            TileDetection.AddTilesToList(mod, FootstepHandler.BluntWood,
                "CreamWood");
            //TileDetection.AddTilesToList(mod, FootstepHandler.DeckWood,
            //    "");
            TileDetection.AddTilesToList(mod, FootstepHandler.StrongIceBlocks,
               "BlueIce");
            TileDetection.AddTilesToList(mod, FootstepHandler.SnowBlocks,
                "CreamBlock", "PinkFairyFloss", "BlueFairyFloss");
            TileDetection.AddTilesToList(mod, FootstepHandler.SandBlocks,
                "Creamsand");
            /*TileDetection.AddTilesToList(mod, FootstepHandler.StickyBlocks,
                "");
            TileDetection.AddTilesToList(mod, FootstepHandler.SmoothStones,
                "");
            TileDetection.AddTilesToList(mod, FootstepHandler.Marbles,
                "");
            TileDetection.AddTilesToList(mod, FootstepHandler.MetalPlatingBlocks,
                "");
            TileDetection.AddTilesToList(mod, FootstepHandler.GravelBlocks,
                "");
            TileDetection.AddTilesToList(mod, FootstepHandler.LeafBlocks,
                "");*/

            Console.WriteLine($"{mod.Name}: {string.Join("\n", Biomes.Select(x => x.Name))}");
            Console.WriteLine();

            var dah = TerrariaAmbience.DefaultAmbientHandler;
            AmbientHandler.AddModBiomeTo(mod, dah.ModdedForests, "ConfectionBiome");
        }
    }
}

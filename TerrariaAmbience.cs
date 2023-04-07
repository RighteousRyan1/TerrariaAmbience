using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using TerrariaAmbience.Core;
using System.Linq;
using System;
using TerrariaAmbience.Content;
using TerrariaAmbience.Helpers;
using Terraria.Audio;
using TerrariaAmbience.Common.Systems;

namespace TerrariaAmbience
{
    public partial class TerrariaAmbience : Mod
	{
        // TODO: add all other types to the call
        public override object Call(params object[] args)
        {
            try {
                string message = args[0] as string;
                if (message == "AddTilesToList")
                {
                    Mod mod = args[1] as Mod;
                    string listName = args[2] as string; // Can be Stone, Grass, Sand, Snow, or Dirt (FOR NOW, OR EVER) (UPDATE THIS)
                    // string[] nameStringList = args[3] as string[];
                    object boxedInstance = args[3];

                    if (!Main.dedServ) {
                        if (mod != null && boxedInstance is string[] s) {
                            if (listName == "Grass")
                                TileDetection.AddTilesToList(mod, TileDetection.GrassBlocks, s);
                            else if (listName == "Stone")
                                TileDetection.AddTilesToList(mod, TileDetection.StoneBlocks, s);
                            else if (listName == "Sand")
                                TileDetection.AddTilesToList(mod, TileDetection.SandBlocks, s);
                            else if (listName == "Snow")
                                TileDetection.AddTilesToList(mod, TileDetection.SnowyBlocks, s);
                            else if (listName == "Dirt")
                                TileDetection.AddTilesToList(mod, TileDetection.DirtBlocks, s);
                            else if (listName == "Metal")
                                TileDetection.AddTilesToList(mod, TileDetection.Metals, s);
                            else if (listName == "Ice")
                                TileDetection.AddTilesToList(mod, TileDetection.IcyBlocks, s);
                            else if (listName == "Leaf")
                                TileDetection.AddTilesToList(mod, TileDetection.LeafBlocks, s);
                            else if (listName == "Glass")
                                TileDetection.AddTilesToList(mod, TileDetection.GlassBlocks, s);
                            else if (listName == "GraniteMarble")
                                TileDetection.AddTilesToList(mod, TileDetection.GraniteAndMarbles, s);
                            else if (listName == "SmoothStone")
                                TileDetection.AddTilesToList(mod, TileDetection.SmoothStones, s);
                            else if (listName == "Sticky")
                                TileDetection.AddTilesToList(mod, TileDetection.StickyBlocks, s);
                            else {
                                Logger.Info("Mod.Call failure: Unknown tile list specified.");
                                return "Unknown tile list specified.";
                            }
                            Logger.Info($"Successfully added modded tiles ({string.Join(", ", s)}) to the {listName} tile list.");
                        }
                        else if (mod == null && boxedInstance is int[] i) {
                            if (listName == "Grass")
                                TileDetection.AddTilesToList(TileDetection.GrassBlocks, i);
                            else if (listName == "Stone")
                                TileDetection.AddTilesToList(TileDetection.StoneBlocks, i);
                            else if (listName == "Sand")
                                TileDetection.AddTilesToList(TileDetection.SandBlocks, i);
                            else if (listName == "Snow")
                                TileDetection.AddTilesToList(TileDetection.SnowyBlocks, i);
                            else if (listName == "Dirt")
                                TileDetection.AddTilesToList(TileDetection.DirtBlocks, i);
                            else if (listName == "Metal")
                                TileDetection.AddTilesToList(TileDetection.Metals, i);
                            else if (listName == "Ice")
                                TileDetection.AddTilesToList(TileDetection.IcyBlocks, i);
                            else if (listName == "Leaf")
                                TileDetection.AddTilesToList(TileDetection.LeafBlocks, i);
                            else if (listName == "Glass")
                                TileDetection.AddTilesToList(TileDetection.GlassBlocks, i);
                            else if (listName == "GraniteMarble")
                                TileDetection.AddTilesToList(TileDetection.GraniteAndMarbles, i);
                            else if (listName == "SmoothStone")
                                TileDetection.AddTilesToList(TileDetection.SmoothStones, i);
                            else if (listName == "Sticky")
                                TileDetection.AddTilesToList(TileDetection.StickyBlocks, i);
                            Logger.Info($"Successfully added modded tiles ({string.Join(", ", i.Select(id => TileID.Search.GetName(id)))} to the {listName} tile list.");
                        }
                    }

                    return "Tiles added successfully!";
                }
                else
                    Logger.Error("Call Error: Unknown Message: " + message);
            }
            catch (Exception e) {
                Logger.Error("Mod.Call Error: " + e.StackTrace + e.Message);
            }
            return "Call Failed";
        }

        private string _versCache;
        public override void Load()
        {

            _versCache = Main.versionNumber;
            ContentInstance.Register(new Ambience());
            Main.versionNumber += $", Terraria Ambience v{Version}";

            SavingSystem.LoadFromConfig();
            Ambience.Initialize();

            On_Main.Update += Main_Update;

            MethodDetours.DetourAll();

            Ambience.PlayAllAmbience();
            var aLoader = Ambience.Instance;

            aLoader.dayCricketsVolume = 0f;
            aLoader.nightCricketsVolume = 0f;
            aLoader.eveningCricketsVolume = 0f;
            aLoader.desertCricketsVolume = 0f;
            aLoader.crackleVolume = 0f;
            aLoader.ugAmbienceVolume = 0f;
            aLoader.crimsonRumblesVolume = 0f;
            aLoader.snowDayVolume = 0f;
            aLoader.snowNightVolume = 0f;
            aLoader.corruptionRoarsVolume = 0f;
            aLoader.dayJungleVolume = 0f;
            aLoader.nightJungleVolume = 0f;
            aLoader.beachWavesVolume = 0f;
            aLoader.hellRumbleVolume = 0f;
            aLoader.rainVolume = 0f;
            aLoader.morningCricketsVolume = 0f;
            aLoader.breezeVolume = 0f;
        }

        private void Main_Update(On_Main.orig_Update orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);

            if (!Main.dedServ)
            {
                GeneralHelpers.ClickHandling();
                GeneralHelpers.UpdateButtons();

                Ambience.DoUpdate_Ambience();
                Ambience.UpdateVolume();
                if (Main.gameMenu)
                    return;
                Ambience.ClampAll();
            }
        }

        public override void PostSetupContent()
        {
            // Mod thor = ModLoader.GetMod("ThoriumMod");

            MenuDetours.Init();

            SoundChanges.Init();
            // TODO: Finish adding these another day...
            #region Calamity Adds
            if (ModLoader.TryGetMod("CalamityMod", out var calamity))
            {
                TileDetection.AddTilesToList(calamity, TileDetection.SnowyBlocks, "AstralSnow");
                TileDetection.AddTilesToList(calamity, TileDetection.SandBlocks,
                    "SulphurousSand",
                    "AstralSand",
                    "EutrophicSand",
                    "AstralClay",
                    "SulphurousSandNoWater");

                TileDetection.AddTilesToList(calamity, TileDetection.GrassBlocks,
                    "AstralDirt",
                    "AstralGrass");

                TileDetection.AddTilesToList(calamity, TileDetection.LeafBlocks,
                    "PlantyMush");

                TileDetection.AddTilesToList(calamity, TileDetection.DirtBlocks,
                    "PlantyMush",
                    "AstralDirt");
                TileDetection.AddTilesToList(calamity, TileDetection.IcyBlocks,
                    "AstralIce");
                TileDetection.AddTilesToList(calamity, TileDetection.GraniteAndMarbles,
                    "StatigelBlock");

                TileDetection.AddTilesToList(calamity, TileDetection.StoneBlocks,
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
                    "LaboratoryPanels", "Cinderplate", "SmoothVoidstone");
            }
            #endregion
            #region Thorium Adds
            if (ModLoader.TryGetMod("ThoriumMod", out var thor))
            {
                TileDetection.AddTilesToList(thor, TileDetection.StoneBlocks,
                    "ThoriumOre", "LifeQuartz", "MarineRock", "MarineRockMoss", "DepthsAmber", "PearlStone", "Aquaite", "DepthsOpal",
                    "SynthPlatinum", "DepthsOnyx", "DepthsSapphire", "DepthsEmerald", "DepthsTopaz", "DepthsAmethyst", "ScarletChestPlatform");

                TileDetection.AddTilesToList(thor, TileDetection.SandBlocks, "Brack", "BrackBare");
            }
            #endregion
        }
        public override void Unload()
        {
            MenuDetours.AddMenuButtonsHook = null;
            Main.versionNumber = _versCache;
        }
        public override void Close()
        {
            base.Close();
        }
        public float lastPlayerPositionOnGround;
        public float delta_lastPos_playerBottom;
    }
    public class VolumeResetterSystem : ModSystem
    {
        public override void PreSaveAndQuit()
        {
            var aLoader = Ambience.Instance;
            aLoader.dayCricketsVolume = 0f;
            aLoader.nightCricketsVolume = 0f;
            aLoader.eveningCricketsVolume = 0f;
            aLoader.desertCricketsVolume = 0f;
            aLoader.crackleVolume = 0f;
            aLoader.ugAmbienceVolume = 0f;
            aLoader.crimsonRumblesVolume = 0f;
            aLoader.snowDayVolume = 0f;
            aLoader.snowNightVolume = 0f;
            aLoader.corruptionRoarsVolume = 0f;
            aLoader.dayJungleVolume = 0f;
            aLoader.nightJungleVolume = 0f;
            aLoader.beachWavesVolume = 0f;
            aLoader.hellRumbleVolume = 0f;
            aLoader.rainVolume = 0f;
            aLoader.morningCricketsVolume = 0f;
            aLoader.breezeVolume = 0f;
        }
    }
}
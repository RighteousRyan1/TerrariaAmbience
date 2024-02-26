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
using NAudio.CoreAudioApi;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Content.Players;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Collections.Generic;

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
                                TileDetection.AddTilesToList(mod, FootstepHandler.GrassBlocks, s);
                            else if (listName == "Stone")
                                TileDetection.AddTilesToList(mod, FootstepHandler.StoneBlocks, s);
                            else if (listName == "Sand")
                                TileDetection.AddTilesToList(mod, FootstepHandler.SandBlocks, s);
                            else if (listName == "Snow")
                                TileDetection.AddTilesToList(mod, FootstepHandler.SnowBlocks, s);
                            else if (listName == "Dirt")
                                TileDetection.AddTilesToList(mod, FootstepHandler.DirtBlocks, s);
                            else if (listName == "Metal")
                                TileDetection.AddTilesToList(mod, FootstepHandler.MetalBlocks, s);
                            else if (listName == "Ice")
                                TileDetection.AddTilesToList(mod, FootstepHandler.IceBlocks, s);
                            else if (listName == "Leaf")
                                TileDetection.AddTilesToList(mod, FootstepHandler.LeafBlocks, s);
                            else if (listName == "Glass")
                                TileDetection.AddTilesToList(mod, FootstepHandler.GlassBlocks, s);
                            else if (listName == "GraniteMarble")
                                TileDetection.AddTilesToList(mod, FootstepHandler.MarblesGranites, s);
                            else if (listName == "SmoothStone")
                                TileDetection.AddTilesToList(mod, FootstepHandler.SmoothStones, s);
                            else if (listName == "Sticky")
                                TileDetection.AddTilesToList(mod, FootstepHandler.StickyBlocks, s);
                            else if (listName == "Gem")
                                TileDetection.AddTilesToList(mod, FootstepHandler.GemBlocks, s);
                            else {
                                Logger.Info("Mod.Call failure: Unknown tile list specified.");
                                return "Unknown tile list specified.";
                            }
                            Logger.Info($"Successfully added modded tiles ({string.Join(", ", s)}) to the {listName} tile list.");
                        }
                        else if (mod == null && boxedInstance is int[] i) {
                            if (listName == "Grass")
                                TileDetection.AddTilesToList(FootstepHandler.GrassBlocks, i);
                            else if (listName == "Stone")
                                TileDetection.AddTilesToList(FootstepHandler.StoneBlocks, i);
                            else if (listName == "Sand")
                                TileDetection.AddTilesToList(FootstepHandler.SandBlocks, i);
                            else if (listName == "Snow")
                                TileDetection.AddTilesToList(FootstepHandler.SnowBlocks, i);
                            else if (listName == "Dirt")
                                TileDetection.AddTilesToList(FootstepHandler.DirtBlocks, i);
                            else if (listName == "Metal")
                                TileDetection.AddTilesToList(FootstepHandler.MetalBlocks, i);
                            else if (listName == "Ice")
                                TileDetection.AddTilesToList(FootstepHandler.IceBlocks, i);
                            else if (listName == "Leaf")
                                TileDetection.AddTilesToList(FootstepHandler.LeafBlocks, i);
                            else if (listName == "Glass")
                                TileDetection.AddTilesToList(FootstepHandler.GlassBlocks, i);
                            else if (listName == "GraniteMarble")
                                TileDetection.AddTilesToList(FootstepHandler.MarblesGranites, i);
                            else if (listName == "SmoothStone")
                                TileDetection.AddTilesToList(FootstepHandler.SmoothStones, i);
                            else if (listName == "Sticky")
                                TileDetection.AddTilesToList(FootstepHandler.StickyBlocks, i);
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

        public static AmbientHandler DefaultAmbientHandler;
        public static FootstepHandler DefaultFootstepHandler;

        public override void Load()
        {

            _versCache = Main.versionNumber;
            Main.versionNumber += $", Terraria Ambience v{Version}";

            // calls DefaultAmbientHandler.Initialize() via the ctor
            // Ambience.Initialize();

            On_Main.Update += Main_Update;

            MethodDetours.DetourAll();
        }

        public static bool JustTurnedDay;
        public static bool JustTurnedNight;

        private static bool _curDay;
        private static bool _oldDay;
        private void Main_Update(On_Main.orig_Update orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);

            CraftSounds.TimeSinceLastCraft++;

            _curDay = Main.dayTime;

            JustTurnedDay = !_oldDay && _curDay;
            JustTurnedNight = _oldDay && !_curDay;

            if (!Main.dedServ)
            {
                GeneralHelpers.ClickHandling();
                GeneralHelpers.UpdateButtons();

                DefaultAmbientHandler?.Update();
                DefaultFootstepHandler?.Update();

                //Ambience.DoUpdate_Ambience();
                //Ambience.UpdateVolume();

                GradientGlobals.Update();

                if (Main.gameMenu)
                    return;
                //Ambience.ClampAll();
            }
            _oldDay = Main.dayTime;
        }
        public override void PostSetupContent() {
            if (!Main.dedServ) {
                MenuDetours.Init();

                SoundChanges.Init();
                // TODO: Finish adding these another day...
                #region Calamity Adds
                if (ModLoader.TryGetMod("CalamityMod", out var calamity)) {
                    IsCalLoaded = true;
                    CalBiomes = calamity.GetContent<ModBiome>().ToList();
                    TileDetection.AddTilesToList(calamity, FootstepHandler.SnowBlocks, "AstralSnow");
                    TileDetection.AddTilesToList(calamity, FootstepHandler.SandBlocks,
                        "SulphurousSand",
                        "AstralSand",
                        "EutrophicSand",
                        "AstralClay",
                        "SulphurousSandNoWater",
                        "CelestialRemains");

                    TileDetection.AddTilesToList(calamity, FootstepHandler.GrassBlocks,
                        "AstralGrass",
                        "VernalSoil");

                    TileDetection.AddTilesToList(calamity, FootstepHandler.MetalBlocks,
                        "ExoPrismPanelTile",
                        "ExoPlatingTile",
                        "ExoPrismPlatformTile",
                        "ExoPlatformTile",
                        "CosmilitePlatform", "CosmiliteBrick",
                        "PlaguedPlatePlatform",
                        "LaboratoryPlating",
                        "LaboratoryPipePlating",
                        "RustedShelf", "RustedPlating", "PlaguedPlate");
                    TileDetection.AddTilesToList(calamity, FootstepHandler.GemBlocks,
                        "SilvaCrystal");
                    TileDetection.AddTilesToList(calamity, FootstepHandler.LeafBlocks,
                        "PlantyMush");
                    TileDetection.AddTilesToList(calamity, FootstepHandler.GlassBlocks,
                        "SeaPrismBrick", "EutrophicGlass", "CryonicBrick");
                    TileDetection.AddTilesToList(calamity, FootstepHandler.DirtBlocks,
                        "PlantyMush",
                        "AstralDirt");
                    TileDetection.AddTilesToList(calamity, FootstepHandler.IceBlocks,
                        "AstralIce");
                    TileDetection.AddTilesToList(calamity, FootstepHandler.MarblesGranites,
                        "StatigelBlock", "StatigelPlatform",
                        "SmoothNavystone",
                        "SmoothBrimstoneSlag");
                    TileDetection.AddTilesToList(calamity, FootstepHandler.SmoothStones,
                        "AshenSlab", "AshenAccentSlab", "AshenPlatform", "SmoothAbyssGravel",
                        "OccultBrickTile", "SilvaPlatform", "OccultPlatformTile",
                        "RunicProfanedBrick", "VoidstoneSlab", "ProfanedSlab", "ScoriaBrick",
                        "PerennialBrick", "AerialiteBrick", "BrimstoneSlab", "AstralBrick");
                    TileDetection.AddTilesToList(calamity, FootstepHandler.StoneBlocks,
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
                }
                #endregion
                #region Thorium Adds
                if (ModLoader.TryGetMod("ThoriumMod", out var thor)) {
                    // multiple sounds for things like CloudSlab because snowy + stone-y sounds would probably fit!
                    TileDetection.AddTilesToList(thor, FootstepHandler.StoneBlocks,
                        "ThoriumOre", "LifeQuartz", "MarineRock", "MarineRockMoss", "DepthsAmber", "PearlStone", "Aquaite", "DepthsOpal",
                        "SynthPlatinum", "DepthsOnyx", "DepthsSapphire", "DepthsEmerald", "DepthsTopaz", "DepthsAmethyst", "ScarletChestPlatform",
                        "SugarCookieBlockNew", "BloodstainedBlock", "Aquamarine",
                        "CursedBlockNew", "OrnateBlock", "SynthGold", "ShadyBlock", "OrnatePlatform");
                    TileDetection.AddTilesToList(thor, FootstepHandler.SmoothStones,
                        "CelestialBrick", "CloudSlab", "CutStoneBlock", "CutSandstoneBlock", 
                        "CutStoneBlockSlab", "CutSandstoneBlockSlab", "NagaBlockNew", "CelestialPlatform", "NagaPlatform");
                    TileDetection.AddTilesToList(thor, FootstepHandler.GrassBlocks,
                        "SpookyAstroturf", "CherryAstroturf");
                    TileDetection.AddTilesToList(thor, FootstepHandler.MarblesGranites,
                        "CheckeredBrickTile", "RefinedMarineBlock");
                    TileDetection.AddTilesToList(thor, FootstepHandler.SnowBlocks,
                        "SnowyAstroturf", "CloudSlab");
                    TileDetection.AddTilesToList(thor, FootstepHandler.GemBlocks,
                        "AquamarineGemsparkNew", "ThoriumBrick", "ThoriumBrickBlock", "ThoriumPlatform");
                    TileDetection.AddTilesToList(thor, FootstepHandler.SandBlocks, "Brack", "BrackBare");
                }
                #region SpookyMod
                /*if (ModLoader.TryGetMod("SpookyMod", out var thor)) {
                    TileDetection.AddTilesToList(thor, FootstepHandler.StoneBlocks,
                        "ThoriumOre", "LifeQuartz", "MarineRock", "MarineRockMoss", "DepthsAmber", "PearlStone", "Aquaite", "DepthsOpal",
                        "SynthPlatinum", "DepthsOnyx", "DepthsSapphire", "DepthsEmerald", "DepthsTopaz", "DepthsAmethyst", "ScarletChestPlatform");

                    TileDetection.AddTilesToList(thor, FootstepHandler.SandBlocks, "Brack", "BrackBare");
                }*/
                #endregion
                #endregion

                var campfireCrackle = Assets.Request<SoundEffect>("Sounds/Custom/ambient/environment/campfire_crackle", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                DefaultAmbientHandler = new();
                DefaultFootstepHandler = new();
                DefaultAmbientHandler.CampfireCrackleInstance = campfireCrackle.CreateInstance(); // what's crashing?
                DefaultAmbientHandler.CampfireCrackleInstance.IsLooped = true;
            }
        }

        public static bool IsCalLoaded;
        public static List<ModBiome> CalBiomes;
        public override void Unload()
        {
            MenuDetours.AddMenuButtonsHook = null;
            Main.versionNumber = _versCache;
        }
        // this seems like fps loss waiting to happen. oh well.
        public static bool IsPlayerInCalBiome(Player player, string calBiomeName) {
            var b = CalBiomes.FirstOrDefault(x => x.Name == calBiomeName);
            if (CalBiomes.IndexOf(b) == -1)
                return false;
            return player.InModBiome(b);
        }
        public float lastPlayerPositionOnGround;
        public float delta_lastPos_playerBottom;

        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            var pos = reader.ReadVector2();
            var path = reader.ReadString();

            if (Main.netMode == NetmodeID.Server) {
                var p = GetPacket();
                p.WriteVector2(pos);
                p.Write(path);
                p.Send(ignoreClient: whoAmI);
            }
            else {
                var volScale = ModContent.GetInstance<GeneralConfig>().craftingSoundsVolume;
                SoundEngine.PlaySound(new SoundStyle(path).WithVolumeScale(CraftSounds.UniversalSoundScale * volScale), pos);
            }
        }
    }
}
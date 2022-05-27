using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using TerrariaAmbience.Core;
using System.Linq;
using System.IO;
using TerrariaAmbience.Content.Players;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;
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
            try
            {
                string message = args[0] as string;
                if (message == "AddTilesToList")
                {
                    Mod mod = args[1] as Mod;
                    string listName = args[2] as string; // Can be Stone, Grass, Sand, Snow, or Dirt (FOR NOW, OR EVER) (UPDATE THIS)
                    string[] nameStringList = args[3] as string[];
                    int[] tiles = args[4] as int[];
                    if (!Main.dedServ)
                        if (mod != null)
                        {
                            if (listName == "Grass")
                                TileDetection.AddTilesToList(mod, TileDetection.GrassBlocks, nameStringList);
                            else if (listName == "Stone")
                                TileDetection.AddTilesToList(mod, TileDetection.StoneBlocks, nameStringList);
                            else if (listName == "Sand")
                                TileDetection.AddTilesToList(mod, TileDetection.SandBlocks, nameStringList);
                            else if (listName == "Snow")
                                TileDetection.AddTilesToList(mod, TileDetection.SnowyBlocks, nameStringList);
                            else if (listName == "Dirt")
                                TileDetection.AddTilesToList(mod, TileDetection.DirtBlocks, nameStringList);
                        }
                        else if (mod == null)
                        {
                            if (listName == "Grass")
                                TileDetection.AddTilesToList(TileDetection.GrassBlocks, tiles);
                            else if (listName == "Stone")
                                TileDetection.AddTilesToList(TileDetection.StoneBlocks, tiles);
                            else if (listName == "Sand")
                                TileDetection.AddTilesToList(TileDetection.SandBlocks, tiles);
                            else if (listName == "Snow")
                                TileDetection.AddTilesToList(TileDetection.SnowyBlocks, tiles);
                            else if (listName == "Dirt")
                                TileDetection.AddTilesToList(TileDetection.DirtBlocks, tiles);
                        }
                    return "Tiles added successfully!";
                }
                else
                {
                    Logger.Error("Call Error: Unknown Message: " + message);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Mod.Call Error: " + e.StackTrace + e.Message);
            }
            return "Call Failed";
        }

        private string _versCache;
        public override void Load()
        {

            _versCache = Main.versionNumber;
            ContentInstance.Register(new Ambience());
            Main.versionNumber += $"\nTerraria Ambience v{Version}";

            SavingSystem.LoadFromConfig();
            Ambience.Initialize();

            On.Terraria.Main.Update += Main_Update;

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

        private void Main_Update(On.Terraria.Main.orig_Update orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);
            GeneralHelpers.ClickHandling();
            GeneralHelpers.UpdateButtons();

            Ambience.DoUpdate_Ambience();
            Ambience.UpdateVolume();
            if (Main.gameMenu) 
                return;
            Ambience.ClampAll();
        }

        public override void PostSetupContent()
        {
            // Mod calamity = ModLoader.GetMod("CalamityMod");
            // Mod thor = ModLoader.GetMod("ThoriumMod");

            SoundChanges.Init();
            // TODO: Finish adding these another day...
            /*
            #region Calamity Adds
            if (calamity != null)
            {
                TileDetection.AddTilesToList(calamity, TileDetection.snowyblocks, "AstralSnow");
                TileDetection.AddTilesToList(calamity, TileDetection.sandBlocks,
                    "SulphurousSand",
                    "AstralSand",
                    "EutrophicSand",
                    "AstralClay");

                TileDetection.AddTilesToList(calamity, TileDetection.grassTiles,
                    "AstralDirt",
                    "AstralGrass",
                    "PlantyMush");

                TileDetection.AddTilesToList(calamity, TileDetection.stoneBlocks,
                    "AstralOre",
                    "AstralStone",
                    "AstralMonolith",
                    "AstralMonolith", "AbyssGravel", "Tenebris", "ChaoticOre", "SmoothCoal", "Voidstone", "Navystone",
                    "TableCoral", "SeaPrism", "HazardChevronPanels", "NavyPlate", "LaboratoryPanels", "LaboratoryPlating", "RustedPlating",
                    "HardenedSulphurousSandstone");
            }
            #endregion
            #region Thorium Adds
            if (thor != null)
            {
                TileDetection.AddTilesToList(thor, TileDetection.stoneBlocks,
                    "ThoriumOre", "LifeQuartz", "MarineRock", "MarineRockMoss", "DepthsAmber", "PearlStone", "Aquaite", "DepthsOpal",
                    "SynthPlatinum", "DepthsOnyx", "DepthsSapphire", "DepthsEmerald", "DepthsTopaz", "DepthsAmethyst", "ScarletChestPlatform");

                TileDetection.AddTilesToList(thor, TileDetection.sandBlocks, "Brack", "BrackBare");
            }
            #endregion
            */
        }
        public override void Unload()
        {
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
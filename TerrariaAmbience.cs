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

namespace TerrariaAmbience
{
    public partial class TerrariaAmbience : Mod
	{
        public override object Call(params object[] args)
        {
            try
            {
                string message = args[0] as string;
                if (message == "AddTilesToList")
                {
                    Mod mod = args[1] as Mod;
                    string listName = args[2] as string; // Can be Stone, Grass, Sand, Snow, or Dirt (FOR NOW, OR EVER)
                    string[] nameStringList = args[3] as string[];
                    int[] tiles = args[4] as int[];
                    if (!Main.dedServ)
                        if (mod != null)
                        {
                            if (listName == "Grass")
                                TileDetection.AddTilesToList(mod, TileDetection.grassTiles, nameStringList);
                            else if (listName == "Stone")
                                TileDetection.AddTilesToList(mod, TileDetection.stoneBlocks, nameStringList);
                            else if (listName == "Sand")
                                TileDetection.AddTilesToList(mod, TileDetection.sandBlocks, nameStringList);
                            else if (listName == "Snow")
                                TileDetection.AddTilesToList(mod, TileDetection.snowyblocks, nameStringList);
                            else if (listName == "Dirt")
                                TileDetection.AddTilesToList(mod, TileDetection.dirtBlocks, nameStringList);
                        }
                        else if (mod == null)
                        {
                            if (listName == "Grass")
                                TileDetection.AddTilesToList(TileDetection.grassTiles, tiles);
                            else if (listName == "Stone")
                                TileDetection.AddTilesToList(TileDetection.stoneBlocks, tiles);
                            else if (listName == "Sand")
                                TileDetection.AddTilesToList(TileDetection.sandBlocks, tiles);
                            else if (listName == "Snow")
                                TileDetection.AddTilesToList(TileDetection.snowyblocks, tiles);
                            else if (listName == "Dirt")
                                TileDetection.AddTilesToList(TileDetection.dirtBlocks, tiles);
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
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
        }
        public override void Load()
        {
            ContentInstance.Register(new Ambience());
            string path = Path.Combine(ModLoader.ModPath, "Cache//ta_secretconfig.txt");

            string replacePath = Path.Combine(ModLoader.ModPath, "TASoundReplace");

            if (!Directory.Exists(replacePath))
            {
                Directory.CreateDirectory(replacePath);
            }
            File.WriteAllText(Path.Combine(replacePath, "README.txt"), "If you are replacing sounds, please read the index/key below for the sounds you might want to replace." +
                    "\nThese replacements will ONLY work if you put it in here word-for-word.\n"
                    + "\nforest_morning\nforest_day\nforest_evening\nforest_night\ndesert_crickets\nsnowfall_day\nsnowfall_night\njungle_day\njungle_night\ncorruption_roars"
                    + "\ncrimson_rumbles\nug_drip\nhell_rumble\nbeach_waves\nbreeze\nrain_new\n\nPlease, do note that these files must be in the WAVE format (.wav)!");

            if (File.Exists(path) && File.ReadAllLines(path)[0] != null)
            {
                string[] lines = File.ReadAllLines(path);

                Ambience.TAAmbient = float.Parse(lines[0]);
            }
            else
            {
                Ambience.TAAmbient = 100f;
            }
            if (File.Exists(path) && File.ReadAllLines(path)[1] != null)
            {
                string[] lines = File.ReadAllLines(path);

                Ambience.fStepsVol = float.Parse(lines[1]);
            }
            else
            {
                Ambience.fStepsVol = 100f;
            }

            Ambience.Initialize();
            On.Terraria.Main.DoUpdate += Main_DoUpdate;
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
        public override void PostSetupContent()
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            Mod thor = ModLoader.GetMod("ThoriumMod");

            SoundChanges.Init();
            // TODO: Finish adding these another day...
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
        }
        public override void Unload()
        {
            string path = "";
            if (Directory.Exists(Path.Combine(ModLoader.ModPath, "Cache")))
            {
                path = Path.Combine(ModLoader.ModPath, "Cache//ta_secretconfig.txt");
                if (File.Exists(path))
                    File.WriteAllText(path, Ambience.TAAmbient.ToString() + $"\n{Ambience.fStepsVol}\n\nDO NOT CHANGE THESE NUMBERS.");
                else
                    Logger.Error("Failed to write save data for TerrariaAmbience volume config.");
            }
            else
            {
                Logger.Error("Failed to write save data for TerrariaAmbience volume config.");
            }
            SoundChanges.Unload();
        }
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


            string path = "";
            if (Directory.Exists(Path.Combine(ModLoader.ModPath, "Cache")))
            {
                path = Path.Combine(ModLoader.ModPath, "Cache//ta_secretconfig.txt");
                if (File.Exists(path))
                    File.WriteAllText(path, Ambience.TAAmbient.ToString() + $"\n{Ambience.fStepsVol}\n\nDO NOT CHANGE THESE NUMBERS.");
                else
                    Logger.Error("Failed to write save data for TerrariaAmbience volume config.");
            }
            else
            {
                Logger.Error("Failed to write save data for TerrariaAmbience volume config.");
            }
        }
        public override void Close()
        {
            base.Close();
        }
        public float lastPlayerPositionOnGround;
        public float delta_lastPos_playerBottom;
        private void Main_DoUpdate(On.Terraria.Main.orig_DoUpdate orig, Main self, GameTime gameTime)
        {
            GeneralHelpers.ClickHandling();
            GeneralHelpers.UpdateButtons();
            orig(self, gameTime);
            var aLoader = Ambience.Instance;

            Ambience.DoUpdate_Ambience();
            Ambience.UpdateVolume();
            if (Main.gameMenu) return;
            Player player = Main.player[Main.myPlayer]?.GetModPlayer<FootstepsPlayer>().player;
            // soundInstancer.Condition = player.ZoneSkyHeight;
            if (Main.hasFocus)
            {
                delta_lastPos_playerBottom = lastPlayerPositionOnGround - player.Bottom.Y;

                // Main.NewText(delta_lastPos_playerBottom);
                if (ModContent.GetInstance<AmbientConfigServer>().newSplashes)
                {
                    if (player.velocity.Y <= 0 || player.teleporting)
                    {
                        lastPlayerPositionOnGround = player.Bottom.Y;
                    }
                    // Main.NewText(player.fallStart - player.Bottom.Y);
                    if (delta_lastPos_playerBottom > -300)
                    {
                        Main.soundSplash[0] = Ambience.SplashNew;
                    }
                    else if (delta_lastPos_playerBottom <= -300)
                    {
                        Main.soundSplash[0] = GetSound($"{Ambience.AmbientPath}/environment/liquid/entity_splash_heavy");
                    }
                }
            }
            Ambience.ClampAll();
        }
    }
    public class CampfireDetection : GlobalTile
    {
        public Vector2 originOfCampfire;

        public float distanceOf;

        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            originOfCampfire.X = i * 16;
            originOfCampfire.Y = j * 16;
            Player player = Main.player[Main.myPlayer].GetModPlayer<FootstepsPlayer>().player;

            if (type == TileID.Campfire && closer && player.HasBuff(BuffID.Campfire))
            {
                /*var t = Main.tile[i, j];
                var orig = new Vector2(i * 16, j * 16);
                Main.NewText(t.frameY);*/
                //if (t.frameY <= 18 && t.frameY >= 0)
                {
                    distanceOf = Vector2.Distance(originOfCampfire, player.Center);
                    player.GetModPlayer<FootstepsPlayer>().isNearCampfire = true;
                }
            }
            if ((type == TileID.Campfire && !closer) || !player.HasBuff(BuffID.Campfire))
            {
                /*var t = Main.tile[i, j];
                var orig = new Vector2(i * 16, j * 16);
                Main.NewText(t.frameY);*/
                //if (t.frameY <= 54 && t.frameY >= 36)
                {
                    Ambience.CampfireCrackleInstance.Volume = 0f;
                    player.GetModPlayer<FootstepsPlayer>().isNearCampfire = false;
                }
            }
        }
    }
    public class NewDoorKnockSound : GlobalNPC
    {
        public override void AI(NPC npc)
        {
            if (npc.aiStyle == NPCID.Zombie)
            {
                /*Tile tileToLeft = Main.tile[(int)npc.Left.X / 16 - 1, (int)npc.Left.Y / 16];
                Tile tileToRight = Main.tile[(int)npc.Right.X / 16 + 1, (int)npc.Right.Y / 16];
                if (TileLoader.IsClosedDoor(tileToLeft) || TileLoader.IsClosedDoor(tileToRight) && npc.ai[0] == 20)
                {
                    // God damn, wtf do i do
                }*/
            }
        }
    }
}
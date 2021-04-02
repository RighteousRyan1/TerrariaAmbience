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

namespace TerrariaAmbience.Content
{
    public partial class TerrariaAmbience : Mod
	{
        public object x;
        public override object Call(params object[] args)
        {
            try
            {
                string message = args[0] as string;
                if (message == "AddTilesToList")
                {
                    Mod mod = args[1] as Mod;
                    string listName = args[2] as string; // Can be Stone, Grass, Sand, or Snow (FOR NOW, OR EVER)
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
                        }
                    return "Tiles added successfully!";
                }
                else if (message == "AddAmbience")
                {
                    Mod mod = args[1] as Mod;
                    string soundPath = args[2] as string;
                    string name = args[3] as string;
                    bool? checkConditional = args[4] as bool?;
                    if (!Main.dedServ)
                        x = new ModAmbience(mod,soundPath, name, checkConditional.Value);
                    return x;
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
            /*int pktType = reader.ReadInt32();
            Player player = Main.player[Main.myPlayer].GetModPlayer<AmbiencePlayer>().player;
            if (Main.netMode == NetmodeID.Server)
            {
                var nPacket = GetPacket();

                nPacket.Write(pktType);
                nPacket.Send();

                Main.NewText($"packetType: {pktType}, whoAmI: {whoAmI}");
            }
            else
            {
                switch (pktType)
                {
                    case 1:
                        Main.PlaySound(GetLegacySoundSlot(SoundType.Custom, CraftSounds.AnvilsSoundDir), player.Center).Volume *= .8f;
                        break;
                    case 2:
                        Main.PlaySound(GetLegacySoundSlot(SoundType.Custom, CraftSounds.WorkBenchesSoundDir), player.Center).Volume *= .8f;
                        break;
                    case 3:
                        Main.PlaySound(GetLegacySoundSlot(SoundType.Custom, CraftSounds.FurnaceSoundDir), player.Center).Volume *= .8f;
                        break;
                    case 4:
                        Main.PlaySound(GetLegacySoundSlot(SoundType.Custom, CraftSounds.BooksSoundDir), player.Center).Volume *= .8f;
                        break;
                }
                Main.NewText(pktType);
            }*/

            var pkt = GetPacket();
            pkt.Write(Main.player[Main.myPlayer].GetModPlayer<FootstepsAndAmbiencePlayer>().playerCraftType);

            pkt.Send();

            reader.Read();
        }
        public override void Load()
        {
            string path = $"C://Users//{Environment.UserName}//Documents//My Games//Terraria//ModLoader//Mods//Cache//ta_secretconfig.txt";

            if (File.Exists(path) && File.ReadAllLines(path)[0] != null)
            {
                string[] lines = File.ReadAllLines(path);

                Ambience.TAAmbient = float.Parse(lines[0]);
            }
            else
            {
                Ambience.TAAmbient = 100f;
            }

            Ambience.Initialize();
            On.Terraria.Main.DoUpdate += Main_DoUpdate;
            MethodDetours.DetourAll();
        }
        public override void PostSetupContent()
        {
            ModAmbience.Initialize();
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
            string path = $"C://Users//{Environment.UserName}//Documents//My Games//Terraria//ModLoader//Mods//Cache//ta_secretconfig.txt";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, Ambience.TAAmbient.ToString() + "\n\nDO NOT CHANGE THIS NUMBER.");
            ModAmbience.Unload();
            SoundChanges.Unload();
        }
        public override void Close()
        {
            for (int i = 0; i < ModAmbience.allAmbiences.Count; i++)
            {
                ModAmbience.allAmbiences[i] = null;
            }
            base.Close();
        }
        public float lastPlayerPositionOnGround;
        public float delta_lastPos_playerBottom;
        private void Main_DoUpdate(On.Terraria.Main.orig_DoUpdate orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);
            var aLoader = Ambience.Instance;

            Ambience.DoUpdate_Ambience();
            Ambience.UpdateVolume();
            if (Main.gameMenu)
            {
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
            if (Main.gameMenu) return;
            Player player = Main.player[Main.myPlayer]?.GetModPlayer<FootstepsAndAmbiencePlayer>().player;
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
                        Main.soundSplash[0] = Ambience.Instance.SplashNew;
                    }
                    else if (delta_lastPos_playerBottom <= -300)
                    {
                        Main.soundSplash[0] = GetSound($"{Ambience.ambienceDirectory}/environment/liquid/entity_splash_heavy");
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
            Player player = Main.player[Main.myPlayer].GetModPlayer<FootstepsAndAmbiencePlayer>().player;

            if (type == TileID.Campfire && closer && player.HasBuff(BuffID.Campfire))
            {
                distanceOf = Vector2.Distance(originOfCampfire, player.Center);
                player.GetModPlayer<FootstepsAndAmbiencePlayer>().isNearCampfire = true;
            }
            if ((type == TileID.Campfire && !closer) || !player.HasBuff(BuffID.Campfire))
            {
                Ambience.Instance.CampfireCrackleInstance.Volume = 0f;
                player.GetModPlayer<FootstepsAndAmbiencePlayer>().isNearCampfire = false;
            }
        }
    }
    public class NewDoorKnockSound :GlobalNPC
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
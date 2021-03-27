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

namespace TerrariaAmbience.Content
{
	public partial class TerrariaAmbience : Mod
	{
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
            ModAmbience.Initialize();
            Ambience.Initialize();
            On.Terraria.Main.DoUpdate += Main_DoUpdate;
            MethodDetours.DetourAll();
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
            }
            if (Main.gameMenu) return;
            Player player = Main.player[Main.myPlayer]?.GetModPlayer<FootstepsAndAmbiencePlayer>().player;
            // soundInstancer.Condition = player.ZoneSkyHeight;
            if (Main.hasFocus)
            {
                delta_lastPos_playerBottom = lastPlayerPositionOnGround - player.Bottom.Y;

                // Main.NewText(delta_lastPos_playerBottom);
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
            Ambience.ClampAll();
        }
    }
    public static class ExtensionMethods
    {
        public static bool ZoneForest(this Player player)
        {
            return !player.ZoneJungle
                   && !player.ZoneDungeon
                   && !player.ZoneCorrupt
                   && !player.ZoneCrimson
                   && !player.ZoneHoly
                   && !player.ZoneSnow
                   && !player.ZoneUndergroundDesert
                   && !player.ZoneGlowshroom
                   && !player.ZoneMeteor
                   && !player.ZoneBeach
                   && !player.ZoneDesert
                   && player.ZoneOverworldHeight;
        }
        public static bool ZoneUnderground(this Player player)
        {
            return player.Center.Y >= Main.rockLayer * 16 && !player.ZoneUnderworldHeight;
        }
        public static bool HeadWet(this Player player)
        {
            return Main.tile[(int)player.Top.X / 16, (int)(player.Top.Y - 1.25) / 16].liquid > 0;
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
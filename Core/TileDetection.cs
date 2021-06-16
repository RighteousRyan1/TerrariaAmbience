using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Content;
using TerrariaAmbience.Content.Players;

namespace TerrariaAmbience.Core
{
    public class TileDetection : GlobalTile
    {
        public static int curTileType;

        public static List<int> grassTiles = new List<int>
        {
                TileID.Grass,
                TileID.BlueMoss,
                TileID.BrownMoss,
                TileID.GreenMoss,
                TileID.LavaMoss,
                TileID.LongMoss,
                TileID.PurpleMoss,
                TileID.RedMoss,
                TileID.JungleGrass,
                TileID.CorruptGrass,
                TileID.FleshGrass,
                TileID.HallowedGrass,
                TileID.LivingMahoganyLeaves,
                TileID.LeafBlock,
                TileID.MushroomGrass,
                TileID.HoneyBlock,
                TileID.CrispyHoneyBlock,
                TileID.PumpkinBlock,
        };
        public static List<int> dirtBlocks = new List<int>
        {
            TileID.Dirt,
            TileID.ClayBlock,
            TileID.Mud,
            TileID.Silt,
            TileID.Slush,
        };
        public static List<int> stoneBlocks = new List<int>
        {               
                TileID.Asphalt,
                TileID.Stone,
                TileID.StoneSlab,
                TileID.ActiveStoneBlock,
                TileID.GrayBrick,
                TileID.IceBlock,
                TileID.HallowedIce,
                TileID.CorruptIce,
                TileID.FleshIce,
                TileID.PinkDungeonBrick,
                TileID.GreenDungeonBrick,
                TileID.BlueDungeonBrick,
                TileID.Granite,
                TileID.Marble,
                TileID.MarbleBlock,
                TileID.GraniteBlock,
                TileID.Diamond,
                TileID.DiamondGemspark,
                TileID.DiamondGemsparkOff,
                TileID.Ruby,
                TileID.RubyGemspark,
                TileID.RubyGemsparkOff,
                TileID.Topaz,
                TileID.TopazGemspark,
                TileID.TopazGemsparkOff,
                TileID.Sapphire,
                TileID.SapphireGemspark,
                TileID.SapphireGemsparkOff,
                TileID.Amethyst,
                TileID.AmethystGemspark,
                TileID.AmethystGemsparkOff,
                TileID.Emerald,
                TileID.EmeraldGemspark,
                TileID.EmeraldGemsparkOff,
                TileID.AmberGemspark,
                TileID.AmberGemsparkOff,
                TileID.LihzahrdBrick,
                TileID.Ebonstone,
                TileID.EbonstoneBrick,
                TileID.FleshBlock,
                TileID.Crimstone,
                TileID.CrimsonSandstone,
                TileID.CorruptHardenedSand,
                TileID.CorruptSandstone,
                TileID.CrimsonHardenedSand,
                TileID.HardenedSand,
                TileID.Sandstone,
                TileID.HallowSandstone,
                TileID.SandstoneBrick,
                TileID.SandStoneSlab,
                TileID.HallowHardenedSand,
                TileID.Sunplate,
                TileID.Obsidian,
                TileID.Pearlstone,
                TileID.PearlstoneBrick,
                TileID.Mudstone,
                TileID.IridescentBrick,
                TileID.CobaltBrick,
                TileID.MythrilBrick,
                TileID.MythrilAnvil,
                TileID.Adamantite,
                TileID.Mythril,
                TileID.Cobalt,
                TileID.Titanium,
                TileID.Titanstone,
                TileID.Palladium,
                TileID.MetalBars,
                TileID.LunarOre,
                TileID.ObsidianBrick,
                TileID.SnowBrick,
                TileID.GreenCandyCaneBlock,
                TileID.CandyCaneBlock,
                TileID.GrayStucco,
                TileID.GreenStucco,
                TileID.RedStucco,
                TileID.YellowStucco,
                TileID.Copper,
                TileID.CopperBrick,
                TileID.Tin,
                TileID.TinBrick,
                TileID.Silver,
                TileID.SilverBrick,
                TileID.Tungsten,
                TileID.TungstenBrick,
                TileID.Iron,
                TileID.Lead,
                TileID.Gold,
                TileID.GoldBrick,
                TileID.Platinum,
                TileID.PlatinumBrick,
                TileID.Hellstone,
                TileID.HellstoneBrick,
                TileID.Asphalt,
                TileID.FossilOre,
                TileID.DesertFossil,
                TileID.RedBrick
        };
        public static List<int> sandBlocks = new List<int>
        {
                TileID.Sand,
                TileID.Ebonsand,
                TileID.Crimsand,
                TileID.Ash,
                TileID.Glass,
                TileID.Pearlsand
        };
        public static List<int> snowyblocks = new List<int>
        {
                TileID.SnowBlock,
                TileID.SnowCloud,
                TileID.RainCloud,
                TileID.Cloud,
        };
        public override void FloorVisuals(int type, Player player)
        {
            curTileType = type;

            var modPlayer = player.GetModPlayer<FootstepsPlayer>();
            if (grassTiles.Any(x => x == type))
                modPlayer.isOnGrassyTile = true;
            else
                modPlayer.isOnGrassyTile = false;

            if (snowyblocks.Any(x => x == type))
                modPlayer.isOnSnowyTile = true;
            else
                modPlayer.isOnSnowyTile = false;

            if (sandBlocks.Any(x => x == type))
                modPlayer.isOnSandyTile = true;
            else
                modPlayer.isOnSandyTile = false;

            // TODO: Include ore bricks later, as well as the ore themselves
            if (stoneBlocks.Any(x => x == type))
                modPlayer.isOnStoneTile = true;
            else
                modPlayer.isOnStoneTile = false;

            if (!modPlayer.isOnStoneTile && !modPlayer.isOnGrassyTile && !modPlayer.isOnSandyTile && !modPlayer.isOnSnowyTile && !modPlayer.isOnDirtyTile)
                modPlayer.isOnWoodTile = true;
            else
                modPlayer.isOnWoodTile = false;

            if (dirtBlocks.Any(x => x == type))
                modPlayer.isOnDirtyTile = true;
            else
                modPlayer.isOnDirtyTile = false;
        }
        /// <summary>
        /// Access this for when you want to add a modded tile to the valid tiles list. Use the ID for the tile.
        /// </summary>
        /// <param name="listToAddTo"></param>
        /// <param name="tiles"></param>
        public static void AddTilesToList(List<int> listToAddTo, params int[] tiles)
        {
            foreach (int tile in tiles)
            {
                listToAddTo.Add(tile);
            }
        }
        /// <summary>
        /// Access this for when you want to add a modded tile to the valid tiles list, use the tile's internal name for the tile.
        /// </summary>
        /// <param name="listToAddTo"></param>
        /// <param name="tiles"></param>
        public static void AddTilesToList(Mod mod, List<int> listToAddTo, params string[] name)
        {
            foreach (string tile in name)
            {
                listToAddTo.Add(mod.TileType(tile));
            }
        }
    }
}

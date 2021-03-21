using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaAmbience
{
    public class TileDetection : GlobalTile
    {
        public static List<int> grassTiles = new List<int>
        {
                TileID.Grass,
                TileID.Dirt,
                TileID.BlueMoss,
                TileID.BrownMoss,
                TileID.GreenMoss,
                TileID.LavaMoss,
                TileID.LongMoss,
                TileID.PurpleMoss,
                TileID.RedMoss,
                TileID.JungleGrass,
                TileID.Mud,
                TileID.CorruptGrass,
                TileID.FleshGrass,
                TileID.HallowedGrass,
                TileID.LivingMahoganyLeaves,
                TileID.LeafBlock,
                TileID.MushroomGrass,
                TileID.ClayBlock,
                TileID.Cloud,
                TileID.Asphalt,
                TileID.RainCloud,
                TileID.HoneyBlock,
                TileID.CrispyHoneyBlock,
                TileID.PumpkinBlock,
        };
        public static List<int> stoneBlocks = new List<int>
        {
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
                TileID.HellstoneBrick
        };
        public static List<int> sandBlocks = new List<int>
        {
                TileID.Sand,
                TileID.Ebonsand,
                TileID.Crimsand,
                TileID.Slush,
                TileID.Silt,
                TileID.Ash,
                TileID.Glass
        };

        public static List<int> snowyblocks = new List<int>
        {
                TileID.SnowBlock,
                TileID.SnowCloud,
                TileID.RainCloud,
                TileID.Cloud
        };
        public override void FloorVisuals(int type, Player player)
        {

            if (grassTiles.Any(x => x == type))
                player.GetModPlayer<AmbiencePlayer>().isOnGrassyTile = true;
            else
                player.GetModPlayer<AmbiencePlayer>().isOnGrassyTile = false;

            if (snowyblocks.Any(x => x == type))
                player.GetModPlayer<AmbiencePlayer>().isOnSnowyTile = true;
            else
                player.GetModPlayer<AmbiencePlayer>().isOnSnowyTile = false;

            if (sandBlocks.Any(x => x == type))
                player.GetModPlayer<AmbiencePlayer>().isOnSandyTile = true;
            else
                player.GetModPlayer<AmbiencePlayer>().isOnSandyTile = false;

            // TODO: Include ore bricks later, as well as the ore themselves
            if (stoneBlocks.Any(x => x == type))
                player.GetModPlayer<AmbiencePlayer>().isOnStoneTile = true;
            else
                player.GetModPlayer<AmbiencePlayer>().isOnStoneTile = false;

            if (!player.GetModPlayer<AmbiencePlayer>().isOnStoneTile && !player.GetModPlayer<AmbiencePlayer>().isOnGrassyTile && !player.GetModPlayer<AmbiencePlayer>().isOnSandyTile && !player.GetModPlayer<AmbiencePlayer>().isOnSnowyTile)
                player.GetModPlayer<AmbiencePlayer>().isOnWoodTile = true;
            else
                player.GetModPlayer<AmbiencePlayer>().isOnWoodTile = false;
        }
    }
}

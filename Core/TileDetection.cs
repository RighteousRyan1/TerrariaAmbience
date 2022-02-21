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
        // TileID.GreenCandyCaneBlock,
        // TileID.CandyCaneBlock,
        public static List<int> GrassBlocks { get; private set; } = new()
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
                TileID.CrimsonGrass,
                TileID.HallowedGrass,
                TileID.MushroomGrass,
                TileID.ArgonMoss,
                TileID.XenonMoss,
                TileID.KryptonMoss,
                TileID.GolfGrass,
                TileID.GolfGrassHallowed
        };
        public static List<int> DirtBlocks { get; private set; } = new()
        {
            TileID.Dirt,
            TileID.ClayBlock,
            TileID.Silt,
            TileID.Slush,
        };
        public static List<int> StoneBlocks { get; private set; } = new()
        {               
                TileID.Asphalt,
                TileID.Stone,
                TileID.ActiveStoneBlock,
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
                TileID.Ebonstone,
                TileID.FleshBlock,
                TileID.Crimstone,
                TileID.CrimsonSandstone,
                TileID.CorruptHardenedSand,
                TileID.CorruptSandstone,
                TileID.CrimsonHardenedSand,
                TileID.HardenedSand,
                TileID.Sandstone,
                TileID.HallowSandstone,
                TileID.HallowHardenedSand,
                TileID.Sunplate,
                TileID.Obsidian,
                TileID.Pearlstone,
                TileID.Mudstone,
                TileID.MythrilAnvil,
                TileID.Adamantite,
                TileID.Mythril,
                TileID.Cobalt,
                TileID.Titanium,
                TileID.Titanstone,
                TileID.Palladium,
                TileID.LunarOre,
                TileID.Copper,
                TileID.Tin,
                TileID.Silver,
                TileID.Tungsten,
                TileID.Iron,
                TileID.Lead,
                TileID.Gold,
                TileID.Platinum,
                TileID.Hellstone,
                TileID.FossilOre,
                TileID.DesertFossil,
                TileID.ShellPile,
				TileID.Meteorite,
				TileID.Demonite
        };
        public static List<int> SandBlocks { get; set; } = new()
        {
                TileID.Sand,
                TileID.Ebonsand,
                TileID.Crimsand,
                TileID.Ash,
                TileID.Pearlsand
        };
        public static List<int> SnowyBlocks { get; private set; } = new()
        {
            TileID.SnowBlock,
            TileID.SnowCloud,
            TileID.RainCloud,
            TileID.Cloud,
        };
        public static List<int> IcyBlocks { get; private set; } = new()
        {
            TileID.IceBlock,
            TileID.BreakableIce,
            TileID.HallowedIce,
            TileID.CorruptIce,
            TileID.FleshIce,
            TileID.MagicalIceBlock,
        };
        public static List<int> SmoothStones { get; private set; } = new()
        {
            TileID.Titanstone,
            TileID.Sunplate,
            TileID.PearlstoneBrick,
            TileID.IridescentBrick,
            TileID.AdamantiteBeam,
            TileID.GraniteColumn,
            TileID.MarbleColumn,
            TileID.PalladiumColumn,
            TileID.SandstoneColumn,
            TileID.SandStoneSlab,
            TileID.SmoothSandstone,
            TileID.ObsidianBrick,
            TileID.Asphalt,
            TileID.StoneSlab,
            TileID.SandStoneSlab,
            TileID.Coralstone,
            TileID.CrimstoneBrick,
            TileID.EbonstoneBrick,
            TileID.CrackedPinkDungeonBrick,
            TileID.CrackedGreenDungeonBrick,
            TileID.CrackedBlueDungeonBrick,
            TileID.SandstoneBrick,
            TileID.RedMossBrick,
            TileID.RedBrick,
            TileID.RainbowBrick,
            TileID.PurpleMossBrick,
            TileID.ArgonMossBrick,
            TileID.XenonMossBrick,
            TileID.KryptonMossBrick,
            TileID.LavaMossBrick,
            TileID.GreenMossBrick,
            TileID.BlueMossBrick,
            TileID.BlueDungeonBrick,
            TileID.GreenDungeonBrick,
            TileID.PinkDungeonBrick,
            TileID.SnowBrick,
            TileID.SolarBrick,
            TileID.StardustBrick,
            TileID.TungstenBrick,
            TileID.VortexBrick,
            TileID.NebulaBrick,
            TileID.GrayBrick,
            TileID.GrayStucco,
            TileID.GreenStucco,
            TileID.RedStucco,
            TileID.YellowStucco,
            TileID.MarbleBlock,
            TileID.GraniteBlock,
            TileID.LihzahrdBrick,
			TileID.MeteoriteBrick,
            TileID.IceBrick,
            TileID.Teleporter,
            TileID.HellstoneBrick
        };
        public static List<int> Metals { get; private set; } = new()
        {
            TileID.MetalBars,
            TileID.Anvils,
            TileID.MythrilAnvil,
            TileID.MythrilBrick,
            TileID.CobaltBrick,
            TileID.LunarBrick,
            TileID.IronBrick,
            TileID.GoldBrick,
            TileID.PlatinumBrick,
            TileID.CopperBrick,
            TileID.TinBrick,
            TileID.SilverBrick,
            TileID.DemoniteBrick,
            TileID.CrimtaneBrick,
            TileID.LeadBrick,
            TileID.MartianConduitPlating,
            TileID.TinPlating,
            TileID.ShroomitePlating,
            TileID.CopperPlating
        };
        public static List<int> GraniteAndMarbles { get; private set; } = new()
        {
            TileID.Granite,
            TileID.GraniteBlock,
            TileID.GraniteColumn,
            TileID.Marble,
            TileID.MarbleBlock,
            TileID.MarbleColumn,
        };
        public static List<int> GlassBlocks { get; private set; } = new()
        {
            TileID.Glass,
            TileID.BlueStarryGlassBlock,
            TileID.GoldStarryGlassBlock,
            TileID.Confetti,
            TileID.ConfettiBlack,
            TileID.Waterfall,
            TileID.Lavafall,
            TileID.Honeyfall,
        };
        public static List<int> LeafBlocks { get; private set; } = new()
        {
            TileID.LivingMahoganyLeaves,
            TileID.LeafBlock,
        };

        public static List<int> StickyBlocks { get; private set; } = new()
        {
            TileID.Mud,
            TileID.SlimeBlock,
            TileID.PinkSlimeBlock,
            TileID.FrozenSlimeBlock
        };

        internal static List<List<int>> AllTileLists = new()
        {
            GrassBlocks,
            DirtBlocks,
            IcyBlocks,
            LeafBlocks,
            SmoothStones,
            SnowyBlocks,
            SandBlocks,
            StoneBlocks,
            GraniteAndMarbles,
            Metals,
            GlassBlocks
        };
        public override void FloorVisuals(int type, Player player)
        {
            curTileType = type;

            var modPlayer = player.GetModPlayer<AmbientPlayer>();
            if (GrassBlocks.Any(x => x == type))
                modPlayer.isOnGrassyTile = true;
            else
                modPlayer.isOnGrassyTile = false;

            if (SnowyBlocks.Any(x => x == type))
                modPlayer.isOnSnowyTile = true;
            else
                modPlayer.isOnSnowyTile = false;

            if (SandBlocks.Any(x => x == type))
                modPlayer.isOnSandyTile = true;
            else
                modPlayer.isOnSandyTile = false;

            // TODO: Include ore bricks later, as well as the ore themselves
            if (StoneBlocks.Any(x => x == type))
                modPlayer.isOnStoneTile = true;
            else
                modPlayer.isOnStoneTile = false;

            if (DirtBlocks.Any(x => x == type))
                modPlayer.isOnDirtyTile = true;
            else
                modPlayer.isOnDirtyTile = false;

            if (IcyBlocks.Any(x => x == type))
                modPlayer.isOnIcyTile = true;
            else
                modPlayer.isOnIcyTile = false;

            if (Metals.Any(x => x == type))
                modPlayer.isOnMetalTile = true;
            else
                modPlayer.isOnMetalTile = false;

            if (SmoothStones.Any(x => x == type))
                modPlayer.isOnSmoothTile = true;
            else
                modPlayer.isOnSmoothTile = false;

            if (GraniteAndMarbles.Any(x => x == type))
                modPlayer.isOnMarbleOrGraniteTile = true;
            else
                modPlayer.isOnMarbleOrGraniteTile = false;

            if (LeafBlocks.Any(x => x == type))
                modPlayer.isOnLeafTile = true;
            else
                modPlayer.isOnLeafTile = false;

            if (GlassBlocks.Any(x => x == type))
                modPlayer.isOnGlassTile = true;
            else
                modPlayer.isOnGlassTile = false;

            if (StickyBlocks.Any(x => x == type))
                modPlayer.isOnStickyTile = true;
            else
                modPlayer.isOnStickyTile = false;

            if (!modPlayer.isOnStoneTile && !modPlayer.isOnGrassyTile && !modPlayer.isOnSandyTile && !modPlayer.isOnSnowyTile && !modPlayer.isOnDirtyTile
                  && !modPlayer.isOnIcyTile && !modPlayer.isOnMetalTile && !modPlayer.isOnSmoothTile && !modPlayer.isOnMarbleOrGraniteTile
                    && !modPlayer.isOnLeafTile && !modPlayer.isOnGlassTile && !modPlayer.isOnStickyTile)
                modPlayer.isOnWoodTile = true;
            else
                modPlayer.isOnWoodTile = false;
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
                listToAddTo.Add(mod.Find<ModTile>(tile).Type);
            }
        }
    }
}

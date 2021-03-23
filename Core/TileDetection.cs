using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Content;

namespace TerrariaAmbience.Core
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
                TileID.Glass,
                TileID.Pearlsand
        };
        /// <summary>
        /// Blocks that are either snow or are akin to snow. Y
        /// </summary>

        public static List<int> snowyblocks = new List<int>
        {
                TileID.SnowBlock,
                TileID.SnowCloud,
                TileID.RainCloud,
                TileID.Cloud,
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
        /// <summary>
        /// Access this for when you want to add a modded tile to the valid tiles list
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
        public static void AddTilesToList(Mod mod, List<int> listToAddTo, params string[] name)
        {
            foreach (string tile in name)
            {
                listToAddTo.Add(mod.TileType(tile));
            }
        }
    }
    public class CraftSounds : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool CloneNewInstances => true;
        /// <summary>
        /// A list containing all ANVILS. Add to this if you wish, mods out there.
        /// </summary>
        public static List<int> anvils = new List<int>
        {
            TileID.Anvils,
            TileID.MythrilAnvil
        };
        /// <summary>
        /// A list containing all WORK BENCHES. Add to this if you wish, mods out there.
        /// </summary>
        public static List<int> workBenches = new List<int>
        {
            TileID.WorkBenches,
            TileID.HeavyWorkBench,
            TileID.TinkerersWorkbench
        };
        /// <summary>
        /// A list containing all FURNACES. Add to this if you wish, mods out there. (By default, no sounds connected)
        /// </summary>
        public static List<int> furnaces = new List<int>
        {
            TileID.Furnaces,
            TileID.LihzahrdFurnace
        };
        /// <summary>
        /// A list containing all BOOK RELATED THINGS. Add to this if you wish, mods out there. (By default, no sounds connected)
        /// </summary>
        public static List<int> books = new List<int>
        {
            TileID.Bookcases,
            TileID.Books
        };

        public static string FurnaceSoundDir { get; set; }
        public static string WorkBenchesSoundDir { get => $"{Ambience.ambienceDirectory}/player/crafting_workbenches";
            set { } }
        public static string AnvilsSoundDir { get => $"{Ambience.ambienceDirectory}/player/crafting_anvils";
            set { } }
        public static string BooksSoundDir { get => $"{Ambience.ambienceDirectory}/player/crafting_bookrelated";
            set { } }
        public Mod UsedMod { get; set; }
        public override void OnCraft(Item item, Recipe recipe)
        {
            if (anvils.Intersect(recipe.requiredTile).Any())
            {
                Main.PlaySound(UsedMod.GetLegacySoundSlot(SoundType.Custom, AnvilsSoundDir)).Volume *= .8f;
            }
            if (workBenches.Intersect(recipe.requiredTile).Any())
            {
                Main.PlaySound(UsedMod.GetLegacySoundSlot(SoundType.Custom, WorkBenchesSoundDir)).Volume *= .8f;
            }
            if (furnaces.Intersect(recipe.requiredTile).Any())
            {
                Main.PlaySound(UsedMod.GetLegacySoundSlot(SoundType.Custom, FurnaceSoundDir)).Volume *= .8f;
            }
            if (books.Intersect(recipe.requiredTile).Any())
            {
                Main.PlaySound(UsedMod.GetLegacySoundSlot(SoundType.Custom, BooksSoundDir)).Volume *= .8f;
            }
            if (recipe.needWater || recipe.needHoney)
            {
                Main.PlaySound(SoundID.Splash);
            }
        }
    }
}

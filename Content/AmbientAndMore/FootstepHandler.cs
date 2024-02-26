using Terraria.ModLoader;
using TerrariaAmbienceAPI.Common;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.ID;
using TerrariaAmbienceAPI;
using System.Linq;
using Terraria;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content.AmbientAndMore;

public class FootstepHandler {
    public FootstepSound Grass;
    public FootstepSound Sand;
    public FootstepSound Stone;
    public FootstepSound Snow;
    public FootstepSound Wood;
    public FootstepSound Wet;
    public FootstepSound Dirt;
    public FootstepSound Leaf;
    public FootstepSound MarbleGranite;
    public FootstepSound Ice;
    public FootstepSound Glass;
    public FootstepSound SmoothStone;
    public FootstepSound Metal;
    public FootstepSound Water;
    public FootstepSound Sticky;
    public FootstepSound Gem;

    public FootstepSound Armor;
    public FootstepSound Vanity;

    public List<FootstepSound> AllSounds;
    public FootstepHandler() {
        Initialize();
    }

    public void Initialize() {
        var mod = ModContent.GetInstance<TerrariaAmbience>();
        Grass = new(mod, "Sounds/Custom/steps/grass/step", 8, "Grass", GrassBlocks.ToArray());
        Sand = new(mod, "Sounds/Custom/steps/sand/step", 6, "Sand", SandBlocks.ToArray());
        Stone = new(mod, "Sounds/Custom/steps/stone/step", 8, "Stone", StoneBlocks.ToArray());
        Snow = new(mod, "Sounds/Custom/steps/snow/step", 11, "Snow", SnowBlocks.ToArray());
        Wood = new(mod, "Sounds/Custom/steps/wood/step", 7, "Wood") {
            FootstepConditions = (p) => {
                return AllTileLists.All(list => !list.Contains(PlayerTileChecker.TileId));
            }
        };
        Wet = new(mod, "Sounds/Custom/steps/wet/step", 3, "Wet") {
            FootstepConditions = (p) => {
                return !p.GetModPlayer<AmbientPlayer>().HasTilesAbove && Main.raining && !p.wet && !p.ZoneSnow && p.ZoneOverworldHeight;
            }
        };
        Dirt = new(mod, "Sounds/Custom/steps/dirt/step", 6, "Dirt", DirtBlocks.ToArray());
        Leaf = new(mod, "Sounds/Custom/steps/leaf/step", 7, "Leaf", LeafBlocks.ToArray());
        MarbleGranite = new(mod, "Sounds/Custom/steps/marblegranite/step", 7, "Marble/Granite", MarblesGranites.ToArray());
        Ice = new(mod, "Sounds/Custom/steps/ice/step", 7, "Ice", IceBlocks.ToArray());
        Glass = new(mod, "Sounds/Custom/steps/glass/step", 6, "Glass", GlassBlocks.ToArray());
        SmoothStone = new(mod, "Sounds/Custom/steps/smoothstones/step", 7, "Smooth Stone", SmoothStones.ToArray());
        Metal = new(mod, "Sounds/Custom/steps/metal/step", 6, "Metal", MetalBlocks.ToArray());
        Gem = new(mod, "Sounds/Custom/steps/gem/step", 14, "Gem", 0.3f, 0.6f, GemBlocks.ToArray());
        Water = new(mod, "Sounds/Custom/steps/water/step", 6, "Water Wading") {
            FootstepConditions = (p) => {
                return !p.IsWaterSuffocating() && p.wet && !p.lavaWet;
            }
        };
        Sticky = new(mod, "Sounds/Custom/steps/sticky/step", 6, "Sticky", StickyBlocks.ToArray());
        Armor = new(mod, "Sounds/Custom/steps/armor/heavy", 9, "Armor") {
            FootstepConditions = (p) => {
                return ModContent.GetInstance<GeneralConfig>().areArmorAndVanitySoundsEnabled;
            }
        };
        Vanity = new(mod, "Sounds/Custom/steps/armor/light", 6, "Vanity") {
            FootstepConditions = (p) => {
                return ModContent.GetInstance<GeneralConfig>().areArmorAndVanitySoundsEnabled;
            }
        };

        AllSounds = new();
        foreach (var fld in GetType().GetFields().Where(x => x.FieldType.TypeHandle.Equals(typeof(FootstepSound).TypeHandle))) {
            AllSounds.Add((FootstepSound)fld.GetValue(this));
        }
    }

    public void Update() {
        if (!Main.gameMenu) {
            var vol = Main.LocalPlayer.GetModPlayer<AmbientPlayer>().GetArmorStepVolume();
            Armor.LandVolume = vol;
            Armor.StepVolume = vol / 2;

            var vol1 = Main.LocalPlayer.GetModPlayer<AmbientPlayer>().GetVanityStepVolume();
            Vanity.LandVolume = vol1;
            Vanity.StepVolume = vol1 / 2;
        }
        AllSounds.ForEach(x => x.HandleByDefault = ModContent.GetInstance<GeneralConfig>().footsteps);
    }

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
                TileID.Ruby,
                TileID.Topaz,
                TileID.Sapphire,
                TileID.Amethyst,
                TileID.Emerald,
                TileID.Ebonstone,
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
                TileID.Demonite,
                TileID.Chlorophyte
        };
    public static List<int> SandBlocks { get; private set; } = new()
    {
                TileID.Sand,
                TileID.Ebonsand,
                TileID.Crimsand,
                TileID.Ash,
                TileID.Pearlsand
        };
    public static List<int> SnowBlocks { get; private set; } = new()
    {
            TileID.SnowBlock,
            TileID.SnowCloud,
            TileID.RainCloud,
            TileID.Cloud,
        };
    public static List<int> IceBlocks { get; private set; } = new()
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
            TileID.AccentSlab,
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
    public static List<int> MetalBlocks { get; private set; } = new()
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
            TileID.CopperPlating,
            TileID.TrapdoorClosed
        };
    public static List<int> MarblesGranites { get; private set; } = new()
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
    public static List<int> GemBlocks { get; private set; } = new()
    {
        TileID.AmberGemspark,
        TileID.AmethystGemspark,
        TileID.DiamondGemspark,
        TileID.EmeraldGemspark,
        TileID.RubyGemspark,
        TileID.SapphireGemspark,
        TileID.TopazGemspark,
    };
    public static List<int> StickyBlocks { get; private set; } = new()
    {
            TileID.Mud,
            TileID.SlimeBlock,
            TileID.PinkSlimeBlock,
            TileID.FrozenSlimeBlock,
            TileID.BeeHive,
            TileID.Hive,
            TileID.HoneyBlock,
            TileID.CrispyHoneyBlock,
            TileID.FleshBlock
    };
    // VERY IMPORTANT TO ADD ALL ABOVE LISTS HERE.
    internal static List<List<int>> AllTileLists = new()
    {
            GrassBlocks,
            DirtBlocks,
            IceBlocks,
            LeafBlocks,
            SmoothStones,
            SnowBlocks,
            SandBlocks,
            StoneBlocks,
            MarblesGranites,
            MetalBlocks,
            GlassBlocks,
            StickyBlocks,
            GemBlocks
        };
}

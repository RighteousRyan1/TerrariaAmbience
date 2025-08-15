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

/// <summary>The default handler. Default use is for players.</summary>
public class FootstepHandler {
    public FootstepSound Grass;
    public FootstepSound Stone;
    public FootstepSound Snow;
    public FootstepSound Wet;
    public FootstepSound Dirt;
    public FootstepSound Leaf;

    public FootstepSound Marble;

    public FootstepSound IceWeak;
    public FootstepSound IceStrong;

    public FootstepSound Glass;
    public FootstepSound SmoothStone;

    public FootstepSound Metal;
    public FootstepSound MetalPlating;

    public FootstepSound Water;
    public FootstepSound Sticky;
    public FootstepSound Gem;

    public FootstepSound Sand;
    public FootstepSound SandRough;

    public FootstepSound WoodFilled;
    public FootstepSound WoodHollow;
    public FootstepSound WoodDeck;

    public FootstepSound Gravel;

    public FootstepSound Armor;
    public FootstepSound Vanity;

    public List<FootstepSound> AllSounds;
    public FootstepHandler() {
        Initialize();
    }

    public void Initialize() {
        var mod = ModContent.GetInstance<TerrariaAmbience>();
        // natural
        Grass = new(mod, "Sounds/Custom/steps/grass/step", 8, "Grass", [.. GrassBlocks]);
        Stone = new(mod, "Sounds/Custom/steps/stone/step", 8, "Stone", [.. StoneBlocks]);
        Snow = new(mod, "Sounds/Custom/steps/snow/step", 11, "Snow", [.. SnowBlocks]);
        Wet = new(mod, "Sounds/Custom/steps/wet/step", 3, "Wet", 0.2f, 0.4f) {
            UseAnyTile = true,
            FootstepConditions = (p) => {
                var tBelow = Main.tile[(int)p.Bottom.X / 16, (int)(p.Bottom.Y + 8) / 16];

                var mPlayer = p.GetModPlayer<AmbientPlayer>();
                var canDoSoundsFromRain = !mPlayer.HasTilesAbove && Main.raining && !p.wet && !p.ZoneSnow && !p.ZoneDesert && p.ZoneOverworldHeight;
                var isWalkingOnWater = (!Main.tileSolid[tBelow.TileType] || !tBelow.HasTile) && tBelow.LiquidAmount > 0 && (p.waterWalk || p.waterWalk2);
                var eitherIsMet = canDoSoundsFromRain || isWalkingOnWater;
                return eitherIsMet;
            }
        };
        Dirt = new(mod, "Sounds/Custom/steps/dirt/step", 6, "Dirt", [.. DirtBlocks]);
        Leaf = new(mod, "Sounds/Custom/steps/leaf/step", 7, "Leaf", [.. LeafBlocks]);

        Marble = new(mod, "Sounds/Custom/steps/marble/step", 7, "Marbles", [.. Marbles]);

        IceWeak = new(mod, "Sounds/Custom/steps/ice_weak/step", 7, "Weak Ice", 0.5f, 0.8f, [.. WeakIceBlocks]);
        IceStrong = new(mod, "Sounds/Custom/steps/ice_strong/step", 10, "Strong Ice", [.. StrongIceBlocks]);

        Glass = new(mod, "Sounds/Custom/steps/glass/step", 6, "Glass", [.. GlassBlocks]);
        SmoothStone = new(mod, "Sounds/Custom/steps/smooth_stones/step", 7, "Smooth Stone", [.. SmoothStones]);

        Metal = new(mod, "Sounds/Custom/steps/metal/step", 6, "Metal", [.. MetalBlocks]);
        MetalPlating = new(mod, "Sounds/Custom/steps/metal_plating/step", 11, "Metal Plating", 0.2f, 0.4f, [.. MetalPlatingBlocks]);

        Gem = new(mod, "Sounds/Custom/steps/gem/step", 14, "Gem", 0.3f, 0.6f, [.. GemBlocks]);
        Sticky = new(mod, "Sounds/Custom/steps/sticky/step", 6, "Sticky", [.. StickyBlocks]);
        Water = new(mod, "Sounds/Custom/steps/water/step", 6, "Water Wading") {
            FootstepConditions = (p) => {
                return !p.IsWaterSuffocating() && p.wet && !p.lavaWet;
            }
        };

        Armor = new(mod, "Sounds/Custom/steps/armor/heavy", 9, "Armor") {
            UseAnyTile = true,
            FootstepConditions = (p) => {
                return ModContent.GetInstance<GeneralConfig>().areArmorAndVanitySoundsEnabled;
            }
        };
        Vanity = new(mod, "Sounds/Custom/steps/armor/light", 6, "Vanity") {
            UseAnyTile = true,
            FootstepConditions = (p) => {
                return ModContent.GetInstance<GeneralConfig>().areArmorAndVanitySoundsEnabled;
            }
        };

        Gravel = new(mod, "Sounds/Custom/steps/gravel/step", 11, "Gravel", 0.2f, 0.4f, [.. GravelBlocks]);

        // sand

        Sand = new(mod, "Sounds/Custom/steps/sand/step", 6, "Sand", [.. SandBlocks]) {
            FootstepConditions = (p) => {
                return !p.ZoneBeach;
            }
        };
        SandRough = new(mod, "Sounds/Custom/steps/sand_crunchy/step", 11, "RoughSand", 0.1f, 0.3f, [.. SandBlocks]) {
            FootstepConditions = (p) => {
                return p.ZoneBeach;
            }
        };

        // wood.

        WoodFilled = new(mod, "Sounds/Custom/steps/wood/step", 7, "Filled Wood", [.. FilledWood]);
        WoodHollow = new(mod, "Sounds/Custom/steps/wood_blunt/step", 11, "Blunt Wood", 0.15f, 0.3f, [.. BluntWood]);
        WoodDeck = new(mod, "Sounds/Custom/steps/wood_deck/step", 11, "Deck Wood", 0.125f, 0.275f, [.. DeckWood]);

        AllSounds = [];
        foreach (var fld in GetType().GetFields().Where(x => x.FieldType.TypeHandle.Equals(typeof(FootstepSound).TypeHandle))) {
            AllSounds.Add((FootstepSound)fld.GetValue(this));
        }
    }

    public void Update() {
        if (Main.gameMenu) return;

        for (int i = 0; i < AllSounds.Count; i++) {
            if (AllSounds[i] is null)
                continue;
            AllSounds[i].VolumeMultiplier = ModContent.GetInstance<GeneralConfig>().footstepsVolMult;
        }
        var vol = Main.LocalPlayer.GetModPlayer<AmbientPlayer>().GetArmorStepVolume();
        Armor.LandVolume = vol;
        Armor.StepVolume = vol / 2;

        var vol1 = Main.LocalPlayer.GetModPlayer<AmbientPlayer>().GetVanityStepVolume();
        Vanity.LandVolume = vol1;
        Vanity.StepVolume = vol1 / 2;

        // TODO: make landing step volumes scale based on fall speed?
        AllSounds.ForEach(x => {
            if (x is null) return;
            x.HandleByDefault = ModContent.GetInstance<GeneralConfig>().footsteps;
        });
    }

    public Tile GetTileBelow(Entity ent) => Framing.GetTileSafely((int)ent.Bottom.X / 16, (int)(ent.Bottom.Y + 8) / 16);
    /// <summary>Get all footstep sounds that are used for a given tile.</summary>
    /// <param name="tileId">The tile to check.</param>
    public FootstepSound[] GetFootstepSoundFromTile(Tile tile) {
        // probably could be optimized...
        if (!tile.HasTile) return [];
        var all = AllSounds.Where(x => x.TileIds.Contains(tile.TileType)).ToArray();
        return all.Length != 0 ? all : [WoodFilled];
    }

    public static List<int> GrassBlocks { get; private set; } =
    [
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
        TileID.GolfGrassHallowed,
        TileID.AshGrass,
    ];
    public static List<int> DirtBlocks { get; private set; } =
    [
        TileID.Dirt,
        TileID.ClayBlock,
    ];
    public static List<int> StoneBlocks { get; private set; } =
    [
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
    ];
    public static List<int> SandBlocks { get; private set; } =
    [
        TileID.Sand,
        TileID.Ebonsand,
        TileID.Crimsand,
        TileID.Ash,
        TileID.Pearlsand
    ];
    public static List<int> SnowBlocks { get; private set; } =
    [
        TileID.SnowBlock,
        TileID.SnowCloud,
        TileID.RainCloud,
        TileID.Cloud,
    ];
    public static List<int> WeakIceBlocks { get; private set; } =
    [
        TileID.BreakableIce,
        TileID.MagicalIceBlock,
    ];
    public static List<int> StrongIceBlocks { get; private set; } =
    [
        TileID.IceBlock,
        TileID.HallowedIce,
        TileID.CorruptIce,
        TileID.FleshIce,
    ];
    public static List<int> SmoothStones { get; private set; } =
    [
        TileID.Titanstone,
        TileID.Sunplate,
        TileID.PearlstoneBrick,
        TileID.IridescentBrick,
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
    ];
    public static List<int> MetalBlocks { get; private set; } =
    [
        TileID.MythrilAnvil,
        TileID.MartianConduitPlating,
        TileID.TinPlating,
        TileID.ShroomitePlating,
        TileID.CopperPlating,
        TileID.TrapdoorClosed
    ];
    public static List<int> MetalPlatingBlocks { get; private set; } =
    [
        TileID.MetalBars,
        TileID.Anvils,
        TileID.MythrilBrick,
        TileID.CobaltBrick,
        TileID.LunarBrick,
        TileID.IronBrick,
        TileID.GoldBrick,
        TileID.AncientGoldBrick,
        TileID.PlatinumBrick,
        TileID.CopperBrick,
        TileID.AncientCopperBrick,
        TileID.TinBrick,
        TileID.SilverBrick,
        TileID.AncientSilverBrick,
        TileID.DemoniteBrick,
        TileID.CrimtaneBrick,
        TileID.LeadBrick,
        TileID.AncientBlueBrick,
        TileID.AncientPinkBrick,
        TileID.AncientGreenBrick,
        TileID.AncientHellstoneBrick,
        TileID.AncientMythrilBrick,
        TileID.AncientCobaltBrick,
    ];
    public static List<int> Marbles { get; private set; } =
    [
        TileID.Granite,
        TileID.GraniteBlock,
        TileID.GraniteColumn,
        TileID.Marble,
        TileID.MarbleBlock,
        TileID.MarbleColumn,
    ];
    public static List<int> GlassBlocks { get; private set; } =
    [
        TileID.Glass,
        TileID.BlueStarryGlassBlock,
        TileID.GoldStarryGlassBlock,
        TileID.Confetti,
        TileID.ConfettiBlack,
        TileID.Waterfall,
        TileID.Lavafall,
        TileID.Honeyfall,
    ];
    public static List<int> LeafBlocks { get; private set; } =
    [
        TileID.LivingMahoganyLeaves,
        TileID.LeafBlock,
    ];
    public static List<int> GemBlocks { get; private set; } =
    [
        TileID.AmberGemspark,
        TileID.AmethystGemspark,
        TileID.DiamondGemspark,
        TileID.EmeraldGemspark,
        TileID.RubyGemspark,
        TileID.SapphireGemspark,
        TileID.TopazGemspark,
    ];
    public static List<int> StickyBlocks { get; private set; } =
    [
        TileID.Mud,
        TileID.SlimeBlock,
        TileID.PinkSlimeBlock,
        TileID.FrozenSlimeBlock,
        TileID.BeeHive,
        TileID.Hive,
        TileID.HoneyBlock,
        TileID.CrispyHoneyBlock,
        TileID.FleshBlock,
        TileID.MushroomBlock
    ];
    public static List<int> BluntWood { get; private set; } =
    [
        TileID.WoodBlock,
        TileID.AshWood,
        TileID.BorealWood,
        TileID.PalmWood,
        TileID.SpookyWood,
        TileID.Ebonwood,
        TileID.Pearlwood,
        TileID.Shadewood,
        TileID.BlueDynastyShingles,
        TileID.RedDynastyShingles
    ];
    public static List<int> FilledWood { get; private set; } =
    [
        // mods can add their tiles here. by default blocks that are not registered will not play a sound now.
        TileID.LivingWood,
        TileID.DynastyWood
    ];
    public static List<int> DeckWood { get; private set; } =
    [
        TileID.Platforms
    ];
    public static List<int> GravelBlocks { get; private set; } =
    [
        TileID.Silt,
        TileID.Slush
    ];

    // VERY IMPORTANT TO ADD ALL ABOVE LISTS HERE.
    internal static List<List<int>> AllTileLists =
    [
        GrassBlocks,
        DirtBlocks,

        WeakIceBlocks,
        StrongIceBlocks,

        LeafBlocks,
        SmoothStones,
        SnowBlocks,

        SandBlocks,

        StoneBlocks,
        Marbles,

        MetalBlocks,
        MetalPlatingBlocks,

        GlassBlocks,
        StickyBlocks,
        GemBlocks,

        FilledWood,
        BluntWood,
        DeckWood,

        GravelBlocks
    ];
}

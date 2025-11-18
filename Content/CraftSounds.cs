using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Content.Players;
using Terraria.Audio;
using Terraria.DataStructures;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Content;

public class CraftSounds : GlobalItem {
    public override bool InstancePerEntity => true;
    /// <summary>
    /// A list containing all TINKER-ISH CRAFTING STATIONS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> TinkerStations = [
        TileID.TinkerersWorkbench
    ];
    /// <summary>
    /// A list containing all FLESHY CRAFTING STATIONS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> FleshyStations = [
        TileID.MeatGrinder,
        //TileID.DemonAltar // will have to use tile framing manipulation lol.
    ];
    public static List<int> Altars = [
        TileID.DemonAltar,
        TileID.LunarCraftingStation
    ];
    /// <summary>
    /// A list containing all ALCHEMY STATIONS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> Alchemy = [
        TileID.AlchemyTable,
        TileID.Bottles
    ];
    /// <summary>
    /// A list containing all STATIONS WITH SAWS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> SawStations = [
        TileID.Sawmill
    ];
    /// <summary>
    /// A list containing all ANVILS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> Anvils = [
        TileID.Anvils,
        TileID.MythrilAnvil
    ];
    /// <summary>
    /// A list containing all WORK BENCHES. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> WorkBenches = [
        TileID.WorkBenches,
        TileID.HeavyWorkBench,
        TileID.TinkerersWorkbench
    ]; // Are good
    /// <summary>
    /// A list containing all FURNACES. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> Furnaces = [
        TileID.Furnaces,
        TileID.LihzahrdFurnace,
        TileID.GlassKiln
    ];
    /// <summary>
    /// A list containing all BOOK RELATED THINGS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> Books = [
        TileID.Bookcases,
        TileID.Books
    ];
    public static List<int> Magical = [
        TileID.CrystalBall
    ];

    public static List<List<int>> AllCategories = [
        Altars,
        SawStations,
        Books,
        Furnaces,
        WorkBenches,
        Anvils,
        Alchemy,
        TinkerStations,
        // FleshyStations // no noise for this yet.
    ];
    public static Dictionary<List<int>, string> CategoryToDir = new() {
        [Altars] = AltarsSoundDir,
        [SawStations] = SawsSoundDir,
        [Books] = BooksSoundDir,
        [Furnaces] = FurnaceSoundDir,
        [WorkBenches] = WorkBenchesSoundDir,
        [Anvils] = AnvilsSoundDir,
        [Alchemy] = AlchemySoundDir,
        [TinkerStations] = TinkerSoundDir,
        [Magical] = MagicSoundDir
    };
    public static string WorkBenchesSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting/work_bench";
    public static string AnvilsSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting/anvil";
    public static string BooksSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting/book";
    public static string FurnaceSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting/furnace";
    public static string AltarsSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting/altar";
    public static string AlchemySoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting/alchemy{Main.rand.Next(1, 3)}"; // a lil bitta random :zany:
    public static string SawsSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting/saws{Main.rand.Next(1, 3)}";
    public static string TinkerSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting/tinker";
    public static string MagicSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting/crystal_ball";

    public static float UniversalSoundScale = 0.8f;
    public static int LastCraftedConsumable = -1;
    // Incremented in TerrariaAmbience.On_Terraria::Main_Update(On_Main.orig_Update, Main, GameTime)
    public static int TimeSinceLastCraft = 0;
    public override void OnCreated(Item item, ItemCreationContext context) {
        if (context is not RecipeItemCreationContext recipeContext)
            return;
        var recipe = recipeContext.Recipe;
        var player = Main.player[Main.myPlayer];
        var aPlayer = player.GetModPlayer<AmbientPlayer>();
        // var pket = mod.GetPacket();
        var wasValidStation = AllCategories.Any(x => x.Intersect(recipe.requiredTile).Any());
        TimeSinceLastCraft = 0;
        
        // a second is probably acceptable.
        // why tf is "TimeSinceLastCraft" shitting its pants? we will never know. ryan out
        if (LastCraftedConsumable != item.type || TimeSinceLastCraft > 60) {
            if (wasValidStation) {
                var audioCfg = ModContent.GetInstance<AudioConfig>();
                var volScale = audioCfg.craftingSoundsVolume;
                var dir = string.Empty;

                var pair = CategoryToDir.FirstOrDefault(x => x.Key.Intersect(recipe.requiredTile).Any());

                if (CategoryToDir.TryGetValue(pair.Key, out string value)) {
                    dir = value;
                }

                // no need to send unnecessary packets
                if (!string.IsNullOrEmpty(dir)) {
                    SoundEngine.PlaySound(new SoundStyle(dir).WithVolumeScale(UniversalSoundScale * volScale), player.Center);

                    if (Main.netMode == NetmodeID.MultiplayerClient) {
                        var p = Mod.GetPacket();
                        p.Write(TAPID.SEND_CRAFT);
                        p.WriteVector2(player.Center);
                        p.Write(dir);
                        p.Send();
                    }
                }
            }
        }
        if (item.createTile >= 0 || item.createWall >= 0 || item.consumable)
            LastCraftedConsumable = item.type;
        // i think this is what i need? not too sure.
        //if (recipe.Conditions.Contains(Condition.NearWater) || recipe.Conditions.Contains(Condition.NearHoney)) {
            //SoundEngine.PlaySound(SoundID.Splash, player.Center);
        //}
    }
    public static void AddCraftingStationsToList(List<int> stationType, params int[] stations) {
        foreach (int station in stations)
            stationType.Add(station);
    }
}

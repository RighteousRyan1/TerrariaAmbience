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

namespace TerrariaAmbience.Content;

public class CraftSounds : GlobalItem {
    public override bool InstancePerEntity => true;
    /// <summary>
    /// A list containing all TINKER-ISH CRAFTING STATIONS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> TinkerStations = new() {
        TileID.TinkerersWorkbench
    };
    /// <summary>
    /// A list containing all FLESHY CRAFTING STATIONS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> FleshyStations = new() {
        TileID.MeatGrinder,
        //TileID.DemonAltar // will have to use tile framing manipulation lol.
    };
    public static List<int> Altars = new() {
        TileID.DemonAltar
    };
    /// <summary>
    /// A list containing all ALCHEMY STATIONS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> Alchemy = new() {
        TileID.AlchemyTable,
        TileID.Bottles
    };
    /// <summary>
    /// A list containing all STATIONS WITH SAWS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> SawStations = new() {
        TileID.Sawmill
    };
    /// <summary>
    /// A list containing all ANVILS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> Anvils = new() {
        TileID.Anvils,
        TileID.MythrilAnvil
    };
    /// <summary>
    /// A list containing all WORK BENCHES. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> WorkBenches = new() {
        TileID.WorkBenches,
        TileID.HeavyWorkBench,
        TileID.TinkerersWorkbench
    }; // Are good
    /// <summary>
    /// A list containing all FURNACES. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> Furnaces = new() {
        TileID.Furnaces,
        TileID.LihzahrdFurnace,
        TileID.GlassKiln
    };
    /// <summary>
    /// A list containing all BOOK RELATED THINGS. Add to this if you wish, mods out there.
    /// </summary>
    public static List<int> Books = new() {
        TileID.Bookcases,
        TileID.Books
    };

    public static List<List<int>> AllCategories = new() {
        Altars,
        SawStations,
        Books,
        Furnaces,
        WorkBenches,
        Anvils,
        Alchemy,
        TinkerStations,
        // FleshyStations // no noise for this yet.
    };
    public static string WorkBenchesSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting_workbench";
    public static string AnvilsSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting_anvil";
    public static string BooksSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting_book";
    public static string FurnaceSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting_furnace";
    public static string AltarsSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting_demonaltar";
    public static string AlchemySoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting_alchemy{Main.rand.Next(1, 3)}"; // a lil bitta random :zany:
    public static string SawsSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting_saws{Main.rand.Next(1, 3)}";
    public static string TinkerSoundDir => $"{AmbientHandler.AmbientPathWithPrefix}player/crafting_tinker";

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
                var volScale = ModContent.GetInstance<GeneralConfig>().craftingSoundsVolume;
                var dir = string.Empty;
                if (Anvils.Intersect(recipe.requiredTile).Any())
                    dir = AnvilsSoundDir;
                if (WorkBenches.Intersect(recipe.requiredTile).Any())
                    dir = WorkBenchesSoundDir;
                if (Furnaces.Intersect(recipe.requiredTile).Any())
                    dir = FurnaceSoundDir;
                if (Books.Intersect(recipe.requiredTile).Any())
                    dir = BooksSoundDir;
                if (SawStations.Intersect(recipe.requiredTile).Any())
                    dir = SawsSoundDir;
                if (TinkerStations.Intersect(recipe.requiredTile).Any())
                    dir = TinkerSoundDir;
                if (Alchemy.Intersect(recipe.requiredTile).Any())
                    dir = AlchemySoundDir;
                if (Altars.Intersect(recipe.requiredTile).Any())
                    dir = AltarsSoundDir;

                SoundEngine.PlaySound(new SoundStyle(dir).WithVolumeScale(UniversalSoundScale * volScale), player.Center);

                if (Main.netMode != NetmodeID.SinglePlayer) {
                    var p = Mod.GetPacket();
                    p.WriteVector2(player.Center);
                    p.Write(dir);
                    p.Send();
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
// why did i make this again
public struct CraftSoundInfo {
    public const int WORK_BENCH = 0;
    public const int ANVIL = 1;
    public const int BOOKCASE = 2;
    public int Context;
}

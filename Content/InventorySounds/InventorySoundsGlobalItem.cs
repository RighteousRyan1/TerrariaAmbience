using Terraria;
using Terraria.ModLoader;

namespace TerrariaAmbience.Content.InventorySounds;
public class InventorySoundsGlobalItem : GlobalItem {
    private static readonly ItemChangeTracker _tracker = new();
    private static InventorySoundsConfig Config => ModContent.GetInstance<InventorySoundsConfig>();

    static Item _prevItem;
    public override void UpdateInventory(Item item, Player player) {
        if (Main.dedServ) return;
        if (Config.volumeMultiplier == 0 || !Config.enabledDynamicSoundsSystem) return;

        var mouseItem = Main.mouseItem;
        var heldItem = player.HeldItem;

        bool useHeldItemSounds = Config.useItemSwapSounds && !Main.playerInventory;
        bool useInventorySounds = true;

        // Check for changes and play sounds
        if (_tracker.HasMouseItemChanged(mouseItem.type) && useInventorySounds) {
            HandleItemChange(mouseItem, Actions.PickingUp);
            HandleItemChange(_prevItem, Actions.PuttingDown); // Previous item
        }

        if (_tracker.HasHeldItemChanged(heldItem.type) && useHeldItemSounds) {
            HandleItemChange(heldItem, Actions.PickingUp);
        }
        _prevItem = Main.mouseItem;
    }

    private static void HandleItemChange(Item item, Actions action) {
        if (item?.type <= 0) return;

        var soundType = ItemClassifier.GetSoundType(item);
        var config = SoundConfiguration.GetConfig(soundType);

        int variants = action == Actions.PickingUp ? config.PickupVariants : config.PutdownVariants;

        InventorySoundsHelpers.PlayUpOrDownSound(
            config.Type,
            soundType,
            action,
            variants
        );
    }
}

using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaAmbience.Content.InventorySounds;
public class InventorySoundsGlobalItem : GlobalItem {
    private static readonly ItemChangeTracker _tracker = new();
    private static InventorySoundsConfig Config => ModContent.GetInstance<InventorySoundsConfig>();

    static Item _prevItem;
    public override void UpdateInventory(Item item, Player player) {
        if (Main.dedServ) return;
        if (player.whoAmI != Main.myPlayer) return;
        if (Config.volumeMultiplier == 0 || !Config.enabledDynamicSoundsSystem) return;

        var mouseItem = Main.mouseItem;
        var heldItem = player.HeldItem;

        bool useHeldItemSounds = Config.useItemSwapSounds && !Main.playerInventory;
        bool useInventorySounds = true;

        if (_tracker.HasMouseItemChanged(mouseItem.type) && useInventorySounds) {
            HandleItemChange(mouseItem, Actions.PickingUp);
            HandleItemChange(_prevItem, Actions.PuttingDown);
        }

        if (_tracker.HasHeldItemChanged(heldItem.type) && useHeldItemSounds) {
            HandleItemChange(heldItem, Actions.PickingUp);
        }

        // quick trash or switch
        if (Main.mouseLeft && Main.mouseLeftRelease) {
            if (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl) ||
                Main.keyState.IsKeyDown(Keys.LeftShift) ||Main.keyState.IsKeyDown(Keys.RightShift)) {
                HandleItemChange(Main.HoverItem, Actions.PuttingDown, 0.05f);
            }
        }

        _prevItem = Main.mouseItem;
    }
    public override bool OnPickup(Item item, Player player) {
        if (!Config.enabledDynamicSoundsSystem) return true;
        if (player.whoAmI != Main.myPlayer) return true;

        HandleItemChange(item, Actions.PickingUp, 0.1f);

        return true;
    }

    private static void HandleItemChange(Item item, Actions action, float volume = 0.5f) {
        if (item?.type <= 0) return;

        var soundType = ItemClassifier.GetSoundType(item);
        var config = SoundConfiguration.GetConfig(soundType);

        int variants = action == Actions.PickingUp ? config.PickupVariants : config.PutdownVariants;

        InventorySoundsHelpers.PlayUpOrDownSound(
            config.Type,
            soundType,
            action,
            variants,
            volume
        );
    }
}

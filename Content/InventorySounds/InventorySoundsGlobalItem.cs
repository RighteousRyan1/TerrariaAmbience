using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Content.InventorySounds;
public class InventorySoundsGlobalItem : GlobalItem {
    static readonly ItemChangeTracker _tracker = new();
    static InventorySoundsConfig Config => ModContent.GetInstance<InventorySoundsConfig>();

    const float STCHANGE_VOL = 0.4f;
    const float PICKUP_VOL = 0.2f;
    const float MOVE_VOL = 0.1f;

    static Item _prevItem;
    public override void UpdateInventory(Item item, Player player) {
        if (Main.dedServ) return;
        if (player.whoAmI != Main.myPlayer) return;
        if (Config.volumeMultiplier == 0 || !Config.enabledDynamicSoundsSystem) return;
        
        var mouseItem = Main.mouseItem;
        var heldItem = player.HeldItem;

        bool useHeldItemSounds = Config.useItemSwapSounds && !Main.playerInventory;

        if (_tracker.HasMouseItemChanged(mouseItem.type)) {
            HandleItemChange(mouseItem, Actions.PickingUp, STCHANGE_VOL);
            HandleItemChange(_prevItem, Actions.PuttingDown, STCHANGE_VOL);
        }

        if (_tracker.HasHeldItemChanged(heldItem.type) && useHeldItemSounds) {
            HandleItemChange(heldItem, Actions.PickingUp, STCHANGE_VOL);
        }

        // quick trash or quick container place
        if (Main.mouseLeft && Main.mouseLeftRelease) {
            if (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl) ||
                Main.keyState.IsKeyDown(Keys.LeftShift) ||Main.keyState.IsKeyDown(Keys.RightShift)) {
                HandleItemChange(Main.HoverItem, Actions.PuttingDown, MOVE_VOL);
            }
        }

        _prevItem = Main.mouseItem;
    }
    public override bool OnPickup(Item item, Player player) {
        if (!Config.enabledDynamicSoundsSystem) return true;
        if (player.whoAmI != Main.myPlayer) return true;
        // literally just an encumbering stone check. whatever. still works i guess
        if (!player.CanAcceptItemIntoInventory(item)) return true;
        // if (!ItemSpace(item, player)) return true;
        if (!GeneralHelpers.CanPlayerAcceptItem(player, item)) return true;

        HandleItemChange(item, Actions.PickingUp, PICKUP_VOL);

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

using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using TerrariaAmbience.Helpers;
using tModPorter;

namespace TerrariaAmbience.Content.InventorySounds;
public class InventorySoundsPlayer : ModPlayer {
    readonly ItemChangeTracker _tracker = new();
    static InventorySoundsConfig Config => ModContent.GetInstance<InventorySoundsConfig>();

    const float STCHANGE_VOL = 0.4f;
    const float PICKUP_VOL = 0.2f;
    const float MOVE_VOL = 0.1f;

    static Item _prevItem;
    public override void PostUpdate() {
        if (Main.dedServ) return;
        if (!TerrariaAmbience.WeaponOutLoaded)
            if (Player.whoAmI != Main.myPlayer) return;
        if (Config.volumeMultiplier == 0 || !Config.enabledDynamicSoundsSystem) return;

        var mouseItem = Main.mouseItem;
        var heldItem = Player.HeldItem;

        bool useHeldItemSounds = Config.useItemSwapSounds && !Main.playerInventory;

        if (_tracker.HasMouseItemChanged(mouseItem.type)) {
            HandleItemChange(mouseItem, Player, Actions.PickingUp, STCHANGE_VOL);
            HandleItemChange(_prevItem, Player, Actions.PuttingDown, STCHANGE_VOL);
        }

        if (_tracker.HasHeldItemChanged(heldItem.type) && useHeldItemSounds) {
            HandleItemChange(heldItem, Player, Actions.PickingUp, STCHANGE_VOL);
        }

        // regardless of weaponout being loaded, return here because mouse logic is entirely local
        if (Player.whoAmI != Main.myPlayer) return;

        // quick trash or quick container place
        if (Main.mouseLeft && Main.mouseLeftRelease) {
            if (Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl) ||
                Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift)) {
                HandleItemChange(Main.HoverItem, Player, Actions.PuttingDown, MOVE_VOL);
            }
        }

        _prevItem = Main.mouseItem;
    }
    public override bool OnPickup(WorldItem item) {
        if (!Config.enabledDynamicSoundsSystem) return true;
        if (Player.whoAmI != Main.myPlayer) return true;
        // literally just an encumbering stone check. whatever. still works i guess
        if (!Player.CanAcceptItemIntoInventory(item)) return true;
        if (!GeneralHelpers.CanPlayerAcceptItem(Player, item)) return true;

        HandleItemChange(item.inner, Player, Actions.PickingUp, PICKUP_VOL);

        return true;
    }

    static void HandleItemChange(Item item, Player player, Actions action, float volume = 0.5f) {
        if (item?.type <= 0) return;

        var soundType = ItemClassifier.GetSoundType(item);
        var config = SoundConfiguration.GetConfig(soundType);

        int variants = action == Actions.PickingUp ? config.PickupVariants : config.PutdownVariants;

        InventorySoundsHelpers.PlayUpOrDownSound(
            player,
            config.Type,
            soundType,
            action,
            variants,
            volume
        );
    }
}
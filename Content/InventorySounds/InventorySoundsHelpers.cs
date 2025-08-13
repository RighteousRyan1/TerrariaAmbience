using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaAmbience.Content.InventorySounds;
public static class InventorySoundsHelpers {
    /// <summary>
    /// Play an item pickup sound without random choice.
    /// </summary>
    public static SlotId PlayUpOrDownSound(HoldableType type, HoldableSoundType hSoundType, Actions action, float volume = 0.5f) {
        var act = action == Actions.PickingUp ? "up" : "down";
        string path = $"{nameof(TerrariaAmbience)}/Sounds/Inventory/{type}/{hSoundType}/{act}";
        return PlaySound(path, volume);
    }

    /// <summary>
    /// Play an item pickup sound with random choice.
    /// </summary>
    public static SlotId PlayUpOrDownSound(HoldableType type, HoldableSoundType hSoundType, Actions action, int variants, float volume = 0.5f) {
        int rand = Main.rand.Next(1, variants + 1);
        var act = action == Actions.PickingUp ? "up" : "down";
        string path = $"{nameof(TerrariaAmbience)}/Sounds/Inventory/{type}/{hSoundType}/{act}{(variants > 1 ? rand : string.Empty)}";
        return PlaySound(path, volume);
    }

    static SlotId PlaySound(string path, float volume) {
        var config = ModContent.GetInstance<InventorySoundsConfig>();
        return SoundEngine.PlaySound(
            new SoundStyle(path, SoundType.Sound).WithVolumeScale(volume * config.volumeMultiplier),
            Main.LocalPlayer.Center
        );
    }
}

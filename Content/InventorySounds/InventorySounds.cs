using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaAmbience.Content.InventorySounds; 
public class InventorySoundsSystem : ModSystem {
    public override void OnModLoad() {
        // monomod automatically unhooks this on unload
        On_SoundPlayer.Play += On_SoundPlayer_Play;
    }

    SlotId On_SoundPlayer_Play(On_SoundPlayer.orig_Play orig, SoundPlayer self, ref SoundStyle style, Vector2? position, SoundUpdateCallback updateCallback) {
        if (ModContent.GetInstance<InventorySoundsConfig>().enabledDynamicSoundsSystem) {
            if (!Main.dedServ && style == SoundID.Grab) {
                style = SimpleSoundStyle($"{nameof(TerrariaAmbience)}/Sounds/Inventory/empty", 0);
                style.MaxInstances = 20;
            }
        }
        return orig(self, ref style, position, updateCallback);
    }

    public static SoundStyle SimpleSoundStyle(string path, int variants, SoundType type = SoundType.Sound, float volume = 1f, float pitch = 0f) =>
        new(path, variants, type) {
            Volume = volume,
            Pitch = pitch,
        };
}
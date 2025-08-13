using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaAmbience.Content.InventorySounds;
public class InventorySoundsConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(true)]
    public bool useItemSwapSounds;

    [DefaultValue(1f)]
    public float volumeMultiplier;

    [DefaultValue(true)]
    public bool enabledDynamicSoundsSystem;
}

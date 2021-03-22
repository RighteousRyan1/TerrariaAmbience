using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaAmbience.Core
{
    public class AmbientConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [Header("Ambience and Sounds")]
        [Label("Toggle Footsteps")]
        [Tooltip("Toggle on to turn on footsteps. Toggle off to turn them off.\nTurned on by default.")]
        [DefaultValue(true)]
        public bool footsteps;

        [Label("Toggle Campfire Volumetric Sound")]
        [Tooltip("Toggle on to turn campfire sounds on. Toggle off to turn them off.\nTurned on by default.")]
        [DefaultValue(true)]
        public bool campfireSounds;
    }
}

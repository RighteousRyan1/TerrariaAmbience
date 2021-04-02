using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaAmbience.Core
{
    public class AmbientConfigClient : ModConfig
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

        [Label("Transition Harshness")]
        [Tooltip("Changes the harshness between ambient track transitions. Keep it low for a nice, soothing transition.\nPut it closer to 1 and make it near-instant.\nAnywhere under 0.075 is a good value." 
            + "\nMaking it go too low can cause issues or make things a bit unwanted. Choose the value at your own risk!\nDISCLAIMER: Making this input a non-floating-point numeric type will cause the value to default to 0.01.")]
        [DefaultValue("0.01")]
        public string transitionHarshness;

        [Header("Debugging")]
        [Label("Toggle Debug Values (Vanilla Ambiences)")]
        [Tooltip("Toggle volume debugging values, such as snow, crickets, and more.")]
        [DefaultValue(false)]
        public bool volVals;
    }
    public class AmbientConfigServer : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;
        [Header("Ambience and Sounds")]
        [Label("Toggle Chest Opening Sounds")]
        [Tooltip("Toggle on to turn chest sounds on. Toggle off to turn them off.\nTurned on by default.\nThis config is shared with everyone in the server.")]
        [DefaultValue(true)]
        public bool chestSounds;

        [Label("Toggle New Splash Sounds")]
        [Tooltip("Toggle on to turn new splash sounds on. Toggle off to turn them off.\nTurned on by default.\nThis config is shared with everyone in the server.\n\nReload Required.")]
        [DefaultValue(true)]
        [ReloadRequired]
        public bool newSplashes;
    }
}

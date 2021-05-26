using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using TerrariaAmbience.Content;
using static TerrariaAmbience.Content.Ambience;

namespace TerrariaAmbience.Core
{
    [Label("Clientside Config")]
    public class AmbientConfigClient : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        #region Ambience and Sounds
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
        #endregion

        #region Debug
        [Header("Debugging")]
        [Label("Toggle Debug Values (Vanilla Ambiences)")]
        [Tooltip("Toggle volume debugging values, such as snow, crickets, and more.")]
        [DefaultValue(false)]
        public bool volVals;
        #endregion

        #region ToggleSounds
        [Header("Disable Ambience Tracks")]
        // [SeparatePage]
        [Label("Forest")]
        [Tooltip("Order: Top -> Bottom\n\nMorning\nAfternoon\nEvening\nNight")]
        [DefaultValue(new bool[] { true, true, true, true })]
        public bool[] enabledForest = new bool[4];

        [Label("Snow")]
        [Tooltip("Order: Top -> Bottom\n\nDay\nNight")]
        [DefaultValue(new bool[] { true, true })]
        public bool[] enabledSnow = new bool[2];

        [Label("Jungle")]
        [Tooltip("Order: Top -> Bottom\n\nDay\nNight")]
        [DefaultValue(new bool[] { true, true })]
        public bool[] enabledJungle = new bool[2];

        [Label("World Evils")]
        [Tooltip("Order: Top -> Bottom\n\nCorruption\nCrimson")]
        [DefaultValue(new bool[] { true, true })]
        public bool[] enabledEvils = new bool[2];

        [Label("Desert")]
        [DefaultValue(true)]
        public bool enabledDesert;

        [Label("Ocean")]
        [DefaultValue(true)]
        public bool enabledOcean;

        [Label("Caverns")]
        [DefaultValue(true)]
        public bool enabledCavern;

        [Label("Hell")]
        [DefaultValue(true)]
        public bool enabledHell;

        [Label("Breeze")]
        [DefaultValue(true)]
        public bool enabledBreeze;
        #endregion

    }
    [Label("Server Config")]
    public class AmbientConfigServer : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;
        #region Ambience and Sounds
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

        [Label("FPS Stabilizer")]
        [Tooltip("Enable this if you have FPS issues underground. This is caused by the system that determines the reverb sounds of steps.\nSince the math required for this operation is hefty, it can cause FPS issues.\nEnable to have reverberated footsteps when underground at all times.\nDisable to have reverberated steps based on your underground environment.")]
        [DefaultValue(false)]
        public bool noReverbMath;
        #endregion
    }
}
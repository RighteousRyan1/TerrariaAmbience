using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;
using TerrariaAmbience.Common.Systems;

namespace TerrariaAmbience.Core;

public class UIConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("MainMenu")]
    [DefaultValue(true)]
    public bool showMainMenuUi;

    [Header("In-game")]
    [DefaultValue(true)]
    public bool showAmbientForecastUi;
}
public class FootstepsConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;
    [DefaultValue(true)]
    public float entityFootstepsVolume;

    [DefaultValue(1f)]
    public float masterFootstepsVolume;

    [DefaultValue(true)]
    public bool wetStepsEnabled;

    [DefaultValue(true)]
    public bool areArmorAndVanitySoundsEnabled;
}
public class AmbientConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;
    [Header("Debugging")]

    [DefaultValue(false)]
    public bool debugInterface;

    [DefaultValue(true)]
    public bool showInfoAndWarnings;

    [Header("AmbienceTrackVolumes")]

    [DefaultValue(1f)]
    public float overallVolume;

    [DefaultValue("0.01")]
    public string transitionHarshness;

    [DefaultValue(new float[] { 1f, 1f, 1f, 1f })]
    public float[] forestVolumes = new float[4];

    [DefaultValue(1f)]
    public float snowVolume;

    [DefaultValue(new float[] { 1f, 1f, 1f, 1f })]
    public float[] jungleVolumes = new float[4];

    [DefaultValue(new float[] { 1f, 1f })]
    public float[] evilVolumes = new float[2];

    [DefaultValue(1f)]
    public float desertVolume;

    [DefaultValue(1f)]
    public float oceanVolume;

    [DefaultValue(1f)]
    public float cavernsVolume;

    [DefaultValue(1f)]
    public float hellVolume;

    [DefaultValue(1f)]
    public float breezeVolume;

    [DefaultValue(1f)]
    public float underwaterVolume;
}
public class AudioConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("SoundEffects")]

    [DefaultValue(true)]
    public bool campfireSounds;

    [DefaultValue(1f)]
    public float craftingSoundsVolume;

    [DefaultValue(true)]
    public bool slimySounds;

    [DefaultValue(true)]
    public bool woodCreaks;

    [DefaultValue(true)]
    public bool dynamicAnimalSounds;

    [DefaultValue(true)]
    public bool chestSounds;

    [ReloadRequired]
    [DefaultValue(true)]
    public bool newSplashSounds;

    [Header("SoundFilters")]

    [DefaultValue(true)]
    public bool isReverbEnabled;

    [DefaultValue(true)]
    public bool reverbUsingRaycasting;

    [DefaultValue(2500)]
    [Range(1500, 5000)]
    // area, in reality
    public int reverbMaxArea;

    [DefaultValue(10)]
    [Range(1, 60)]
    public int audioFiltersRefreshTime;

    [DefaultValue(true)]
    public bool advancedReverbCalculation;

    [DefaultValue(true)]
    public bool isSoundDampeningEnabled;

    [DefaultValue(true)]
    public bool isSoundOcclusionEnabled;

    public override void OnChanged() {
        FloodFillSystem.MaxRoomArea = reverbMaxArea;
    }
}
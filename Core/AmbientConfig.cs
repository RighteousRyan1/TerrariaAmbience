using System.ComponentModel;
using Terraria.ModLoader.Config;

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
public class GeneralConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;
    #region Ambience and Sounds
    [Header("AmbienceAndSounds")]

    [DefaultValue(true)]
    public float entityFootstepsVolume;

    [DefaultValue(true)]
    public bool footsteps;

    [DefaultValue(1f)]
    public float footstepsVolMult;

    [DefaultValue(true)]
    public bool campfireSounds;

    [DefaultValue("0.01")]
    public string transitionHarshness;

    [DefaultValue(true)]
    public bool areArmorAndVanitySoundsEnabled;

    [DefaultValue(true)]
    public bool wetStepsEnabled;

    [DefaultValue(1f)]
    public float craftingSoundsVolume;
    #endregion

    #region Debug
    [Header("Debugging")]
    [DefaultValue(false)]
    public bool debugInterface;
    #endregion

    #region ToggleSounds
    [Header("AmbienceTrackVolumes")]

    [DefaultValue(1f)]
    public float overallVolume;

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
    #endregion
}
public class AudioAdditionsConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("NewSounds")]
    [DefaultValue(true)]
    public bool slimySounds;

    [DefaultValue(true)]
    public bool woodCreaks;

    [DefaultValue(true)]
    public bool dynamicAnimalSounds;

    [Header("SoundFilters")]

    [DefaultValue(true)]
    public bool isReverbEnabled;

    [DefaultValue(true)]
    public bool advancedReverbCalculation;

    [DefaultValue(true)]
    public bool isSoundDampeningEnabled;

    [DefaultValue(true)]
    public bool isSoundOcclusionEnabled;

    [DefaultValue(true)]
    public bool floodFillAmbientOcclusion;
}
public class AmbientConfigServer : ModConfig {
    public override ConfigScope Mode => ConfigScope.ServerSide;
    #region Sound Configs

    [DefaultValue(true)]
    public bool chestSounds;

    [ReloadRequired]
    [DefaultValue(true)]
    public bool newSplashSounds;

    #endregion
}
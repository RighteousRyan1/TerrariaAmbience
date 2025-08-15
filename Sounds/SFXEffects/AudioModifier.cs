using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaAmbience.Sounds.SFXEffects.FAudioHacks;

namespace TerrariaAmbience.Sounds.SFXEffects;

public static class AudioModifier
{
    internal static MethodInfo ReverbInfo { get; private set; }
    internal static MethodInfo LowPassInfo { get; private set; }
    internal static MethodInfo HighPassInfo { get; private set; }
    internal static MethodInfo BandPassInfo { get; private set; }
    public static void CacheReflection() {
        ReverbInfo = typeof(SoundEffectInstance)
            .GetMethod("INTERNAL_applyReverb",
            BindingFlags.Instance | BindingFlags.NonPublic);
        LowPassInfo = typeof(SoundEffectInstance)
                .GetMethod("INTERNAL_applyLowPassFilter",
                BindingFlags.Instance | BindingFlags.NonPublic);
        HighPassInfo = typeof(SoundEffectInstance)
                .GetMethod("INTERNAL_applyHighPassFilter",
                BindingFlags.Instance | BindingFlags.NonPublic);
        BandPassInfo = typeof(SoundEffectInstance)
                .GetMethod("INTERNAL_applyBandPassFilter",
                BindingFlags.Instance | BindingFlags.NonPublic);
    }
    public const string FILTER_ISSUE_WARNING = "Error applying sound filters. Check the pins in the [c/FFFF00:#bug-reports] channel in the [c/5865F2:Discord] server in the top left of the main menu.";

    public static SoundEffectInstance ApplyReverb(this SoundEffectInstance instance, float rvGain, FilterParams param = default) {
        try {
            FAudioReverbController.RvInit();

            FAudioReverbController.ApplyCustomReverb(instance, rvGain, param);

            return instance;
            /*rvGain = MathHelper.Clamp(rvGain, 0f, 1f);
            if (instance != null)
                ReverbInfo?.Invoke(instance, [rvGain]);
            return instance;*/
        }
        catch {
            Main.NewText(FILTER_ISSUE_WARNING, Color.Red);
            return instance;
        }
    }
    public static SoundEffectInstance ApplyLowPassFilter(this SoundEffectInstance instance, float cutoff) {
        try {
            cutoff = MathHelper.Clamp(cutoff, 0f, 1f);
            if (instance != null)
                LowPassInfo?.Invoke(instance, [cutoff]);
            return instance;
        }
        catch {
            Main.NewText(FILTER_ISSUE_WARNING, Color.Red);
            return instance;
        }
    }
    public static SoundEffectInstance ApplyHighPassFilter(this SoundEffectInstance instance, float cutoff) {
        try {
            cutoff = MathHelper.Clamp(cutoff, 0f, 1f);
            if (instance != null)
                HighPassInfo?.Invoke(instance, [cutoff]);
            return instance;
        }
        catch {
            Main.NewText(FILTER_ISSUE_WARNING, Color.Red);
            return instance;
        }
    }
    public static SoundEffectInstance ApplyBandPassFilter(this SoundEffectInstance instance, float center) {
        try {
            center = MathHelper.Clamp(center, -1f, 1f);
            if (instance != null)
                BandPassInfo?.Invoke(instance, [center]);
            return instance;
        }
        catch {
            Main.NewText(FILTER_ISSUE_WARNING, Color.Red);
            return instance;
        }
    }
}

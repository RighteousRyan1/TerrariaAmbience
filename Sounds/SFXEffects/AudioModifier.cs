using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaAmbience.Sounds.SFXEffects.FAudioHacks;

namespace TerrariaAmbience.Sounds.SFXEffects;

public static class AudioModifier {
    public const string FILTER_ISSUE_WARNING = "Error applying sound filters. Check the pins in the [c/FFFF00:#bug-reports] channel in the [c/5865F2:Discord] server in the top left of the main menu.";

    public static SoundEffectInstance ApplyReverb(this SoundEffectInstance instance, float gain, FilterParams param = default) {
        try {
            FAudioReverbController.RvInit();

            FAudioReverbController.ApplyCustomReverb(instance, gain, param);

            return instance;
        }
        catch {
            Main.NewText(FILTER_ISSUE_WARNING, Color.Red);
            return instance;
        }
    }
    public static SoundEffectInstance ApplyLowPassFilter(this SoundEffectInstance instance, float cutoff) {
        try {
            cutoff = MathHelper.Clamp(cutoff, 0f, 1f);

            instance.INTERNAL_applyLowPassFilter(cutoff);
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

            instance.INTERNAL_applyHighPassFilter(cutoff);
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

            instance.INTERNAL_applyBandPassFilter(center);
            return instance;
        }
        catch {
            Main.NewText(FILTER_ISSUE_WARNING, Color.Red);
            return instance;
        }
    }
}

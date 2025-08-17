using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Reflection;
using Terraria;
using TerrariaAmbience.Sounds.SoundFilters.FAudioHacks;

namespace TerrariaAmbience.Sounds.SoundFilters;

public static class FilterHelpers {
    public const string FILTER_ISSUE_WARNING = "Error applying sound filter for {0}. Check the pins in the [c/FFFF00:#bug-reports] channel in the [c/5865F2:Discord] server in the top left of the main menu.";
    public const string FILTER_SAVED_GAME = "Recovered from fatal error in {0}. If you are seeing this message, please report this in our [c/5865F2:Discord] server in the top left of the main menu.";
    public static SoundEffectInstance ApplyReverb(this SoundEffectInstance instance, float gain, FilterParams param = default) {
        if (gain < 0) {
            Main.NewText(string.Format(FILTER_SAVED_GAME, "Reverb"));
            return instance;
        }

        try {
            FAudioReverbController.RvInit();

            FAudioReverbController.ApplyCustomReverb(instance, gain, param);

            return instance;
        }
        catch {
            Main.NewText(string.Format(FILTER_ISSUE_WARNING, "Reverb"), Color.Red);
            return instance;
        }
    }
    public static SoundEffectInstance ApplyLowPassFilter(this SoundEffectInstance instance, float cutoff) {
        if (cutoff < 0) {
            Main.NewText(string.Format(FILTER_SAVED_GAME, "LowPass"));
            return instance;
        }
        try {
            cutoff = MathHelper.Clamp(cutoff, 0f, 1f);

            instance.INTERNAL_applyLowPassFilter(cutoff);
            return instance;
        }
        catch {
            Main.NewText(string.Format(FILTER_ISSUE_WARNING, "LowPass"), Color.Red);
            return instance;
        }
    }
    public static SoundEffectInstance ApplyHighPassFilter(this SoundEffectInstance instance, float cutoff) {
        if (cutoff < 0) {
            Main.NewText(string.Format(FILTER_SAVED_GAME, "HighPass"));
            return instance;
        }
        try {
            cutoff = MathHelper.Clamp(cutoff, 0f, 1f);

            instance.INTERNAL_applyHighPassFilter(cutoff);
            return instance;
        }
        catch {
            Main.NewText(string.Format(FILTER_ISSUE_WARNING, "HighPass"), Color.Red);
            return instance;
        }
    }
    public static SoundEffectInstance ApplyBandPassFilter(this SoundEffectInstance instance, float center) {
        if (center < 0) {
            Main.NewText(string.Format(FILTER_SAVED_GAME, "BandPass"));
            return instance;
        }
        try {
            center = MathHelper.Clamp(center, -1f, 1f);

            instance.INTERNAL_applyBandPassFilter(center);
            return instance;
        }
        catch {
            Main.NewText(string.Format(FILTER_ISSUE_WARNING, "BandPass"), Color.Red);
            return instance;
        }
    }
}

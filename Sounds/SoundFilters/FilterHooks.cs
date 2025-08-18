using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Sounds.SoundFilters;

public class FilterHooks : ModSystem
{
	public static List<ActiveSound> dynamicSfxActiveSounds = [];
	// public static List<Vector2> dynamicSfxPrevPos = [];

	static List<float> _oldLowPasses = [];
	public override void PreSaveAndQuit() {
		TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance.Stop();
	}
	public override void Load() {
		On_ActiveSound.Play += ActiveSound_Play;
		//On_ActiveSound.Update += UpdateActiveSoundFilters;
	}
	// TODO: check multiple players' room statuses. if the speaker is
	// in a different room, or the speaker is in a room and the listening player is not, muffle the voice.
	// this same logic could be applied to sounds, but that could be costly... idk if more costly than the reverb calculation but who knows
	public override void PostUpdateEverything() {
        //var lisPos = SoundFilterSystem.ScreenListeningPosition;
		//var lisVel = lisPos - _oldScreenPos;
        for (int i = 0; i < dynamicSfxActiveSounds.Count; i++) {
			var sfx = dynamicSfxActiveSounds[i];
			if (sfx.Sound is not DynamicSoundEffectInstance self) continue;
			if (!sfx.Position.HasValue) continue;
			var sndPos = sfx.Position.Value;
			//var sndVel = sndPos - dynamicSfxPrevPos[i];

            // var param = SoundFilterSystem.CreateAudioFX(sndPos);

            var bandIntensity = SoundFilterSystem.CalculateBandPass(sndPos, Main.LocalPlayer.IsWaterSuffocating(), out var enableBand);
            var lowPassIntensity = SoundFilterSystem.CalculateLowPass(sndPos, Vector2.Zero, out var enabledLP);

            // const float C_TILES_PER_SEC = 100f;

            /*float targetPitch = SoundFilterSystem.PitchFromPerFrame(
                sndPos, sndVel,
                lisPos, lisVel,
                speedOfSoundTilesPerSec: C_TILES_PER_SEC);*/

            // var test = Main.MouseScreen.X / Main.screenWidth * 25;
            // FAudio.FAudioSourceVoice_SetFrequencyRatio(self.handle, 1f - MathF.Abs(targetPitch), 0);
            // self.Pitch = targetPitch;
            // Main.NewText(MathF.Abs(targetPitch) + 1);
            self.ApplyReverb(SoundFilterSystem.LatestParams.ReverbGain);
            if (ModContent.GetInstance<AudioConfig>().isSoundOcclusionEnabled && enabledLP) {
				// _currentLowPasses[i] holds the *applied* value, initialized to 1.0f or whatever default
				float target = lowPassIntensity;

                // smooth approach: t is a small factor (e.g. 0.05)
                float t = 0.4f * TerrariaAmbience.WorkaroundDeltaTime;
                float smoothed = MathHelper.Lerp(_oldLowPasses[i], target, t);

                self.ApplyLowPassFilter(smoothed);

                // save the applied value, not the target
                _oldLowPasses[i] = smoothed;
            }
			// overrides lowpass??
            if (ModContent.GetInstance<AudioConfig>().isSoundDampeningEnabled && enableBand)
                self.ApplyBandPassFilter(bandIntensity);

            // dynamicSfxPrevPos[i] = sfx.Position.Value;
        }
		// _oldScreenPos = SoundFilterSystem.ScreenListeningPosition;
    }

	private void ActiveSound_Play(On_ActiveSound.orig_Play orig, ActiveSound self) {
		orig(self);

        var vol = self.Sound.Volume;
		self.Sound.Volume = 0;
		// self.Sound.Pause();
		if (self.Sound is DynamicSoundEffectInstance) {
			dynamicSfxActiveSounds.Add(self);
			_oldLowPasses.Add(0f);
		}

        // dont apply filters to sounds without positions
        if (!self.Position.HasValue) return;

        bool ignoresFilters = SoundsThatIgnoreFilters.Contains(self.Style);

        if (!ignoresFilters && !Main.gameMenu) {
            var param = SoundFilterSystem.LatestParams;
            var playerUnderwater = Main.LocalPlayer.IsWaterSuffocating();
			var bandIntensity = SoundFilterSystem.CalculateBandPass(self.Position.Value, playerUnderwater, out var enableBand);
			var lowPassIntensity = SoundFilterSystem.CalculateLowPass(self.Position.Value, Vector2.Zero, out var enabledLP);
            self.Sound.ApplyReverb(param.ReverbGain, param);

			if (ModContent.GetInstance<AudioConfig>().isSoundOcclusionEnabled && enabledLP)
				self.Sound.ApplyLowPassFilter(lowPassIntensity);
			if (ModContent.GetInstance<AudioConfig>().isSoundDampeningEnabled && enableBand)
				self.Sound.ApplyBandPassFilter(bandIntensity);
		}

		// hacky but ok???
		// self.Sound.Play();
		self.Sound.Volume = vol;
	}

	public static List<SoundStyle> SoundsThatIgnoreFilters = [
		SoundID.Grab,
		SoundID.MenuOpen,
		SoundID.MenuClose,
		SoundID.MenuTick,
		SoundID.Chat,
		SoundID.Research,
		SoundID.ResearchComplete
	];
}
public struct FilterParams {
    public float ReverbGain;
    public FAudio.FAudioFXReverbParameters Reverb;

    public float LowPassIntensity;
	public bool LowPassEnabled;
    public float BandPassIntensity;
	public bool BandPassEnabled;
}
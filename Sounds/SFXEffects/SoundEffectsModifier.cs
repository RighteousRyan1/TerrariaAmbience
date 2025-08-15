using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Sounds.SFXEffects;

public class SoundEffectsModifier : ModSystem
{
	public static List<ActiveSound> dynamicSfxActiveSounds = [];
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
		foreach (var sfx in dynamicSfxActiveSounds) {
			if (sfx.Sound is not DynamicSoundEffectInstance self) continue;
			if (!sfx.Position.HasValue) continue;
			var param = ReverbAudioSystem.CreateAudioFX(sfx.Position.Value);

			if (SoundsThatIgnoreFilters.Contains(sfx.Style)) continue;

			self.ApplyReverb(param.ReverbGain / 2);
			if (ModContent.GetInstance<AudioAdditionsConfig>().isSoundOcclusionEnabled)
				self.ApplyLowPassFilter(param.LowPassIntensity);
			if (ModContent.GetInstance<AudioAdditionsConfig>().isSoundDampeningEnabled && param.BandPassEnabled)
				self.ApplyBandPassFilter(param.BandPassIntensity);
		}
	}

	private void ActiveSound_Play(On_ActiveSound.orig_Play orig, ActiveSound self) {
		orig(self);

		// dont apply filters to sounds without positions
        if (!self.Position.HasValue) return;

        var vol = self.Sound.Volume;

		self.Sound.Volume = 0;
		if (self.Sound is DynamicSoundEffectInstance)
			dynamicSfxActiveSounds.Add(self);

		bool ignoresFilters = SoundsThatIgnoreFilters.Contains(self.Style);
        // var param = ReverbAudioSystem.CreateAudioFX(self.Position.Value);
        var param = Main.LocalPlayer.GetModPlayer<ReverbPlayer>().LatestParams;

        if (!ignoresFilters) {
			self.Sound.ApplyReverb(param.ReverbGain / 2, param);
			if (ModContent.GetInstance<AudioAdditionsConfig>().isSoundOcclusionEnabled)
				self.Sound.ApplyLowPassFilter(param.LowPassIntensity);
			if (ModContent.GetInstance<AudioAdditionsConfig>().isSoundDampeningEnabled && param.BandPassEnabled)
				self.Sound.ApplyBandPassFilter(param.BandPassIntensity);
		}

		// hacky but ok???
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
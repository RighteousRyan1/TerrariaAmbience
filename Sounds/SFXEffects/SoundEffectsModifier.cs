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
		AudioModifier.CacheReflection();
		On_ActiveSound.Play += ActiveSound_Play;
		//On_ActiveSound.Update += UpdateActiveSoundFilters;
	}
	// TODO: check multiple players' room statuses. if the speaker is
	// in a different room, or the speaker is in a room and the listening player is not, muffle the voice.
	// this same logic could be applied to sounds, but that could be costly... idk if more costly than the reverb calculation but who knows
	public override void PostUpdateEverything() {
		foreach (var sfx in dynamicSfxActiveSounds) {
			if (sfx.Sound is DynamicSoundEffectInstance self) {
				if (sfx.Position.HasValue) {
					bool containsIgnoreablePos = badStyles.Contains(sfx.Style);
					ReverbAudioSystem.CreateAudioFX(sfx.Position.Value, out float gain, out float occ, out float damp, out bool sDamp);
					if (!ModContent.GetInstance<AudioAdditionsConfig>().isReverbEnabled)
						gain = 0f;
					if (!ModContent.GetInstance<AudioAdditionsConfig>().isSoundDampeningEnabled) {
						sDamp = false;
						damp = 0f;
					}
					if (!ModContent.GetInstance<AudioAdditionsConfig>().isSoundOcclusionEnabled) {
						occ = 0f;
					}
					if (containsIgnoreablePos && Main.LocalPlayer.grappling[0] == 1 || (Main.LocalPlayer.itemAnimation > 0 && Main.LocalPlayer.HeldItem.pick > 0))
						gain = Main.player[Main.myPlayer].GetModPlayer<ReverbPlayer>().ReverbFactor;
                    if (!badStyles.Contains(sfx.Style)) {
                        self.ApplyReverbReturnInstance(gain / 2);
                        if (ModContent.GetInstance<AudioAdditionsConfig>().isSoundOcclusionEnabled)
                            self.ApplyLowPassFilterReturnInstance(occ);
                        if (ModContent.GetInstance<AudioAdditionsConfig>().isSoundDampeningEnabled && sDamp)
                            self.ApplyBandPassFilter(damp);
                    }
                }
			}
		}
	}

	private void ActiveSound_Play(On_ActiveSound.orig_Play orig, ActiveSound self) {
		orig(self);

		if (self.Sound is DynamicSoundEffectInstance)
			dynamicSfxActiveSounds.Add(self);
		if (self.Position.HasValue) {
			bool containsIgnoreablePos = badStyles.Contains(self.Style);
			ReverbAudioSystem.CreateAudioFX(self.Position.Value, out float gain, out float occ, out float damp, out bool sDamp);
			if (!ModContent.GetInstance<AudioAdditionsConfig>().isReverbEnabled)
				gain = 0f;
			if (!ModContent.GetInstance<AudioAdditionsConfig>().isSoundDampeningEnabled) {
				sDamp = false;
				damp = 0f;
			}
			if (containsIgnoreablePos && Main.LocalPlayer.grappling[0] == 1 || (Main.LocalPlayer.itemAnimation > 0 && Main.LocalPlayer.HeldItem.pick > 0))
				gain = Main.player[Main.myPlayer].GetModPlayer<ReverbPlayer>().ReverbFactor;
			/*if (sDamp) {
				if (!badStyles.Contains(self.Style))
					self.Sound.ApplyReverbReturnInstance(gain / 2)
						.ApplyLowPassFilterReturnInstance(occ)
						.ApplyBandPassFilter(damp);
				return;
			}*/
			if (!badStyles.Contains(self.Style)) {
				self.Sound.ApplyReverbReturnInstance(gain / 2);
				if (ModContent.GetInstance<AudioAdditionsConfig>().isSoundOcclusionEnabled)
					self.Sound.ApplyLowPassFilterReturnInstance(occ);
				if (ModContent.GetInstance<AudioAdditionsConfig>().isSoundDampeningEnabled && sDamp)
					self.Sound.ApplyBandPassFilter(damp);
			}
		}
	}

	public static List<SoundStyle> badStyles = new()
	{
		SoundID.Grab,
		SoundID.MenuOpen,
		SoundID.MenuClose,
		SoundID.MenuTick,
		SoundID.Chat,
		SoundID.Research,
		SoundID.ResearchComplete
	};
	public static float occludeAmount;
	public static float reverbActual;
	public static int soundX;
	public static int soundY;
	public static int showTime;
}
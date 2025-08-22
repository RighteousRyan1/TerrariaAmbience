using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaAmbience.Common.Systems;

public class RemoveDefaultWindAndRainSystem : ModSystem {
    private static IAudioTrack _audioTrack_28;
    private static IAudioTrack _audioTrack_45;
    public override void Load() {
        if (Main.dedServ) return;
        // delete those losers 😈
        var lAudioSystem = Main.audioSystem as LegacyAudioSystem;

        _audioTrack_28 = lAudioSystem.AudioTracks[28];
        _audioTrack_45 = lAudioSystem.AudioTracks[45];

        On_Main.Update += RemoveRainAndWind;
        On_LegacyAudioSystem.UpdateAmbientCueTowardStopping += IgnoreWindAndRain_1;
        On_LegacyAudioSystem.UpdateAmbientCueState += IgnoreWindAndRain_2;
    }
    public override void Unload() {
        // replace these losers 💀
        var lAudioSystem = Main.audioSystem as LegacyAudioSystem;

        lAudioSystem.AudioTracks[28] = _audioTrack_28;
        lAudioSystem.AudioTracks[45] = _audioTrack_45;
    }

    void IgnoreWindAndRain_1(On_LegacyAudioSystem.orig_UpdateAmbientCueTowardStopping orig, LegacyAudioSystem self, int i, float stoppingSpeed, ref float trackVolume, float systemVolume) {
        // to refuse to update rain or wind audio tracks lol.
        if (i == 28 || i == 45) {
            return;
        }
        orig(self, i, stoppingSpeed, ref trackVolume, systemVolume);
    }

    void IgnoreWindAndRain_2(On_LegacyAudioSystem.orig_UpdateAmbientCueState orig, LegacyAudioSystem self, int i, bool gameIsActive, ref float trackVolume, float systemVolume) {
        // to refuse to update rain or wind audio tracks lol.
        if (i == 28 || i == 45) {
            return;
        }
        orig(self, i, gameIsActive, ref trackVolume, systemVolume);
    }

    void RemoveRainAndWind(On_Main.orig_Update orig, Main self, Microsoft.Xna.Framework.GameTime gameTime) {
        // is Music_45 windy and Music_28 rainy?
        orig(self, gameTime);

        if (Main.gameMenu)
            return;
        var lAudioSystem = Main.audioSystem as LegacyAudioSystem;

        // rain cue
        lAudioSystem.AudioTracks[28] = null;
        // wind cue
        lAudioSystem.AudioTracks[45] = null;

        /*// woind
        Main.musicFade[45] = -1f;
        // rain
        Main.musicFade[28] = -1f;
        Main.audioSystem.UpdateAmbientCueTowardStopping(45, 1, ref Main.musicFade[45], 0f);
        Main.audioSystem.UpdateAmbientCueTowardStopping(28, 1, ref Main.musicFade[28], 0f);*/
    }
}

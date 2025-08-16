using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Content.Players;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using TerrariaAmbience.Sounds;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Sounds.SoundFilters;

namespace TerrariaAmbience.Content.AddedNPCSounds;

// TODO: fix ass lag
public class SlimeSounds : GlobalNPC {
    public override bool InstancePerEntity => true;

    public Vector2 oldVelocity;

    public static HashSet<int> Slimes = [];

    public static void PopulateSlimes() {
        for (int i = 0; i < NPCID.Search.Count; i++) {
            var name = NPCID.Search.GetName(i);

            if (!name.Contains("slime", StringComparison.CurrentCultureIgnoreCase)) continue;

            Slimes.Add(i);
        }
    }
    public override void PostAI(NPC npc) {
        if (Main.dedServ) return;
        if (!ModContent.GetInstance<AudioAdditionsConfig>().slimySounds) return;

        if (!Slimes.Contains(npc.type)) return;

        var vel = npc.velocity;

        SoundStyle soundStyle = default;

        bool soundStyleGiven = false;

        if (vel.Y == 0 && oldVelocity.Y != 0) {
            int oneOrTwo = Main.rand.Next(1, 3);
            soundStyle = new SoundStyle($"TerrariaAmbience/Sounds/Custom/npcs/slimeland{oneOrTwo}") {
                Volume = MathHelper.Clamp(oldVelocity.Y / 10f, 0f, 1f),
                PitchVariance = 0.1f,
            };
            soundStyleGiven = true;
        }
        else if (vel.Y != 0 && oldVelocity.Y == 0f) {
            soundStyle = new SoundStyle($"TerrariaAmbience/Sounds/Custom/npcs/slimejump") {
                Volume = 0.5f,
                PitchVariance = 0.1f
            };
            soundStyleGiven = true;
        }

        oldVelocity = vel;

        if (!soundStyleGiven) return;

        var param = SoundFilterSystem.LatestParams;

        if (param.BandPassEnabled) {
            var soundWRvb = GeneralHelpers.PlaySound(soundStyle, npc.position);

            if (soundWRvb != null) {
                soundWRvb.ApplyReverb(param.ReverbGain, param);
                soundWRvb.ApplyLowPassFilter(param.LowPassIntensity);
                soundWRvb.ApplyBandPassFilter(param.BandPassIntensity);
            }
        }
        else if (param.LowPassEnabled) {
            var soundWRvb = GeneralHelpers.PlaySound(soundStyle, npc.position);

            if (soundWRvb != null) {
                soundWRvb.ApplyReverb(param.ReverbGain, param);
                soundWRvb.ApplyLowPassFilter(param.LowPassIntensity);
            }
        }

        /*if (param.BandPassEnabled)
            GeneralHelpers.PlaySound(soundStyle, npc.position).ApplyReverb(param.ReverbGain, param).ApplyLowPassFilter(param.LowPassIntensity).ApplyBandPassFilter(param.BandPassIntensity);
        else
            GeneralHelpers.PlaySound(soundStyle, npc.position).ApplyReverb(param.ReverbGain, param).ApplyLowPassFilter(param.LowPassIntensity);*/

    }
}
public class SplashingSounds : GlobalNPC {
    public override bool InstancePerEntity => true;
    bool _wet;
    public override void PostAI(NPC npc) {
        var cfg3 = ModContent.GetInstance<AmbientConfigServer>();

        if (cfg3.newSplashSounds)
            HandleSplashing(npc);
    }

    public void HandleSplashing(NPC npc) {
        bool justWet = npc.wet && !_wet;
        bool justUnwet = !npc.wet && _wet;

        if (justWet || justUnwet) {
            var vel = npc.velocity.Y;

            const float loud_thresh = 10f;

            var soundSplash = new SoundStyle($"TerrariaAmbience/Sounds/Custom/ambient/environment/liquid/entity_splash_{(vel >= loud_thresh ? "heavy" : "light")}");

            if (vel < 10f)
                soundSplash.Volume = vel / loud_thresh / 4;
            if (vel == 0)
                soundSplash.Volume = 0.1f;

            SoundEngine.PlaySound(soundSplash, npc.position);
        }

        _wet = npc.wet;
    }
}
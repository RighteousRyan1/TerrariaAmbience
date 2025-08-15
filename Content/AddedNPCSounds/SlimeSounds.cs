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
using TerrariaAmbience.Sounds.SFXEffects;

namespace TerrariaAmbience.Content.AddedNPCSounds
{
    // TODO: fix ass lag
    public class SlimeSounds : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public Vector2 oldVelocityReal;
        public override void PostAI(NPC npc) {
            if (Main.dedServ) return;
            if (!ModContent.GetInstance<AudioAdditionsConfig>().slimySounds) return;
            if (!npc.FullName.Contains("slime", StringComparison.CurrentCultureIgnoreCase)) return;

            var vel = npc.velocity;
            //var param = ReverbAudioSystem.CreateAudioFX(npc.Center);
            float volume = oldVelocityReal.Y / 30f;

            SoundStyle soundStyle = default;

            if (vel.Y == 0 && oldVelocityReal.Y != 0) {
                int oneOrTwo = Main.rand.Next(1, 3);
                soundStyle = new SoundStyle($"TerrariaAmbience/Sounds/Custom/npcs/slimeland{oneOrTwo}") {
                    Volume = MathHelper.Clamp(oldVelocityReal.Y, 0f, 1f),
                    PitchVariance = 0.1f,
                };
            }
            else if (vel.Y != 0 && oldVelocityReal.Y == 0f) {
                soundStyle = new SoundStyle($"TerrariaAmbience/Sounds/Custom/npcs/slimejump") {
                    Volume = 0.5f,
                    PitchVariance = 0.1f
                };
            }

            oldVelocityReal = vel;

            if (soundStyle == default) return;

            var param = Main.LocalPlayer.GetModPlayer<ReverbPlayer>().LatestParams;

            if (param.BandPassEnabled)
                GeneralHelpers.PlaySound(soundStyle, npc.position).ApplyReverb(param.ReverbGain, param).ApplyLowPassFilter(param.LowPassIntensity).ApplyBandPassFilter(param.BandPassIntensity);
            else
                GeneralHelpers.PlaySound(soundStyle, npc.position).ApplyReverb(param.ReverbGain, param).ApplyLowPassFilter(param.LowPassIntensity);

            /*if (param.BandPassEnabled)
                GeneralHelpers.PlaySound(soundStyle, npc.position).ApplyReverb(param.ReverbGain, param).ApplyLowPassFilter(param.LowPassIntensity).ApplyBandPassFilter(param.BandPassIntensity);
            else
                GeneralHelpers.PlaySound(soundStyle, npc.position).ApplyReverb(param.ReverbGain, param).ApplyLowPassFilter(param.LowPassIntensity);*/

        }
    }

    public class SplashingSounds : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        private bool _wet;
        public override void PostAI(NPC npc)
        {
            var cfg3 = ModContent.GetInstance<AmbientConfigServer>();

            if (cfg3.newSplashSounds)
                HandleSplashing(npc);
        }

        public void HandleSplashing(NPC npc)
        {
            bool justWet = npc.wet && !_wet;
            bool justUnwet = !npc.wet && _wet;

            if (justWet || justUnwet)
            {
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
}

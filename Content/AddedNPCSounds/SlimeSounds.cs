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

namespace TerrariaAmbience.Content.AddedNPCSounds
{
    // TODO: fix ass lag
    public class SlimeSounds : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public ModSoundStyle SlimeLandStyle { get; private set; }
        public ModSoundStyle SlimeJumpStyle { get; private set; }

        public Vector2 oldVelocityReal;
        public override void PostAI(NPC npc)
        {
            if (ModContent.GetInstance<AudioAdditionsConfig>().slimySounds)
            {
                if (npc.FullName.ToLower().Contains("slime"))
                {
                    var vel = npc.velocity;
                    ReverbAudioSystem.CreateAudioFX(npc.Center, out var rv, out var occ, out var dampen, out var shouldDampen);
                    float volume = oldVelocityReal.Y / 30f;
                    if (vel.Y == 0f && oldVelocityReal.Y != 0f)
                    {
                        int oneOrTwo = Main.rand.Next(1, 3);
                        SlimeLandStyle = new ModSoundStyle($"TerrariaAmbience/Sounds/Custom/npcs/slimeland{oneOrTwo}", 0, default, MathHelper.Clamp(oldVelocityReal.Y, 0f, 1f), Main.rand.NextFloat(-0.1f, 0.1f));
                        if (shouldDampen)
                            SoundEngine.PlaySound(SlimeLandStyle, npc.position).ApplyReverbReturnInstance(rv).ApplyLowPassFilterReturnInstance(occ).ApplyBandPassFilter(dampen);
                        else
                            SoundEngine.PlaySound(SlimeLandStyle, npc.position).ApplyReverbReturnInstance(rv).ApplyLowPassFilterReturnInstance(occ);
                    }
                    if (oldVelocityReal.Y == 0f && vel.Y != 0f)
                    {
                        SlimeJumpStyle = new ModSoundStyle($"TerrariaAmbience/Sounds/Custom/npcs/slimejump", 0, default, 0.5f, Main.rand.NextFloat(-0.1f, 0.1f));
                        if (shouldDampen)
                            SoundEngine.PlaySound(SlimeJumpStyle, npc.position).ApplyReverbReturnInstance(rv).ApplyLowPassFilterReturnInstance(occ).ApplyBandPassFilter(dampen);
                        else
                            SoundEngine.PlaySound(SlimeJumpStyle, npc.position).ApplyReverbReturnInstance(rv).ApplyLowPassFilterReturnInstance(occ);
                    }
                    oldVelocityReal = vel;
                }
            }
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

                var soundSplash = new ModSoundStyle($"TerrariaAmbience/Sounds/Custom/ambient/environment/liquid/entity_splash_{(vel >= 9f ? "heavy" : "light")}");

                SoundEngine.PlaySound(soundSplash, npc.position);
            }

            _wet = npc.wet;
        }
    }
}

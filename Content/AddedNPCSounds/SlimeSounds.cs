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
        public override bool CloneNewInstances => true;
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
                        SlimeLandStyle = new ModSoundStyle($"TerrariaAmbience/Sounds/Custom/ambient/npcs/slimeland{oneOrTwo}", 0, default, MathHelper.Clamp(oldVelocityReal.Y, 0f, 1f), Main.rand.NextFloat(-0.1f, 0.1f));
                        if (shouldDampen)
                            SoundEngine.PlaySound(SlimeLandStyle, npc.position).ApplyReverbReturnInstance(rv).ApplyLowPassFilterReturnInstance(occ).ApplyBandPassFilter(dampen);
                        else
                            SoundEngine.PlaySound(SlimeLandStyle, npc.position).ApplyReverbReturnInstance(rv).ApplyLowPassFilterReturnInstance(occ);
                    }
                    if (oldVelocityReal.Y == 0f && vel.Y != 0f)
                    {
                        SlimeJumpStyle = new ModSoundStyle($"TerrariaAmbience/Sounds/Custom/ambient/npcs/slimejump", 0, default, 0.5f, Main.rand.NextFloat(-0.1f, 0.1f));
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
}

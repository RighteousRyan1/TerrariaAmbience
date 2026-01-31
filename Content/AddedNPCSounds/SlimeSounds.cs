using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Sounds.SoundFilters;

namespace TerrariaAmbience.Content.AddedNPCSounds;

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
        if (!ModContent.GetInstance<AudioConfig>().slimySounds) return;

        if (!Slimes.Contains(npc.type)) return;

        var vel = npc.velocity;

        SoundStyle soundStyle = default;

        bool soundStyleGiven = false;

        if (vel.Y == 0 && oldVelocity.Y != 0) {
            int oneOrTwo = Main.rand.Next(1, 3);
            soundStyle = new SoundStyle($"TerrariaAmbience/Sounds/Custom/npcs/slimeland{oneOrTwo}") {
                Volume = MathHelper.Clamp(oldVelocity.Y / 2.5f, 0f, 1f),
                Pitch = GeneralHelpers.NaturalPitchVariance
            };
            soundStyleGiven = true;
        }
        else if (vel.Y != 0 && oldVelocity.Y == 0f) {
            soundStyle = new SoundStyle($"TerrariaAmbience/Sounds/Custom/npcs/slimejump") {
                Volume = 0.5f,
                Pitch = GeneralHelpers.NaturalPitchVariance
            };
            soundStyleGiven = true;
        }

        oldVelocity = vel;

        if (!soundStyleGiven) return;

        var param = SoundFilterSystem.LatestParams;
        var sfx = GeneralHelpers.PlaySound(soundStyle, npc.position);

        sfx?.ApplyFiltersQuick(param);
    }
}
public class SplashingSounds : GlobalNPC {
    public override bool InstancePerEntity => true;
    bool _wet;
    public override void PostAI(NPC npc) {
        var cfg3 = ModContent.GetInstance<AudioConfig>();

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
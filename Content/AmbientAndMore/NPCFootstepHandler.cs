using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaAmbience.Content.AmbientAndMore;

public class NPCFootstepHandler : GlobalNPC {
    private int curFrame;
    private int oldFrame;
    public override bool InstancePerEntity => true;

    public static Dictionary<int, (int[] Frames, float VolMult)> NPCIDToStepFrame = new();
    public static void InitializeNPCStepping() {
        NPCIDToStepFrame.Clear();
        foreach (var elem in ContentSamples.NpcsByNetId) {
            if (elem.Value.townNPC)
                NPCIDToStepFrame.Add(elem.Value.type, ([5, 12], 0.1f));
        }
        AddGenerics([NPCID.Skeleton, NPCID.UndeadMiner, NPCID.UndeadViking, NPCID.ArmoredSkeleton, NPCID.SkeletonArcher,
            NPCID.GoblinScout, NPCID.GoblinPeon, NPCID.GoblinThief, NPCID.GoblinWarrior,
            NPCID.Mummy, NPCID.BloodMummy, NPCID.DarkMummy, NPCID.LightMummy,
            NPCID.AngryBones, NPCID.AngryBonesBig, NPCID.AngryBonesBigHelmet, NPCID.AngryBonesBigMuscle,
            NPCID.HellArmoredBones, NPCID.HellArmoredBonesMace, NPCID.HellArmoredBonesSpikeShield, NPCID.HellArmoredBonesSword, NPCID.BoneLee,
            NPCID.BlueArmoredBones, NPCID.BlueArmoredBonesMace, NPCID.BlueArmoredBonesNoPants, NPCID.BlueArmoredBonesSword,
            NPCID.RustyArmoredBonesAxe, NPCID.RustyArmoredBonesFlail, NPCID.RustyArmoredBonesSword, NPCID.RustyArmoredBonesSwordNoArmor,
            NPCID.Werewolf,
            NPCID.PossessedArmor], 0.2f);
        // certainly more to add later
        List<int> zombieIds = [];

        for (int i = 0; i < NPCID.Search.Count; i++) {
            var name = NPCID.Search.GetName(i);
            if (name.Contains("zombie", StringComparison.CurrentCultureIgnoreCase)) {
                if (ContentSamples.NpcsByNetId[i].frame.Height < 150) {
                    zombieIds.Add(i);
                }
            }
        }
        // zombies arent spawning now for some reason?
        AddZombies([.. zombieIds], 0.2f);
    }
    public static void AddGeneric(int type, float vol) => NPCIDToStepFrame.Add(type, ([5, 12], vol));
    public static void AddGenerics(int[] types, float vol) => Array.ForEach(types, x => AddGeneric(x, vol));
    public static void AddZombie(int type, float vol) => NPCIDToStepFrame.Add(type, ([0], vol));
    public static void AddZombies(int[] types, float vol) => Array.ForEach(types, x => AddZombie(x, vol));
    public override void PostAI(NPC npc) {
        /*if (npc.Hitbox.Contains(Main.MouseWorld.ToPoint())) {
            if (Main.mouseRight && Main.mouseRightRelease) {
                npc.StrikeInstantKill();
                InitializeNPCStepping();
            }
        }*/
        // implement landing sounds i presume?
        if (NPCIDToStepFrame.TryGetValue(npc.type, out (int[] Frames, float VolMult) value)) {
            // avoid division by zero.
            if (npc.frame.Height > 0) {
                curFrame = npc.frame.Y / npc.frame.Height;
                var (Frames, VolMult) = value;
                if (Frames.Any(x => x == curFrame) && oldFrame != curFrame) {
                    var tileBelow = TerrariaAmbience.DefaultFootstepHandler.GetTileBelow(npc);
                    var sounds = TerrariaAmbience.DefaultFootstepHandler.GetFootstepSoundFromTile(tileBelow);
                    foreach (var item in sounds) {
                        item.PlayAny(item.StepVolume * VolMult, npc.Bottom - new Vector2(0, 8));
                    }
                }
                oldFrame = curFrame;
            }
        }
    }
}

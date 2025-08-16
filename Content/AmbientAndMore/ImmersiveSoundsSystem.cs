using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Helpers;
using Microsoft.Xna.Framework;
using Terraria;
using Microsoft.Xna.Framework.Audio;
using TerrariaAmbience.Sounds.SoundFilters;
using Terraria.ID;
using System.Collections.Generic;
using System;

namespace TerrariaAmbience.Content.AmbientAndMore;

public class ImmersiveSoundsSystem : ModSystem {
    // get center and choose a random position above
    public static Vector2 RandFromCenter(float x, float dx, float y, float dy) => new(x + Main.rand.NextFloat(-dx, dx), y + Main.rand.NextFloat(-dy, dy));
    public override void PostUpdateEverything() {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;

        float rainingChanceNumber = 1f / Main.cloudAlpha;
        // Main.NewText(Main.cloudAlpha + " : " + rainingChanceNumber);

        float thunderChance = Main.rand.NextFloat(rainingChanceNumber);
        float howlChance = Main.rand.NextFloat();

        var eligibleThunderLocations = new List<Vector2>();
        var eligibleHowlLocations = new List<Vector2>();

        foreach (var player in Main.ActivePlayers) {
            if (Main.raining && !player.ZoneSnow && player.ZoneOverworldHeight) {
                eligibleThunderLocations.Add(player.Center);
            }
            if (!Main.dayTime && player.ZoneSnow && !player.ZoneUnderworldHeight && !player.ZoneRockLayerHeight) {
                eligibleHowlLocations.Add(player.Center);
            }
        }
        if (eligibleThunderLocations.Count > 0) {
            var randomThunderCenter = eligibleThunderLocations[Main.rand.Next(eligibleThunderLocations.Count)];
            if (thunderChance < 0.005f) {
                // y is at 100 tiles above worldSurface
                // Main.NewText(Main.GameUpdateCount);
                var origX = randomThunderCenter.X;
                var origY = (float)(Main.worldSurface * 0.6f) * 16;
                var dX = 1000f;
                var dY = 100f;
                var rand = RandFromCenter(origX, dX, origY, dY);
                var pitch = Main.rand.NextFloat(-0.2f, -0.1f);

                //var dbg = rand;
                //Dust.QuickBox(dbg, dbg + new Vector2(16), 1, Color.White, null);

                int randClose = Main.rand.Next(1, 6);
                int randDistant = Main.rand.Next(1, 7);
                int randFar = Main.rand.Next(1, 3);
                int randMedium = Main.rand.Next(1, 10);

                string pick = GeneralHelpers.Pick($"close/{randClose}", $"distant/{randDistant}", $"far/{randFar}", $"medial/{randMedium}");
                string pathToThunder = $"TerrariaAmbience/Sounds/Custom/ambient/rain/thunder/{pick}";


                if (Main.dedServ) {
                    var mp = Mod.GetPacket();

                    mp.Write(TAPID.SEND_AMB_SFX);
                    mp.Write(pathToThunder);
                    mp.WriteVector2(rand);
                    mp.Write(pitch);
                    mp.Send();
                }
                else {
                    PlaySound(pathToThunder, rand, pitch);
                }
            }
        }
        if (eligibleHowlLocations.Count > 0) {
            var randomHowlCenter = eligibleHowlLocations[Main.rand.Next(eligibleHowlLocations.Count)];
            if (howlChance < 0.001f) {
                int howlRand = Main.rand.Next(1, 3);
                string pathToHowl = $"TerrariaAmbience/Sounds/Custom/ambient/animals/howl" + howlRand;
                var origX = randomHowlCenter.X;
                var origY = (float)(Main.worldSurface * 0.6f) * 16;
                var dX = 1250f;
                var dY = 100f;
                var rand = RandFromCenter(origX, dX, origY, dY);
                var pitch = Main.rand.NextFloat(-0.4f, -0.1f);

                //Dust.QuickBox(new Vector2(origX, origY), new Vector2(origX + 16, origY + 16), 1, Color.Red, null);

                if (Main.dedServ) {
                    var mp = Mod.GetPacket();

                    mp.Write(TAPID.SEND_AMB_SFX);
                    mp.Write(pathToHowl);
                    mp.WriteVector2(rand);
                    mp.Write(pitch);
                    mp.Send();
                }
                else {
                    PlaySound(pathToHowl, rand, pitch);
                }
            }
        }
        eligibleHowlLocations.Clear();
        eligibleThunderLocations.Clear();
    }
    public static void PlaySound(string path, Vector2 pos, float pitch = 0f) {
        var id = SoundEngine.PlaySound(new SoundStyle(path, 0, SoundType.Ambient), pos);
        if (SoundEngine.TryGetActiveSound(id, out var snd)) {
            var inst = snd.Sound;
            inst.Volume = Main.ambientVolume * (pos.Y > Main.worldSurface * 16 ? 0.35f : 0.9f);
            inst.Pitch = pitch;

            if (RoomDetectionPlayer.IsInRoom) {
                inst.ApplyLowPassFilter(0.02f);
            }
        }
    }
}

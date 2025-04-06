using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Content.AmbientAndMore;

public class SyncAmbienceSystem : ModSystem {
    // doesnt belong here but whatever jit
    public static bool JustTurnedDay;
    public static bool JustTurnedNight;

    private static bool _curDay;
    private static bool _oldDay;
    public override void PostUpdateEverything() {
        _curDay = Main.dayTime;
        CraftSounds.TimeSinceLastCraft++;

        JustTurnedDay = !_oldDay && _curDay;
        JustTurnedNight = _oldDay && !_curDay;

        if (Main.dedServ) {
            if (JustTurnedDay) {
                CreateEveningNight(out var evening, out var night);

                if (Main.dedServ) {
                    var mp = ModContent.GetInstance<TerrariaAmbience>().GetPacket();

                    Console.WriteLine("Sending even " + evening);
                    Console.WriteLine("Sending night " + night);

                    mp.Write(TAPID.SEND_PM_AMB);
                    mp.Write(evening);
                    mp.Write(night);
                    mp.Send();
                }
            }
            else if (JustTurnedNight) {
                CreateMorningDay(out var morning, out var day);

                if (Main.dedServ) {
                    var mp = ModContent.GetInstance<TerrariaAmbience>().GetPacket();
                    Console.WriteLine("Sending morn " + morning);
                    Console.WriteLine("Sending day " + day);
                    mp.Write(TAPID.SEND_AM_AMB);
                    mp.Write(morning);
                    mp.Write(day);
                    mp.Send();
                }
            }
        }
        else if (Main.netMode == NetmodeID.SinglePlayer) {
            if (JustTurnedDay) {
                TerrariaAmbience.DefaultAmbientHandler?.RandomizeDuskTracks();
            }
            else if (JustTurnedNight) {
                TerrariaAmbience.DefaultAmbientHandler?.RandomizeDawnTracks();
            }
        }

        _oldDay = Main.dayTime;
    }

    public static void CreateMorningDay(out int morning, out int day) {
        morning = Main.rand.Next(1, AmbientHandler.NUM_MORNING_AMBIENCE + 1);
        day = Main.rand.Next(1, AmbientHandler.NUM_DAY_AMBIENCE + 1);
        if (Main.raining)
            day = AmbienceID.Day_Quiet;
    }
    public static void CreateEveningNight(out int evening, out int night) {
        evening = Main.rand.Next(1, AmbientHandler.NUM_EVENING_AMBIENCE + 1);
        night = Main.rand.Next(1, AmbientHandler.NUM_NIGHT_AMBIENCE + 1);
    }
}

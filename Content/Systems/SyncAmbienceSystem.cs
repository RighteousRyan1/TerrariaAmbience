using System;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using TerrariaAmbience.Helpers;
using TerrariaAmbience.Content.AmbientAndMore;

namespace TerrariaAmbience.Content.Systems;

public class SyncAmbienceSystem : ModSystem {
    // doesnt belong here but whatever jit
    public static bool JustTurnedDay;
    public static bool JustTurnedNight;

    static bool _curDay;
    static bool _curNight;

    public static int curMorningAmb;
    public static int curDayAmb;
    public static int curEveningAmb;
    public static int curNightAmb;

    static bool _didInitNextLoop;
    public override void PostUpdateEverything() {
        if (!_didInitNextLoop) {
            if (!Main.dedServ) {
                if (Main.dayTime) {
                    TerrariaAmbience.DefaultAmbientHandler?.RandomizeDuskTracks();
                }
                else {
                    TerrariaAmbience.DefaultAmbientHandler?.RandomizeDawnTracks();
                }
            }
            else {
                //if (Main.dayTime) {
                    CreateEveningNight(out var e, out var n);
                    SendPM(e, n);
                //}
                //else {
                    CreateMorningDay(out var m, out var d);
                    SendAM(m, d);
                //}
            }
        }
        _didInitNextLoop = !Main.gameMenu;

        _curDay = Main.dayTime;
        CraftSounds.TimeSinceLastCraft++;

        JustTurnedDay = !_curNight && _curDay;
        JustTurnedNight = _curNight && !_curDay;

        if (Main.dedServ) {
            if (JustTurnedDay) {
                CreateEveningNight(out var evening, out var night);
                SendPM(evening, night);
            }
            else if (JustTurnedNight) {
                CreateMorningDay(out var morning, out var day);
                SendAM(morning, day);
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

        _curNight = Main.dayTime;
    }
    public static void CreateMorningDay(out int morning, out int day) {
        morning = Main.rand.Next(1, AmbientHandler.NUM_MORNING_AMBIENCE + 1);
        day = Main.rand.Next(1, AmbientHandler.NUM_DAY_AMBIENCE + 1);
        if (Main.raining)
            day = AmbienceID.Day_Quiet;

        curDayAmb = day;
        curMorningAmb = morning;
    }
    public static void CreateEveningNight(out int evening, out int night) {
        evening = Main.rand.Next(1, AmbientHandler.NUM_EVENING_AMBIENCE + 1);
        night = Main.rand.Next(1, AmbientHandler.NUM_NIGHT_AMBIENCE + 1);

        curEveningAmb = evening;
        curNightAmb = night;
    }

    public static void SendAM(int morning, int day) {
        var mp = ModContent.GetInstance<TerrariaAmbience>().GetPacket();
        //Console.WriteLine("Sending morn " + morning);
        //Console.WriteLine("Sending day " + day);
        mp.Write(TAPID.SEND_AM_AMB);
        mp.Write(morning);
        mp.Write(day);
        mp.Send();
    }
    public static void SendPM(int evening, int night) {
        var mp = ModContent.GetInstance<TerrariaAmbience>().GetPacket();

        Console.WriteLine("Sending even " + evening);
        Console.WriteLine("Sending night " + night);

        mp.Write(TAPID.SEND_PM_AMB);
        mp.Write(evening);
        mp.Write(night);
        mp.Send();
    }

    public static void AskForAmbiences() {
        var mp = ModContent.GetInstance<TerrariaAmbience>().GetPacket();

        mp.Write(TAPID.AMB_ASK);

        mp.Send();
    }
}

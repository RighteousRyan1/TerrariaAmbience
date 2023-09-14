using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerrariaAmbience.Content;
using System;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Common.Systems
{
    public class GradualValueSystem : ModSystem
    {
        public static float Gradient_FromNoon { get; private set; }
        public static float Gradient_FromEvening { get; private set; }
        public static float Gradient_FromNight { get; private set; }
        public static float Gradient_FromMorning { get; private set; }
        public static float Gradient_SkyToUnderground { get; private set; }
        public static float Gradient_Underground { get; private set; }
        public static float Gradient_Hell { get; private set; }

        public static float Gradient_AllDayPartNight { get; private set; }
        public static float Gradient_AllNightPartDay { get; private set; }

        public static float Gradient_WindSpeedsLow { get; private set; }
        public static float Gradient_WindSpeedsHigh { get; private set; }

        public static double IngameGlobalTime { get; private set; }

        public static double CreateGradientValue(double value, double min, double max)
        {
            double mid = (max + min) / 2;
            double returnValue;

            if (value > mid)
            {
                var thing = 1f - (value - min) / (max - min) * 2;
                returnValue = 1f + thing;
                return Utils.Clamp(returnValue, 0f, 1f);
            }
            returnValue = (value - min) / (max - min) * 2;
            return Utils.Clamp(returnValue, 0f, 1f);
        }

        public static float CreateGradientValue(float value, float min, float max)
        {
            float mid = (max + min) / 2;
            float returnValue;

            if (value > mid)
            {
                var thing = 1f - (value - min) / (max - min) * 2;
                returnValue = 1f + thing;
                return Utils.Clamp(returnValue, 0f, 1f);
            }
            returnValue = (value - min) / (max - min) * 2;
            return Utils.Clamp(returnValue, 0f, 1f);
        }

        private static void UpdateGradients()
        {
            IngameGlobalTime = Main.time + (Main.dayTime ? 0 : Main.dayLength);

            Gradient_WindSpeedsLow = CreateGradientValue(MathF.Abs(Main.windSpeedCurrent), -0.2f, 0.4f);
            Gradient_WindSpeedsHigh = CreateGradientValue(MathF.Abs(Main.windSpeedCurrent), 0.2f, 1.0f);

            Gradient_Hell = (float)CreateGradientValue(Main.LocalPlayer.Center.Y, (Main.maxTilesY - 300) * 16, Main.maxTilesY * 16);
            Gradient_SkyToUnderground = (float)CreateGradientValue(Main.LocalPlayer.Center.Y, Main.worldSurface * 16 * 0.35f, Main.worldSurface * 1.05f * 16);
            Gradient_Underground = (float)CreateGradientValue(Main.LocalPlayer.Center.Y, Main.worldSurface * 16, (Main.maxTilesY - 150) * 16);
            if (Main.dayTime)
            {
                Gradient_FromEvening = (float)CreateGradientValue(Main.time, Main.dayLength - 4000, Main.dayLength + 4000) * Gradient_SkyToUnderground;
                Gradient_FromMorning = (float)CreateGradientValue(Main.time, -5000, 8000); // * Gradient_SkyToUnderground; // accidental?
                Gradient_FromNoon = (float)CreateGradientValue(Main.time, 0, Main.dayLength); // * Gradient_SkyToUnderground;
                Gradient_FromNight = 0;

                Gradient_AllDayPartNight = (float)CreateGradientValue(Main.time, -4000, Main.dayLength + 4000);
                if (Main.time < Main.dayLength / 2)
                    Gradient_AllNightPartDay = (float)CreateGradientValue(Main.time, -Main.nightLength - 4000, 4000);
                else
                    Gradient_AllNightPartDay = (float)CreateGradientValue(Main.time, Main.dayLength - 4000, Main.dayLength + Main.nightLength + 4000);
            }
            else
            {
                Gradient_FromEvening = (float)CreateGradientValue(Main.time, -4000, 4000) * Gradient_SkyToUnderground;
                Gradient_FromMorning = (float)CreateGradientValue(Main.time, Main.nightLength - 5000, Main.nightLength + 8000) * Gradient_SkyToUnderground;
                Gradient_FromNoon = 0;
                Gradient_FromNight = (float)CreateGradientValue(Main.time, 0, Main.nightLength) * Gradient_SkyToUnderground;

                if (Main.time < Main.nightLength / 2)
                    Gradient_AllDayPartNight = (float)CreateGradientValue(Main.time, -Main.dayLength - 4000, 4000);
                else
                    Gradient_AllDayPartNight = (float)CreateGradientValue(Main.time, Main.nightLength - 4000, Main.dayLength + 4000);
                Gradient_AllNightPartDay = (float)CreateGradientValue(Main.time, -4000, Main.nightLength + 4000);
            }
        }

        public override void PostUpdateEverything()
        {
            // Mod.Logger.Debug($"{Gradient_SkyToUnderground} | {Gradient_Underground}");
            UpdateGradients();
        }
    }
}
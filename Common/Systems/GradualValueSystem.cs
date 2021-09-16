using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerrariaAmbience.Content;

namespace TerrariaAmbience.Common.Systems
{
    public class GradualValueSystem : ModSystem
    {
        public static double Gradient_FromNoon { get; private set; }
        public static double Gradient_FromEvening { get; private set; }
        public static double Gradient_FromNight { get; private set; }
        public static double Gradient_FromMorning { get; private set; }
        public static double Gradient_SkyToUnderground { get; private set; }
        public static double Gradient_Underground { get; private set; }
        public static double Gradient_Hell { get; private set; }

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
            Gradient_Hell = CreateGradientValue(Main.LocalPlayer.Center.Y, (Main.maxTilesY - 300) * 16, Main.maxTilesY * 16);
            Gradient_SkyToUnderground = CreateGradientValue(Main.LocalPlayer.Center.Y, Main.worldSurface * 16 * 0.25f, Main.rockLayer * 16);
            Gradient_Underground = CreateGradientValue(Main.LocalPlayer.Center.Y, Main.worldSurface * 16, (Main.maxTilesY - 150) * 16);
            if (Main.dayTime)
            {
                Gradient_FromEvening = CreateGradientValue(Main.time, Main.dayLength - 4000, Main.dayLength + 4000) * Gradient_SkyToUnderground;
                Gradient_FromMorning = CreateGradientValue(Main.time, -5000, 8000) * Gradient_SkyToUnderground;
                Gradient_FromNoon = CreateGradientValue(Main.time, 0, Main.dayLength) * Gradient_SkyToUnderground;
                Gradient_FromNight = 0;
            }
            else
            {
                Gradient_FromEvening = CreateGradientValue(Main.time, -4000, 4000) * Gradient_SkyToUnderground;
                Gradient_FromMorning = CreateGradientValue(Main.time, Main.nightLength - 5000, Main.nightLength + 8000) * Gradient_SkyToUnderground;
                Gradient_FromNoon = 0;
                Gradient_FromNight = CreateGradientValue(Main.time, 0, Main.nightLength) * Gradient_SkyToUnderground;
            }
        }

        public override void PostUpdateEverything()
        {
            // Mod.Logger.Debug($"{Gradient_SkyToUnderground} | {Gradient_Underground}");
            UpdateGradients();
        }
    }
}
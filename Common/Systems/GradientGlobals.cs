using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerrariaAmbience.Content;
using System;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Common.Systems
{
    public static class GradientGlobals
    {
        public static float FromNoon { get; private set; }
        public static float FromEvening { get; private set; }
        public static float FromMidnight { get; private set; }
        public static float FromMorning { get; private set; }
        public static float SkyToUnderground { get; private set; }
        public static float Underground { get; private set; }
        public static float Hell { get; private set; }

        public static float AllDayPartNight { get; private set; }
        public static float AllNightPartDay { get; private set; }

        public static float WindSpeedsLow { get; private set; }

        public static double IngameGlobalTime { get; private set; }


        public static float WindCapped;
        public static float WindLight;
        private static float MinLowWind = -0.2f;
        private static float MaxLowWind = 0.4f;

        private static float MinHighWind = 0.2f;
        private static float MaxHighWind = 1f;

        public static float WindSpeedsHigh;
        public static float RainIntensityForSnow;
        public static float RainIntensityForRainLight;
        public static float RainIntensityForRainMed;
        public static float RainIntensityForRainHeavy;
        /// <summary>
        /// If the <see cref="Gradient"/> specified is higher than the median, cap it at 1.
        /// </summary>
        public static float GradientCapped(Gradient gradient) {
            if (gradient.Value > ((gradient.Min + gradient.Max) / 2))
                return 1;
            return gradient;
        }
        // same
        public static float GradientCapped(float value, float min, float max) {
            if (value > ((min + max) / 2))
                return 1;
            return Gradient.CreateFloat(value, min, max);
        }
        internal static void Update() {
            IngameGlobalTime = Main.time + (Main.dayTime ? 0 : Main.dayLength);

            WindSpeedsHigh = GradientCapped(MathF.Abs(Main.windSpeedCurrent), MinHighWind, MaxHighWind);

            WindSpeedsLow = Gradient.CreateFloat(MathF.Abs(Main.windSpeedCurrent), MinLowWind, MaxLowWind);
            WindSpeedsHigh = GradientCapped(MathF.Abs(Main.windSpeedCurrent), MinHighWind, MaxHighWind);

            WindLight = Gradient.CreateFloat(MathF.Abs(Main.windSpeedCurrent), 0f, 0.65f * 1.5f);
            WindCapped = GradientCapped(MathF.Abs(Main.windSpeedCurrent), 0.35f * 1.5f, MaxHighWind * 1.5f);
            // magical numbers (aka 1.5f)
            // to future me, these are just values to make the breeze more seamlessly combine at higher/lower values

            RainIntensityForSnow = GradientCapped(Main.maxRaining, 0.1f, 1.1f);
            RainIntensityForRainLight = Gradient.CreateFloat(Main.maxRaining, 0f, 0.4f);
            RainIntensityForRainMed = Gradient.CreateFloat(Main.maxRaining, 0.2f, 1f);
            RainIntensityForRainHeavy = GradientCapped(Main.maxRaining, 0.5f, 1.5f);

            Hell = Gradient.CreateFloat(Main.LocalPlayer.Center.Y, (Main.maxTilesY - 300) * 16, Main.maxTilesY * 16);
            SkyToUnderground = (float)Gradient.CreateDouble(Main.LocalPlayer.Center.Y, Main.worldSurface * 16 * 0.35f, Main.worldSurface * 1.05f * 16);
            Underground = (float)Gradient.CreateDouble(Main.LocalPlayer.Center.Y, Main.worldSurface * 16, (Main.maxTilesY - 150) * 16);
            if (Main.dayTime) {
                FromEvening = (float)Gradient.CreateDouble(Main.time, Main.dayLength - 4000, Main.dayLength + 4000); // * Gradient_SkyToUnderground;
                FromMorning = (float)Gradient.CreateDouble(Main.time, -5000, 8000); // * Gradient_SkyToUnderground; // accidental?
                FromNoon = (float)Gradient.CreateDouble(Main.time, 0, Main.dayLength); // * Gradient_SkyToUnderground;
                FromMidnight = 0;

                AllDayPartNight = (float)Gradient.CreateDouble(Main.time, -4000, Main.dayLength + 4000);
                if (Main.time < Main.dayLength / 2)
                    AllNightPartDay = (float)Gradient.CreateDouble(Main.time, -Main.nightLength - 4000, 4000);
                else
                    AllNightPartDay = (float)Gradient.CreateDouble(Main.time, Main.dayLength - 4000, Main.dayLength + Main.nightLength + 4000);
            }
            else {
                FromEvening = (float)Gradient.CreateDouble(Main.time, -4000, 4000) * SkyToUnderground;
                FromMorning = (float)Gradient.CreateDouble(Main.time, Main.nightLength - 5000, Main.nightLength + 8000) * SkyToUnderground;
                FromNoon = 0;
                FromMidnight = (float)Gradient.CreateDouble(Main.time, 0, Main.nightLength) * SkyToUnderground;

                if (Main.time < Main.nightLength / 2)
                    AllDayPartNight = (float)Gradient.CreateDouble(Main.time, -Main.dayLength - 4000, 4000);
                else
                    AllDayPartNight = (float)Gradient.CreateDouble(Main.time, Main.nightLength - 4000, Main.dayLength + 4000);
                AllNightPartDay = (float)Gradient.CreateDouble(Main.time, -4000, Main.nightLength + 4000);
            }
        }
    }
}
// lowkey created this just to use it for a tiny thing but okay :||||
public class Gradient
{
    public Func<float> FuncMin;
    public float Min {
        get => FuncMin.Invoke();
    }
    public Func<float> FuncMax;
    public float Max {
        get => FuncMax.Invoke();
    }
    private Func<float> _value;

    /// <summary>If this value (<see cref="ValueOverride"/>) is <see cref="float.NaN"/>, it will not override.</summary>
    public float ValueOverride = float.NaN;

    public float Value => _value.Invoke();

    public float GradientValue => float.IsNaN(ValueOverride) ? CreateFloat(Value, FuncMin.Invoke(), FuncMax.Invoke()) : ValueOverride;

    public Gradient(Func<float> value, Func<float> min, Func<float> max) {
        FuncMin = min;
        FuncMax = max;
        _value = value;
    }

    public static implicit operator float(Gradient g) => g.GradientValue;

    /// <summary>Just in case you don't want to abuse the garbage collector.</summary>
    public static float CreateFloat(float value, float min, float max) {
        float mid = (max + min) / 2;
        float returnValue;

        if (value > mid) {
            var thing = 1f - (value - min) / (max - min) * 2;
            returnValue = 1f + thing;
            return Utils.Clamp(returnValue, 0f, 1f);
        }
        returnValue = (value - min) / (max - min) * 2;
        return Utils.Clamp(returnValue, 0f, 1f);
    }
    /// <summary>Just in case you don't want to abuse the garbage collector.</summary>
    public static double CreateDouble(double value, double min, double max) {
        double mid = (max + min) / 2;
        double returnValue;

        if (value > mid) {
            var thing = 1f - (value - min) / (max - min) * 2;
            returnValue = 1f + thing;
            return Utils.Clamp(returnValue, 0f, 1f);
        }
        returnValue = (value - min) / (max - min) * 2;
        return Utils.Clamp(returnValue, 0f, 1f);
    }
}

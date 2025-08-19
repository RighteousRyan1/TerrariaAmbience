using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using TerrariaAmbience.Content.Systems;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Content.AmbientAndMore;

public class AmbientDisplaySystem : ModSystem
{
    public static Texture2D Arrow;

    public static Dictionary<int, string> FlavorsMorning = new() {
        [0] = "Not loaded",
        [AmbienceID.Morning_CricketsQuiet] = "Crickets",
        [AmbienceID.Morning_CricketsTrillingQuiet] = "Trilling crickets",
        [AmbienceID.Morning_CricketsAndCicadas] = "Crickets and cicadas",
        [AmbienceID.Morning_CricketsTrillingLoud] = "Loud crickets",
        [AmbienceID.Morning_CicadasAndBugs] = "Cicadas and bugs",
        [AmbienceID.Morning_EverythingTrilling] = "Trilling crickets and cicadas",
    };
    public static Dictionary<int, string> FlavorsDay = new() {
        [0] = "Not loaded",
        [AmbienceID.Day_BirdsAndCrowsLoud] = "Crows and chirping birds",
        [AmbienceID.Day_BirdsSinging] = "Singing birds",
        [AmbienceID.Day_BirdsSingingLoud] = "Loud singing birds",
        [AmbienceID.Day_BirdsSingingQuiet] = "Quiet singing birds",
        [AmbienceID.Day_Quiet] = "Quiet crickets and bugs",
        [AmbienceID.Day_BirdsSingingOften] = "Persistent singing birds",
    };
    public static Dictionary<int, string> FlavorsEvening = new() {
        [0] = "Not loaded",
        [AmbienceID.Evening_SinewaveCicadas] = "Cicadas",
        [AmbienceID.Evening_LoudCricketsWithCicadas] = "Crickets and cicadas",
        [AmbienceID.Evening_HumidSoundingCrickets] = "Loud cicadas",
        [AmbienceID.Evening_CricketsTrilling] = "Trilling crickets",
        [AmbienceID.Evening_CricketsAndQuietCicadas] = "Intermittent crickets and cicadas",
        [AmbienceID.Evening_VariousAnimals] = "Many quiet fauna",
    };
    public static Dictionary<int, string> FlavorsNight = new() {
        [0] = "Not loaded",
        [AmbienceID.Night_CricketsQuiet] = "Quiet crickets",
        [AmbienceID.Night_CricketsPersistent] = "Trilling crickets",
        [AmbienceID.Night_CicadasAndCrickets] = "Chirping crickets",
        [AmbienceID.Night_CicadasConstant] = "Loud cicadas",
        [AmbienceID.Night_LoudEverything] = "Many loud fauna",
        [AmbienceID.Night_TrillingCricketsAndFrogs] = "Crickets and frogs"
    };

    bool _show = true;
    float _ease;
    public override void PostSetupContent() {
        Arrow = Mod.Assets.Request<Texture2D>("Content/UI/UIButtonRight", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
    }
    public override void PostDrawInterface(SpriteBatch spriteBatch) {
        var cfg = ModContent.GetInstance<UIConfig>();

        if (!cfg.showAmbientForecastUi)
            return;

        var posOrig = new Vector2(Main.playerInventory ? 500 : 470, Main.playerInventory ? 42.5f : 44f);
        var origin = new Vector2(0, Arrow.Height / 2);
        var scale = Main.playerInventory ? 1.15f : 1f;

        var rect = new Rectangle((int)(posOrig.X - origin.X), (int)(posOrig.Y - origin.Y), (int)(Arrow.Width * scale), (int)(Arrow.Height * scale));

        var mouseOver = rect.Contains(Main.MouseScreen.ToPoint());

        if (mouseOver) {
            Main.isMouseLeftConsumedByUI = true;
            if (Main.mouseLeft && Main.mouseLeftRelease) {
                _show = !_show;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            
        }
        EasingFunction easeToUse = EasingFunction.OutQuad;

        if (_show) {
            //easeToUse = EasingFunction.OutQuart;
            if (_ease > 1) _ease = 1;
            else _ease += 0.02f * TerrariaAmbience.WorkaroundDeltaTime;
        } else {
            if (_ease < 0) _ease = 0;
            else _ease -= 0.02f * TerrariaAmbience.WorkaroundDeltaTime;
            //easeToUse = EasingFunction.InQuart;
        }
        var textOff = new Vector2(Easings.GetEasingBehavior(easeToUse, _ease) * (Arrow.Width + 5f), 0f);

        var curMorningThing = FlavorsMorning[SyncAmbienceSystem.curMorningAmb];
        var curDayThing = FlavorsDay[SyncAmbienceSystem.curDayAmb];
        var curEveningThing = FlavorsEvening[SyncAmbienceSystem.curEveningAmb];
        var curNightThing = FlavorsNight[SyncAmbienceSystem.curNightAmb];


        var txtScale = 0.6f;
        var measure = FontAssets.MouseText.Value.MeasureString(curMorningThing) * scale * txtScale;
        var textOrig = new Vector2(0, measure.Y / 2);

        var display = !Main.dayTime ? $"Morning: {curMorningThing}\nDaytime: {curDayThing}" : $"Evening: {curEveningThing}\nNight: {curNightThing}";

        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, display, posOrig + textOff - new Vector2(0, measure.Y / 2), Color.White * _ease, 0f, textOrig, Vector2.One * scale * txtScale);

        if (mouseOver) {
            for (int i = 0; i < 4; i++) {
                spriteBatch.Draw(Arrow, posOrig + new Vector2(2f).RotatedBy(MathHelper.TwoPi / 4 * i), null, Color.Black, 0f, origin, scale, default, 0f);
            }
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, _show ? "Hide ambient forecast" : "Display ambient forecast", Main.MouseScreen + new Vector2(25), Color.White, 0f, Vector2.Zero, Vector2.One);
        }

        spriteBatch.Draw(Arrow, posOrig, null, Color.White, 0f, origin, scale, default, 0f);
    }
}

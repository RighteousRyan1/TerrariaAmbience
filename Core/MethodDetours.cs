using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaAmbience.Content;
using ReLogic.Graphics;
using Microsoft.Xna.Framework.Audio;
using Terraria.ID;
using Terraria.UI.Chat;
using TerrariaAmbience.Content.Players;

namespace TerrariaAmbience.Core
{
    internal class MethodDetours
    {
        public static void DetourAll()
        {
            On.Terraria.Main.DrawMenu += Main_DrawMenu;
            On.Terraria.Main.DrawInterface_30_Hotbar += Main_DrawInterface_30_Hotbar;
        }
        private static Vector2 drawPos;
        private static string displayable;
        private static void Main_DrawInterface_30_Hotbar(On.Terraria.Main.orig_DrawInterface_30_Hotbar orig, Main self)
        {
            orig(self);

            // Main.spriteBatch.Begin();
            var loader = Ambience.Instance;

            var aPlayer = Main.player[Main.myPlayer].GetModPlayer<FootstepsAndAmbiencePlayer>();
            if (aPlayer.soundInstanceSnowStep != null && aPlayer.soundInstanceWoodStep != null && aPlayer.soundInstanceStoneStep != null && aPlayer.soundInstanceGrassStep != null && aPlayer.soundInstanceSandStep != null)
            {
                displayable = ModAmbience.modAmbienceList.Count <= 0 ?$"{loader.BeachWaves.Name}: {loader.beachWavesVolume}"
                    + $"\n{loader.CampfireCrackle.Name}: {loader.crackleVolume}"
                    + $"\n{loader.SnowBreezeDay.Name}: {loader.snowDayVolume}"
                    + $"\n{loader.SnowBreezeNight.Name}: {loader.snowNightVolume}"
                    + $"\n{loader.DayCrickets.Name}: {loader.dayCricketsVolume}"
                    + $"\n{loader.NightCrickets.Name}: {loader.nightCricketsVolume}"
                    + $"\n{loader.EveningCrickets.Name}: {loader.eveningCricketsVolume}"
                    + $"\n{loader.CavesAmbience.Name}: {loader.ugAmbienceVolume}"
                    + $"\n{loader.CrimsonRumbles.Name}: {loader.crimsonRumblesVolume}"
                    + $"\n{loader.CorruptionRoars.Name}: {loader.corruptionRoarsVolume}"
                    + $"\n{loader.DayJungle.Name}: {loader.dayJungleVolume}"
                    + $"\n{loader.NightJungle.Name}: {loader.nightJungleVolume}"
                    + $"\n{loader.DesertAmbience.Name}: {loader.desertCricketsVolume}"
                    + $"\n{loader.HellRumble.Name}: {loader.hellRumbleVolume}"
                    + $"\n{loader.Rain.Name}: {loader.rainVolume}"
                    + $"\nFootsteps:\nGrass: {aPlayer.soundInstanceGrassStep.State}"
                    + $"\nStone: {aPlayer.soundInstanceStoneStep.State}"
                    + $"\nWood: {aPlayer.soundInstanceWoodStep.State}"
                    + $"\nSnow: {aPlayer.soundInstanceSnowStep.State}"
                    + $"\nSand: {aPlayer.soundInstanceSandStep.State}" :

               displayable = $"{loader.BeachWaves.Name}: {loader.beachWavesVolume}"
                    + $"\n{loader.CampfireCrackle.Name}: {loader.crackleVolume}"
                    + $"\n{loader.SnowBreezeDay.Name}: {loader.snowDayVolume}"
                    + $"\n{loader.SnowBreezeNight.Name}: {loader.snowNightVolume}"
                    + $"\n{loader.DayCrickets.Name}: {loader.dayCricketsVolume}"
                    + $"\n{loader.NightCrickets.Name}: {loader.nightCricketsVolume}"
                    + $"\n{loader.EveningCrickets.Name}: {loader.eveningCricketsVolume}"
                    + $"\n{loader.CavesAmbience.Name}: {loader.ugAmbienceVolume}"
                    + $"\n{loader.CrimsonRumbles.Name}: {loader.crimsonRumblesVolume}"
                    + $"\n{loader.CorruptionRoars.Name}: {loader.corruptionRoarsVolume}"
                    + $"\n{loader.DayJungle.Name}: {loader.dayJungleVolume}"
                    + $"\n{loader.NightJungle.Name}: {loader.nightJungleVolume}"
                    + $"\n{loader.DesertAmbience.Name}: {loader.desertCricketsVolume}"
                    + $"\n{loader.HellRumble.Name}: {loader.hellRumbleVolume}"
                    + $"\n{loader.Rain.Name}: {loader.rainVolume}"
                    + $"\nFootsteps:\nGrass: {aPlayer.soundInstanceGrassStep.State}"
                    + $"\nStone: {aPlayer.soundInstanceStoneStep.State}"
                    + $"\nWood: {aPlayer.soundInstanceWoodStep.State}"
                    + $"\nSnow: {aPlayer.soundInstanceSnowStep.State}"
                    + $"\nSand: {aPlayer.soundInstanceSandStep.State}\nModded Sounds:";


                int index = 0;
                foreach (ModAmbience ambient in ModAmbience.modAmbienceList)
                {
                    if (ambient != null)
                    {
                        if (ModAmbience.modAmbienceList.Count > 0)
                        {
                            index = ModAmbience.modAmbienceList.Count;
                            displayable += $"\n{ambient.Name}: {ambient.Volume}";
                        }
                    }
                }
                // Main.NewText(index);
            }

            if (ModContent.GetInstance<AmbientConfig>().volVals)
            {
                if (Main.playerInventory && (Main.mapStyle == 0 || Main.mapStyle == 2))
                {
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 2, 175);
                }
                if (Main.mapStyle == 1 && Main.playerInventory)
                {
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 2, 375);
                }
                if (Main.mapStyle == 1 && !Main.playerInventory)
                {
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 3, 375);
                }
                if ((Main.mapStyle == 0 || Main.mapStyle == 2) && !Main.playerInventory)
                {
                    drawPos = new Vector2(Main.screenWidth - Main.screenHeight / 3, 175);
                }

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, 
                    Main.fontDeathText, 
                    displayable != default ? displayable : "NUT", 
                    position: drawPos, 
                    Color.LightGray, 
                    0f, 
                    origin: Vector2.Zero, 
                    baseScale: new Vector2(0.25f, 0.25f), 
                    -1, 
                    1);
            }
        }

        private static void Main_DrawMenu(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            var sb = Main.spriteBatch;

            string viewPost = "View the Terraria Ambience forums post here:";
            string forums = "Forums Post";
            var rect = new Rectangle((int)(Main.fontDeathText.MeasureString(viewPost).X * 0.35f) + 10, 4, (int)(Main.fontDeathText.MeasureString(forums).X * 0.35f), (int)(Main.fontDeathText.MeasureString(forums).Y * 0.25f));

            bool hovering = rect.Contains(Main.MouseScreen.ToPoint());

            Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(sb, Main.fontDeathText, viewPost, new Vector2(4, 4), Color.LightGray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);
            Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(sb, Main.fontDeathText, forums, new Vector2((int)(Main.fontDeathText.MeasureString(viewPost).X * 0.35f) + 10, 4), hovering ? Color.White : Color.Gray, 0f, Vector2.Zero, new Vector2(0.35f, 0.35f), 0, 1);

            if (hovering)
            {
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    System.Diagnostics.Process.Start("https://forums.terraria.org/index.php?threads/terraria-ambience-mod.104161/");
                }
            }
            orig(self, gameTime);
        }
    }
}

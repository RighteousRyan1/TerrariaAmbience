using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaAmbience.Content;
using ReLogic.Graphics;
using Microsoft.Xna.Framework.Audio;
using Terraria.ID;

namespace TerrariaAmbience.Core
{
    internal class MethodDetours
    {
        public static void DetourAll()
        {
            On.Terraria.Main.DrawMenu += Main_DrawMenu;
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

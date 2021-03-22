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
        public static MethodDetours Instance
        {
            get => ModContent.GetInstance<MethodDetours>();
        }
        public void DetourAll()
        {
            On.Terraria.Main.DrawMenu += Main_DrawMenu;
            // On.Terraria.Main.DoUpdate += Main_DoUpdate;
            On.Terraria.Main.PlaySoundInstance += Main_PlaySoundInstance;
        }

        private void Main_PlaySoundInstance(On.Terraria.Main.orig_PlaySoundInstance orig, SoundEffectInstance sound)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (Main.player[Main.myPlayer].HeadWet())
                {
                    sound.Volume = 0.4f;
                    sound.Pitch = -0.25f;
                }
                else
                {
                    sound.Volume = 1f;
                    sound.Pitch = 0f;
                }
            }
        }

        private void Main_DrawMenu(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            var sb = Main.spriteBatch;
            sb.Begin();
            sb.DrawString(Main.fontDeathText, "View the forums post here:", new Vector2(4, 4), Color.White);
            sb.End();
            orig(self, gameTime);
        }
        private float lastPlayerPositionOnGround;
        private float delta_lastPos_playerBottom;
        private void Main_DoUpdate(On.Terraria.Main.orig_DoUpdate orig, Main self, GameTime gameTime)
        {
            var mod = ModContent.GetInstance<TerrariaAmbience.Content.TerrariaAmbience>();
            orig(self, gameTime);
            var aLoader = Ambience.Instance;

            Ambience.DoUpdate_Ambience();
            Ambience.UpdateVolume();
            if (Main.gameMenu)
            {
                aLoader.dayCricketsVolume = 0f;
                aLoader.nightCricketsVolume = 0f;
                aLoader.eveningCricketsVolume = 0f;
                aLoader.desertCricketsVolume = 0f;
                aLoader.crackleVolume = 0f;
                aLoader.ugAmbienceVolume = 0f;
                aLoader.crimsonRumblesVolume = 0f;
                aLoader.snowDayVolume = 0f;
                aLoader.snowNightVolume = 0f;
                aLoader.corruptionRoarsVolume = 0f;
                aLoader.dayJungleVolume = 0f;
                aLoader.nightJungleVolume = 0f;
                aLoader.beachWavesVolume = 0f;
                aLoader.hellRumbleVolume = 0f;
                aLoader.rainVolume = 0f;
            }
            if (Main.gameMenu) return;
            Player player = Main.player[Main.myPlayer]?.GetModPlayer<AmbiencePlayer>().player;
            // soundInstancer.Condition = player.ZoneSkyHeight;
            if (Main.hasFocus)
            {
                delta_lastPos_playerBottom = lastPlayerPositionOnGround - player.Bottom.Y;

                // Main.NewText(delta_lastPos_playerBottom);
                if (player.velocity.Y <= 0 || player.teleporting)
                {
                    lastPlayerPositionOnGround = player.Bottom.Y;
                }
                // Main.NewText(player.fallStart - player.Bottom.Y);
                if (delta_lastPos_playerBottom > -300)
                {
                    Main.soundSplash[0] = Ambience.Instance.SplashNew;
                }
                else if (delta_lastPos_playerBottom <= -300)
                {
                    Main.soundSplash[0] = mod.GetSound($"{Ambience.ambienceDirectory}/environment/liquid/entity_splash_heavy");
                }
                /*if (Main.mouseRight)
                {
                    Main.soundInstanceSplash[0]?.Play(); // entity splash
                }
                if (Main.mouseLeft)
                {
                    Main.soundInstanceSplash[1]?.Play(); // hook bob?
                // Finish mini-spla
                }*/
            }
            Ambience.ClampAll();
        }
    }
}

using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Content
{
    internal class SoundChanges
    {
        public static void Init()
        {
			if (!Main.dedServ)
			{
				var mod = ModContent.GetInstance<TerrariaAmbience>();
                // 45 in MusicID is the wind ambience

                // Now change the sound (Main.soundX = y)
                On.Terraria.Audio.SoundPlayer.Play += Switch;
            }
        }

        private static ReLogic.Utilities.SlotId Switch(On.Terraria.Audio.SoundPlayer.orig_Play orig, SoundPlayer self, ref SoundStyle style, Microsoft.Xna.Framework.Vector2? position)
        {
            if (!Main.dedServ)
            {
                if (style == SoundID.Splash)
                {
                    style = GeneralHelpers.SimpleSoundStyle($"{nameof(TerrariaAmbience)}/Sounds/Custom/nothingness", 0);
                }
                if (style == SoundID.ZombieMoan)
                {
                    int rand = Main.rand.Next(1, 4);
                    style = GeneralHelpers.SimpleSoundStyle($"{nameof(TerrariaAmbience)}/Sounds/Custom/npcs/zombie{rand}", 0);
                }
            }
            return orig(self, ref style, position);
        }
    }
}
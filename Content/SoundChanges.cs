using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Content
{
    internal class SoundChanges
    {
        private static Asset<SoundEffect>[] zombieCache;

        private static Asset<SoundEffect>[] splashCache;

        public static void Init()
        {
			if (!Main.dedServ)
			{
                splashCache = SoundEngine.LegacySoundPlayer.SoundSplash;
				zombieCache = SoundEngine.LegacySoundPlayer.SoundZombie;

				var mod = ModContent.GetInstance<TerrariaAmbience>();
				// 45 in MusicID is the wind ambience

				// Now change the sound (Main.soundX = y)

				// NOTE: drip.Len = 3 (0, 1 ,2) (3 different variants)
				// NOTE: liquid.Len = 2 (0, 1) (0 == Water | 1 == Lava)
                SoundEngine.LegacySoundPlayer.SoundZombie[0] = mod.Assets.Request<SoundEffect>("Sounds/Custom/npcs/zombie1", AssetRequestMode.ImmediateLoad);
                SoundEngine.LegacySoundPlayer.SoundZombie[1] = mod.Assets.Request<SoundEffect>("Sounds/Custom/npcs/zombie2", AssetRequestMode.ImmediateLoad);
                SoundEngine.LegacySoundPlayer.SoundZombie[2] = mod.Assets.Request<SoundEffect>("Sounds/Custom/npcs/zombie3", AssetRequestMode.ImmediateLoad);

                if (ModContent.GetInstance<AmbientConfigServer>().newSplashSounds)
                {
                    SoundEngine.LegacySoundPlayer.SoundSplash[0] = mod.Assets.Request<SoundEffect>("Sounds/Custom/nothingness", AssetRequestMode.ImmediateLoad);
                }
            }
        }
        public static void Unload()
        {
            SoundEngine.LegacySoundPlayer.SoundZombie = zombieCache;
        }

        public static void Swap<T>(ref Asset<T> swapFrom, ref Asset<T> swapTo) where T : class
        {
            Asset<T> temp;
            temp = swapFrom;
            swapFrom = swapTo;
            swapTo = temp;
        }
        public static void SwapArray<T>(ref Asset<T>[] swapFrom, ref Asset<T>[] swapTo) where T : class
        {
            Asset<T>[] temp;
            temp = swapFrom;
            swapFrom = swapTo;
            swapTo = temp;
        }


    }
}
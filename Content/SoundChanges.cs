using Microsoft.Xna.Framework.Audio;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaAmbience.Content
{
    internal class SoundChanges
    {
        private static Asset<SoundEffect>[] newZombieArr;

        private static Asset<SoundEffect>[] zombieCache;

        public static void Init()
        {
            zombieCache = SoundEngine.LegacySoundPlayer.SoundZombie;
            var mod = ModContent.GetInstance<TerrariaAmbience>();
            newZombieArr = new Asset<SoundEffect>[] {
                mod.Assets.Request<SoundEffect>("Sounds/Custom/npcs/zombie1", AssetRequestMode.ImmediateLoad),
                mod.Assets.Request<SoundEffect>("Sounds/Custom/npcs/zombie2", AssetRequestMode.ImmediateLoad),
                mod.Assets.Request<SoundEffect>("Sounds/Custom/npcs/zombie3", AssetRequestMode.ImmediateLoad)
            };
            // 45 in MusicID is the wind ambience

            // loader.splashCashe = Main.soundSplash;

            // Now change the sound (Main.soundX = y)

            // NOTE: drip.Len = 3 (0, 1 ,2) (3 different variants)
            // NOTE: liquid.Len = 2 (0, 1) (0 == Water | 1 == Lava)
            if (!Main.dedServ)
            {
                SwapArray(ref SoundEngine.LegacySoundPlayer.SoundZombie, ref newZombieArr);
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
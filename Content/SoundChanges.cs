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
        internal SoundEffect[] splashCashe = new SoundEffect[] { };
        private SoundEffect[] zombieCashe = new SoundEffect[] { };

        public static SoundChanges Instance
        {
            get
            {
                return ModContent.GetInstance<SoundChanges>();
            }
        }

        public static void Init()
        {
            // 45 in MusicID is the wind ambience
            /*ContentInstance.Register(new SoundChanges());
            var mod = ModContent.GetInstance<TerrariaAmbience>();
            var lsp = SoundEngine.LegacySoundPlayer;
            var changes = Instance;
            var loader = Ambience.Instance;

            #region SFX Changes
            changes.runSFX_Cashe = lsp.SoundRun.Value;
            changes.runSFX_Cashe = Main.soundRun;
            changes.drip_Cashe = Main.soundDrip;
            changes.liquid_Cashe = Main.soundLiquid;

            changes.splashCashe = Main.soundSplash;
            changes.zombieCashe = Main.soundZombie;


            // loader.splashCashe = Main.soundSplash;

            // Now change the sound (Main.soundX = y)

            // NOTE: drip.Len = 3 (0, 1 ,2) (3 different variants)
            // NOTE: liquid.Len = 2 (0, 1) (0 == Water | 1 == Lava)
            if (!Main.dedServ)
            {
                // SetSoundValue(lsp.SoundRun, mod.GetSound("Sounds/Custom/nothingness").Value);

                /*Main.soundDrip[0] = Ambience.Drip1;
                Main.soundDrip[1] = Ambience.Drip2;
                Main.soundDrip[2] = Ambience.Drip3;

                Main.soundLiquid[0] = Ambience.WaterStream;
                Main.soundLiquid[1] = Ambience.LavaStream;

                Main.soundInstanceDrip[0] = Ambience.Drip1Instance;
                Main.soundInstanceDrip[1] = Ambience.Drip2Instance;
                Main.soundInstanceDrip[2] = Ambience.Drip3Instance;

                Main.soundInstanceLiquid[0] = Ambience.WaterStreamInstance;
                Main.soundInstanceLiquid[1] = Ambience.LavaStreamInstance;

                Main.soundZombie[0] = mod.GetSound($"{Ambience.AmbientPath}/npcs/zombie1");
                Main.soundZombie[1] = mod.GetSound($"{Ambience.AmbientPath}/npcs/zombie2");
                Main.soundZombie[2] = mod.GetSound($"{Ambience.AmbientPath}/npcs/zombie3");
            }*/
        }
        public static void Unload()
        {
            // Main.soundRun = Instance.runSFX_Cashe;
            /*Main.soundDrip = Instance.drip_Cashe;
            Main.soundLiquid = Instance.liquid_Cashe;

            Main.soundZombie = Instance.zombieCashe;

            Main.soundSplash = Instance.splashCashe;*/
        }

        public static void Swap<T>(ref Asset<T> swapFrom, ref Asset<T> swapTo) where T : class
        {
            Asset<T> temp;
            temp = swapFrom;
            swapFrom = swapTo;
            swapTo = temp;
        }
    }
}
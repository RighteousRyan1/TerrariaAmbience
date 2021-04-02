using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TerrariaAmbience.Content;

namespace TerrariaAmbience.Core
{
    /// <summary>
    /// A class that represents an ambient track or loop. You can use ModContent.GetInstance<T>() to access a ModAmbience.
    /// </summary>
    public class ModAmbience : IDisposable
    {
        public void Dispose() { }
        public SoundEffect soundEffect;
        public SoundEffectInstance soundEffectInstance;
        public bool Condition { get; set; }
        public string SFXName { get; private set; }
        public string Path { get; private set; }
        public float Volume { get; private set; }
        private Mod Mod { get; set; }

        internal static int count;

        internal static List<ModAmbience> allAmbiences = new List<ModAmbience> { };

        internal static ModAmbience Instance { get => ModContent.GetInstance<ModAmbience>(); }


        /// <summary>
        /// Wh... why are you abusing reflection to make an instance? You're only going to get bad results :/
        /// </summary>
        internal ModAmbience() { }
        /// <summary>
        /// Creates a completely new ambience sound. This will do all of the work for you.
        /// </summary>
        /// <param name="mod">The mod to default a path from.</param>
        /// <param name="pathForSound">The path to the ambient after your mod's direcotry.</param>
        /// <param name="name">The name for the ambience effect created.</param>
        /// <param name="conditionToPlayUnder">When or when to not play this ambience.</param>
        /// <param name="isLooped">Whether or not to loop the ambient.</param>
        public ModAmbience(Mod mod, string pathForSound, string name, bool conditionToPlayUnder)
        {
            if (allAmbiences.All(x => x.Name != name))
            {
                allAmbiences.Add(this);
            }

            Name = name;

            Path = pathForSound;

            Mod = mod;
            soundEffect = mod.GetSound(pathForSound);
            soundEffectInstance = soundEffect.CreateInstance();

            soundEffect.Name = name;
            Condition = conditionToPlayUnder;

            Volume = soundEffectInstance.Volume;

            soundEffectInstance.Volume = conditionToPlayUnder ? 1f : 0f;
        }
        public override string ToString()
        {
            return "{ path: " + Path + " | name: " + Name + " | condition: " + Condition + " }";
        }
        /// <summary>
        /// Do things while your ambience is playing.
        /// </summary>
        public virtual void UpdateActive(ref SoundEffectInstance sfx)
        {
            sfx = soundEffectInstance;
        }
        /// <summary>
        /// Return a new boolean value to decide when you want this ambience sound to play.
        /// </summary>
        /// <returns></returns>
        public virtual bool WhenToPlay()
        {
            return false;
        }
        public virtual string Name { get; set; }
        public static Type[] AllTypes => Assembly.GetExecutingAssembly().GetTypes();
        public static List<T> AllSubClassesOf<T>() where T : class
        {
            var Types = AllTypes;
            List<T> TypeListBuffer = new List<T>();

            for (int Index = 0; Index < Types.Length; Index++)
            {
                if (Types[Index].IsSubclassOf(typeof(T)))
                {
                    TypeListBuffer.Add(Activator.CreateInstance(Types[Index]) as T);
                }
            }

            return TypeListBuffer;
        }
        public static void Initialize()
        {
            ContentInstance.Register(new ModAmbience());
            foreach (var ambient in AllSubClassesOf<ModAmbience>())
            {
                ContentInstance.Register(ambient);
                ambient?.soundEffectInstance?.Play();
            }
        }
        public static void Unload()
        {
        }

        internal static ModAmbience modAmbiences;
        internal static List<ModAmbience> modAmbienceList;
        internal static int forIterationNum;
        public static void UpdateModAmbience()
        {
            modAmbienceList = AllSubClassesOf<ModAmbience>();
            for (int i = 0; i < allAmbiences.Count; i++)
            {
                forIterationNum = i;
                modAmbiences = allAmbiences[i];
            }
            // if (!Main.gameMenu) Main.NewText(modAmbienceList[forIterationNum].Name);
            /*if (modAmbienceList[forIterationNum] != null)
            {
                if (modAmbienceList[forIterationNum].Volume != 0f)
                {
                    modAmbienceList[forIterationNum].UpdateActive(ref Instance.soundEffectInstance);
                }
            }*/

            /*if (modAmbienceList[forIterationNum].WhenToPlay())
            {
                Instance.soundEffectInstance.Volume += Ambience.decOrIncRate;
            }*/
        }
    }
    public static class ModAmbienceExtensions
    {
        public static void Remove(this ModAmbience modAmbience)
        {
            ModAmbience.allAmbiences.Remove(modAmbience);
            // This is literally the definition of being stupid
            // Mirsario is gonna see this and flame me but it's fine :)
        }
    }
}

using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerrariaAmbience.Content;
using System.IO;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Common.Systems
{
    public class SavingSystem : ModSystem
    {
        public override void Unload()
        {
            if (!Directory.Exists(Path.Combine(ModLoader.ModPath, "TAConfig")))
                Directory.CreateDirectory(Path.Combine(ModLoader.ModPath, "TAConfig"));
            if (Directory.Exists(Path.Combine(ModLoader.ModPath, "TAConfig")))
            {
                string path = Path.Combine(ModLoader.ModPath, "TAConfig", "ta_secretconfig.txt");
                File.WriteAllText(path, $"{Ambience.TAAmbient}\n{Ambience.SFXReplacePath}\n{MethodDetours.soundPack_whoAmI}\n\nPlease do not change these unless you know what you're doing.");
            }
            else
                Mod.Logger.Error("Failed to write save data for TerrariaAmbience config. (Directory not found!)");
        }
        public static void LoadFromConfig()
        {
            string path = Path.Combine(ModLoader.ModPath, "TAConfig", "ta_secretconfig.txt");

            string replacePath = Path.Combine(ModLoader.ModPath, "TASoundReplace");

            if (!Directory.Exists(replacePath))
                Directory.CreateDirectory(replacePath);
            File.WriteAllText(Path.Combine(replacePath, "README.txt"), "If you are replacing sounds, please read the index/key below for the sounds you might want to replace." +
                    "\nThese replacements will ONLY work if you put it in here word-for-word.\n"
                    + "\nforest_morning\nforest_day\nforest_evening\nforest_night\ndesert_crickets\nsnowfall_day\nsnowfall_night\njungle_day\njungle_night\ncorruption_roars"
                    + "\ncrimson_rumbles\nug_drip\nhell_rumble\nbeach_waves\nbreeze\n\nPlease, do note that these files must be in the WAVE format (.wav)!");

            if (File.Exists(path) && File.ReadAllLines(path)[0] != null)
            {
                string[] lines = File.ReadAllLines(path);

                Ambience.TAAmbient = float.Parse(lines[0]);
            }
            else
                Ambience.TAAmbient = 100f;

            if (File.Exists(path) && File.ReadAllLines(path)[1] != null)
            {
                string[] lines = File.ReadAllLines(path);
                Ambience.SFXReplacePath = lines[1];
            }
            else
                Ambience.SFXReplacePath = Ambience.INTERNAL_CombinedPath;

            if (File.Exists(path) && File.ReadAllLines(path)[2] != null && File.ReadAllLines(path)[2] != string.Empty)
            {
                string[] lines = File.ReadAllLines(path);
                MethodDetours.soundPack_whoAmI = int.Parse(lines[2]);
            }
            else
                MethodDetours.soundPack_whoAmI = -1;
        }
    }
}
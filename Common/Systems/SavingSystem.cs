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
                File.WriteAllText(path, $"{Ambience.SFXReplacePath}\n{MethodDetours.soundPack_whoAmI}\n\nPlease do not change these unless you know what you're doing.");
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

            retry:
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);

                if (int.TryParse(lines[0], out var num))
                {
                    if (lines[1] != null)
                        Ambience.SFXReplacePath = lines[1];
                    else
                        Ambience.SFXReplacePath = Ambience.INTERNAL_CombinedPath;

                    if (lines[2] != null && lines[2] != string.Empty)
                        MethodDetours.soundPack_whoAmI = int.Parse(lines[2]);
                    else
                        MethodDetours.soundPack_whoAmI = -1;
                }
                else
                {
                    if (lines[0] != null)
                        Ambience.SFXReplacePath = lines[0];
                    else
                        Ambience.SFXReplacePath = Ambience.INTERNAL_CombinedPath;

                    if (lines[1] != null && lines[1] != string.Empty)
                        MethodDetours.soundPack_whoAmI = int.Parse(lines[1]);
                    else
                        MethodDetours.soundPack_whoAmI = -1;
                }
            }
            else
            {
                File.WriteAllText(path, Ambience.INTERNAL_CombinedPath + "\n0");
                goto retry;
            }
        }
    }
}
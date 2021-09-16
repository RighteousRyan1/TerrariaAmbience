using TerrariaAmbience.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria.ModLoader;
using System.IO;
using TerrariaAmbience.Helpers;
using Terraria;
using TerrariaAmbience.Core;

namespace TerrariaAmbience.Common.Systems
{
    public class AudioLoopsSystem : ModSystem
    {
        public static PositionalAudio2D[] grassCritters = new PositionalAudio2D[10];
        public static PositionalAudio2D[] owls = new PositionalAudio2D[4];

        public override void OnModLoad()
        {
            grassCritters[0] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/crickets1"), (5, 10), 0.25f, (0, 0)).WithName("QuietCricket");
            grassCritters[1] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/crickets2"), (2, 3), 0.25f, (0, 0)).WithName("ClickCricket_0");
            grassCritters[2] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/crickets3"), (3, 5), 0.25f, (0, 0)).WithName("ClickCricket_1");
            grassCritters[3] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/crickets4"), (2, 5), 0.25f, (0, 0)).WithName("ClickCricket_2");
            grassCritters[4] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/crickets5"), (1, 3), 0.25f, (0, 0)).WithName("HummingCricket");
            grassCritters[5] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/crickets6"), (3, 6), 0.25f, (0, 0)).WithName("HummingCricket_DeepPitched");
            grassCritters[6] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/crickets7"), (1, 2), 0.25f, (0, 0)).WithName("HummingCricket_Monotone");
            grassCritters[7] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/crickets8"), (1, 2), 0.25f, (0, 0)).WithName("HummingCricket_Buzz");
            grassCritters[8] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/crickets9"), (1, 1), 0.25f, (0, 0)).WithName("YellingCricket");
            grassCritters[9] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/grasshopper"), (1, 1), 0.25f, (0, 0)).WithName("Grasshopper");
            owls[0] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/owl1"), (1, 2), 0.25f, (30, 50)).WithName("Owl_0");
            owls[1] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/owl2"), (1, 2), 0.25f, (35, 55)).WithName("Owl_1");
            owls[2] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/owl3"), (1, 2), 0.25f, (40, 60)).WithName("Owl_2");
            owls[3] = new PositionalAudio2D(GeneralHelpers.GetAssetValue<SoundEffect>(Mod, "Sounds/Custom/variousanimals/multipleowls"), (1, 1), 1f).WithName("CooCooOwl");
            owls[3].MaxVolume = 0.25f;
        }
        private static int _attempts; // make private lol
        private static string _reason;
        private static Vector2 RandomPosNearPlayer
        {
            get
            {

                var player = Main.LocalPlayer;
                Retry:
                _attempts++;
                var pos = player.Center + new Vector2(Main.rand.Next(-2000, 2000), Main.rand.Next(-500, 500));
                if (_attempts > 30)
                {
                    _attempts = 0;
                    return Vector2.Zero;
                }
                if (Collision.SolidCollision(pos, 1, 1))
                {
                    _reason = "TileSolid";
                    goto Retry;
                }

                if (!Collision.SolidCollision(pos + new Vector2(0, 16), 1, 1))
                {
                    _reason = "TileUnderNotSolid";
                    goto Retry;
                }

                var landingTile = Main.tile[(int)pos.X / 16, (int)pos.Y / 16 + 1];
                if (!TileDetection.GrassBlocks.Contains(landingTile.type) && !TileDetection.DirtBlocks.Contains(landingTile.type))
                {
                    _reason = "IsNotGrassOrDirt";
                    goto Retry;
                }
                _attempts = 0;
                return pos;
                // hopefully this is a fix, removing the wall check
            }
        }
        public override void PostUpdateEverything()
        {
            //Main.NewText("Tile: " + GeneralHelpers.GetTileNameAt(Main.MouseWorld / 16).ToLower());
            //Main.NewText("Wall: " + GeneralHelpers.GetWallNameAt(Main.MouseWorld / 16).ToLower());
            if (ModContent.GetInstance<AudioAdditionsConfig>().dynamicAnimalSounds)
            {
                if (Main.LocalPlayer.ZoneOverworldHeight)
                {
                    if (RandomPosNearPlayer != Vector2.Zero)
                    {
                        int chance = Main.rand.Next(0, 680);
                        int rand = Main.rand.Next(grassCritters.Length);
                        if (chance == 0)
                            grassCritters[rand]?.Play(RandomPosNearPlayer, 0.50f);
                        if (!Main.dayTime)
                        {
                            int chance2 = Main.rand.Next(0, 750);
                            int rand2 = Main.rand.Next(owls.Length);
                            if (chance2 == 0)
                                owls[rand2]?.Play(RandomPosNearPlayer, 0.75f);
                        }
                    }
                }
            }
        }
    }
}
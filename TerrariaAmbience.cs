using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace TerrariaAmbience
{
	public class TerrariaAmbience : Mod
	{
        public override void Load()
        {
            Ambience.Initialize();
            On.Terraria.Main.DoUpdate += Main_DoUpdate;
        }

        public override void Unload()
        {
            Ambience.Unload();
        }

        public static float decOrIncRate = 0.01f;
        private void Main_DoUpdate(On.Terraria.Main.orig_DoUpdate orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);
            decOrIncRate = 0.01f;
            var aLoader = Ambience.Instance;

            Ambience.DoUpdate_Ambience();

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

            if (Main.hasFocus)
            {
                if ((player.ZoneForest() || player.ZoneHoly) && (Main.dayTime && Main.time > 14400) && (Main.dayTime && Main.time < 46800))
                {
                    aLoader.dayCricketsVolume += decOrIncRate;
                }
                else
                {
                    aLoader.dayCricketsVolume -= decOrIncRate;
                }

                if ((player.ZoneForest() || player.ZoneHoly) && aLoader.DayCricketsInstance.Volume == 0 && Main.dayTime)
                {
                    aLoader.eveningCricketsVolume += decOrIncRate;
                }
                else
                {

                    aLoader.eveningCricketsVolume -= decOrIncRate;
                }

                if ((player.ZoneForest() || player.ZoneHoly) && !Main.dayTime)
                {
                    aLoader.nightCricketsVolume += decOrIncRate;
                }
                else
                {
                    aLoader.nightCricketsVolume -= decOrIncRate;
                }

                if (player.ZoneUnderground())
                {
                    aLoader.ugAmbienceVolume += decOrIncRate;
                }
                else
                {
                    aLoader.ugAmbienceVolume -= decOrIncRate;
                }

                if (player.ZoneDesert)
                {
                    aLoader.desertCricketsVolume += decOrIncRate;
                }
                else
                {
                    aLoader.desertCricketsVolume -= decOrIncRate;
                }

                if (player.ZoneDesert && Main.dayTime)
                {
                    aLoader.DesertAmbienceInstance.Pitch = -0.075f;
                }
                else
                {
                    aLoader.DesertAmbienceInstance.Pitch = 0f;
                }

                if (player.ZoneSnow && Main.dayTime && !player.ZoneUnderworldHeight)
                {
                    aLoader.snowDayVolume += decOrIncRate;
                }
                else
                {
                    aLoader.snowDayVolume -= decOrIncRate;
                }

                if (player.ZoneSnow && !Main.dayTime && !player.ZoneUnderworldHeight)
                {
                    aLoader.snowNightVolume += decOrIncRate;
                }
                else
                {
                    aLoader.snowNightVolume -= decOrIncRate;
                }

                if (player.ZoneCrimson)
                {
                    aLoader.crimsonRumblesVolume += decOrIncRate;
                }
                else
                {
                    aLoader.crimsonRumblesVolume -= decOrIncRate;
                }
                if (player.ZoneCorrupt)
                {
                    aLoader.corruptionRoarsVolume += decOrIncRate;
                }
                else
                {
                    aLoader.corruptionRoarsVolume -= decOrIncRate;
                }

                if (player.ZoneJungle && !player.ZoneRockLayerHeight && !player.ZoneUnderworldHeight && Main.dayTime)
                {
                    aLoader.dayJungleVolume += decOrIncRate;
                }
                else if(!player.ZoneRockLayerHeight)
                {
                    aLoader.dayJungleVolume -= decOrIncRate;
                }
                else if (player.ZoneRockLayerHeight && player.ZoneJungle)
                {
                    aLoader.dayJungleVolume = 0.05f;
                }

                if (player.ZoneJungle && !player.ZoneRockLayerHeight && !player.ZoneUnderworldHeight && !Main.dayTime)
                {
                    aLoader.nightJungleVolume += decOrIncRate;
                }
                else if (!player.ZoneRockLayerHeight)
                {
                    aLoader.nightJungleVolume -= decOrIncRate;
                }
                else if (player.ZoneRockLayerHeight && player.ZoneJungle)
                {
                    aLoader.nightJungleVolume = 0.05f;
                }

                if (player.ZoneBeach && !player.ZoneRockLayerHeight)
                {
                    aLoader.beachWavesVolume += decOrIncRate;
                }
                else
                {
                    aLoader.beachWavesVolume -= decOrIncRate;
                }

                if (player.ZoneUnderworldHeight)
                {
                    aLoader.hellRumbleVolume += decOrIncRate;
                }
                else
                {
                    aLoader.hellRumbleVolume -= decOrIncRate;
                }

                if (Main.raining && (player.ZoneOverworldHeight || player.ZoneSkyHeight))
                {
                    aLoader.rainVolume += decOrIncRate;
                }
                else
                {
                    aLoader.rainVolume -= decOrIncRate;
                }
            }
            else
            {
                aLoader.dayCricketsVolume -= decOrIncRate;
                aLoader.eveningCricketsVolume -= decOrIncRate;
                aLoader.nightCricketsVolume -= decOrIncRate;
                aLoader.crackleVolume -= decOrIncRate;
                aLoader.desertCricketsVolume -= decOrIncRate;
                aLoader.ugAmbienceVolume -= decOrIncRate;
                aLoader.snowDayVolume -= decOrIncRate;
                aLoader.snowNightVolume -= decOrIncRate;
                aLoader.crimsonRumblesVolume -= decOrIncRate;
                aLoader.corruptionRoarsVolume -= decOrIncRate;
                aLoader.dayJungleVolume -= decOrIncRate;
                aLoader.nightJungleVolume -= decOrIncRate;
                aLoader.beachWavesVolume -= decOrIncRate;
                aLoader.hellRumbleVolume -= decOrIncRate;
                aLoader.rainVolume -= decOrIncRate;
            }
            Ambience.ClampAll();
        }
    }
    public class AmbiencePlayer : ModPlayer
    {
        public bool isOnWoodTile;
        public bool isOnStoneTile;
        public bool isOnGrassyTile;
        public bool isOnSandyTile;
        public bool isOnSnowyTile;
        public override void OnEnterWorld(Player player)
        {
            Ambience.PlayAllAmbience();
        }

        public bool isNearCampfire;

        public int stepTimerToPlaySoundOnce;

        public override void PreUpdate()
        {
            int randWood = Main.rand.Next(1, 8);
            int randSand = Main.rand.Next(1, 4);
            int randStone = Main.rand.Next(1, 9);
            int randGrass = Main.rand.Next(1, 9);
            int randSnow = Main.rand.Next(1, 12);

            string pathWood = $"Sounds/Custom/steps/wood/step{randWood}";
            string pathSand = $"Sounds/Custom/steps/sand/step{randSand}";
            string pathStone = $"Sounds/Custom/steps/stone/step{randStone}";
            string pathGrass = $"Sounds/Custom/steps/grass/step{randGrass}";
            string pathSnow = $"Sounds/Custom/steps/snow/step{randSnow}";

            if (ModContent.GetInstance<AmbientConfig>().footsteps)
            {
                if (!player.wet)
                {
                    if (Main.soundVolume != 0f)
                    {
                        if ((player.legFrame.Y == player.legFrame.Height * 9 || player.legFrame.Y == player.legFrame.Height * 16) && player.GetModPlayer<AmbiencePlayer>().isOnSandyTile)
                        {
                            Main.soundInstanceRun.Volume = Main.soundVolume * 0.75f;
                            Main.soundRun = ModContent.GetInstance<TerrariaAmbience>().GetSound($"Sounds/Custom/steps/sand/step{randSand}");
                            if (stepTimerToPlaySoundOnce == 1)
                            {
                                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathSand), player.Bottom).Volume = Main.soundVolume * 0.75f;
                            }
                        }
                        if ((player.legFrame.Y == player.legFrame.Height * 9 || player.legFrame.Y == player.legFrame.Height * 16) && player.GetModPlayer<AmbiencePlayer>().isOnGrassyTile)
                        {
                            Main.soundInstanceRun.Volume = Main.soundVolume / 2;
                            Main.soundRun = ModContent.GetInstance<TerrariaAmbience>().GetSound($"Sounds/Custom/steps/grass/step{randGrass}");
                            if (stepTimerToPlaySoundOnce == 1)
                            {
                                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathGrass), player.Bottom).Volume = Main.soundVolume / 2;
                            }
                        }
                        if ((player.legFrame.Y == player.legFrame.Height * 9 || player.legFrame.Y == player.legFrame.Height * 16) && player.GetModPlayer<AmbiencePlayer>().isOnStoneTile)
                        {
                            Main.soundInstanceRun.Volume = Main.soundVolume / 8;
                            Main.soundRun = ModContent.GetInstance<TerrariaAmbience>().GetSound($"Sounds/Custom/steps/stone/step{randStone}");
                            if (stepTimerToPlaySoundOnce == 1)
                            {
                                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathStone), player.Bottom).Volume = Main.soundVolume / 8;
                            }
                        }
                        if ((player.legFrame.Y == player.legFrame.Height * 9 || player.legFrame.Y == player.legFrame.Height * 16) && player.GetModPlayer<AmbiencePlayer>().isOnWoodTile)
                        {
                            Main.soundInstanceRun.Volume = Main.soundVolume / 8;
                            Main.soundRun = ModContent.GetInstance<TerrariaAmbience>().GetSound($"Sounds/Custom/steps/wood/step{randWood}");
                            if (stepTimerToPlaySoundOnce == 1)
                            {
                                Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathWood), player.Bottom).Volume = Main.soundVolume / 8;
                            }
                        }
                        if ((player.legFrame.Y == player.legFrame.Height * 9 || player.legFrame.Y == player.legFrame.Height * 16) && player.GetModPlayer<AmbiencePlayer>().isOnSnowyTile)
                        {
                            Main.soundInstanceRun.Volume = Main.soundVolume / 4;
                            Main.soundRun = ModContent.GetInstance<TerrariaAmbience>().GetSound($"Sounds/Custom/steps/snow/step{randSnow}");
                            if (stepTimerToPlaySoundOnce == 1)
                            {
                                var x = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathSnow), player.Bottom);
                                x.Volume = Main.soundVolume / 4;
                                x.Pitch = 0.3f;
                            }
                        }
                        if (player.legFrame.Y == player.legFrame.Height * 9 || player.legFrame.Y == player.legFrame.Height * 16)
                        {
                            stepTimerToPlaySoundOnce++;
                        }
                        else
                        {
                            stepTimerToPlaySoundOnce = 0;
                        }
                    }
                }
            }
        }
        public override void PostUpdate()
        {
            // chatter.Volume = 0 + player.townNPCs / 10
            if (!Main.dayTime)
            {
                int rand = Main.rand.Next(1, 3);
                string pathToHoot = $"Sounds/Custom/ambient/animals/hoot{rand}";
                string pathToHowl = $"Sounds/Custom/ambient/animals/howl";

                int randX = Main.rand.Next(0, 1750);
                int randY = Main.rand.Next(0, 1750);
                int chance1 = Main.rand.Next(750);
                int chance2 = Main.rand.Next(1500);

                // Main.NewText(chance);
                if (Main.soundVolume > 0f)
                {
                    if (chance1 == 0)
                    {
                        if (player.ZoneForest())
                        {
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathToHoot), player.Center + new Vector2(randX, randY)).Volume = Main.ambientVolume * 0.9f;
                        }
                    }
                    if (player.ZoneSnow && !player.ZoneUnderworldHeight && !player.ZoneRockLayerHeight)
                    {
                        if (chance2 == 1)
                        {
                            var SFE = Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, pathToHowl), player.Center + new Vector2(randX, randY));
                            SFE.Volume = Main.ambientVolume * 0.1f;
                            SFE.Pitch = Main.rand.NextFloat(-0.4f, -0.1f);
                        }
                    }
                }
            }
            if (player.ZoneDungeon)
            {
                int possibleChance = Main.rand.Next(1000);
                int randX = Main.rand.Next(0, 2250);
                int randY = Main.rand.Next(0, 2250);

                if (possibleChance == 0)
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/ambient/animals/rattling_bones"), player.Center + new Vector2(randX, randY)).Volume = Main.ambientVolume * 0.05f;
                }
            }
            // Main.NewText(Main.tile[(int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16]);
            var aLoader = Ambience.Instance;
            if (aLoader.crackleVolume > 1)
            {
                aLoader.crackleVolume = 1;
            }
            if (aLoader.crackleVolume < 0)
            {
                aLoader.crackleVolume = 0;
            }
            if (isNearCampfire && ModContent.GetInstance<AmbientConfig>().campfireSounds)
            {
                Ambience.Instance.crackleVolume = 1f - ModContent.GetInstance<CampfireDetection>().distanceOf / 780;
            }
            else
            {
                Ambience.Instance.crackleVolume = 0f;
            }
        }
    }
    public static class ExtensionMethods
    {
        public static bool ZoneForest(this Player player)
        {
            return !player.ZoneJungle
                   && !player.ZoneDungeon
                   && !player.ZoneCorrupt
                   && !player.ZoneCrimson
                   && !player.ZoneHoly
                   && !player.ZoneSnow
                   && !player.ZoneUndergroundDesert
                   && !player.ZoneGlowshroom
                   && !player.ZoneMeteor
                   && !player.ZoneBeach
                   && !player.ZoneDesert
                   && player.ZoneOverworldHeight;
        }
        public static bool ZoneUnderground(this Player player)
        {
            return player.Center.Y >= Main.rockLayer * 16 && !player.ZoneUnderworldHeight;
        }
    }
    public class CampfireDetection : GlobalTile
    {
        public Vector2 originOfCampfire;

        public float distanceOf;

        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            originOfCampfire.X = i * 16;
            originOfCampfire.Y = j * 16;
            Player player = Main.player[Main.myPlayer].GetModPlayer<AmbiencePlayer>().player;

            if (type == TileID.Campfire && closer && player.HasBuff(BuffID.Campfire))
            {
                distanceOf = Vector2.Distance(originOfCampfire, player.Center);
                player.GetModPlayer<AmbiencePlayer>().isNearCampfire = true;
            }
            if ((type == TileID.Campfire && !closer) || !player.HasBuff(BuffID.Campfire))
            {
                Ambience.Instance.CampfireCrackleInstance.Volume = 0f;
                player.GetModPlayer<AmbiencePlayer>().isNearCampfire = false;
            }
        }
        public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
        {
        }
    }
}
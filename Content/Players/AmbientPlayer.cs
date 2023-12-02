using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;
using Terraria.Audio;
using TerrariaAmbience.Sounds;
using TerrariaAmbience.Common.Enums;
using TerrariaAmbience.Common.Systems;
using System.Reflection;
using System.Linq;

namespace TerrariaAmbience.Content.Players
{
    public class AmbientPlayer : ModPlayer
    {
        // This class serves the purpose of playing sounds and/or playing footsteps.

        public int WallsAround => ReverbAudioSystem.WallsAround(Player.Center, new(4, 4), out var coords);

        private float _multInternal;
        public float BehindWallMultiplier => MathHelper.Clamp(_multInternal, 0.5f, 1f);

        public FieldInfo _stormShaderObstruction = typeof(Player).GetField("_stormShaderObstruction", BindingFlags.Instance | BindingFlags.NonPublic);
        public float BlizzardVisualIntensity => (float)_stormShaderObstruction.GetValue(Player);

        public bool BehindBackWall_MyStyle;

        public bool IsNearCampfire;

        public SoundEffectInstance thunderInstance;
        public SoundEffectInstance hootInstance;
        public SoundEffectInstance howlInstance;

        internal int timerUntilValidChestStateChange;
        public override void OnEnterWorld() {
            TerrariaAmbience.DefaultAmbientHandler.HandleEnterWorld();
        }

        private int chestStateNew;
        private int chestStateOld; // Same as above.

        /// <summary>
        /// For the use of rain. Not modifyable publically due to potential anomalies happening otherwise.
        /// <para></para>
        /// Checks above the player for about 8 tiles for a ceiling. Used for wet-sounding footsteps.
        /// </summary>
        public bool HasTilesAbove;

        private bool _wet; // old state of player wet

        private bool _showReverbTiles;

        private List<string> _ignoredWalls = new() {
            "fence",
            "grass",
            "flower",
            "leaf"
        };
        public override void PreUpdate() {
            var cfg = ModContent.GetInstance<GeneralConfig>();
            var cfg2 = ModContent.GetInstance<AudioAdditionsConfig>();
            var cfg3 = ModContent.GetInstance<AmbientConfigServer>();

            if (cfg3.newSplashSounds)
                ManagePlayerSplashes();

            #region ShowReverbTiles

            if (GeneralHelpers.KeyPress(Microsoft.Xna.Framework.Input.Keys.RightAlt))
                _showReverbTiles = !_showReverbTiles;

            if (_showReverbTiles) {
                if (cfg.debugInterface && cfg2.ugReverbCalculation && cfg2.advancedReverbCalculation && cfg2.isReverbEnabled) {
                    int wallCount = 0;
                    int tileCount = 0;
                    if (Main.GameUpdateCount % 10 == 0) {
                        if (Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneUnderworldHeight || Main.LocalPlayer.ZoneDirtLayerHeight) {
                            int wallsNear = ReverbAudioSystem.WallsAround(Player.Center, new Point(15, 15), out List<Point> wallPoints);
                            int tilesNear = ReverbAudioSystem.TilesAround(Player.Center, new Point(15, 15), out List<Point> tilePoints);
                            foreach (var tilePos in tilePoints) {
                                var worldCoords = tilePos.ToVector2() * 16;
                                var left = new Point(tilePos.X - 1, tilePos.Y);
                                var right = new Point(tilePos.X + 1, tilePos.Y);
                                var up = new Point(tilePos.X, tilePos.Y - 1);
                                var down = new Point(tilePos.X, tilePos.Y + 1);
                                if (ReverbAudioSystem.CanRaycastTo(Player.Center, left.ToVector2() * 16) || ReverbAudioSystem.CanRaycastTo(Player.Center, right.ToVector2() * 16)
                                    || ReverbAudioSystem.CanRaycastTo(Player.Center, up.ToVector2() * 16) || ReverbAudioSystem.CanRaycastTo(Player.Center, down.ToVector2() * 16)) {
                                    tileCount++;
                                    if (Main.GameUpdateCount % 10 == 0) {
                                        Dust.QuickBox(worldCoords, worldCoords + new Vector2(16), 0, Color.Green, null);
                                    }
                                }
                            }
                            foreach (var wallPos in wallPoints) {
                                if (ReverbAudioSystem.CanRaycastTo(Player.Center, wallPos.ToVector2() * 16)) {
                                    wallCount++;
                                    if (Main.GameUpdateCount % 10 == 0) {
                                        Dust.QuickBox(wallPos.ToVector2() * 16, wallPos.ToVector2() * 16 + new Vector2(16), 0, Color.Red, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            #region Sound Paths / StepRands / Whatever
            timerUntilValidChestStateChange++;
            // This is a pretty niche finding for tiles above said player
            if (cfg.wetStepsEnabled) {
                if (!Player.wet) {
                    Tile playerTile = Main.tile[(int)Player.Center.X / 16, (int)Player.Center.Y / 16];
                    for (int i = (int)Player.Top.X - 1; i < Player.Top.X + 1; i++) {
                        for (int j = (int)Player.Top.Y - 1; playerTile.WallType <= 0 ? j > Player.Top.Y - 350 : j > Player.Top.Y - 600; j--) {
                            if (WorldGen.InWorld(i / 16, j / 16)) {
                                Tile tile = Main.tile[i / 16, j / 16];
                                if (tile.HasTile && tile.CollisionType() == 1) {
                                    HasTilesAbove = true;
                                    break;
                                }
                                else {
                                    HasTilesAbove = false;
                                }
                            }
                        }
                    }
                }
            }
            else {
                // this essentially just works as a way to not have wet sounds play. no need to do extra magic.
                HasTilesAbove = true;
            }
            if (Main.GameUpdateCount % 8 == 0) {
                if (ModContent.GetInstance<AudioAdditionsConfig>().isReverbEnabled) {
                    ReverbAudioSystem.CreateAudioFX(Player.Center, out var reverb, out float occ, out float dampening, out bool sOcclude);
                    Player.GetModPlayer<ReverbPlayer>().ReverbFactor = reverb / 2;
                }
            }

            // this is left in because my reverb system thinks that the sound originates from within a tile.
            #endregion
            // HandleContainerOpenings(ContainerContext.Chest, out var opened);
            chestStateNew = Player.chest;
            if (Main.soundVolume > 0f) {
                if (ModContent.GetInstance<AmbientConfigServer>().chestSounds) {
                    if (timerUntilValidChestStateChange > 10) {
                        if (chestStateNew >= -1) {
                            if ((chestStateNew != chestStateOld) && chestStateNew > -1) {
                                GeneralHelpers.PlaySound(new SoundStyle("TerrariaAmbience/Sounds/Custom/ambient/player/chest_open") { Volume = 0.25f }, Player.Center);
                            }
                            if ((chestStateNew != chestStateOld) && chestStateNew == -1 && chestStateOld > -1) {
                                GeneralHelpers.PlaySound(new SoundStyle("TerrariaAmbience/Sounds/Custom/ambient/player/chest_close") { Volume = 0.55f }, Player.Center);
                            }
                            else if ((chestStateNew != chestStateOld) && chestStateOld > -1) {
                                GeneralHelpers.PlaySound(new SoundStyle("TerrariaAmbience/Sounds/Custom/ambient/player/chest_close") { Volume = 0.55f }, Player.Center);
                            }
                        }
                    }
                }
            }
            chestStateOld = chestStateNew;
            Player.runSoundDelay = 100;
        }
        public override void PostUpdate() {
            if (Player.whoAmI != Main.myPlayer)
                return;
            var tile = Framing.GetTileSafely(Player.Center);
            var modTile = ModContent.GetModTile(tile.TileType);

            // TODO: this isn't exactly working as planned but that can be fixed.
            if (modTile is null)
                BehindBackWall_MyStyle = tile.WallType > 0 && !_ignoredWalls.Contains(TileID.Search.GetName(tile.TileType).ToLower());
            else // let's hope each modtile's internal name for fences includes "fence"
                BehindBackWall_MyStyle = tile.WallType > 0 && !_ignoredWalls.Contains(modTile.Name.ToLower());

            if (BehindBackWall_MyStyle) {
                _multInternal -= 0.0075f;
            }
            else {
                _multInternal += 0.0075f;
            }
            _multInternal = MathHelper.Clamp(_multInternal, 0, 1);
            HandleIceScraping();
            // ^ method above will work in multiplayer eventually... when i care enough.
            int randX = Main.rand.Next(-1750, 1750);
            int randY = Main.rand.Next(-1750, 1750);
            int randC = Main.rand.Next(1, 6);
            int randD = Main.rand.Next(1, 7);
            int randF = Main.rand.Next(1, 3);
            int randM = Main.rand.Next(1, 10);

            string pick = GeneralHelpers.Pick($"close/{randC}", $"distant/{randD}", $"far/{randF}", $"medial/{randM}");
            string pathToThunder = $"TerrariaAmbience/Sounds/Custom/ambient/rain/thunder/{pick}";

            float mafs = 2000 / (Main.maxRaining * 2);
            float chance3 = Main.rand.NextFloat(mafs);

            if (Main.raining && !Player.ZoneSnow && Player.ZoneOverworldHeight) {
                if (chance3 < 2) {
                    var id = SoundEngine.PlaySound(new SoundStyle(pathToThunder, 0, SoundType.Ambient), Player.Center + new Vector2(randX, randY));
                    SoundEngine.TryGetActiveSound(id, out var snd);
                    thunderInstance = snd.Sound;
                    thunderInstance.Volume = Main.ambientVolume * (Player.ZoneDirtLayerHeight ? 0.35f : 0.9f);
                    thunderInstance.Pitch = Main.rand.NextFloat(-0.2f, -0.1f);
                }
            }
            if (!Main.dayTime) {
                string pathToHowl = $"TerrariaAmbience/Sounds/Custom/ambient/animals/howl";

                int chance = Main.rand.Next(1500);
                if (Player.ZoneSnow && !Player.ZoneUnderworldHeight && !Player.ZoneRockLayerHeight) {
                    if (chance == 1) {
                        var id = SoundEngine.PlaySound(new SoundStyle(pathToHowl), Player.Center + new Vector2(randX, randY));
                        SoundEngine.TryGetActiveSound(id, out var snd);
                        howlInstance = snd.Sound;
                        howlInstance.Volume = Main.ambientVolume * 0.1f;
                        howlInstance.Pitch = Main.rand.NextFloat(-0.4f, -0.1f);
                    }
                }
            }
            if (Player.ZoneDungeon) {
                int possibleChance = Main.rand.Next(1000);
                int randX1 = Main.rand.Next(-2250, 2250);
                int randY1 = Main.rand.Next(-2250, 2250);
                if (possibleChance == 0)
                    GeneralHelpers.PlaySound(new SoundStyle("TerrariaAmbience/Sounds/Custom/ambient/animals/rattling_bones"), Player.Center + new Vector2(randX1, randY1)).Volume = Main.ambientVolume * 0.05f; ;
            }

            if (!TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance.IsPlaying())
                TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance.Play();
            float maxDist = 780f;
            float campfireVolumeScalar = 0.75f;
            if (IsNearCampfire && ModContent.GetInstance<GeneralConfig>().campfireSounds) {
                cracklePan = (ModContent.GetInstance<CampfireDetection>().distanceToCampfire / maxDist) * (ModContent.GetInstance<CampfireDetection>().isOnRight ? -1 : 1) / 2;
                crackleVolume = 1f - ModContent.GetInstance<CampfireDetection>().distanceToCampfire / maxDist * campfireVolumeScalar;
            }
            else
                crackleVolume = 0f;

            crackleVolume = MathHelper.Clamp(crackleVolume, 0f, 1f);
            cracklePan = MathHelper.Clamp(cracklePan, -1f, 1f);
            TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance.Volume = crackleVolume;
            TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance.Pan = cracklePan;
        }
        public static float crackleVolume;
        public static float cracklePan;
        /// <summary>
        /// Gets distance from surface of water to the player.
        /// </summary>
        /// <param name="maxTileCheck">The maximum tiles to check above the player.</param>
        /// <param name="defaultCheckPos">The position to check water pressure for. By default is the player's position.</param>
        /// <returns>The subpixel Y-component distance from the player's head to the surface.</returns>
        public float GetWaterPressureFloat(int heightCheck, Vector2 checkPos) {
            return TileUtils.GetWaterPressureFloat(heightCheck, checkPos);
        }
        /// <summary>
        /// Used for when the "splashes" config is enabled.
        /// </summary>
        public void ManagePlayerSplashes() {
            bool justWet = Player.wet && !_wet;
            bool justUnwet = !Player.wet && _wet;

            if (justWet || justUnwet) {
                var vel = Math.Abs(Player.velocity.Y);

                const float loud_thresh = 10f;

                var soundSplash = new SoundStyle($"TerrariaAmbience/Sounds/Custom/ambient/environment/liquid/entity_splash_{(vel >= loud_thresh ? "heavy" : "light")}");

                if (vel < 10f)
                    soundSplash.Volume = vel / loud_thresh / 4;
                if (vel == 0)
                    soundSplash.Volume = 0.1f;


                GeneralHelpers.PlaySound(soundSplash, Player.position);
            }

            // for some reason heavy sounds arent playing

            _wet = Player.wet;
        }

        public float GetArmorStepVolume() {
            bool hasHeadArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.HeadSlot, out var head);
            bool hasChestArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.ChestSlot, out var chest);
            bool hasLegArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.LegSlot, out var legs);

            float determinedValue = 0f;

            // 10 <= x <= 12 == vanity armor
            bool isWood(Item item) {
                if (ItemID.Search.TryGetName(item.type, out var name)) {
                    return name.ToLower().Contains("wood");
                }
                return false;
            }
            bool hasVanityCovering(int context) {
                return !Player.armor[context + 10].IsAir;
            }

            if (hasHeadArmor) {
                if (!head.vanity) {
                    if (!isWood(head)) {
                        if (!hasVanityCovering(GeneralHelpers.IDs.ArmorSlotID.HeadSlot)) {
                            determinedValue *= 1.25f + 0.05f;
                        }
                    }
                }
            }
            if (hasChestArmor) {
                if (!chest.vanity) {
                    if (!isWood(chest)) {
                        if (!hasVanityCovering(GeneralHelpers.IDs.ArmorSlotID.ChestSlot)) {
                            determinedValue += 0.05f;
                        }
                    }
                }
            }
            if (hasLegArmor) {
                if (!legs.vanity) {
                    if (!isWood(legs)) {
                        if (!hasVanityCovering(GeneralHelpers.IDs.ArmorSlotID.LegSlot)) {
                            determinedValue += 0.01f;
                        }
                    }
                }
            }

            return determinedValue;
        }

        public float GetVanityStepVolume() {
            bool hasHeadArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.VanityHeadSlot, out var head);
            bool hasChestArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.VanityChestSlot, out var chest);
            bool hasLegArmor = GeneralHelpers.CheckPlayerArmorSlot(Player, GeneralHelpers.IDs.ArmorSlotID.VanityLegSlot, out var legs);

            float determinedValue = 0f;

            // 10 <= x <= 12 == vanity armor
            if (hasHeadArmor) {
                if (head.vanity) {
                    determinedValue *= 1.25f + 0.1f;
                }
            }
            if (hasChestArmor) {
                if (chest.vanity) {
                    determinedValue += 0.14f;
                }
            }
            if (hasLegArmor) {
                if (legs.vanity) {
                    determinedValue += 0.1f;
                }
            }

            return determinedValue;
        }

        public void HandleContainerOpenings(ContainerContext context, out bool opened) {
            opened = false;
            chestStateNew = Player.chest;
            if (timerUntilValidChestStateChange > 10) {
                // Main.NewText($"{chestStateNew} : {chestStateOld}");
                switch (context) {
                    case ContainerContext.Chest:
                        // > -1
                        if (chestStateNew >= -1) {
                            if ((chestStateNew != chestStateOld) && chestStateNew > -1)
                                opened = true;
                            // orignal closed, none other opened
                            if ((chestStateNew != chestStateOld) && chestStateNew == -1 && chestStateOld > -1)
                                opened = false;
                            // original closed, new opened
                            else if ((chestStateNew != chestStateOld) && chestStateOld > -1)
                                opened = false;
                        }
                        break;
                    case ContainerContext.PiggyBank:
                        // -2
                        break;
                    case ContainerContext.Safe:
                        // -3
                        break;
                }
            }
            chestStateOld = chestStateNew;
        }

        public SoundEffect soundSlippyRough;
        public SoundEffectInstance soundSlippyRoughInst;

        public SoundEffect soundSlippySmooth;
        public SoundEffectInstance soundSlippySmoothInst;

        private bool _areSoundsInitialized;
        public void HandleIceScraping() {
            if (!Main.dedServ) {
                if (!_areSoundsInitialized) {
                    soundSlippyRough = Mod.Assets.Request<SoundEffect>("Sounds/Custom/ambient/player/ice_slide_rough", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    soundSlippyRoughInst = soundSlippyRough.CreateInstance();
                    soundSlippyRoughInst.IsLooped = true;
                    soundSlippyRoughInst?.Play();
                    soundSlippyRoughInst.Volume = 0f;
                    soundSlippySmooth = Mod.Assets.Request<SoundEffect>("Sounds/Custom/ambient/player/ice_slide_smooth", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    soundSlippySmoothInst = soundSlippySmooth.CreateInstance();
                    soundSlippySmoothInst.IsLooped = true;
                    soundSlippySmoothInst?.Play();
                    soundSlippySmoothInst.Volume = 0f;
                    _areSoundsInitialized = true;
                }
                bool slippyRough = Player.slippy;
                bool slippySmooth = Player.slippy2;

                if (slippyRough)
                    soundSlippyRoughInst.Volume = Math.Abs(Player.velocity.X / 50f);
                else
                    soundSlippyRoughInst.Volume -= 0.01f;
                if (slippySmooth)
                    soundSlippySmoothInst.Volume = Math.Abs(Player.velocity.X / 50f);
                else
                    soundSlippySmoothInst.Volume -= 0.01f;
                soundSlippyRoughInst.Volume = MathHelper.Clamp(soundSlippyRoughInst.Volume, 0f, 0.5f);
                soundSlippySmoothInst.Volume = MathHelper.Clamp(soundSlippySmoothInst.Volume, 0f, 0.5f);
            }
        }
    }
    public class CampfireDetection : GlobalTile
    {
        public Vector2 originOfCampfire;

        public float distanceToCampfire;

        public bool isOnRight;
        public override void NearbyEffects(int i, int j, int type, bool closer)
        {
            if (!Main.dedServ)
            {
                originOfCampfire.X = i * 16;
                originOfCampfire.Y = j * 16;
                Player player = Main.player[Main.myPlayer].GetModPlayer<AmbientPlayer>().Player;

                if (ModContent.GetInstance<GeneralConfig>().campfireSounds)
                {
                    if (type == TileID.Campfire && closer && player.HasBuff(BuffID.Campfire))
                    {
                        /*var t = Main.tile[i, j];
                        var orig = new Vector2(i * 16, j * 16);
                        Main.NewText(t.frameY);*/
                        //if (t.frameY <= 18 && t.frameY >= 0)
                        {
                            distanceToCampfire = Vector2.Distance(originOfCampfire, player.Center);
                            isOnRight = originOfCampfire.X < player.Center.X;
                            player.GetModPlayer<AmbientPlayer>().IsNearCampfire = true;
                        }
                    }
                    if ((type == TileID.Campfire && !closer) || !player.HasBuff(BuffID.Campfire))
                    {
                        /*var t = Main.tile[i, j];
                        var orig = new Vector2(i * 16, j * 16);
                        Main.NewText(t.frameY);*/
                        //if (t.frameY <= 54 && t.frameY >= 36)
                        {
                            //if (TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance is not null)
                                //TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance.Volume = 0f;
                            player.GetModPlayer<AmbientPlayer>().IsNearCampfire = false;
                        }
                    }
                }
            }
        }
    }
}

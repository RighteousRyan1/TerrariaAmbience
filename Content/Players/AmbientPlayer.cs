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
using TerrariaAmbience.Common.Enums;
using System.Linq;
using TerrariaAmbience.Sounds.SFXEffects;
using TerrariaAmbience.Content.AmbientAndMore;

namespace TerrariaAmbience.Content.Players;

public class AmbientPlayer : ModPlayer
{
    // This class serves the purpose of playing sounds and/or playing footsteps.
    // public int WallsAround => ReverbAudioSystem.WallsAround(Player.Center, new(4, 4), out var coords);
    private float _multInternal;
    public float InRoomAmbientMultiplier => MathHelper.Clamp(_multInternal, 0.2f, 1f);

    public static SoundEffectInstance soundSlippyRoughInst;
    public static SoundEffectInstance soundSlippySmoothInst;
    private bool _areSoundsInitialized;

    internal int timerUntilValidChestStateChange;

    private int chestStateNew;
    private int chestStateOld; // Same as above.

    public static HashSet<int> NonArmoryArmors = [];

    public static void DetermineNonArmoryArmors() {
        for (int i = 0; i < ItemID.Search.Count; i++) {
            var item = ItemID.Search.GetName(i);

            if (!item.Contains("wood", StringComparison.InvariantCultureIgnoreCase)) continue;
            
            NonArmoryArmors.Add(i);
        }
    }

    /// <summary>
    /// For the use of rain. Not modifiable publically due to potential anomalies happening otherwise.
    /// <para></para>
    /// Checks above the player for about 8 tiles for a ceiling. Used for wet-sounding footsteps.
    /// </summary>
    public bool HasTilesAbove { get; private set; }

    private bool _wet; // old state of player wet

    private bool _showReverbTiles;
    public override void OnEnterWorld() {
        TerrariaAmbience.DefaultAmbientHandler.HandleEnterWorld();

        if (Main.netMode == NetmodeID.SinglePlayer) return;

        SyncAmbienceSystem.AskForAmbiences();
    }
    public override void PreUpdate() {
        if (Main.soundVolume == 0) return; 

        var genCfg = ModContent.GetInstance<GeneralConfig>();
        var aaCfg = ModContent.GetInstance<AudioAdditionsConfig>();
        var servCfg = ModContent.GetInstance<AmbientConfigServer>();

        if (servCfg.newSplashSounds)
            ManagePlayerSplashes();

        #region ShowReverbTiles

        if (GeneralHelpers.KeyPress(Microsoft.Xna.Framework.Input.Keys.RightAlt))
            _showReverbTiles = !_showReverbTiles;

        if (_showReverbTiles) {
            if (genCfg.debugInterface && aaCfg.ugReverbCalculation && aaCfg.advancedReverbCalculation && aaCfg.isReverbEnabled) {

                bool isUnderground = Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneUnderworldHeight || Main.LocalPlayer.ZoneDirtLayerHeight;
                bool shouldDrawDust = Main.GameUpdateCount % 10 == 0;

                if (!isUnderground) return;

                int wallCount = 0, tileCount = 0;
                Point gridSize = new(15, 15);
                Vector2 playerCenter = Player.Center;

                int wallsNear = ReverbAudioSystem.WallsAround(playerCenter, gridSize, out List<Point> wallPoints);
                int tilesNear = ReverbAudioSystem.TilesAround(playerCenter, gridSize, out List<Point> tilePoints);

                foreach (var tilePos in tilePoints) {
                    Vector2 worldCoords = tilePos.ToVector2() * 16;
                    Point[] adjacentTiles = {
                        new(tilePos.X - 1, tilePos.Y), new(tilePos.X + 1, tilePos.Y),
                        new(tilePos.X, tilePos.Y - 1), new(tilePos.X, tilePos.Y + 1)
                    };

                    if (adjacentTiles.Any(adj => ReverbAudioSystem.CanRaycastTo(playerCenter, adj.ToVector2() * 16))) {
                        tileCount++;
                        if (shouldDrawDust) {
                            Dust.QuickBox(worldCoords, worldCoords + new Vector2(16), 0, Color.Green, null);
                        }
                    }
                }

                foreach (var wallPos in wallPoints) {
                    Vector2 worldCoords = wallPos.ToVector2() * 16;
                    if (ReverbAudioSystem.CanRaycastTo(playerCenter, worldCoords)) {
                        wallCount++;
                        if (shouldDrawDust) {
                            Dust.QuickBox(worldCoords, worldCoords + new Vector2(16), 0, Color.Red, null);
                        }
                    }
                }
            }
        }
        #endregion
        #region Step/Sound handling

        if (genCfg.wetStepsEnabled)
            HandleWetSteps();
        else
            // this essentially just works as a way to not have wet sounds play. no need to do extra magic.
            HasTilesAbove = true;

        UpdateReverbParams(8);

        // this is left in because my reverb system thinks that the sound originates from within a tile.
        #endregion
        // HandleContainerOpenings(ContainerContext.Chest, out var opened);
        
        ManageChestSounds();

        Player.runSoundDelay = 100;
    }
    void ManageChestSounds() {
        if (!ModContent.GetInstance<AmbientConfigServer>().chestSounds) return;

        timerUntilValidChestStateChange++;
        if (timerUntilValidChestStateChange < 60) return;
        if (chestStateNew < -1) return;

        chestStateNew = Player.chest;

        if ((chestStateNew != chestStateOld) && chestStateNew > -1)
            GeneralHelpers.PlaySound(new SoundStyle("TerrariaAmbience/Sounds/Custom/ambient/player/chest_open") { Volume = 0.25f }, Player.Center);
        else if ((chestStateNew != chestStateOld) && chestStateNew == -1 && chestStateOld > -1)
            GeneralHelpers.PlaySound(new SoundStyle("TerrariaAmbience/Sounds/Custom/ambient/player/chest_close") { Volume = 0.55f }, Player.Center);
        else if ((chestStateNew != chestStateOld) && chestStateOld > -1)
            GeneralHelpers.PlaySound(new SoundStyle("TerrariaAmbience/Sounds/Custom/ambient/player/chest_close") { Volume = 0.55f }, Player.Center);

        chestStateOld = chestStateNew;
    }
    void HandleWetSteps() {
        // This is a pretty niche finding for tiles above said player
        // can obviously be optimized... fml

        if (Player.wet) return;

        int checkX = (int)Player.Top.X / 16;
        int checkStartY = (int)Player.Top.Y / 16;
        int checkEndY = (int)Player.Top.Y / 16 - 30;

        for (int j = checkStartY; j > checkEndY; j--) {
            // don't bother checking further
            if (!WorldGen.InWorld(checkX, j)) break;

            Tile tile = Main.tile[checkX, j];

            // check for solid tiles only
            if (!tile.HasTile || !Main.tileSolid[tile.TileType]) continue;

            HasTilesAbove = true;
        }
    }
    void UpdateReverbParams(uint time) {
        if (!ModContent.GetInstance<AudioAdditionsConfig>().isReverbEnabled) return;
        if (Main.GameUpdateCount % time != 0) return;

        var param = ReverbAudioSystem.CreateAudioFX(Player.Center);
        Player.GetModPlayer<ReverbPlayer>().LatestParams = param;
    }
    public override void PostUpdate() {
        if (Player.whoAmI != Main.myPlayer)
            return;

        if (RoomDetectionPlayer.IsInRoom)
            _multInternal -= 0.05f;
        else
            _multInternal += 0.05f;

        _multInternal = MathHelper.Clamp(_multInternal, 0, 1);

        HandleIceScraping();

        // why is this shit here
        if (!TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance.IsPlaying())
            TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance.Play();
        float maxDist = 780f;
        float campfireVolumeScalar = 0.75f;
        if (ModContent.GetInstance<GeneralConfig>().campfireSounds && CampfireDetection.IsNearCampfire) {
            cracklePan = (CampfireDetection.CampfireDistance / maxDist) * (CampfireDetection.IsCampfireOnTheRight ? -1 : 1) / 2;
            crackleVolume = 1f - CampfireDetection.CampfireDistance / maxDist * campfireVolumeScalar;
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
    /// <param name="heightCheck">The maximum tiles to check above the player.</param>
    /// <param name="checkPos">The position to check water pressure for. By default is the player's position.</param>
    /// <returns>The subpixel Y-component distance from the player's head to the surface.</returns>
    public float GetWaterPressureFloat(int heightCheck) => TileUtils.GetWaterPressureFloat(heightCheck, Player.position);
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
        bool isNotMetallic(Item item) {
            return NonArmoryArmors.Contains(item.type);
        }
        bool hasVanityCovering(int context) {
            return !Player.armor[context + 10].IsAir;
        }

        // what the sigma. (ryan, 2025)
        if (hasHeadArmor) {
            if (!head.vanity) {
                if (!isNotMetallic(head)) {
                    if (!hasVanityCovering(GeneralHelpers.IDs.ArmorSlotID.HeadSlot)) {
                        determinedValue *= 1.25f + 0.05f;
                    }
                }
            }
        }
        if (hasChestArmor) {
            if (!chest.vanity) {
                if (!isNotMetallic(chest)) {
                    if (!hasVanityCovering(GeneralHelpers.IDs.ArmorSlotID.ChestSlot)) {
                        determinedValue += 0.05f;
                    }
                }
            }
        }
        if (hasLegArmor) {
            if (!legs.vanity) {
                if (!isNotMetallic(legs)) {
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

    public void HandleIceScraping() {
        if (Player.whoAmI != Main.myPlayer) return;
        // ^ eventually add support for hearing other people
        if (!Main.dedServ) {
            if (!_areSoundsInitialized) {
                soundSlippyRoughInst = Mod.Assets.Request<SoundEffect>("Sounds/Custom/ambient/player/ice_slide_rough", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value.CreateInstance();
                soundSlippyRoughInst.IsLooped = true;
                soundSlippyRoughInst?.Play();
                soundSlippyRoughInst.Volume = 0f;
                soundSlippySmoothInst = Mod.Assets.Request<SoundEffect>("Sounds/Custom/ambient/player/ice_slide_smooth", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value.CreateInstance();
                soundSlippySmoothInst.IsLooped = true;
                soundSlippySmoothInst?.Play();
                soundSlippySmoothInst.Volume = 0f;
                _areSoundsInitialized = true;
            }
            bool slippyRough = Player.slippy;
            bool slippySmooth = Player.slippy2;

            if (slippyRough) {
                soundSlippyRoughInst.Volume = Math.Abs(Player.velocity.X / 50f) * GeneralHelpers.GetVolumeFromPosition(Player.Bottom) * Main.soundVolume;
                soundSlippyRoughInst.Pan = GeneralHelpers.GetPanFromPosition(Player.Bottom);
            }
            else
                soundSlippyRoughInst.Volume -= 0.01f;
            if (slippySmooth) {
                soundSlippySmoothInst.Volume = Math.Abs(Player.velocity.X / 50f) * GeneralHelpers.GetVolumeFromPosition(Player.Bottom) * Main.soundVolume;
                soundSlippySmoothInst.Pan = GeneralHelpers.GetPanFromPosition(Player.Bottom);
            }
            else
                soundSlippySmoothInst.Volume -= 0.01f;
            soundSlippyRoughInst.Volume = MathHelper.Clamp(soundSlippyRoughInst.Volume, 0f, 0.5f);
            soundSlippySmoothInst.Volume = MathHelper.Clamp(soundSlippySmoothInst.Volume, 0f, 0.5f);
        }
    }
}

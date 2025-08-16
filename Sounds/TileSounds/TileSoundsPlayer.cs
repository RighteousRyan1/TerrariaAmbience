using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using TerrariaAmbience.Helpers;
using System.Linq;
using System.Collections.Generic;
using TerrariaAmbience.Core;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Sounds.SoundFilters;

namespace TerrariaAmbience.Sounds.TileSounds;

public class TileSoundsPlayer : ModPlayer {
    public SoundStyle WoodCreak;

    public override void PostUpdate() {
        var config = ModContent.GetInstance<AudioAdditionsConfig>();
        if (!config.woodCreaks) return;

        Point playerTilePos = new((int)Player.Center.X / 16, (int)Player.Center.Y / 16);
        var tileList = GeneralHelpers.GetTileSquareCoordinates(playerTilePos.X, playerTilePos.Y, 80, 80);

        static bool IsTileTypeWood(int type) => !FootstepHandler.AllTileLists.Any(tList => tList.Contains(type));

        foreach (var tileCoord in tileList) {
            Tile acTile = Framing.GetTileSafely(tileCoord);
            Tile belowTile = Framing.GetTileSafely(tileCoord.X, tileCoord.Y + 1);

            if (IsTileTypeWood(acTile.TileType) && acTile.CollisionType() == 1 && !belowTile.HasTile && Main.rand.NextBool(11000)) {
                int rand = Main.rand.Next(1, 8);
                Vector2 worldPos = tileCoord.ToVector2().ToWorldCoordinates();

                var param = SoundFilterSystem.LatestParams;

                WoodCreak = new SoundStyle($"TerrariaAmbience/Sounds/Custom/ambient/blocks/wood_creak{rand}") {
                    PitchVariance = 0.1f,
                    Volume = 0.04f,
                };

                var soundInstance = GeneralHelpers.PlaySound(WoodCreak, worldPos);
                if (soundInstance != null) {
                    soundInstance.ApplyReverb(param.ReverbGain, param);
                    if (param.LowPassEnabled) soundInstance.ApplyLowPassFilter(param.LowPassIntensity);
                    if (param.BandPassEnabled) soundInstance.ApplyBandPassFilter(param.BandPassIntensity);
                }
            }
        }
    }
}
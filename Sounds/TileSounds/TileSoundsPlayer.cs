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

    public static readonly List<Point> _woodsBuffer = [];

    // heavy fps loss.... no more!
    public override void PostUpdate() {
        var config = ModContent.GetInstance<AudioConfig>();
        if (!config.woodCreaks) return;

        Point playerTilePos = new((int)Player.Center.X / 16, (int)Player.Center.Y / 16);

        var chanceSuccess = Main.rand.NextBool(100);

        // if success, just calculate all once and choose one.
        if (!chanceSuccess) return;

        _woodsBuffer.Clear();

        // was i on crack doing an 80x80 square before???
        var tileList = GeneralHelpers.GetTileSquareCoordinates(playerTilePos.X, playerTilePos.Y, 40, 40);

        static bool IsTileTypeWood(int type) => FootstepHandler.BluntWood.Contains(type) || FootstepHandler.FilledWood.Contains(type);

        foreach (var tileCoord in tileList) {
            Tile acTile = Framing.GetTileSafely(tileCoord);
            Tile belowTile = Framing.GetTileSafely(tileCoord.X, tileCoord.Y + 1);

            // is wood?
            if (!IsTileTypeWood(acTile.TileType)) continue;
            // unsupported?
            if (belowTile.HasTile) continue;
            // actuated?
            if (acTile.CollisionType() != 1) continue;

            _woodsBuffer.Add(tileCoord);
        }

        if (_woodsBuffer.Count == 0) return;

        int randWood = Main.rand.Next(_woodsBuffer.Count);

        var tileCoordChosen = _woodsBuffer[randWood];

        var param = SoundFilterSystem.LatestParams;

        int rand = Main.rand.Next(1, 8);
        WoodCreak = new SoundStyle($"TerrariaAmbience/Sounds/Custom/ambient/blocks/wood_creak{rand}") {
            PitchVariance = 0.1f,
            Volume = 0.04f,
        };

        Vector2 worldPos = tileCoordChosen.ToVector2().ToWorldCoordinates();

        var soundInstance = GeneralHelpers.PlaySound(WoodCreak, worldPos);
        if (soundInstance != null) {
            soundInstance.ApplyReverb(param.ReverbGain, param);
            if (param.LowPassEnabled) soundInstance.ApplyLowPassFilter(param.LowPassIntensity);
            if (param.BandPassEnabled) soundInstance.ApplyBandPassFilter(param.BandPassIntensity);
        }
    }
}
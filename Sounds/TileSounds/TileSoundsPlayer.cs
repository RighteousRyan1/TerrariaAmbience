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

namespace TerrariaAmbience.Sounds.TileSounds
{
    public class TileSoundsPlayer : ModPlayer
    {
        public SoundStyle WoodCreak;
        public override void PostUpdate()
        {
			if (ModContent.GetInstance<AudioAdditionsConfig>().woodCreaks)
            {
                var tileList = GeneralHelpers.GetTileSquareCoordinates((int)Player.Center.X / 16, (int)Player.Center.Y / 16, 80, 80);

                static bool IsTileTypeWood(int type)
                {
                    foreach (var tList in FootstepHandler.AllTileLists)
                    {
                        if (tList.Contains(type))
                            return false;
                    }
                    return true;
                }
                // Main.NewText(Main.tile[(int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16].CollisionType + $" ({TileID.Search.GetName(Main.tile[(int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16].type)})");
                foreach (var tileCoord in tileList)
                {
                    var acTile = Framing.GetTileSafely(tileCoord);
                    if (IsTileTypeWood(acTile.TileType) && acTile.CollisionType() == 1 && !Framing.GetTileSafely(tileCoord.X, tileCoord.Y + 1).HasTile)
                    {
                        if (Main.rand.NextBool(11000))
                        {
                            int rand = Main.rand.Next(1, 8);

                            ReverbAudioSystem.CreateAudioFX(tileCoord.ToVector2().ToWorldCoordinates(), out var r, out var o, out var d, out var sd, new Vector2(0, -16));
                            WoodCreak = new SoundStyle($"TerrariaAmbience/Sounds/Custom/ambient/blocks/wood_creak{rand}")
                            {
                                PitchVariance = 0.1f,
                                Volume = 0.04f,
                            };
                            if (sd)
                                GeneralHelpers.PlaySound(WoodCreak, tileCoord.ToVector2().ToWorldCoordinates()).ApplyReverbReturnInstance(r).ApplyLowPassFilterReturnInstance(o).ApplyBandPassFilter(d);
                            else
                                GeneralHelpers.PlaySound(WoodCreak, tileCoord.ToVector2().ToWorldCoordinates()).ApplyReverbReturnInstance(r).ApplyLowPassFilterReturnInstance(o);
                        }
                    }
                }
            }
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaAmbience.Core;
using TerrariaAmbience.Helpers;

namespace TerrariaAmbience.Sounds
{
	public class SoundEffectsModifier : ModSystem
    {
		public override void Load()
		{
			AudioModifier.CacheReflection();
            On.Terraria.Audio.SoundEngine.PlaySound_int_int_int_int_float_float += ApplySoundFilters;
            On.Terraria.Audio.SoundEngine.PlaySound_ISoundStyle_int_int += ApplySoundFilters;
            On.Terraria.Audio.SoundEngine.PlaySound_ISoundStyle_Vector2 += ApplySoundFilters;
		}

        private SoundEffectInstance ApplySoundFilters(On.Terraria.Audio.SoundEngine.orig_PlaySound_ISoundStyle_Vector2 orig, ISoundStyle type, Vector2 position)
        {
			if (type is ModSoundStyle)
			{
				ReverbAudioSystem.CreateAudioFX(position, out float gain, out float occ, out float damp, out bool sDamp);

				if (Main.LocalPlayer.grappling[0] == 1 || (Main.LocalPlayer.itemAnimation > 0 && Main.LocalPlayer.HeldItem.pick > 0))
					gain = Main.player[Main.myPlayer].GetModPlayer<ReverbPlayer>().ReverbFactor;
				if (sDamp)
				{
					return orig(type, position).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ).ApplyBandPassFilter(damp);
				}
				return orig(type, position).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ);
			}
			else
			{
				LegacySoundStyle style = (LegacySoundStyle)type;

				bool containsIgnoreablePos = stylesToIgnorePosition.Contains(style.SoundId);
				ReverbAudioSystem.CreateAudioFX(position, out float gain, out float occ, out float damp, out bool sDamp);

				if (containsIgnoreablePos && Main.LocalPlayer.grappling[0] == 1 || (Main.LocalPlayer.itemAnimation > 0 && Main.LocalPlayer.HeldItem.pick > 0))
					gain = Main.player[Main.myPlayer].GetModPlayer<ReverbPlayer>().ReverbFactor;
				if (sDamp)
				{
					if (!listNoAffect.Contains(style.SoundId))
						return orig(type, position).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ).ApplyBandPassFilter(damp);
				}
				if (!listNoAffect.Contains(style.SoundId))
					return orig(type, position).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ);
				return orig(type, position);
			}
		}

        private SoundEffectInstance ApplySoundFilters(On.Terraria.Audio.SoundEngine.orig_PlaySound_ISoundStyle_int_int orig, ISoundStyle type, int x, int y)
        {
			// x and y being -1 means no location, just for client
			if (type is ModSoundStyle mss)
			{
				ReverbAudioSystem.CreateAudioFX(new(x, y), out float gain, out float occ, out float damp, out bool sDamp);

				if (Main.LocalPlayer.grappling[0] == 1 || (Main.LocalPlayer.itemAnimation > 0 && Main.LocalPlayer.HeldItem.pick > 0))
					gain = Main.player[Main.myPlayer].GetModPlayer<ReverbPlayer>().ReverbFactor;
				if (sDamp)
				{
					return orig(type, x, y).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ).ApplyBandPassFilter(damp);
				}
				return orig(type, x, y).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ);
			}
			else
            {
				LegacySoundStyle style = (LegacySoundStyle)type;

				bool containsIgnoreablePos = stylesToIgnorePosition.Contains(style.SoundId);
				ReverbAudioSystem.CreateAudioFX(new(x, y), out float gain, out float occ, out float damp, out bool sDamp);

				if (containsIgnoreablePos && Main.LocalPlayer.grappling[0] == 1 || (Main.LocalPlayer.itemAnimation > 0 && Main.LocalPlayer.HeldItem.pick > 0))
					gain = Main.player[Main.myPlayer].GetModPlayer<ReverbPlayer>().ReverbFactor;
				if (sDamp)
				{
					if (!listNoAffect.Contains(style.SoundId))
						return orig(type, x, y).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ).ApplyBandPassFilter(damp);
				}
				if (!listNoAffect.Contains(style.SoundId))
					return orig(type, x, y).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ);
				return orig(type, x, y);
			}
		}

        private SoundEffectInstance ApplySoundFilters(On.Terraria.Audio.SoundEngine.orig_PlaySound_int_int_int_int_float_float orig, int type, int x, int y, int Style, float volumeScale, float pitchOffset)
        {
			// CombatText.NewText(new(x, y, 1, 1), Color.White, $"{Style}");
			bool containsIgnoreablePos = stylesToIgnorePosition.Contains(type);
			ReverbAudioSystem.CreateAudioFX(new Vector2(x, y), out float gain, out float occ, out float damp, out bool sDamp);

			if (containsIgnoreablePos && Main.LocalPlayer.grappling[0] == 1 || (Main.LocalPlayer.itemAnimation > 0 && Main.LocalPlayer.HeldItem.pick > 0))
				gain = Main.player[Main.myPlayer].GetModPlayer<ReverbPlayer>().ReverbFactor;
			if (sDamp)
			{
				if (!listNoAffect.Contains(type))
					return orig(type, x, y, Style, volumeScale, pitchOffset).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ).ApplyBandPassFilter(damp);
			}
			if (!listNoAffect.Contains(type))
				return orig(type, x, y, Style, volumeScale, pitchOffset).ApplyReverbReturnInstance(gain).ApplyLowPassFilterReturnInstance(occ);
			return orig(type, x, y, Style, volumeScale, pitchOffset);
		}

		public static List<ISoundStyle> badStyles = new()
		{
			new LegacySoundStyle(SoundID.Grab, -1),
			new LegacySoundStyle(SoundID.MenuOpen, -1),
			new LegacySoundStyle(SoundID.MenuClose, -1),
			new LegacySoundStyle(SoundID.MenuTick, -1),
			new LegacySoundStyle(SoundID.Chat, -1),
			new LegacySoundStyle(SoundID.Research, -1),
			new LegacySoundStyle(SoundID.ResearchComplete, -1),
		};
		public static List<int> stylesToIgnorePosition = new()
		{
			SoundID.Dig,
			SoundID.Tink
		};
		public static float occludeAmount;
        public static float reverbActual;
		public static bool playingSound;
		public static int soundX;
		public static int soundY;
		public static int showTime;

		public static List<int> listNoAffect = new()
		{
			SoundID.Grab,
			SoundID.MenuTick,
			SoundID.MenuOpen,
			SoundID.MenuClose,
			SoundID.Research,
			SoundID.ResearchComplete
		};
    }
	public class ReverbAudioSystem : ModSystem
	{
		public override void PostUpdateEverything()
		{
			SoundEffectsModifier.playingSound = false;
		}
		public static bool CanFindEscapeRoute(Vector2 position, int dist)
		{
			for (int i = (int)position.X / 16 - dist; i < (int)position.X / 16 + dist; i++)
			{
				for (int j = (int)position.Y / 16 - dist; j < (int)position.Y / 16 + dist; j++)
				{
					Tile tile = Framing.GetTileSafely(i, j);

					if (tile.wall > 0 && CanRaycastTo(position, new Vector2(i, j).ToWorldCoordinates()))
					{
						continue;
					}
					else if (!tile.IsActive && tile.wall <= 0)
					{
						return true;
					}
				}
			}
			return false;
		}
		/// <summary>
		/// TILE COORDS!
		/// </summary>
		public static bool HasWallNextTo(Vector2 fromPosition, Point position)
		{
			var tileUp = Framing.GetTileSafely(position.X, position.Y - 1);
			var tileDown = Framing.GetTileSafely(position.X, position.Y + 1);
			var tileLeft = Framing.GetTileSafely(position.X - 1, position.Y);
			var tileRight = Framing.GetTileSafely(position.X + 1, position.Y);

			if (tileUp.wall > 0 && !tileUp.IsActive)
			{
				if (CanRaycastTo(fromPosition, new Vector2(position.X, position.Y - 1)))
				{
					return true;
				}
			}
			if (tileDown.wall > 0 && !tileDown.IsActive)
			{
				if (CanRaycastTo(fromPosition, new Vector2(position.X, position.Y + 1)))
				{
					return true;
				}
			}
			if (tileRight.wall > 0 && !tileRight.IsActive)
			{
				if (CanRaycastTo(fromPosition, new Vector2(position.X + 1, position.Y)))
				{
					return true;
				}
			}
			if (tileLeft.wall > 0 && !tileLeft.IsActive)
			{
				if (CanRaycastTo(fromPosition, new Vector2(position.X - 1, position.Y)))
				{
					return true;
				}
			}
			return false;
		}
		public static bool CanRaycastTo(Vector2 begin, Vector2 destination)
		{
			if (Collision.CanHitLine(begin, 1, 1, destination, 1, 1))
				return true;
			return false;
		}
		/// <summary>
		/// This returns tile coordinates!
		/// </summary>
		public static int TilesAround(Vector2 position, Point grid, out List<Point> tileCoords)
		{
			tileCoords = new();
			int index = 0;
			for (int i = (int)position.X / 16 - grid.X; i < (int)position.X / 16 + grid.X; i++)
			{
				for (int j = (int)position.Y / 16 - grid.Y; j < (int)position.Y / 16 + grid.Y; j++)
				{
					Tile tile = Framing.GetTileSafely(i, j);

					if (tile.IsActive && tile.CollisionType == 1)
					{
						index++;
						tileCoords.Add(new Point(i, j));
					}
				}
			}
			return index;
		}
		public static int TilesAround(Vector2 position, Point grid, out List<Tile> tiles)
		{
			tiles = new();
			int index = 0;
			for (int i = (int)position.X / 16 - grid.X; i < (int)position.X / 16 + grid.X; i++)
			{
				for (int j = (int)position.Y / 16 - grid.Y; j < (int)position.Y / 16 + grid.Y; j++)
				{
					Tile tile = Framing.GetTileSafely(i, j);

					if (tile.IsActive && tile.CollisionType == 1)
					{
						index++;
						tiles.Add(tile);
					}
				}
			}
			return index;
		}
		public static int WallsAround(Vector2 position, Point grid, out List<Point> tileCoords)
		{
			tileCoords = new();
			int index = 0;
			for (int i = (int)position.X / 16 - grid.X; i < (int)position.X / 16 + grid.X; i++)
			{
				for (int j = (int)position.Y / 16 - grid.Y; j < (int)position.Y / 16 + grid.Y; j++)
				{
					Tile tile = Framing.GetTileSafely(i, j);

					if (tile.wall > 0 && !tile.IsActive)
					{
						index++;
						tileCoords.Add(new Point(i, j));
					}
				}
			}
			return index;
		}
		public static int EmptyTilesAround(Vector2 position, int dist, bool condition, out List<Point> tileCoords)
		{
			tileCoords = new();
			int index = 0;
			if (condition)
			{
				for (int i = (int)position.X / 16 - dist; i < (int)position.X / 16 + dist; i++)
				{
					for (int j = (int)position.Y / 16 - dist; j < (int)position.Y / 16 + dist; j++)
					{
						Tile tile = Framing.GetTileSafely(i, j);

						if (!tile.IsActive)
						{
							index++;
							tileCoords.Add(new Point(i, j));
						}
					}
				}
			}
			return index;
		}
		public static void CreateAudioFX(Vector2 fromV2, out float rvGain, out float occlusion, out float dampening, out bool shouldDampen, Vector2 offset = default)
        {
			var cfg = ModContent.GetInstance<AmbientConfigServer>();
			shouldDampen = false;
			dampening = 0f;
			rvGain = 0;
			occlusion = 1f;
			var reverbActual = 0f;
			if (Main.gameMenu)
				return;
			bool playerUnderwater = Main.LocalPlayer.Underwater();
			if (!cfg.noReverbMath)
			{
				if (cfg.advancedReverbCalculation)
				{
					bool playerUnderground = Main.LocalPlayer.ZoneRockLayerHeight || Main.LocalPlayer.ZoneUnderworldHeight || Main.LocalPlayer.ZoneDirtLayerHeight;
					if (Main.LocalPlayer.ZoneOverworldHeight)
					{
						if (cfg.surfaceReverbCalculation)
						{
							int wallsNear = WallsAround(fromV2, new Point(15, 15), out List<Point> wallPoints);
							foreach (var pt in wallPoints.Where(pt => Framing.GetTileSafely(pt).wall > 0))
							{
								var check = !WallID.Search.GetName(Framing.GetTileSafely(pt).wall).ToLower().Contains("fence");
								if (check)
								{
									if (CanRaycastTo(fromV2, pt.ToVector2() * 16 + offset))
									{
										var name = WallID.Search.GetName(Framing.GetTileSafely(pt).wall).ToLower();
										bool has(string input) => name.Contains(input);
										if (has("dirt"))
											reverbActual += 0.0004f;
										if (has("stone") || has("brick") || has("cave") || has("rock"))
											reverbActual += 0.001f;
										if (has("wood") || has("planked"))
											reverbActual += 0.0001f;
									}
								}
							}
						}
					}
					if (playerUnderground)
					{
						if (cfg.ugReverbCalculation)
						{
							int wallCount = 0;
							int tileCount = 0;
							int wallsNear = WallsAround(fromV2, new Point(15, 15), out var wallPoints);
							int tilesNear = TilesAround(fromV2, new Point(15, 15), out List<Point> tilePoints);
							foreach (var tilePos in tilePoints)
							{
								var worldCoords = tilePos.ToVector2() * 16;
								var left = new Point(tilePos.X - 1, tilePos.Y);
								var right = new Point(tilePos.X + 1, tilePos.Y);
								var up = new Point(tilePos.X, tilePos.Y - 1);
								var down = new Point(tilePos.X, tilePos.Y + 1);
								if (CanRaycastTo(fromV2, left.ToVector2() * 16) || CanRaycastTo(fromV2, right.ToVector2() * 16)
									|| CanRaycastTo(fromV2, up.ToVector2() * 16) || CanRaycastTo(fromV2, down.ToVector2() * 16))
								{
									tileCount++;
									reverbActual += 0.015f;
								}
							}
							foreach (var wallPos in wallPoints)
							{
								if (!WallID.Search.GetName(Framing.GetTileSafely(wallPos).type).ToLower().Contains("fence"))
								{
									if (CanRaycastTo(fromV2, wallPos.ToVector2() * 16))
									{
										wallCount++;
										reverbActual += 0.0005f;
									}
								}
							}
						}
						else
                        {
							int tilesNear = TilesAround(fromV2, new Point(15, 15), out List<Point> tilePoints);
							reverbActual += (float)tilesNear / 2000;
                        }
					}
				}
				else
                {
					reverbActual = MathUtils.InverseLerp((float)(Main.worldSurface * 16), Main.maxTilesY * 16, Main.LocalPlayer.Center.Y) / 2;
                }
			}
			float dist = Vector2.Distance(Main.LocalPlayer.Center, fromV2 + offset);

			bool hasLOS = CanRaycastTo(Main.LocalPlayer.Center, fromV2 + offset);

			float getGoodOcclusion = 1f - dist / 1000;
			if (getGoodOcclusion <= 0.01f)
				getGoodOcclusion = 0.01f;

			occlusion = 1f;
			if (!hasLOS)
				occlusion = getGoodOcclusion;
			rvGain = reverbActual;
			bool underWater = Collision.DrownCollision(fromV2, 1, 1);
			if (playerUnderwater && !underWater)
			{
				shouldDampen = true;
				dampening = 0.015f;
			}
			if (underWater && playerUnderwater)
			{
				shouldDampen = true;
				dampening = 0.075f;
			}
			if (underWater && !playerUnderwater)
			{
				shouldDampen = true;
				dampening = 0.225f;
			}
		}
	}
	public class ReverbPlayer : ModPlayer
    {
		public float ReverbFactor { get; set; }
    }
	public static class AudioModifier
	{
		internal static MethodInfo ReverbInfo { get; private set; }
		internal static MethodInfo LowPassInfo { get; private set; }
		internal static MethodInfo HighPassInfo { get; private set; }
		internal static MethodInfo BandPassInfo { get; private set; }
		public static void CacheReflection()
        {
			ReverbInfo = typeof(SoundEffectInstance)
				.GetMethod("INTERNAL_applyReverb",
				BindingFlags.Instance | BindingFlags.NonPublic);
			LowPassInfo = typeof(SoundEffectInstance)
					.GetMethod("INTERNAL_applyLowPassFilter",
					BindingFlags.Instance | BindingFlags.NonPublic);
			HighPassInfo = typeof(SoundEffectInstance)
					.GetMethod("INTERNAL_applyHighPassFilter",
					BindingFlags.Instance | BindingFlags.NonPublic);
			BandPassInfo = typeof(SoundEffectInstance)
					.GetMethod("INTERNAL_applyBandPassFilter",
					BindingFlags.Instance | BindingFlags.NonPublic);
		}
		public static float ApplyReverb(this SoundEffectInstance instance, float rvGain)
		{
			rvGain = MathHelper.Clamp(rvGain, 0f, 1f);
			if (instance != null)
				ReverbInfo?.Invoke(instance, new object[] { rvGain });
			return rvGain;
		}
		public static SoundEffectInstance ApplyReverbReturnInstance(this SoundEffectInstance instance, float rvGain)
		{
			rvGain = MathHelper.Clamp(rvGain, 0f, 1f);
			if (instance != null)
				ReverbInfo?.Invoke(instance, new object[] { rvGain });
			return instance;
		}
		public static ActiveSound ApplyReverbOnActiveSound(this ActiveSound sound, float rvGain)
		{
			rvGain = MathHelper.Clamp(rvGain, 0f, 1f);
			if (sound.Sound != null)
				ReverbInfo?.Invoke(sound.Sound, new object[] { rvGain });
			return sound;
		}
		public static float ApplyLowPassFilter(this SoundEffectInstance instance, float cutoff)
		{
			cutoff = MathHelper.Clamp(cutoff, 0f, 1f);
			if (instance != null)
				LowPassInfo?.Invoke(instance, new object[] { cutoff });
			return cutoff;
		}
		public static SoundEffectInstance ApplyLowPassFilterReturnInstance(this SoundEffectInstance instance, float cutoff)
		{
			cutoff = MathHelper.Clamp(cutoff, 0f, 1f);
			if (instance != null)
				LowPassInfo?.Invoke(instance, new object[] { cutoff });
			return instance;
		}
		public static SoundEffectInstance ApplyHighPassFilter(this SoundEffectInstance instance, float cutoff)
		{
			cutoff = MathHelper.Clamp(cutoff, 0f, 1f);
			if (instance != null)
				HighPassInfo?.Invoke(instance, new object[] { cutoff });
			return instance;
		}
		public static SoundEffectInstance ApplyBandPassFilter(this SoundEffectInstance instance, float center)
		{
			center = MathHelper.Clamp(center, -1f, 1f);
			if (instance != null)
				BandPassInfo?.Invoke(instance, new object[] { center });
			return instance;
		}
		public static void SafelySetPan(this SoundEffectInstance sfx, float pan)
		{
			pan = MathHelper.Clamp(pan, -1f, 1f);
			sfx.Pan = pan;
		}
		public static void SafelySetVolume(this SoundEffectInstance sfx, float volume)
		{
			volume = MathHelper.Clamp(volume, -1f, 1f);
			sfx.Volume = volume;
		}
		public static void SafelySetPitch(this SoundEffectInstance sfx, float pitch)
		{
			pitch = MathHelper.Clamp(pitch, -1f, 1f);
			sfx.Pitch = pitch;
		}
	}
}
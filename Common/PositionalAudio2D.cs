using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ModLoader;
using TerrariaAmbience.Helpers;
using Terraria;
using System;

namespace TerrariaAmbience.Common
{
    /// <summary>
    /// Get a 2D positional audio (no up or down) from a position relative to the player.
    /// </summary>
    public sealed class PositionalAudio2D
    {
        public static List<PositionalAudio2D> Total2DPosAudios { get; private set; } = new();

        public float X => Position.X;
        public float Y => Position.Y;
        public Vector2 Position { get; set; }

        public SoundEffect Audio { get; private set; }
        private SoundEffectInstance _instancedAudio;

        public readonly int maxLoopTimes;
        private int curLoopTimes;

        public float volume = 1f;

        private float _maxDistance;
        private int _lastRecordedLoopTime;

        public float MaxVolume { get; set; }

        public int maxDelay;
        private int _delay;

        public string Name { get; set; }

        public bool IsPlaying => _instancedAudio.IsPlaying();

        private ValueTuple<int, int> minMax;
        private ValueTuple<int, int> minMaxLoops;

        public PositionalAudio2D(SoundEffect sfx, ValueTuple<int, int> loopTimes, float maxVolume, ValueTuple<int, int> minMaxDelay = default)
        {
            if (!Main.dedServ)
            {
                MaxVolume = maxVolume;
                Audio = sfx;
                minMaxLoops = loopTimes;
                minMax = minMaxDelay;
                _instancedAudio = Audio.CreateInstance();
                Total2DPosAudios.Add(this);
            }
        }
        public void Play(Vector2 position, float maxDist)
        {
            _maxDistance = maxDist;
            Position = position;
            curLoopTimes = Main.rand.Next(minMaxLoops.Item1, minMaxLoops.Item2 + 1);
            curLoopTimes--;
            _instancedAudio = Audio.CreateInstance();
            _instancedAudio?.Play();
        }
        internal static void Update()
        {
            for (int i = 0; i < Total2DPosAudios.Count; i++)
            {
                var audio2d = Total2DPosAudios[i];
                if (audio2d.volume > audio2d.MaxVolume)
                    audio2d.volume = audio2d.MaxVolume;
                if (audio2d._instancedAudio != null)
                {
                    if (audio2d._instancedAudio.IsPlaying())
                    {
                        audio2d._instancedAudio.Pan = GetPanFromPosition(audio2d.Position, audio2d._maxDistance);
                        audio2d._instancedAudio.Volume = GetVolumeFromPosition(audio2d.Position, audio2d._maxDistance) * Main.ambientVolume;
                    }
                    else if (audio2d.curLoopTimes > 0)
                    {
                        if (audio2d._delay > 0)
                            audio2d._delay--;
                        if (audio2d._delay <= 0)
                        {
                            audio2d.curLoopTimes--;
                            audio2d._delay = Main.rand.Next(audio2d.minMax.Item1, audio2d.minMax.Item2 + 1);
                            audio2d?._instancedAudio?.Play();
                        }
                    }
                }
            }
        }
        public override string ToString()
        {
            return $"Name: {Name} | MaxLoopTimes: {maxLoopTimes} | CurLoopTime: {curLoopTimes} | Pos: ({(int)X}, {(int)Y}) | Vol: {_instancedAudio.Volume} | Pan: {_instancedAudio.Pan} | Delay: {_delay}";
        }
        public static float GetPanFromPosition(Vector2 position, float distance = 1f)
        {
            try
            {
                bool onScreen = false;
                float panFromVector = 0f;
                if (position.X == -1 || position.Y == -1)
                    onScreen = true;
                else
                {
                    Rectangle screenBounds = new((int)(Main.screenPosition.X - (Main.screenWidth * 2)), (int)(Main.screenPosition.Y - (Main.screenHeight * 2)), Main.screenWidth * 5, Main.screenHeight * 5);
                    Rectangle rectFromVect = new((int)position.X, (int)position.Y, 1, 1);
                    Vector2 midScreen = new(Main.screenPosition.X + Main.screenWidth / 2, Main.screenPosition.Y + Main.screenHeight / 2);
                    if (rectFromVect.Intersects(screenBounds))
                        onScreen = true;
                    if (onScreen)
                    {
                        // panFromVector = (position.X - midScreen.X) / (Main.screenWidth * 0.5f);
                        panFromVector = (position.X - midScreen.X) / (Main.screenWidth * distance);
                        float absPanX = Math.Abs(position.X - midScreen.X);
                        float absPanY = Math.Abs(position.Y - midScreen.Y);
                        float panSQRT = (float)Math.Sqrt(absPanX * absPanX + absPanY * absPanY);
                    }
                }
                if (panFromVector < -1f)
                    panFromVector = -1f;
                if (panFromVector > 1f)
                    panFromVector = 1f;
                return panFromVector;
            }
            catch { }
            return 0f;
        }
        public static float GetVolumeFromPosition(Vector2 position, float distance = 1f)
        {
            try
            {
                bool onScreen = false;
                float volumeFromVector = 1f;
                if (position.X == -1 || position.Y == -1)
                    onScreen = true;
                else
                {
                    Rectangle screenBounds = new((int)(Main.screenPosition.X - (Main.screenWidth * 2)), (int)(Main.screenPosition.Y - (Main.screenHeight * 2)), Main.screenWidth * 5, Main.screenHeight * 5);
                    Rectangle rectFromVect = new((int)position.X, (int)position.Y, 1, 1);
                    Vector2 midScreen = new(Main.screenPosition.X + Main.screenWidth / 2, Main.screenPosition.Y + Main.screenHeight / 2);
                    if (rectFromVect.Intersects(screenBounds))
                        onScreen = true;
                    if (onScreen)
                    {
                        float num4 = Math.Abs(position.X - midScreen.X);
                        float num5 = Math.Abs(position.Y - midScreen.Y);
                        float num6 = (float)Math.Sqrt(num4 * num4 + num5 * num5);
                        volumeFromVector = 1f - num6 / ((float)Main.screenWidth * distance);
                    }
                }
                if (volumeFromVector > 1f)
                    volumeFromVector = 1f;
                if (volumeFromVector < 0f)
                    volumeFromVector = 0f;
                if (Vector2.Distance(Main.screenPosition, position) > 3850)
                    volumeFromVector = 0f;
                return volumeFromVector;
            }
            catch { }
            return 0f;
        }
    }
    internal class Update2DPositionalAudioSystem : ModSystem
    {
        public override void PostUpdateEverything() 
            => PositionalAudio2D.Update();
    }
    public static class PositionalAudio2DExtensionMethods
    {
        public static PositionalAudio2D WithName(this PositionalAudio2D audio, string name)
        {
            audio.Name = name;
            return audio;
        }
    }
}
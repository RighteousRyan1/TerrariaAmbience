using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Audio;

namespace TerrariaAmbience
{
    public class Utils
    {
        public static bool CheckPlayerArmorSlot(Player player, int slot)
        {
            return !player.armor[slot].IsAir;
        }
        public static Item GetItemFromArmorSlot(Player player, int slot)
        {
            return player.armor[slot];
        }

        public static int GetAllUsedAccSlots(Player player)
        {
            int x = 0;
            foreach (Item item in player.armor)
            {
                if (!item.IsAir)
                {
                    x++;
                }
            }
            return x;
        }
        public static string DisplayAllUsedAccSlots(Player player)
        {
            int i = 0;
            string s = "";
            foreach (Item item in player.armor)
            {
                if (!item.IsAir)
                {
                    i++;
                    s+= $" | {i}:{item.Name}";
                }
            }
            return s == "" ? "No valid slots" : s;  
        }

        public class IDs
        {
            public struct ArmorSlotID
            {
                public const short HeadSlot = 0;
                public const short ChestSlot = 1;
                public const short LegSlot = 2;
                public const short AccSlot1 = 3;
                public const short AccSlot2 = 4;
                public const short AccSlot3 = 4;
                public const short AccSlot4 = 5;
                public const short AccSlot5 = 6;
            }
        }
    }
    public static class ExtensionMethods
    {
        /// <summary>
        /// Checks if this SoundEffectInstance is Playing.
        /// </summary>
        /// <returns>A boolean determining if this sound is Paused.</returns>
        public static bool IsPlaying(this SoundEffectInstance sfx)
        {
            bool isNull = sfx == null;

            if (!isNull)
                return sfx.State == SoundState.Playing;

            return false;
        }
        /// <summary>
        /// Checks if this SoundEffectInstance is Stopped.
        /// </summary>
        /// <returns>A boolean determining if this sound is Stopped.</returns>
        public static bool IsStopped(this SoundEffectInstance sfx)
        {
            bool isNull = sfx == null;

            if (!isNull)
                return sfx.State == SoundState.Stopped;

            return false;
        }
        /// <summary>
        /// Checks if this SoundEffectInstance is Paused.
        /// </summary>
        /// <returns>A boolean determining if this sound is Paused.</returns>
        public static bool IsPaused(this SoundEffectInstance sfx)
        {
            bool isNull = sfx == null;

            if (!isNull)
                return sfx.State == SoundState.Paused;

            return false;
        }
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
        public static bool HeadWet(this Player player)
        {
            return Main.tile[(int)player.Top.X / 16, (int)(player.Top.Y - 1.25) / 16].liquid > 0;
        }
    }
}

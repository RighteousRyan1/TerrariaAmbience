using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaAmbience.Content.InventorySounds;
public static class SoundConfiguration {
    static readonly Dictionary<HoldableSoundType, SoundConfig> _soundConfigs = new()
    {
            { HoldableSoundType.Generic, new SoundConfig(HoldableType.Item, 3, 2) },
            { HoldableSoundType.Sword, new SoundConfig(HoldableType.Weapon, 2, 1) },
            { HoldableSoundType.Armor, new SoundConfig(HoldableType.Item, 2, 2) },
            { HoldableSoundType.Coin, new SoundConfig(HoldableType.Item, 2, 2) },
            { HoldableSoundType.Book, new SoundConfig(HoldableType.Item, 1, 1) },
            { HoldableSoundType.Potion, new SoundConfig(HoldableType.Item, 2, 2) },
            { HoldableSoundType.Key, new SoundConfig(HoldableType.Item, 2, 2) },
            { HoldableSoundType.Gun, new SoundConfig(HoldableType.Weapon, 1, 1) },
            { HoldableSoundType.Bow, new SoundConfig(HoldableType.Weapon, 2, 2) },
            { HoldableSoundType.Arrow, new SoundConfig(HoldableType.Item, 4, 2) },
            { HoldableSoundType.Clothing, new SoundConfig(HoldableType.Item, 2, 2) }
    };

    public static SoundConfig GetConfig(HoldableSoundType soundType) =>
        _soundConfigs.TryGetValue(soundType, out var config) ? config : _soundConfigs[HoldableSoundType.Generic];

    public readonly struct SoundConfig(HoldableType type, int pickupVariants, int putdownVariants) {
        public HoldableType Type { get; } = type;
        public int PickupVariants { get; } = pickupVariants;
        public int PutdownVariants { get; } = putdownVariants;
    }
}

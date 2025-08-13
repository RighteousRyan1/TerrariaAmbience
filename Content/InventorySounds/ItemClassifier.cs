using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaAmbience.Content.InventorySounds;
public static class ItemClassifier {
    static readonly HashSet<int> CoinTypes = [
            ItemID.PlatinumCoin, ItemID.GoldCoin, ItemID.SilverCoin,
            ItemID.LuckyCoin, ItemID.CopperCoin
    ];

    static readonly HashSet<int> SpellBookTypes = [
            ItemID.WaterBolt, ItemID.GoldenShower, ItemID.CrystalStorm,
            ItemID.CursedFlames, ItemID.SpellTome
    ];
    static readonly HashSet<int> WateryTypes = [
        ItemID.BottomlessBucket,
        ItemID.BottomlessHoneyBucket,
        ItemID.BottomlessLavaBucket,
        ItemID.BottomlessShimmerBucket
    ];


    public static HoldableSoundType GetSoundType(Item item) {
        if (item?.type <= 0) return HoldableSoundType.Generic;

        // check specifics
        if (CoinTypes.Contains(item.type)) return HoldableSoundType.Coin;
        if (SpellBookTypes.Contains(item.type)) return HoldableSoundType.Book;

        // then check data to determine a sound type
        if (IsArmor(item)) return HoldableSoundType.Armor;
        if (WateryTypes.Contains(item.type) || IsPotion(item)) return HoldableSoundType.Potion;
        if (IsWeapon(item)) return GetWeaponType(item);
        if (IsClothing(item)) return HoldableSoundType.Clothing;
        if (IsBook(item)) return HoldableSoundType.Book;
        if (IsKey(item)) return HoldableSoundType.Key;
        if (IsArrow(item)) return HoldableSoundType.Arrow;

        return HoldableSoundType.Generic;
    }

    private static bool IsArmor(Item item) =>
        (item.headSlot > 0 || item.bodySlot > 0 || item.legSlot > 0) && !item.vanity;

    private static bool IsPotion(Item item) =>
        // item3 is the "gulp"
        (item.UseSound == SoundID.Item3 && item.buffType > 0) ||
        item.healLife > 0 || item.healMana > 0;

    private static bool IsWeapon(Item item) => item.damage > 0;

    private static HoldableSoundType GetWeaponType(Item item) {
        if (item.useAmmo == AmmoID.Arrow) return HoldableSoundType.Bow;
        if (item.useAmmo > 0) return HoldableSoundType.Gun;
        if (item.DamageType == DamageClass.Melee &&
            (!item.noUseGraphic && (item.useStyle == ItemUseStyleID.Swing || item.useStyle == ItemUseStyleID.Thrust) ||
             item.axe > 0 || item.hammer > 0 || item.pick > 0))
            return HoldableSoundType.Sword;

        return HoldableSoundType.Generic;
    }

    private static bool IsClothing(Item item) => item.vanity;

    private static bool IsBook(Item item) =>
        ItemID.Search.GetName(item.type).Contains("book", StringComparison.CurrentCultureIgnoreCase) && item.damage <= 0;

    private static bool IsKey(Item item) {
        var name = ItemID.Search.GetName(item.type);

        return name.Contains("key", StringComparison.CurrentCultureIgnoreCase) &&
            !name.Contains("mold", StringComparison.CurrentCultureIgnoreCase) &&
            item.damage <= 0;
    }
    private static bool IsArrow(Item item) => item.ammo == AmmoID.Arrow;
}

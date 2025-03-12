using System;
using UnityEngine;
using MoreMountains.InventoryEngine;

namespace JayMountains
{
    [CreateAssetMenu(fileName = "MineralItem", menuName = "JayMountains/InventoryEngine/MineralItem", order = 1)]
    [Serializable]
    public class MineralItem : BagItem
    {
        public string Region = "";
        // Upgrades per additional star based on total collected.
        public MineralUpgrade[] CollectionPerkUpgrades = new MineralUpgrade[4];

        // We set rarity here because a mineral item only has one rarity unlike dino's menu collection items.
        public RarityGrades RarityCategory = RarityGrades.Unset;
    }
}


[Serializable]
public class MineralUpgrade
{
    public enum Upgrade
    {
        None = 0,
        EnergyIncrease = 1,
        SpeedBoost = 2,
        RadiusMineralHelm = 3,
    }
    public Upgrade UpgradeType = Upgrade.None;

    public int Amount = 0;
}
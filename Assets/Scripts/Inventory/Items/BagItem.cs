using System;
using UnityEngine;
using MoreMountains.InventoryEngine;

namespace JayMountains
{
    [Serializable]
    public class BagItem : InventoryItem
    {
        public int Level = 1;
        public bool CanSell = false;
        public int goldBuy = 0;
        public int goldSell = 0;
        public bool CanUpgrade = false;
        public float[] UpgradeCost; // Note: This is also the number of times item can be upgraded.

        public void LevelUp()
        {
            ++Level;
        }
    }
}
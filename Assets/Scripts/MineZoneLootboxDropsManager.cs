using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace JayMountains
{
    public class MineZoneLootboxDropsManager : MonoBehaviour
    {
        [ReadOnly] public int MineIndex;
        [ReadOnly] public int ZoneIndex;

        [Title("Hidden in play mode.")]
        [HideInPlayMode]
        public Mine[] MineZoneLootboxDrops;


        //----------------------------------------------------------------
        public void Init()
        {
            if (MineIndex >= MineZoneLootboxDrops.Length) return;
            if (MineIndex == -1) return;

            for (int i = 0; i < MineZoneLootboxDrops[MineIndex].ZoneLootboxDrops[ZoneIndex].Lootboxes.Length; i++)
            {
                Lootbox lootbox = this.gameObject.AddComponent<Lootbox>();
                lootbox.LootboxItems = MineZoneLootboxDrops[MineIndex].ZoneLootboxDrops[ZoneIndex].Lootboxes[i].LootboxItems;
                lootbox.RandomSelect = MineZoneLootboxDrops[MineIndex].ZoneLootboxDrops[ZoneIndex].Lootboxes[i].RandomSelect;
                lootbox.WeightedRandom = MineZoneLootboxDrops[MineIndex].ZoneLootboxDrops[ZoneIndex].Lootboxes[i].WeightedRandom;
                lootbox.RandomAmount = MineZoneLootboxDrops[MineIndex].ZoneLootboxDrops[ZoneIndex].Lootboxes[i].RandomAmount;
            }
        }
    }


    [Serializable]
    public class Mine
    {
        public int MineIndex;
        public ZoneLootboxDrop[] ZoneLootboxDrops;
    }


    [Serializable]
    public class ZoneLootboxDrop
    {
        public int ZoneIndex;
        public LootboxStruct[] Lootboxes;
    }


    [Serializable]
    public struct LootboxStruct
    {
        /// <summary>
        /// An array of inventory items contained in the lootbox.
        /// </summary>
        public LootboxItem[] LootboxItems;

        /// <summary>
        /// False: Gives everything in the lootbox. <br/>
        /// True:  Gives random item(s) from the lootbox.
        /// </summary>
        public bool RandomSelect;

        /// <summary>
        /// False: Normal random. <br/>
        /// True:  Weighted random.
        /// </summary>
        [ShowIf("RandomSelect")] public bool WeightedRandom;

        /// <summary>
        /// Number of items to randomly give from the lootbox.
        /// </summary>
        [ShowIf("RandomSelect")] public int RandomAmount;
    }
}
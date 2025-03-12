using UnityEngine;
using MoreMountains.InventoryEngine;

namespace JayMountains
{
    public enum RarityGrades
    {
        Unset = -1,
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        // 8-19 for new stuff in future updates?
        CollectibleCommon = 20,
        CollectibleUncommon = 21,
        CollectibleRare = 22,
        CollectibleEpic = 23,
    }

    public static class CheckItemRarityGrade
    {
        private const string CommonSprite = "spr_slot_grey";
        private const string UncommonSprite = "spr_slot_green";
        private const string RareSprite = "spr_slot_blue";
        private const string EpicSprite = "spr_slot_purple";

        //----------------------------------------------------------------
        public static RarityGrades GetRarity(InventoryItem item)
        {
            if (item is BagItem)
            {
                if (item.name != item.ItemID)
                {
                    Debug.LogWarning($"Scriptable Object name is different from it's ItemID.");
                    return RarityGrades.Unset;
                }
                if (item.ItemID.IndexOf("_") == -1)
                {
                    Debug.LogWarning($"Scriptable Object does not have a rarity specified in it's ItemID.");
                    return RarityGrades.Unset;
                }

                if (item is MineralItem mineralItem)
                {
                    return mineralItem.RarityCategory;
                }

                // Once we reach here, it means the item has a rarity.
                var raritySuffix = item.ItemID.Split("_");
                RarityGrades rarity = RarityGrades.Unset;

                switch (raritySuffix[1])
                {
                    case "0C":
                        rarity = RarityGrades.Common;
                        break;

                    case "1U":
                        rarity = RarityGrades.Uncommon;
                        break;

                    case "2R":
                        rarity = RarityGrades.Rare;
                        break;

                    case "3E":
                        rarity = RarityGrades.Epic;
                        break;
                }

                return rarity;
            }

            //if (item is CurrencyItem)
            //{
            //    if (item.ItemID == WalletInventoryManager.Instance.GoldCurrency.ItemID)
            //        return RarityGrades.CollectibleCommon;
            //
            //    if (item.ItemID == WalletInventoryManager.Instance.GemsCurrency.ItemID)
            //        return RarityGrades.CollectibleRare;
            //}

            // If the code reached here. For sanity check log a warning.
            Debug.LogWarning($"Scriptable Object does not have a rarity.");
            return RarityGrades.Unset;
        }


        //----------------------------------------------------------------
        public static Sprite GetRaritySprite(InventoryItem item)
        {
            RarityGrades rarity = GetRarity(item);

            switch (rarity)
            {
                case RarityGrades.Common:
                    return Resources.Load<Sprite>(CommonSprite);

                case RarityGrades.Uncommon:
                    return Resources.Load<Sprite>(UncommonSprite);

                case RarityGrades.Rare:
                    return Resources.Load<Sprite>(RareSprite);

                case RarityGrades.CollectibleCommon:
                    return Resources.Load<Sprite>(CommonSprite);

                case RarityGrades.CollectibleRare:
                    return Resources.Load<Sprite>(RareSprite);

                case RarityGrades.CollectibleEpic:
                    return Resources.Load<Sprite>(EpicSprite);
            }

            return null;
        }
    }
}
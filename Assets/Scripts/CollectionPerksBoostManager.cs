using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;

namespace JayMountains
{
    public class CollectionPerksBoostManager : MMSingleton<CollectionPerksBoostManager>
    {
        private float energyIncreaseValue = 0;
        private float speedBoostValue = 0;
        private float radiusMineralHelmValue = 0;

        private float totalEnergyIncreaseValue = 0;
        private float totalSpeedBoostValue = 0;
        private float totalRadiusMineralHelmValue = 0;

        public float TotalEnergyIncreaseValue => totalEnergyIncreaseValue;
        public float TotalRadiusMineralHelmValue => totalRadiusMineralHelmValue;


        //----------------------------------------------------------------
        private void Awake()
        {
            Inventory inventory = MineralsCollectionPerksManager.Instance.MineralsCollectionPerks_Inventory;

            for (int i = 0; i < inventory.Content.Length; i++)
            {
                if (inventory.Content[i] != null)
                {
                    MineralItem mineralItem = inventory.Content[i] as MineralItem;

                    energyIncreaseValue = 0;
                    speedBoostValue = 0;
                    radiusMineralHelmValue = 0;
                    if (inventory.Content[i].Quantity >= mineralItem.UpgradeCost[0]) { AddValue(mineralItem.CollectionPerkUpgrades[0]); }
                    if (inventory.Content[i].Quantity >= mineralItem.UpgradeCost[1]) { AddValue(mineralItem.CollectionPerkUpgrades[1]); }
                    if (inventory.Content[i].Quantity >= mineralItem.UpgradeCost[2]) { AddValue(mineralItem.CollectionPerkUpgrades[2]); }
                    if (inventory.Content[i].Quantity >= mineralItem.UpgradeCost[3]) { AddValue(mineralItem.CollectionPerkUpgrades[3]); }

                    Debug.Log($"Boosted {mineralItem.ItemName} of:\n" +
                        $"{MineralUpgrade.Upgrade.EnergyIncrease}: {energyIncreaseValue}\n" +
                        $"{MineralUpgrade.Upgrade.SpeedBoost}: {speedBoostValue}\n" +
                        $"{MineralUpgrade.Upgrade.RadiusMineralHelm}: {radiusMineralHelmValue}");

                    totalEnergyIncreaseValue += energyIncreaseValue;
                    totalSpeedBoostValue += speedBoostValue;
                    totalRadiusMineralHelmValue += radiusMineralHelmValue;

                    Debug.Log($"Boosted TOTALLITY of:\n" +
                        $"{MineralUpgrade.Upgrade.EnergyIncrease}: {totalEnergyIncreaseValue}\n" +
                        $"{MineralUpgrade.Upgrade.SpeedBoost}: {totalSpeedBoostValue}\n" +
                        $"{MineralUpgrade.Upgrade.RadiusMineralHelm}: {totalRadiusMineralHelmValue}");
                }
            }
        }


        //----------------------------------------------------------------
        private void Start()
        {
            AddPerks();
        }


        //----------------------------------------------------------------
        private void AddValue(MineralUpgrade upgrade)
        {
            switch (upgrade.UpgradeType)
            {
                case MineralUpgrade.Upgrade.None:
                    break;

                case MineralUpgrade.Upgrade.EnergyIncrease:
                    energyIncreaseValue += upgrade.Amount;
                    break;

                case MineralUpgrade.Upgrade.SpeedBoost:
                    speedBoostValue += upgrade.Amount;
                    break;

                case MineralUpgrade.Upgrade.RadiusMineralHelm:
                    radiusMineralHelmValue += upgrade.Amount;
                    break;
            }
        }


        //----------------------------------------------------------------
        private void AddPerks()
        {
            var player = LevelManager.Instance.Players[0];
            Debug.Log(player.gameObject.name);

            // Add energy.
            //var newHP = player.CharacterHealth.CurrentHealth + totalEnergyIncreaseValue;
            //player.CharacterHealth.SetHealth(newHP);
            //player.CharacterHealth.InitialHealth = newHP;
            //player.CharacterHealth.MaximumHealth = newHP;
            //player.CharacterHealth.UpdateHealthBar(true);

            // Add movement speed.
            SetSpeedBoost(player.GetComponent<CharacterMovement>().WalkSpeed + (totalSpeedBoostValue / 20));
        }


        //----------------------------------------------------------------
        [Button]
        private void SetSpeedBoost(float amount)
        {
            var player = LevelManager.Instance.Players[0];
            player.GetComponent<CharacterMovement>().WalkSpeed = amount;
            player.GetComponent<CharacterMovement>().MovementSpeed = amount;
        }
    }
}
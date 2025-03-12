using UnityEngine;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;

namespace JayMountains
{
    public class LootboxOnDeath : MonoBehaviour
    {
        [ReadOnly, ShowInInspector] private Health health;
        [ReadOnly, ShowInInspector] private Lootbox[] lootboxes;

        private void Awake()
        {
            health = this.transform.GetComponent<Health>();
            if (health == null)
                Debug.LogError("Health component not found on object.");
        }

        private void Start()
        {
            lootboxes = this.transform.GetComponents<Lootbox>();
            if (lootboxes.Length == 0)
                Debug.LogError("Lootbox component not found on object.");
        }

        private void OnEnable() => health.OnDeath += AddLootboxItemsToInventory;

        private void OnDisable() => health.OnDeath -= AddLootboxItemsToInventory;

        private void AddLootboxItemsToInventory()
        {
            for (int i = 0; i < lootboxes.Length; i++)
            {
                lootboxes[i].GiveLootboxItems();
            }

            WalletInventoryManager.Instance.SaveInventory();
            BagInventoryManager.Instance.SaveInventory();
        }
    }
}
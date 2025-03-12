using System;
using UnityEngine;
using MoreMountains.InventoryEngine;

namespace JayMountains
{
    [CreateAssetMenu(fileName = "EscapeRopeItem", menuName = "JayMountains/InventoryEngine/EscapeRopeItem", order = 1)]
    [Serializable]
    public class EscapeRopeItem : BagItem
    {
        public override bool Use(string playerID)
        {
            base.Use(playerID);
            ScenesManager.Instance.ProceedToMainMenu();
            Debug.Log("Left the cave.");
            return true;
        }
    }
}
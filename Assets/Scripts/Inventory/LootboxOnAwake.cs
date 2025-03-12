using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JayMountains
{
    public class LootboxOnAwake : Lootbox
    {
        private void Awake()
        {
            GiveLootboxItems();
        }
    }
}
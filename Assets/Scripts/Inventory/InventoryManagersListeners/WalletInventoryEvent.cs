using UnityEngine;
using MoreMountains.Tools;

public enum WalletInventoryEventTypes
{
    AddFast,
    RemoveFast,
}

public struct WalletInventoryEvent
{
    public WalletInventoryEventTypes EventType;
    public string CurrencyItemID;

    public WalletInventoryEvent(WalletInventoryEventTypes eventType, string currencyItemID)
    {
        EventType = eventType;
        CurrencyItemID = currencyItemID;
    }

    static WalletInventoryEvent e;
    public static void Trigger(WalletInventoryEventTypes eventType, string currencyItemID)
    {
        e.EventType = eventType;
        e.CurrencyItemID = currencyItemID;
        MMEventManager.TriggerEvent(e);
    }
}
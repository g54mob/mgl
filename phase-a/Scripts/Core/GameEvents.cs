using System;

/// <summary>
/// Central event bus for decoupled communication between systems.
/// Subscribe/unsubscribe via static events. No direct references needed.
/// </summary>
public static class GameEvents
{
    /// <summary>Fired when the player's money amount changes. Parameter is new total.</summary>
    public static event Action<float> OnMoneyChanged;

    /// <summary>Fired when an item is purchased from the shop.</summary>
    public static event Action OnItemPurchased;

    /// <summary>Fired when a shop item becomes unlocked.</summary>
    public static event Action<ShopItem> OnShopItemUnlocked;

    /// <summary>Fired when any menu is opened or closed. Parameter is true if any menu is open.</summary>
    public static event Action<bool> OnMenuStateChanged;

    /// <summary>Fired when something requests the shop UI to toggle open/closed.</summary>
    public static event Action OnToggleShopRequested;

    /// <summary>Broadcasts a money change to all listeners.</summary>
    public static void RaiseMoneyChanged(float newAmount)
    {
        if (OnMoneyChanged != null)
        {
            OnMoneyChanged.Invoke(newAmount);
        }
    }

    /// <summary>Broadcasts that an item was purchased.</summary>
    public static void RaiseItemPurchased()
    {
        if (OnItemPurchased != null)
        {
            OnItemPurchased.Invoke();
        }
    }

    /// <summary>Broadcasts that a shop item was unlocked.</summary>
    public static void RaiseShopItemUnlocked(ShopItem item)
    {
        if (OnShopItemUnlocked != null)
        {
            OnShopItemUnlocked.Invoke(item);
        }
    }

    /// <summary>Broadcasts a menu open/close state change.</summary>
    public static void RaiseMenuStateChanged(bool anyMenuOpen)
    {
        if (OnMenuStateChanged != null)
        {
            OnMenuStateChanged.Invoke(anyMenuOpen);
        }
    }

    /// <summary>Broadcasts a request to toggle the shop UI.</summary>
    public static void RaiseToggleShopRequested()
    {
        if (OnToggleShopRequested != null)
        {
            OnToggleShopRequested.Invoke();
        }
    }
}

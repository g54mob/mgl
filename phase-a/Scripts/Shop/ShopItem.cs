using System;

/// <summary>
/// Runtime wrapper around a ShopItemDefinition.
/// Tracks lock state and purchase count during gameplay.
/// </summary>
[Serializable]
public class ShopItem
{
    public SO_ShopItemDefinition Definition;
    public bool IsLocked;
    private int _timesPurchased;

    /// <summary>Creates a runtime shop item from a definition, applying default lock state.</summary>
    public ShopItem(SO_ShopItemDefinition definition)
    {
        Definition = definition;
        IsLocked = definition.IsLockedByDefault;
        _timesPurchased = 0;
    }

    /// <summary>Returns the price of this item from its definition.</summary>
    public int GetPrice()
    {
        return Definition.Price;
    }

    /// <summary>Returns the display name from the definition.</summary>
    public string GetName()
    {
        return Definition.GetName();
    }

    /// <summary>Returns the description from the definition.</summary>
    public string GetDescription()
    {
        return Definition.GetDescription();
    }

    /// <summary>Returns how many times this item has been purchased this session.</summary>
    public int GetTimesPurchased()
    {
        return _timesPurchased;
    }

    /// <summary>Increments the purchase counter by the given amount.</summary>
    public void AddPurchase(int quantity)
    {
        _timesPurchased += quantity;
    }

    /// <summary>Returns true if the item was locked by default but is now unlocked and never purchased.</summary>
    public bool IsNewlyUnlocked()
    {
        return !IsLocked && _timesPurchased == 0 && Definition.IsLockedByDefault;
    }
}

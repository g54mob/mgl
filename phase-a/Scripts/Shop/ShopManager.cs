using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that manages all shop categories, items, and their runtime state.
/// Initializes ShopItem wrappers from ShopCategory ScriptableObjects on Start.
/// </summary>
[DefaultExecutionOrder(-100)]
public class ShopManager : Singleton<ShopManager>
{
    [SerializeField]
    private List<ShopCategory> _allShopCategories = new List<ShopCategory>();

    private HashSet<ShopItemDefinition> _allDefinitions = new HashSet<ShopItemDefinition>();

    /// <summary>All runtime ShopItem instances across every category.</summary>
    public List<ShopItem> AllShopItems { get; private set; }

    /// <summary>Builds runtime ShopItem wrappers from all category definitions.</summary>
    private void Start()
    {
        AllShopItems = new List<ShopItem>();

        for (int i = 0; i < _allShopCategories.Count; i++)
        {
            ShopCategory category = _allShopCategories[i];
            category.ShopItems = new List<ShopItem>();

            for (int j = 0; j < category.ShopItemDefinitions.Count; j++)
            {
                ShopItemDefinition def = category.ShopItemDefinitions[j];
                if (def == null)
                {
                    continue;
                }

                ShopItem existing = FindItemByDefinition(def);
                if (existing != null)
                {
                    category.ShopItems.Add(existing);
                }
                else
                {
                    ShopItem newItem = new ShopItem(def);
                    category.ShopItems.Add(newItem);
                    AllShopItems.Add(newItem);
                }

                _allDefinitions.Add(def);
            }
        }
    }

    /// <summary>Returns all available shop categories.</summary>
    public List<ShopCategory> GetAvailableCategories()
    {
        return _allShopCategories;
    }

    /// <summary>Finds a runtime ShopItem by its definition, or null if not found.</summary>
    public ShopItem FindItemByDefinition(ShopItemDefinition definition)
    {
        if (AllShopItems == null)
        {
            return null;
        }
        for (int i = 0; i < AllShopItems.Count; i++)
        {
            if (AllShopItems[i].Definition == definition)
            {
                return AllShopItems[i];
            }
        }
        return null;
    }

    /// <summary>Unlocks a shop item by its definition if it is currently locked.</summary>
    public void UnlockShopItem(ShopItemDefinition definition)
    {
        ShopItem item = FindItemByDefinition(definition);
        if (item != null && item.IsLocked)
        {
            item.IsLocked = false;
            GameEvents.RaiseShopItemUnlocked(item);
        }
    }

    /// <summary>Unlocks all shop items in every category.</summary>
    public void UnlockAllShopItems()
    {
        if (AllShopItems == null)
        {
            return;
        }
        for (int i = 0; i < AllShopItems.Count; i++)
        {
            AllShopItems[i].IsLocked = false;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject grouping shop items into a browsable category tab.
/// Create via Assets > Create > Shop > ShopCategory.
/// </summary>
[CreateAssetMenu(fileName = "New ShopCategory", menuName = "Shop/ShopCategory")]
public class SO_ShopCategory : ScriptableObject
{
    public string CategoryName;

    public List<SO_ShopItemDefinition> ShopItemDefinitions = new List<SO_ShopItemDefinition>();

    [NonSerialized]
    public List<ShopItem> ShopItems = new List<ShopItem>();

    public bool HideIfAllLocked;

    /// <summary>Returns true if any item in this category is newly unlocked and unpurchased.</summary>
    public bool ContainsNewItems()
    {
        for (int i = 0; i < ShopItems.Count; i++)
        {
            if (ShopItems[i].IsNewlyUnlocked())
            {
                return true;
            }
        }
        return false;
    }
}

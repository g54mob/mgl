using UnityEngine;

/// <summary>
/// ScriptableObject defining a purchasable shop item.
/// Create via Assets > Create > Shop > ShopItemDefinition.
/// </summary>
[CreateAssetMenu(fileName = "New ShopItemDefinition", menuName = "Shop/ShopItemDefinition")]
public class SO_ShopItemDefinition : ScriptableObject
{
    public string Name;

    [TextArea]
    public string Description;

    public int Price;

    public Sprite Icon;

    public GameObject PrefabToSpawn;

    public bool IsLockedByDefault;

    public int MaxStackSize = 10;

    /// <summary>Returns the display name for this item.</summary>
    public string GetName()
    {
        return Name;
    }

    /// <summary>Returns the description text for this item.</summary>
    public string GetDescription()
    {
        return Description;
    }

    /// <summary>Returns the icon sprite for this item.</summary>
    public Sprite GetIcon()
    {
        return Icon;
    }
}

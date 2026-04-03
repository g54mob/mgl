using UnityEngine;

/// <summary>
/// Singleton managing global UI state: which menus are open, cursor visibility, etc.
/// All UI panels register themselves here so other systems can query menu state.
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [Header("UI Panel References")]
    public ShopUI ShopUI;

    public InteractionWheelUI InteractionWheelUI;

    [Header("HUD")]
    [SerializeField]
    private GameObject _hudObject;

    [SerializeField]
    private GameObject _backgroundBlur;

    /// <summary>Returns true if any menu panel is currently active.</summary>
    public bool IsInAnyMenu()
    {
        if (ShopUI != null && ShopUI.gameObject.activeSelf)
        {
            return true;
        }
        if (InteractionWheelUI != null && InteractionWheelUI.gameObject.activeSelf)
        {
            return true;
        }
        return false;
    }

    /// <summary>Returns true if the shop UI is currently open.</summary>
    public bool IsInShop()
    {
        return ShopUI != null && ShopUI.gameObject.activeSelf;
    }

    /// <summary>Enables or disables the background blur overlay.</summary>
    public void SetBackgroundBlur(bool enabled)
    {
        if (_backgroundBlur != null)
        {
            _backgroundBlur.SetActive(enabled);
        }
    }

    /// <summary>Shows or hides the HUD elements.</summary>
    public void SetHudVisible(bool visible)
    {
        if (_hudObject != null)
        {
            _hudObject.SetActive(visible);
        }
    }

    /// <summary>Updates blur and HUD state based on whether any menu is open.</summary>
    private void LateUpdate()
    {
        bool anyMenu = IsInAnyMenu();
        SetBackgroundBlur(anyMenu);
    }
}

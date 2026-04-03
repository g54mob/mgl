using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI element representing a single item entry in the shopping cart.
/// Supports quantity editing, increment/decrement, and removal.
/// </summary>
public class ShopCartItemButton : MonoBehaviour
{
    [SerializeField]
    private Text _itemNameText;

    [SerializeField]
    private Text _itemPriceText;

    [SerializeField]
    private Image _itemIcon;

    [SerializeField]
    private InputField _quantityInputField;

    private ShopUI _shopUI;
    private int _quantity = 1;

    /// <summary>The runtime ShopItem this cart entry represents.</summary>
    public ShopItem ShopItem { get; private set; }

    /// <summary>Returns the current quantity in this cart entry.</summary>
    public int GetQuantity()
    {
        return _quantity;
    }

    /// <summary>Configures this cart entry with item data, shop reference, and initial quantity.</summary>
    public void Initialize(ShopItem shopItem, ShopUI shopUI, int quantity)
    {
        ShopItem = shopItem;
        _shopUI = shopUI;
        _quantity = quantity;

        if (_itemIcon != null && shopItem.Definition.GetIcon() != null)
        {
            _itemIcon.sprite = shopItem.Definition.GetIcon();
            _itemIcon.enabled = true;
        }
        else if (_itemIcon != null)
        {
            _itemIcon.enabled = false;
        }

        UpdateUI();
    }

    /// <summary>Subscribes to the quantity input field's submit event.</summary>
    private void OnEnable()
    {
        if (_quantityInputField != null)
        {
            _quantityInputField.onEndEdit.AddListener(OnInputSubmitted);
        }
        UpdateUI();
    }

    /// <summary>Unsubscribes from the quantity input field's submit event.</summary>
    private void OnDisable()
    {
        if (_quantityInputField != null)
        {
            _quantityInputField.onEndEdit.RemoveListener(OnInputSubmitted);
        }
    }

    /// <summary>Parses user-typed quantity and applies it.</summary>
    private void OnInputSubmitted(string input)
    {
        int result;
        if (int.TryParse(input, out result))
        {
            ChangeQuantity(result);
        }
    }

    /// <summary>Sets a new quantity, clamped to affordable range. Removes entry if zero or below.</summary>
    public void ChangeQuantity(int quantity)
    {
        int maxAffordable = _quantity;

        if (Singleton<EconomyManager>.Instance != null && _shopUI != null && ShopItem != null)
        {
            float available = Singleton<EconomyManager>.Instance.Money
                - _shopUI.TotalCartPrice
                + (ShopItem.GetPrice() * _quantity);
            maxAffordable = Mathf.FloorToInt(available / Mathf.Max(1, ShopItem.GetPrice()));
            maxAffordable = Mathf.Max(0, maxAffordable);
        }

        int maxStack = ShopItem.Definition.MaxStackSize;
        int clamped = Mathf.Clamp(quantity, 0, Mathf.Min(maxAffordable, maxStack));
        _quantity = clamped;

        if (_quantity > 0)
        {
            UpdateUI();
        }
        else
        {
            if (_shopUI != null)
            {
                _shopUI.RemoveFromCart(this);
            }
        }
    }

    /// <summary>Adds (or subtracts if negative) to the current quantity.</summary>
    public void AddQuantity(int amount)
    {
        ChangeQuantity(_quantity + amount);
    }

    /// <summary>Removes this entry from the cart entirely.</summary>
    public void RemoveFromCart()
    {
        ChangeQuantity(0);
    }

    /// <summary>Refreshes name, price, and quantity display text.</summary>
    public void UpdateUI()
    {
        if (_shopUI == null || ShopItem == null)
        {
            return;
        }

        float totalPrice = ShopItem.GetPrice() * _quantity;

        if (_itemPriceText != null)
        {
            _itemPriceText.text = string.Format("${0}", totalPrice);
        }

        if (_itemNameText != null)
        {
            _itemNameText.text = ShopItem.GetName();
        }

        if (_quantityInputField != null)
        {
            _quantityInputField.text = _quantity.ToString();
        }
    }
}

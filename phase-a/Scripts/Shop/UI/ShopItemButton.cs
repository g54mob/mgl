using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI button for a single shop item in the item listing panel.
/// Shows name, description, price, icon, and an "Add to Cart" button.
/// </summary>
public class ShopItemButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private Button _addButton;

    [SerializeField]
    private Image _buttonImage;

    [SerializeField]
    private Text _buttonText;

    [SerializeField]
    private Text _itemNameText;

    [SerializeField]
    private Text _itemDescriptionText;

    [SerializeField]
    private Text _itemPriceText;

    [SerializeField]
    private Image _itemIcon;

    [Header("Colors")]
    [SerializeField]
    private Color _canBuyButtonColor = new Color(0.2f, 0.7f, 0.3f, 1f);

    [SerializeField]
    private Color _cantBuyButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    [SerializeField]
    private Color _canBuyTextColor = Color.white;

    [SerializeField]
    private Color _cantBuyTextColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    private ShopUI _shopUI;
    private int _quantity = 1;

    /// <summary>The runtime ShopItem this button represents.</summary>
    public ShopItem ShopItem { get; private set; }

    /// <summary>Configures this button with item data and registers the click handler.</summary>
    public void Initialize(ShopItem shopItem, ShopUI shopUI)
    {
        ShopItem = shopItem;
        _shopUI = shopUI;

        if (_itemDescriptionText != null)
        {
            _itemDescriptionText.text = shopItem.GetDescription();
        }

        if (_itemIcon != null && shopItem.Definition.GetIcon() != null)
        {
            _itemIcon.sprite = shopItem.Definition.GetIcon();
            _itemIcon.enabled = true;
        }
        else if (_itemIcon != null)
        {
            _itemIcon.enabled = false;
        }

        if (_addButton != null)
        {
            _addButton.onClick.AddListener(OnButtonClick);
        }

        UpdateUI();
    }

    /// <summary>Adds this item to the shop cart when clicked.</summary>
    private void OnButtonClick()
    {
        if (_shopUI != null)
        {
            _shopUI.AddToCart(ShopItem, _quantity);
        }
        UpdateUI();
    }

    /// <summary>Changes the purchase quantity shown on this button.</summary>
    public void ChangeQuantity(int quantity)
    {
        _quantity = quantity;
        UpdateUI();
    }

    /// <summary>Refreshes all UI elements based on current affordability and lock state.</summary>
    public void UpdateUI()
    {
        if (_shopUI == null || ShopItem == null)
        {
            return;
        }

        float itemCost = ShopItem.GetPrice() * _quantity;
        float availableMoney = 0f;

        if (Singleton<EconomyManager>.Instance != null)
        {
            availableMoney = Singleton<EconomyManager>.Instance.Money - _shopUI.TotalCartPrice;
        }

        bool canAfford = availableMoney >= itemCost && !ShopItem.IsLocked;

        if (_addButton != null)
        {
            _addButton.interactable = canAfford;
        }

        if (_itemNameText != null)
        {
            _itemNameText.text = ShopItem.GetName();
        }

        if (_itemPriceText != null)
        {
            if (_quantity == 1)
            {
                _itemPriceText.text = string.Format("${0}", itemCost);
            }
            else
            {
                _itemPriceText.text = string.Format("(x{0}) ${1}", _quantity, itemCost);
            }
        }

        if (_buttonText != null)
        {
            if (ShopItem.IsLocked)
            {
                _buttonText.text = "Locked";
                _buttonText.color = _cantBuyTextColor;
            }
            else if (!canAfford)
            {
                _buttonText.text = "Can't Afford";
                _buttonText.color = _cantBuyTextColor;
            }
            else
            {
                _buttonText.text = "Add to Cart";
                _buttonText.color = _canBuyTextColor;
            }
        }

        if (_buttonImage != null)
        {
            _buttonImage.color = canAfford ? _canBuyButtonColor : _cantBuyButtonColor;
        }
    }
}

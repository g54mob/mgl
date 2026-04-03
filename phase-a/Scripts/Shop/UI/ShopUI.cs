using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main shop UI controller. Manages category tabs, item listing, cart, and purchasing.
/// Attach to the root shop panel GameObject.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField]
    private Transform _categoryButtonsContainer;

    [SerializeField]
    private Transform _itemListContainer;

    [SerializeField]
    private Transform _cartListContainer;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject _categoryButtonPrefab;

    [SerializeField]
    private GameObject _shopItemButtonPrefab;

    [SerializeField]
    private GameObject _cartItemButtonPrefab;

    [Header("UI Elements")]
    [SerializeField]
    private Text _moneyText;

    [SerializeField]
    private Text _cartTotalText;

    [SerializeField]
    private Button _purchaseButton;

    [SerializeField]
    private Text _purchaseButtonText;

    [Header("Colors")]
    public Color CanAffordColor = Color.green;
    public Color CantAffordColor = Color.red;

    private SO_ShopCategory _selectedCategory;
    private List<ShopCategoryButton> _categoryButtons = new List<ShopCategoryButton>();
    private List<ShopCartItemButton> _cartItems = new List<ShopCartItemButton>();

    /// <summary>Gets the computed total price of all items in the cart.</summary>
    public int TotalCartPrice { get; private set; }

    /// <summary>Sets up categories on enable and subscribes to unlock events.</summary>
    private void OnEnable()
    {
        if (_selectedCategory == null)
        {
            SetupCategories();
        }
        RefreshCurrency();
        GameEvents.OnShopItemUnlocked += RefreshOnItemUnlocked;
        GameEvents.RaiseMenuStateChanged(true);
    }

    /// <summary>Clears cart on first start then subscribes to the shop toggle event.</summary>
    private void Start()
    {
        ClearCart();
        GameEvents.OnToggleShopRequested += ToggleShop;
    }

    /// <summary>Unsubscribes from events on disable.</summary>
    private void OnDisable()
    {
        GameEvents.OnShopItemUnlocked -= RefreshOnItemUnlocked;
        GameEvents.RaiseMenuStateChanged(false);
    }

    /// <summary>Unsubscribes from the toggle event when destroyed.</summary>
    private void OnDestroy()
    {
        GameEvents.OnToggleShopRequested -= ToggleShop;
    }

    /// <summary>Toggles this panel on or off in response to the global event.</summary>
    private void ToggleShop()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    /// <summary>Refreshes currency display every frame and handles close input.</summary>
    private void Update()
    {
        RefreshCurrency();

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>Refreshes the item list when a shop item is unlocked.</summary>
    private void RefreshOnItemUnlocked(ShopItem item)
    {
        RepopulateItemList();
    }

    /// <summary>Creates category tab buttons from ShopManager data.</summary>
    public void SetupCategories()
    {
        for (int i = 0; i < _categoryButtons.Count; i++)
        {
            if (_categoryButtons[i] != null)
            {
                _categoryButtons[i].OnPressed -= OpenCategory;
                Object.Destroy(_categoryButtons[i].gameObject);
            }
        }
        _categoryButtons.Clear();

        ClearChildren(_categoryButtonsContainer);

        if (Singleton<ShopManager>.Instance == null)
        {
            return;
        }

        List<SO_ShopCategory> categories = Singleton<ShopManager>.Instance.GetAvailableCategories();
        for (int i = 0; i < categories.Count; i++)
        {
            SO_ShopCategory category = categories[i];
            GameObject buttonObj = Object.Instantiate(_categoryButtonPrefab, _categoryButtonsContainer);
            ShopCategoryButton categoryBtn = buttonObj.GetComponent<ShopCategoryButton>();
            if (categoryBtn != null)
            {
                categoryBtn.Initialize(category);
                categoryBtn.OnPressed += OpenCategory;
                _categoryButtons.Add(categoryBtn);

                if (category.HideIfAllLocked && AreAllItemsLocked(category))
                {
                    buttonObj.SetActive(false);
                }
            }
        }

        if (_categoryButtons.Count > 0)
        {
            OpenCategory(_categoryButtons[0].Category);
        }
    }

    /// <summary>Switches the displayed items to the selected category.</summary>
    public void OpenCategory(SO_ShopCategory category)
    {
        _selectedCategory = category;

        for (int i = 0; i < _categoryButtons.Count; i++)
        {
            _categoryButtons[i].SetSelected(_categoryButtons[i].Category == category);
        }

        RepopulateItemList();
    }

    /// <summary>Destroys and recreates item buttons for the selected category.</summary>
    private void RepopulateItemList()
    {
        if (_selectedCategory == null)
        {
            return;
        }

        ClearChildren(_itemListContainer);

        List<ShopItem> items = _selectedCategory.ShopItems;
        for (int i = 0; i < items.Count; i++)
        {
            GameObject buttonObj = Object.Instantiate(_shopItemButtonPrefab, _itemListContainer);
            ShopItemButton itemBtn = buttonObj.GetComponent<ShopItemButton>();
            if (itemBtn != null)
            {
                itemBtn.Initialize(items[i], this);
            }
        }
    }

    /// <summary>Adds an item to the cart, merging with existing entries of the same item.</summary>
    public void AddToCart(ShopItem item, int quantity)
    {
        ShopCartItemButton existing = FindCartItem(item);
        if (existing != null)
        {
            existing.ChangeQuantity(existing.GetQuantity() + quantity);
            return;
        }

        GameObject cartObj = Object.Instantiate(_cartItemButtonPrefab, _cartListContainer);
        ShopCartItemButton cartBtn = cartObj.GetComponent<ShopCartItemButton>();
        if (cartBtn != null)
        {
            cartBtn.Initialize(item, this, quantity);
            _cartItems.Add(cartBtn);
        }
    }

    /// <summary>Removes a cart entry button from the cart list and destroys it.</summary>
    public void RemoveFromCart(ShopCartItemButton button)
    {
        _cartItems.Remove(button);
        if (button != null)
        {
            Object.Destroy(button.gameObject);
        }
    }

    /// <summary>Returns true if the player has enough money and the cart is not empty.</summary>
    public bool CanAffordCart()
    {
        TotalCartPrice = 0;
        for (int i = 0; i < _cartItems.Count; i++)
        {
            TotalCartPrice += _cartItems[i].ShopItem.GetPrice() * _cartItems[i].GetQuantity();
        }

        if (Singleton<EconomyManager>.Instance == null)
        {
            return false;
        }

        return _cartItems.Count > 0 && Singleton<EconomyManager>.Instance.Money >= TotalCartPrice;
    }

    /// <summary>Purchases all cart items, spawns prefabs, deducts money, and clears the cart.</summary>
    public void PurchaseCart()
    {
        if (!CanAffordCart())
        {
            Debug.Log("Cannot afford cart contents.");
            return;
        }

        List<ShopCartItemButton> toProcess = new List<ShopCartItemButton>(_cartItems);
        for (int i = 0; i < toProcess.Count; i++)
        {
            ShopCartItemButton cartBtn = toProcess[i];
            ShopItem shopItem = cartBtn.ShopItem;
            int qty = cartBtn.GetQuantity();
            float cost = shopItem.GetPrice() * qty;

            if (TrySpawnItem(shopItem.Definition, qty))
            {
                Singleton<EconomyManager>.Instance.AddMoney(-cost);
                shopItem.AddPurchase(qty);
                _cartItems.Remove(cartBtn);
                Object.Destroy(cartBtn.gameObject);
                GameEvents.RaiseItemPurchased();
            }
        }

        RefreshCurrency();
    }

    /// <summary>Spawns the purchased item prefab at a random ShopSpawnPoint.</summary>
    private bool TrySpawnItem(SO_ShopItemDefinition definition, int quantity)
    {
        if (definition.PrefabToSpawn == null)
        {
            Debug.LogWarning("ShopItemDefinition '" + definition.GetName() + "' has no PrefabToSpawn assigned.");
            return false;
        }

        ShopSpawnPoint spawnPoint = ShopSpawnPoint.GetRandomSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("No ShopSpawnPoint in scene. Cannot spawn purchased item.");
            return false;
        }

        for (int i = 0; i < quantity; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.3f;
            offset.y = Mathf.Abs(offset.y);
            Object.Instantiate(
                definition.PrefabToSpawn,
                spawnPoint.transform.position + offset,
                spawnPoint.transform.rotation
            );
        }

        return true;
    }

    /// <summary>Updates the money display text, cart total, and purchase button state.</summary>
    private void RefreshCurrency()
    {
        if (Singleton<EconomyManager>.Instance != null && _moneyText != null)
        {
            _moneyText.text = EconomyManager.FormatMoney(Singleton<EconomyManager>.Instance.Money);
        }

        bool canAfford = CanAffordCart();

        if (_purchaseButton != null)
        {
            _purchaseButton.interactable = canAfford;
        }

        if (_cartTotalText != null)
        {
            _cartTotalText.text = _cartItems.Count > 0
                ? EconomyManager.FormatMoney(TotalCartPrice)
                : "$0.00";

            _cartTotalText.color = canAfford ? CanAffordColor : CantAffordColor;
        }

        RefreshItemButtons();
    }

    /// <summary>Updates every item button's affordability state.</summary>
    private void RefreshItemButtons()
    {
        if (_itemListContainer == null)
        {
            return;
        }
        for (int i = 0; i < _itemListContainer.childCount; i++)
        {
            ShopItemButton btn = _itemListContainer.GetChild(i).GetComponent<ShopItemButton>();
            if (btn != null)
            {
                btn.UpdateUI();
            }
        }
    }

    /// <summary>Destroys all items in the cart and clears the cart list.</summary>
    private void ClearCart()
    {
        ClearChildren(_cartListContainer);
        _cartItems.Clear();
    }

    /// <summary>Finds an existing cart entry for the given shop item, or null.</summary>
    private ShopCartItemButton FindCartItem(ShopItem item)
    {
        for (int i = 0; i < _cartItems.Count; i++)
        {
            if (_cartItems[i].ShopItem == item)
            {
                return _cartItems[i];
            }
        }
        return null;
    }

    /// <summary>Returns true if every item in the category is locked.</summary>
    private bool AreAllItemsLocked(SO_ShopCategory category)
    {
        for (int i = 0; i < category.ShopItems.Count; i++)
        {
            if (!category.ShopItems[i].IsLocked)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>Destroys all child GameObjects of a transform container.</summary>
    private void ClearChildren(Transform container)
    {
        if (container == null)
        {
            return;
        }
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(container.GetChild(i).gameObject);
        }
    }
}

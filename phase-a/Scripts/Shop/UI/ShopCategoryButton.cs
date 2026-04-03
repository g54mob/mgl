using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI button representing a shop category tab.
/// Changes color when selected and fires an event when pressed.
/// </summary>
public class ShopCategoryButton : MonoBehaviour
{
    [SerializeField]
    private Image _backgroundImage;

    [SerializeField]
    private Text _nameText;

    [SerializeField]
    private Color _selectedColor = new Color(0.3f, 0.6f, 1f, 1f);

    [SerializeField]
    private Color _normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    /// <summary>The category data this button represents.</summary>
    public SO_ShopCategory Category { get; private set; }

    /// <summary>Fired when this category button is clicked. Parameter is the category.</summary>
    public event Action<SO_ShopCategory> OnPressed;

    /// <summary>Configures this button with category data and sets the label text.</summary>
    public void Initialize(SO_ShopCategory category)
    {
        Category = category;
        if (_nameText != null)
        {
            _nameText.text = category.CategoryName;
        }
    }

    /// <summary>Sets the visual selected/deselected state via background color.</summary>
    public void SetSelected(bool selected)
    {
        if (_backgroundImage != null)
        {
            _backgroundImage.color = selected ? _selectedColor : _normalColor;
        }
    }

    /// <summary>Called by the Unity Button OnClick event to fire the OnPressed event.</summary>
    public void OnButtonPressed()
    {
        if (OnPressed != null)
        {
            OnPressed.Invoke(Category);
        }
    }
}

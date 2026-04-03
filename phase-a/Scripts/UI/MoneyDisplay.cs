using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD element that displays the player's current money.
/// Subscribes to GameEvents for decoupled updates.
/// </summary>
public class MoneyDisplay : MonoBehaviour
{
    [SerializeField]
    private Text _moneyText;

    /// <summary>Subscribes to the money changed event on enable.</summary>
    private void OnEnable()
    {
        GameEvents.OnMoneyChanged += HandleMoneyChanged;
        RefreshDisplay();
    }

    /// <summary>Unsubscribes from the money changed event on disable.</summary>
    private void OnDisable()
    {
        GameEvents.OnMoneyChanged -= HandleMoneyChanged;
    }

    /// <summary>Refreshes the display when money changes via GameEvents.</summary>
    private void HandleMoneyChanged(float newAmount)
    {
        RefreshDisplay();
    }

    /// <summary>Reads current money from EconomyManager and updates the text.</summary>
    private void RefreshDisplay()
    {
        if (_moneyText == null)
        {
            return;
        }

        if (Singleton<EconomyManager>.Instance != null)
        {
            _moneyText.text = EconomyManager.FormatMoney(Singleton<EconomyManager>.Instance.Money);
        }
        else
        {
            _moneyText.text = "$0.00";
        }
    }
}

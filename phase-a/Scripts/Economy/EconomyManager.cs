using System;
using UnityEngine;

/// <summary>
/// Singleton manager that owns the player's money.
/// All money operations go through this class. Fires events on changes.
/// </summary>
[DefaultExecutionOrder(-100)]
public class EconomyManager : Singleton<EconomyManager>
{
    [SerializeField]
    private float _money = 500f;

    /// <summary>Fired locally when money changes. Parameter is new total.</summary>
    public event Action<float> OnMoneyUpdated;

    /// <summary>Gets the current money amount.</summary>
    public float Money
    {
        get { return _money; }
        private set
        {
            _money = value;
            if (OnMoneyUpdated != null)
            {
                OnMoneyUpdated.Invoke(_money);
            }
            GameEvents.RaiseMoneyChanged(_money);
        }
    }

    /// <summary>Adds (or subtracts if negative) the given amount to money.</summary>
    public void AddMoney(float amount)
    {
        Money += amount;
    }

    /// <summary>Sets money to an exact amount.</summary>
    public void SetMoney(float amount)
    {
        Money = amount;
    }

    /// <summary>Returns true if the player can afford the given amount.</summary>
    public bool CanAfford(float amount)
    {
        return Money >= amount;
    }

    /// <summary>Formats a float as a dollar string like "$1,234.56".</summary>
    public static string FormatMoney(float amount)
    {
        return string.Format("${0:#,##0.00}", amount);
    }
}

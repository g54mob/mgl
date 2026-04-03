using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// In-world interactable object that opens the shop UI when the player interacts.
/// Place on a 3D object in the scene with a collider.
/// </summary>
public class ShopTerminal : MonoBehaviour, IInteractable
{
    [SerializeField]
    private List<Interaction> _interactions = new List<Interaction>();

    /// <summary>Returns false so single-interaction triggers directly without the wheel.</summary>
    public bool ShouldUseInteractionWheel()
    {
        return false;
    }

    /// <summary>Returns the display name for the interaction prompt.</summary>
    public string GetObjectName()
    {
        return "Shop Terminal";
    }

    /// <summary>Returns the list of interactions assigned in the inspector.</summary>
    public List<Interaction> GetInteractions()
    {
        return _interactions;
    }

    /// <summary>Fires a global event requesting the shop UI to toggle.</summary>
    public void Interact(Interaction selectedInteraction)
    {
        GameEvents.RaiseToggleShopRequested();
    }
}

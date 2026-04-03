using System.Collections.Generic;

/// <summary>
/// Interface for any world object the player can interact with.
/// Implemented by shop terminals, machines, tools, etc.
/// </summary>
public interface IInteractable
{
    /// <summary>Returns true if this object should show the radial interaction wheel.</summary>
    bool ShouldUseInteractionWheel();

    /// <summary>Returns the list of available interactions for this object.</summary>
    List<SO_Interaction> GetInteractions();

    /// <summary>Returns the display name shown in the interaction UI.</summary>
    string GetObjectName();

    /// <summary>Executes the chosen interaction on this object.</summary>
    void Interact(SO_Interaction selectedInteraction);
}

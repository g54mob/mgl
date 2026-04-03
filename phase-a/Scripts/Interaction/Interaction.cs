using UnityEngine;

/// <summary>
/// ScriptableObject defining a single interaction option (e.g. "Use", "Open", "Take").
/// Create via Assets > Create > Interactions > Interaction.
/// </summary>
[CreateAssetMenu(fileName = "New Interaction", menuName = "Interactions/Interaction")]
public class Interaction : ScriptableObject
{
    public string Name;

    [TextArea]
    public string Description;

    public Sprite Icon;
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Radial interaction menu that appears when an object has multiple interactions.
/// Dynamically spawns buttons for each available interaction option.
/// </summary>
public class InteractionWheelUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _interactionButtonPrefab;

    [SerializeField]
    private Transform _contentTransform;

    [SerializeField]
    private Text _objectNameText;

    [SerializeField]
    private KeyCode _closeKey = KeyCode.Escape;

    private List<GameObject> _interactionButtons = new List<GameObject>();
    private Dictionary<Button, IInteractable> _buttonMapping = new Dictionary<Button, IInteractable>();

    /// <summary>Creates buttons for each interaction on the given interactable.</summary>
    public void PopulateInteractionWheel(IInteractable interactable)
    {
        if (_objectNameText != null)
        {
            _objectNameText.text = interactable.GetObjectName();
        }

        List<Interaction> interactions = interactable.GetInteractions();
        if (interactions == null)
        {
            return;
        }

        for (int i = 0; i < interactions.Count; i++)
        {
            Interaction interaction = interactions[i];
            GameObject buttonObj = Object.Instantiate(_interactionButtonPrefab, _contentTransform);

            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = interaction.Name;
            }

            _interactionButtons.Add(buttonObj);

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                Interaction capturedInteraction = interaction;
                IInteractable capturedInteractable = interactable;
                button.onClick.AddListener(delegate
                {
                    SelectInteraction(capturedInteraction, capturedInteractable);
                });
                _buttonMapping[button] = interactable;
            }
        }
    }

    /// <summary>Opens the interaction wheel panel.</summary>
    public void OpenWheel()
    {
        gameObject.SetActive(true);
    }

    /// <summary>Closes the wheel and clears all spawned buttons.</summary>
    public void CloseWheel()
    {
        ClearInteractionWheel();
        gameObject.SetActive(false);
    }

    /// <summary>Checks for close key input each frame.</summary>
    private void Update()
    {
        if (Input.GetKeyDown(_closeKey) || Input.GetKeyDown(KeyCode.E))
        {
            CloseWheel();
        }
    }

    /// <summary>Executes the selected interaction and closes the wheel.</summary>
    private void SelectInteraction(Interaction selectedInteraction, IInteractable interactable)
    {
        if (interactable != null)
        {
            interactable.Interact(selectedInteraction);
        }
        CloseWheel();
    }

    /// <summary>Destroys all spawned buttons and clears internal collections.</summary>
    public void ClearInteractionWheel()
    {
        foreach (KeyValuePair<Button, IInteractable> pair in _buttonMapping)
        {
            if (pair.Key != null)
            {
                pair.Key.onClick.RemoveAllListeners();
            }
        }
        _buttonMapping.Clear();

        for (int i = 0; i < _interactionButtons.Count; i++)
        {
            if (_interactionButtons[i] != null)
            {
                Object.Destroy(_interactionButtons[i]);
            }
        }
        _interactionButtons.Clear();
    }
}

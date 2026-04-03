using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player-to-world interaction via raycast from the camera center.
/// Detects IInteractable objects and triggers interactions or the wheel UI.
/// Attach to the player GameObject.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    [SerializeField]
    private Camera _playerCamera;

    [SerializeField]
    private float _interactRange = 2.5f;

    [SerializeField]
    private LayerMask _interactLayerMask;

    [SerializeField]
    private InteractionWheelUI _interactionWheelUI;

    [SerializeField]
    private KeyCode _interactKey = KeyCode.E;

    /// <summary>Checks for interact input each frame and attempts interaction.</summary>
    private void Update()
    {
        if (Input.GetKeyDown(_interactKey))
        {
            TryInteract();
        }
    }

    /// <summary>Raycasts from camera and triggers interaction on the first IInteractable hit.</summary>
    private void TryInteract()
    {
        if (Singleton<UIManager>.Instance != null && Singleton<UIManager>.Instance.IsInAnyMenu())
        {
            return;
        }

        RaycastHit hit;
        if (!Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out hit, _interactRange, _interactLayerMask))
        {
            return;
        }

        if (_interactionWheelUI != null)
        {
            _interactionWheelUI.ClearInteractionWheel();
        }

        List<IInteractable> interactables = new List<IInteractable>();
        interactables.AddRange(hit.collider.GetComponentsInParent<IInteractable>());

        if (interactables.Count == 0)
        {
            return;
        }

        if (interactables.Count == 1 && !interactables[0].ShouldUseInteractionWheel())
        {
            List<Interaction> interactions = interactables[0].GetInteractions();
            if (interactions != null && interactions.Count > 0)
            {
                interactables[0].Interact(interactions[0]);
            }
        }
        else
        {
            if (_interactionWheelUI == null)
            {
                return;
            }
            _interactionWheelUI.gameObject.SetActive(true);
            for (int i = 0; i < interactables.Count; i++)
            {
                _interactionWheelUI.PopulateInteractionWheel(interactables[i]);
            }
        }
    }

    /// <summary>Returns the object the player is currently looking at, or null.</summary>
    public GameObject GetLookedAtObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out hit, _interactRange, _interactLayerMask))
        {
            return hit.collider.gameObject;
        }
        return null;
    }
}

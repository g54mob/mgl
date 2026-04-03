using UnityEngine;

/// <summary>
/// Minimal first-person player controller for testing interaction and shop systems.
/// Provides basic WASD movement, mouse look, and cursor management.
/// Attach to a GameObject with a CharacterController component.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float _walkSpeed = 4f;

    [SerializeField]
    private float _gravity = -9.81f;

    [Header("Look")]
    [SerializeField]
    private float _mouseSensitivity = 2f;

    [SerializeField]
    private Camera _playerCamera;

    private CharacterController _characterController;
    private float _xRotation;
    private Vector3 _velocity;
    private bool _anyMenuOpen;

    /// <summary>Caches CharacterController, locks cursor, and subscribes to menu events.</summary>
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameEvents.OnMenuStateChanged += HandleMenuStateChanged;
    }

    /// <summary>Unsubscribes from menu events on destroy.</summary>
    private void OnDestroy()
    {
        GameEvents.OnMenuStateChanged -= HandleMenuStateChanged;
    }

    /// <summary>Caches menu state from the global event instead of polling UIManager.</summary>
    private void HandleMenuStateChanged(bool anyMenuOpen)
    {
        _anyMenuOpen = anyMenuOpen;
    }

    /// <summary>Handles movement, look, gravity, and cursor state each frame.</summary>
    private void Update()
    {
        HandleCursorState();

        if (!Cursor.visible)
        {
            HandleLook();
        }

        HandleMovement();
    }

    /// <summary>Rotates camera vertically and player horizontally based on mouse input.</summary>
    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -88f, 88f);

        if (_playerCamera != null)
        {
            _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    /// <summary>Moves the player using WASD input and applies gravity.</summary>
    private void HandleMovement()
    {
        bool isGrounded = _characterController.isGrounded;

        if (isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        _characterController.Move(move * _walkSpeed * Time.deltaTime);

        _velocity.y += _gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }

    /// <summary>Locks or unlocks cursor based on cached menu state from GameEvents.</summary>
    private void HandleCursorState()
    {
        if (_anyMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

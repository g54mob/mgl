using System.Collections;
using UnityEngine;

/// <summary>
/// Lowers the player into the mine on scene start via code-driven movement.
/// Uses Perlin noise for shake, roof collider during descent, and landing particle.
/// Attach to the elevator platform GameObject.
/// </summary>
[DefaultExecutionOrder(1000)]
public class StartingElevator : MonoBehaviour
{
    [Header("Elevator Settings")]
    public float StartingHeight = 15f;
    public float EndHeight = 0f;

    [Header("References")]
    public Transform PlayerTeleportPosition;
    public GameObject RoofCollider;
    public GameObject LandingParticle;

    [Header("Shake")]
    [SerializeField]
    private float _shakeFrequency = 20f;

    [SerializeField]
    private float _maxShakeAmplitude = 0.02f;

    private bool _isLowering;
    private bool _hasPlayedLandingParticle;

    /// <summary>Subscribes to pause events once after all Awake() calls are complete.</summary>
    private void Start()
    {
        GameEvents.OnGamePaused += HandleGamePaused;
        GameEvents.OnGameUnpaused += HandleGameUnpaused;
    }

    /// <summary>Initializes elevator state and starts descent on scene load.</summary>
    private void OnEnable()
    {
        if (LandingParticle != null)
        {
            LandingParticle.SetActive(false);
        }

        TeleportPlayerAndLowerElevator();
    }

    /// <summary>Unsubscribes from pause events on permanent destruction.</summary>
    private void OnDestroy()
    {
        GameEvents.OnGamePaused -= HandleGamePaused;
        GameEvents.OnGameUnpaused -= HandleGameUnpaused;
    }

    /// <summary>Moves the elevator downward each frame with Perlin noise shake.</summary>
    private void Update()
    {
        if (!_isLowering)
        {
            return;
        }

        Vector3 localPos = transform.localPosition;

        float distanceRemaining = Mathf.Max(0f, localPos.y - EndHeight);
        float normalizedProgress = Mathf.InverseLerp(0.15f, 0f, distanceRemaining);

        float speed = Mathf.Lerp(1.25f, 0.1f, Mathf.Clamp01(normalizedProgress));
        float shakeAmount = Mathf.Lerp(_maxShakeAmplitude, 0f, Mathf.Clamp01(normalizedProgress));

        localPos.y -= speed * Time.deltaTime;

        localPos.x = Mathf.PerlinNoise(Time.time * _shakeFrequency, 0f) * shakeAmount - shakeAmount / 2f;
        localPos.z = Mathf.PerlinNoise(0f, Time.time * _shakeFrequency) * shakeAmount - shakeAmount / 2f;

        if (!_hasPlayedLandingParticle && localPos.y <= EndHeight + 1f)
        {
            _hasPlayedLandingParticle = true;
            if (LandingParticle != null)
            {
                LandingParticle.SetActive(true);
            }
        }

        if (localPos.y <= EndHeight + 0.001f)
        {
            localPos.y = EndHeight;
            localPos.x = 0f;
            localPos.z = 0f;
            if (RoofCollider != null)
            {
                RoofCollider.SetActive(false);
            }
            _isLowering = false;
            GameEvents.RaiseElevatorLanded();
        }

        transform.localPosition = localPos;
    }

    /// <summary>Pauses elevator sound when game is paused (placeholder for future sound).</summary>
    private void HandleGamePaused()
    {
        // Sound pause will be handled in Phase H
    }

    /// <summary>Resumes elevator sound when game is unpaused (placeholder for future sound).</summary>
    private void HandleGameUnpaused()
    {
        // Sound unpause will be handled in Phase H
    }

    /// <summary>Teleports the player to the elevator and starts lowering.</summary>
    public void TeleportPlayerAndLowerElevator()
    {
        LowerTheElevator();

        if (PlayerTeleportPosition != null)
        {
            SimplePlayerController player = Object.FindObjectOfType<SimplePlayerController>();
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    player.transform.position = PlayerTeleportPosition.position;
                    cc.enabled = true;
                }
                else
                {
                    player.transform.position = PlayerTeleportPosition.position;
                }
            }
        }
    }

    /// <summary>Resets elevator to starting height and begins the descent.</summary>
    public void LowerTheElevator()
    {
        if (LandingParticle != null)
        {
            LandingParticle.SetActive(false);
        }
        _hasPlayedLandingParticle = false;

        if (RoofCollider != null)
        {
            RoofCollider.SetActive(true);
        }

        transform.localPosition = new Vector3(0f, StartingHeight, 0f);
        _isLowering = true;
    }
}

using UnityEngine;

/// <summary>
/// Applies subtle Perlin noise sway to a camera plus one-shot view punch effects.
/// Attach to the player camera or any camera that should have ambient shake.
/// </summary>
public class CameraShaker : MonoBehaviour
{
    [Header("Ambient Sway")]
    public float positionAmplitude = 0.05f;
    public float rotationAmplitude = 0.2f;
    public float positionFrequency = 0.2f;
    public float rotationFrequency = 0.1f;

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private float _timeOffset;

    private Vector3 _currentPunchRotation = Vector3.zero;
    private Vector3 _targetPunchRotation = Vector3.zero;
    private Vector3 _punchVelocity = Vector3.zero;
    private float _punchSmoothTime = 0.2f;
    private float _punchRecoverSpeed = 4f;

    /// <summary>Caches initial transform and randomizes time offset for varied noise.</summary>
    private void Start()
    {
        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;
        _timeOffset = Random.value * 100f;
    }

    /// <summary>Applies Perlin noise position/rotation sway and decays any active view punch.</summary>
    private void Update()
    {
        float t = Time.time + _timeOffset;

        Vector3 posNoise = new Vector3(
            (Mathf.PerlinNoise(t * positionFrequency, 0f) - 0.5f) * 2f,
            (Mathf.PerlinNoise(t * positionFrequency, 1f) - 0.5f) * 2f,
            (Mathf.PerlinNoise(t * positionFrequency, 2f) - 0.5f) * 2f
        ) * positionAmplitude;

        Vector3 rotNoise = new Vector3(
            (Mathf.PerlinNoise(t * rotationFrequency, 3f) - 0.5f) * 2f,
            (Mathf.PerlinNoise(t * rotationFrequency, 4f) - 0.5f) * 2f,
            (Mathf.PerlinNoise(t * rotationFrequency, 5f) - 0.5f) * 2f
        ) * rotationAmplitude;

        _currentPunchRotation = Vector3.SmoothDamp(
            _currentPunchRotation, _targetPunchRotation,
            ref _punchVelocity, _punchSmoothTime
        );
        _targetPunchRotation = Vector3.Lerp(
            _targetPunchRotation, Vector3.zero,
            Time.deltaTime * _punchRecoverSpeed
        );

        transform.localPosition = _initialPosition + posNoise;
        transform.localRotation = _initialRotation * Quaternion.Euler(rotNoise + _currentPunchRotation);
    }

    /// <summary>Applies a one-shot rotational kick that decays smoothly back to zero.</summary>
    public void ApplyViewPunch(Vector3 punch)
    {
        _targetPunchRotation += punch;
    }
}

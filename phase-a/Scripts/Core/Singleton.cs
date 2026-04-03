using UnityEngine;

/// <summary>
/// Generic singleton base class for MonoBehaviour managers.
/// First instance wins; duplicates are destroyed automatically.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    /// <summary>Registers this instance as the singleton or destroys the duplicate.</summary>
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }
        else if (Instance != this)
        {
            Debug.Log(string.Format("{0} singleton already exists, destroying duplicate: {1}", typeof(T), gameObject.name));
            Object.Destroy(gameObject);
        }
    }
}

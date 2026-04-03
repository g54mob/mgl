using UnityEngine;

/// <summary>
/// Marks a transform as a valid spawn location for purchased items.
/// Place multiple in the scene near the shop terminal.
/// </summary>
public class ShopSpawnPoint : MonoBehaviour
{
    private static ShopSpawnPoint[] _cachedPoints;

    /// <summary>Returns a random ShopSpawnPoint from the scene.</summary>
    public static ShopSpawnPoint GetRandomSpawnPoint()
    {
        _cachedPoints = Object.FindObjectsOfType<ShopSpawnPoint>();
        if (_cachedPoints == null || _cachedPoints.Length == 0)
        {
            Debug.LogWarning("No ShopSpawnPoints found in the scene!");
            return null;
        }
        int index = Random.Range(0, _cachedPoints.Length);
        return _cachedPoints[index];
    }

    /// <summary>Draws a gizmo sphere in the editor so spawn points are visible.</summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}

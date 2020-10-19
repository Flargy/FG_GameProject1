using UnityEngine;

public class ProximityDetector : MonoBehaviour
{
    [SerializeField] public Transform ThingToCheck = default;
    [SerializeField, Range(0.1f, 20.0f)] public float DetectionRadius = default;

    public bool IsInRange
    {
        get
        { 
            return ThingToCheck && Vector3.Distance(transform.position, ThingToCheck.position) < DetectionRadius;
        }
    }
}

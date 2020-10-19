using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BoxCollider))]
public class PlayBounds : MonoBehaviour
{
    private static PlayBounds instance;
    private static BoxCollider coll;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        coll = GetComponent<BoxCollider>();
    }

    public static Bounds GetPlayBounds() => coll.bounds;
    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.tag);
        if (!other.CompareTag("Player")) return;
        
        NavMesh.SamplePosition(other.transform.position, out var hit, Single.PositiveInfinity, ~0);
        other.transform.position = hit.position;
        other.attachedRigidbody.velocity = Vector3.zero;
    }
}

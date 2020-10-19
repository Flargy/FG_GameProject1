
using System.Collections;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class ChewingGum : MonoBehaviour, ITriggerZone
{
    [SerializeField, Range(2.0f, 10.0f)] public float snapDistance = 3.0f;
    [SerializeField, Range(0.1f, 0.7f)] public float minimumSlow = 0.3f;
    [SerializeField, Range(0.1f, 0.95f)] public float maximumSlow = 0.9f;
    [SerializeField, Range(0f, 8f)] private float activationDelay = 2f;

    private SphereCollider collider;
    
    private void Awake()
    {
        collider = GetComponent<SphereCollider>();
    }

    public void Trigger(GameObject other)
    {
        AudioController.Instance.GenerateAudio(AudioController.ClipName.SteppingInGum, transform.position, 0.5f);
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        StartCoroutine(ActivationCooldown());
    }

    private void OnDisable()
    {
        if (!collider.enabled)
        {
            StopCoroutine(ActivationCooldown());
        }

        collider.enabled = false;
    }

    private IEnumerator ActivationCooldown()
    {
        yield return new WaitForSeconds(activationDelay);
        collider.enabled = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        using (new Handles.DrawingScope(Color.blue))
        {
            Handles.DrawWireDisc(transform.position, transform.up, snapDistance);
        }
    }
    #endif
}

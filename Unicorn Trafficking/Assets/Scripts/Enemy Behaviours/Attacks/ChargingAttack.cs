using EnemyBehaviours;
using UnityEngine;

public class ChargingAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] public float explosionForce = 3.0f;
    [SerializeField] public float upwardsModifier = 0.0f;
    [SerializeField] public float ministunDuration = 0.8f;

    private bool isActive;
    
    private void OnCollisionEnter(Collision other)
    {
        if (isActive && other.collider.CompareTag("Player"))
        {
            var ministun = other.gameObject.AddComponent<MiniStun>();
            ministun.duration = ministunDuration;
            other.rigidbody.AddExplosionForce(explosionForce, transform.position + Vector3.up,
                20.0f, upwardsModifier, ForceMode.VelocityChange);
            Disable();
        }
    }

    public void Disable()
    {
        isActive = false;
    }

    public void Enable()
    {
        isActive = true;
    }
    
    public bool IsEnabled()
    {
        return isActive;
    }


}

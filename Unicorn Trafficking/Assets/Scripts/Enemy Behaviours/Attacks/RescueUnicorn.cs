using EnemyBehaviours;
using UnityEngine;

public class RescueUnicorn : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private float attackDuration;
    
    private bool isActive = false;
    private float attackTimer;
    
    private void FixedUpdate()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer < 0.0f) Disable();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isActive && other.collider.CompareTag("Player"))
        {
            PlayerInventory.DropAUnicorn();
            Disable();
        }
    }

    public void Disable() => isActive = false;
    public void Enable()
    {
        attackTimer = attackDuration;
        isActive = true;
    }
    public bool IsEnabled() => isActive;
}

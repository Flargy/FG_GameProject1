using UnityEngine;
using UnityEngine.AI;
using static StateMachine;

namespace EnemyBehaviours {
public class WalkCloseToPlayer : IState
{
    private EnemyBehaviour behaviour;
    private Transform player;
    private float radius;

    public WalkCloseToPlayer(EnemyBehaviour Behaviour, Transform player, float radius)
    {
        this.behaviour = Behaviour;
        this.player = player;
        this.radius = radius;

    }
    
    // StateMachine.IState Implementation
    public void FixedTick() {}
    public void UpdateTick() {}

    public void OnEnter()
    {
        // Get the vector to the player.
        Vector3 ourPosition = behaviour.transform.position;
        Vector3 playerPos = player.position;

        Vector3 playerToUs = ourPosition - playerPos;
        Vector3 pointOnRadius = playerToUs.normalized * radius;

        // Add a random unit vector to it
        float hitRange = 2.0f;
        if (NavMesh.SamplePosition(pointOnRadius + Random.onUnitSphere, out var hit, hitRange, ~0))
        {
            hitRange += 5.0f;
        }

        // set it as our target
        behaviour.goalPosition = hit.position;
    }

    public void OnExit() {}
#if UNITY_EDITOR
    public void DrawGizmos() {}
#endif
    
    // Methods

}
} // end namespace EnemyBehaviours

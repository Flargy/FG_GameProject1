using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static StateMachine;

namespace EnemyBehaviours {
public class MaintainRangeFromTransform : IState
{
    private EnemyBehaviour behaviour;
    private Transform player;

    private float minRange;
    private float maxRange;
    
    public MaintainRangeFromTransform(IBehaviourWithRange behaviour, Transform player, ProximityDetector playerProximity)
    {
        this.behaviour = (EnemyBehaviour) behaviour;
        this.player = player;
        this.minRange = behaviour.GetMinRange();
        this.maxRange = behaviour.GetMaxRange();
        playerProximity.DetectionRadius = minRange;
    }
    
    // StateMachine.IState Implementation
    public void FixedTick() {}
    public void UpdateTick() {}

    public void OnEnter()
    {
        // Get the vector to the player.
        var ourTransform = behaviour.transform;
        Vector3 ourPosition = ourTransform.position;
        Vector3 playerPos = player.position;

        Vector3 playerToUs = ourPosition - playerPos;
        Vector3 unitPlayerToUs = playerToUs.normalized;
        float avgRadius = (minRange + maxRange) * 0.5f;

        // in space local to player
        Vector3 pointOnAvgRadius = unitPlayerToUs * avgRadius;
        
        // Bias towards the direction we are facing, so we don't jitter so much
        Vector3 biasTowardsLookDirection = pointOnAvgRadius + (ourTransform.forward * 2.0f);
        
        // Add a random unit vector to it and fix the range to be the midpoint.
        Vector3 candidateDirection = (biasTowardsLookDirection + (Random.onUnitSphere * 1.5f) - playerPos).normalized * avgRadius;
        
        // And move it into world space.
        Vector3 candidatePosition = playerPos + candidateDirection;
        
        float hitRange = 2.0f;
        NavMeshHit hit;
        // @NOTE: Shennanigans. NavMesh will not exist on the first frame of execution, so we stop after 3 attempts.
        for (int attempts = 0;
            NavMesh.SamplePosition(candidatePosition, out hit, hitRange, ~0) && (attempts < 3);
            attempts++)
        {
            hitRange += 7.0f;
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

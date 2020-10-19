using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyBehaviours {
public class StartRunningAway : StateMachine.IState
{
    private readonly EnemyBehaviour enemyBehaviour;
    private readonly ProximityDetector proximityDetector;
    
    private readonly float distanceToRun;
    private float navmeshHitRadius;
    private float prevDetectRange;
    
    public StartRunningAway(EnemyBehaviour enemyBehaviour, NavMeshAgent agent,
        ProximityDetector proximityDetector, float distanceToRun)
    {
        this.enemyBehaviour = enemyBehaviour;
        this.proximityDetector = proximityDetector;
        this.distanceToRun = distanceToRun;
    }
    
    // StateMachine.IState Implementation
    public void FixedTick() {}
    public void UpdateTick() {}

    public void OnEnter()
    {
        // Pick a distant point in the opposite direction to the player
        var currentPosition = enemyBehaviour.transform.position;
        var playerPosition = proximityDetector.ThingToCheck.position;
        
        Vector3 fleeDir = (currentPosition - playerPosition).normalized;
        Vector3 fleePos = currentPosition + fleeDir * distanceToRun;
        
        // Find the closest point to fleePos on the navmesh and set it as our new goal.
        NavMesh.SamplePosition(fleePos, out NavMeshHit meshHit, float.PositiveInfinity, ~0);
        
        var candidatePosition = meshHit.position;
        var playerToCandidatePos = playerPosition - candidatePosition;

        if(Vector3.Dot(playerToCandidatePos, playerToCandidatePos) < distanceToRun * distanceToRun)
        {
            // We're cornered!
            var fleeLeft = Vector3.Cross(fleeDir, Vector3.up);
            var fleeRight = Vector3.Cross(Vector3.up, fleeDir);
            
            var posLeft = currentPosition + fleeLeft * distanceToRun;
            var posRight = currentPosition + fleeRight * distanceToRun;

            NavMesh.SamplePosition(posLeft, out var hitLeft, float.PositiveInfinity, ~0);
            NavMesh.SamplePosition(posRight, out var hitRight, float.PositiveInfinity, ~0);

            var leftVector = hitLeft.position - playerPosition;
            var rightVector = hitRight.position - playerPosition;

            var leftDist = Vector3.Dot(leftVector, leftVector);
            var rightDist = Vector3.Dot(rightVector, rightVector);

            candidatePosition = leftDist > rightDist ? hitLeft.position : hitRight.position;
        }

        enemyBehaviour.goalPosition = candidatePosition;
        
        // Decrease the detection radius so we don't immediately re-trigger
        prevDetectRange = proximityDetector.DetectionRadius; 
        proximityDetector.DetectionRadius = 0.0f;
    }

    public void OnExit()
    {
        // Reset the detection radius
        proximityDetector.DetectionRadius = prevDetectRange;
    }
#if UNITY_EDITOR
    public void DrawGizmos()
    {
        Handles.color = Color.yellow;
        Handles.ArrowHandleCap(0, enemyBehaviour.goalPosition, Quaternion.identity, 1.0f, EventType.Repaint);
    }
#endif
    
    // Methods

    
}
} // end namespace EnemyBehaviours

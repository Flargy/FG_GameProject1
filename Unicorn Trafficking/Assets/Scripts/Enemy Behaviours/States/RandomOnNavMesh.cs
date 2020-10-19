using System;
using UnityEngine;
using UnityEngine.AI;
using static StateMachine;
using Random = UnityEngine.Random;

namespace EnemyBehaviours {
public class RandomOnNavMesh : IState
{
    private LazyBehaviour behaviour;
    private Transform ourTransform;
    private float minDistance;

    public RandomOnNavMesh(LazyBehaviour behaviour, Transform ourTransform, float minDistance)
    {
        this.behaviour = behaviour;
        this.ourTransform = ourTransform;
        this.minDistance = minDistance;
    }
    
    // StateMachine.IState Implementation
    public void FixedTick()
    {
        var bounds = PlayBounds.GetPlayBounds();

        Vector3 randomPoint = GenerateRandomPointInBounds(bounds);
        
        NavMeshHit hit;

        for (int attempts = 0;
            !NavMesh.SamplePosition(randomPoint, out hit, float.PositiveInfinity, ~0) && attempts < 8;
            attempts++)
        {
            randomPoint = GenerateRandomPointInBounds(bounds);
        }

        behaviour.goalPosition = behaviour.initialPosition = hit.position;
    }
    public void UpdateTick() {}

    public void OnEnter() {}

    private Vector3 GenerateRandomPointInBounds(Bounds bounds)
    {
        Vector3 randomPoint;
        Vector3 usToRandomPoint;
        do
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);

            randomPoint = new Vector3(randomX, randomY, randomZ);
            usToRandomPoint = randomPoint - ourTransform.position;
        } while (Vector3.Dot(usToRandomPoint, usToRandomPoint) < minDistance * minDistance);

        return randomPoint;
    }

    public void OnExit() {}
#if UNITY_EDITOR
    public void DrawGizmos()
    {
        Gizmos.DrawLine(ourTransform.position, behaviour.initialPosition);
    }
#endif
    
    // Methods
    
}
} // end namespace EnemyBehaviours

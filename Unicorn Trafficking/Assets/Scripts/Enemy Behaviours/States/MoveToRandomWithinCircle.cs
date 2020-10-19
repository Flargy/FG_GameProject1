
using UnityEngine;
using UnityEngine.AI;

namespace EnemyBehaviours {
public class MoveToRandomWithinCircle : StateMachine.IState
{
    private readonly EnemyBehaviour enemyBehaviour;
    private readonly IBehaviourWithInitialPosition initPosition;
    private readonly NavMeshAgent agent;
    private readonly float radius;
    
    public MoveToRandomWithinCircle(IBehaviourWithInitialPosition enemyBehaviour, NavMeshAgent agent,  float radius)
    {
        this.enemyBehaviour = (EnemyBehaviour) enemyBehaviour;
        this.initPosition = enemyBehaviour;
        this.agent = agent;
        this.radius = radius;
    }
    
    // StateMachine.IState Implementation
    public void FixedTick() {}
    public void UpdateTick() {}

    public void OnEnter()
    {
        enemyBehaviour.goalPosition = PickNewPatrolPoint();
    }

    public void OnExit() {}
    
#if UNITY_EDITOR
    public void DrawGizmos() { }
#endif

    // Methods
    private Vector3 PickNewPatrolPoint()
    {
        NavMeshHit meshHit;
        NavMeshQueryFilter queryFilter = new NavMeshQueryFilter();
        queryFilter.areaMask = ~0;
        Vector3 point;
        do
        {
            float angRad = Random.Range(0.0f, Mathfs.TAU);
            Vector3 dir = new Vector3(Mathfs.Cos(angRad), 0.0f, Mathfs.Sin(angRad)).normalized;
            point = dir * Random.Range(0.0f, radius);
        } while (!NavMesh.SamplePosition(initPosition.GetInitialPosition() + point, out meshHit, radius, queryFilter) &&
                 Vector3.Distance(meshHit.position, enemyBehaviour.transform.position) < agent.stoppingDistance);
        
        return meshHit.position;
    }
}
} // end namespace EnemyBehaviours

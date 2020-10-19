using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyBehaviours {
public class MoveToTarget : StateMachine.IState, IWaitableState
{
    protected readonly EnemyBehaviour behaviour;
    protected readonly NavMeshAgent agent;

    private EnemyBehaviour.MoveState moveState;
    
    public Vector3 LastPos;
    private float elapsed;

    public float GetTimeElapsed() => elapsed;

    public MoveToTarget(EnemyBehaviour Behaviour, NavMeshAgent agent, EnemyBehaviour.MoveState moveState)
    {
        this.behaviour = Behaviour;
        this.agent = agent;
        this.moveState = moveState;
    }

    // StateMachine.IState Implementation
    public virtual void UpdateTick() {}
    public virtual void FixedTick()
    {
        elapsed += Time.deltaTime;
        agent.speed = behaviour.CurrentMoveSpeed(moveState);
        
        float minMoveToConsiderAsProgress = agent.speed * Time.deltaTime * 0.5f; // Half our estimated move.
        if (Vector3.Distance(behaviour.transform.position, LastPos) > minMoveToConsiderAsProgress)
        {
            behaviour.timeSinceLastProgress = 0.0f;
        }
        else
            behaviour.timeSinceLastProgress += Time.deltaTime;

        LastPos = behaviour.transform.position;
    }

    public virtual void OnEnter()
    {
        // Start counting unstuck time
        behaviour.timeSinceLastProgress = 0.0f;
        
        // Start counting the seconds that we have been moving for;
        elapsed = 0.0f;

        // Setup agent to move
        agent.isStopped = false;
        agent.speed = behaviour.CurrentMoveSpeed(moveState);
        agent.SetDestination(behaviour.goalPosition);
    }

    public virtual void OnExit()
    {
        agent.isStopped = true;
    }
    
#if UNITY_EDITOR
    public virtual void DrawGizmos()
    {
        using (new Handles.DrawingScope(Color.cyan))
            Handles.DrawDottedLine(behaviour.transform.position, behaviour.goalPosition, 4.0f);
    }
#endif
}

} // end namespace EnemyBehaviours

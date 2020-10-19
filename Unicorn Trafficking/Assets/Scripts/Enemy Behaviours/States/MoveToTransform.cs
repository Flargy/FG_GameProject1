using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyBehaviours {
public class MoveToTransform : MoveToTarget
{
    protected Transform target;
    
    public MoveToTransform(EnemyBehaviour Behaviour, NavMeshAgent agent, EnemyBehaviour.MoveState moveState,
        Transform target) : base(Behaviour, agent, moveState)
    {
        this.target = target;
    }

    public override void FixedTick()
    {
        var targetPos = target.position;
        behaviour.goalPosition = targetPos;
        agent.destination = targetPos;
        base.FixedTick();
    }
}

} // end namespace EnemyBehaviours

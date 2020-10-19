using UnityEngine;
using UnityEngine.AI;

namespace EnemyBehaviours {
public class ChargeAtPlayer : MoveToTransform
{
    private IEnemyAttack chargingAttack;

    private float oldAcceleration;

    public ChargeAtPlayer(EnemyBehaviour behaviour, NavMeshAgent agent, Transform player,
        IEnemyAttack chargingAttack) : base(behaviour, agent, EnemyBehaviour.MoveState.Running, player)
    {
        this.chargingAttack = chargingAttack;
    }
    
    // StateMachine.IState Implementation

    public override void OnEnter()
    {
        base.OnEnter();
        chargingAttack.Enable();
        agent.angularSpeed = behaviour.TurnRate * 0.1f;
        oldAcceleration = agent.acceleration;
        agent.acceleration = oldAcceleration * 3.0f;
    }

    public override void OnExit()
    {
        chargingAttack.Disable();
        agent.angularSpeed = behaviour.TurnRate;
        agent.acceleration = oldAcceleration;
        base.OnExit();
    }
#if UNITY_EDITOR
    public override void DrawGizmos()
    {
        base.DrawGizmos();
    }
#endif
    
    // Methods
    
}
} // end namespace EnemyBehaviours

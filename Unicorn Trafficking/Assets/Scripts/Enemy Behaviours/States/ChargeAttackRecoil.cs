using UnityEngine;
using UnityEngine.AI;
using static StateMachine;

namespace EnemyBehaviours {
public class ChargeAttackRecoil : IState, IWaitableState
{
    private Transform player;
    private NavMeshAgent agent;
    private float elapsed;
    private Rigidbody rBody;
    private ChargingAttack chargingAttack;

    public ChargeAttackRecoil(NavMeshAgent agent, Transform player, ChargingAttack chargingAttack)
    {
        this.agent = agent;
        this.player = player;
        this.chargingAttack = chargingAttack;
        this.rBody = agent.gameObject.GetComponent<Rigidbody>();
    }
    
    // StateMachine.IState Implementation
    public void FixedTick() => elapsed += Time.deltaTime;

    public void UpdateTick() {}

    public void OnEnter()
    {
        elapsed = 0.0f;
        agent.enabled = false;
        rBody.AddExplosionForce(chargingAttack.explosionForce * 1.5f, player.position,
            5.0f, chargingAttack.upwardsModifier, ForceMode.VelocityChange);
        chargingAttack.Disable();

    }

    public void OnExit()
    {
        agent.enabled = true;
    }
#if UNITY_EDITOR
    public void DrawGizmos() {}
#endif
    
    // Methods

    public float GetTimeElapsed() => elapsed;
}
} // end namespace EnemyBehaviours

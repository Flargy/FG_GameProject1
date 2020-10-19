using UnityEngine;
using UnityEngine.AI;

namespace EnemyBehaviours {
public class RunAwayFromTransform : MoveToTarget, IWaitableState
{
    private IBehaviourWithRange rangeBehaviour;
    private ProximityDetector proximityDetector;
    private float elapsed;
    private float oldProximityRange;
    
    public RunAwayFromTransform(IBehaviourWithRange behaviour, NavMeshAgent agent,
        ProximityDetector proximityDetector, EnemyBehaviour.MoveState moveState)
        : base((EnemyBehaviour) behaviour, agent, moveState)
    {
        this.proximityDetector = proximityDetector;
        this.rangeBehaviour = behaviour;
    }
    
    // StateMachine.IState Implementation
    public override void UpdateTick()
    {
        elapsed += Time.deltaTime;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        elapsed = 0.0f;
        // Set new proximity range
        oldProximityRange = proximityDetector.DetectionRadius;
        proximityDetector.DetectionRadius = rangeBehaviour.GetMaxRange();
    }

    public override void OnExit()
    {
        base.OnExit();
        // Reset proximity range
        proximityDetector.DetectionRadius = oldProximityRange;
    }
    
    public float GetTimeElapsed() => elapsed;
    
#if UNITY_EDITOR
    public void DrawGizmos() {}
#endif
    
    
    // Methods
    
}
} // end namespace EnemyBehaviours

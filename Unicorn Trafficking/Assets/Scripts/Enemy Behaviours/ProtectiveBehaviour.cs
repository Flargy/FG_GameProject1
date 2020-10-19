using System;
using EnemyBehaviours;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(ChargingAttack))]
public class ProtectiveBehaviour : EnemyBehaviour
{
    public Transform player;
    [NonSerialized] public Transform guardTarget;
    public ProximityDetector playerProximity;
    private ChargingAttack chargingAttack;

    private bool isAggressive = false;
    
    [SerializeField, Tooltip("How close do we have to be before we charge the player?")] float chargePlayerRadius = 8.0f;
    [SerializeField, Tooltip("How long should we stare at the player before charging?")] float chargeUpTime = 0.75f;
    [SerializeField, Tooltip("How long should we stay still after we fail an attack run?")] float unsuccessfulWaitTime = 0.8f;
    
    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindWithTag("Player").transform;
        chargingAttack = GetComponent<ChargingAttack>();
        
        // Proximity Detector Setup
        playerProximity = GetComponent<ProximityDetector>();
        playerProximity.DetectionRadius = chargePlayerRadius;
        playerProximity.ThingToCheck = player;
        
        // ChargingAttack Setup
        chargingAttack = GetComponent<ChargingAttack>();
    }
    
    protected override void OnEnable()
    {
        chargingAttack.Disable();
        base.OnEnable();
    }
    
    protected override void OnDisable()
    {
        chargingAttack.Disable();
        base.OnDisable();
    }
    
    protected override void SetupStateMachine()
    {
        stateMachine = new StateMachine();
        
        /*         @NOTE:
         *     Current intent is to not actually implement the AI as described. We don't have the API to make
         *     assertions about whether or not a child has received the balloon they wanted. We can only check
         *     unicorns for captured states. As a result, I see fit to only implement guarding *unicorns*
         *     and not children from being delivered. We can revisit this later if there is time.
         */

        var findGuardTarget = new FindGuardTarget(this);
        var betweenTargetAndGuard = new TargetPositionBetween(this, player);
        var moveToGuardPoint = new MoveToTarget(this, agent, MoveState.Walking);
        var passivelyWait = new StandAndWait();

        var stareAtPlayer = new StareAtTransform(this, agent, player);
        var chargeAtPlayer = new ChargeAtPlayer(this, agent, player, chargingAttack);
        var chargeAttackRecoil = new ChargeAttackRecoil(agent, player, chargingAttack);
        var aggressivelyWait = new StandAndWait();

        #region Transitions

        // Passive State Transitions

        // findGuardTarget
        stateMachine.AddTransition(findGuardTarget, betweenTargetAndGuard, HasGuardTarget());
        stateMachine.AddTransition(betweenTargetAndGuard, moveToGuardPoint, () => true);

        // moveToGuardPoint
        stateMachine.AddTransition(moveToGuardPoint, findGuardTarget, GotStuck());
        stateMachine.AddTransition(moveToGuardPoint, findGuardTarget, WaitFor(moveToGuardPoint, 1.0f));
        stateMachine.AddTransition(moveToGuardPoint, passivelyWait, ReachedGoalPosition());

        // standAndWait
        stateMachine.AddTransition(passivelyWait, findGuardTarget, WaitFor(passivelyWait, 1.0f));

        
        
        // Aggressive State Transitions
        
        // stareAtPlayer
        stateMachine.AddTransition(stareAtPlayer, chargeAtPlayer, WaitFor(stareAtPlayer, chargeUpTime));
        stateMachine.AddTransition(stareAtPlayer, findGuardTarget, PlayerNotInRange());
        
        // chargeAtPlayer
        stateMachine.AddTransition(chargeAtPlayer, findGuardTarget, PlayerNotInRange());
        stateMachine.AddTransition(chargeAtPlayer, aggressivelyWait, PlayerNotInFront());
        stateMachine.AddTransition(chargeAtPlayer, chargeAttackRecoil, ChargeAttackSuccessful());
        
        //chargeAttackRecoil
        stateMachine.AddTransition(chargeAttackRecoil, stareAtPlayer, WaitFor(chargeAttackRecoil, chargingAttack.ministunDuration * 2.0f));
        
        // aggressivelyWait
        stateMachine.AddTransition(aggressivelyWait, stareAtPlayer, WaitFor(aggressivelyWait, unsuccessfulWaitTime));
     
        
        
        // Universal State Transitions
        stateMachine.AddUniversalTransition(stareAtPlayer, GoAggressive());
        stateMachine.AddUniversalTransition(findGuardTarget, GoPassive());

        #endregion

        // Conditions
        Func<bool> HasGuardTarget() =>
            () => guardTarget != null;
        
        Func<bool> GotStuck() =>
            () => timeSinceLastProgress > StuckTime;
        
        Func<bool> ReachedGoalPosition() =>
            () => Vector3.Distance(transform.position, goalPosition) <= agent.stoppingDistance + 0.2f;

        Func<bool> WaitFor(IWaitableState state, float seconds) =>
            () => state.GetTimeElapsed() > seconds;

        Func<bool> GoAggressive() =>
            () =>
            {
                if (isAggressive || !playerProximity.IsInRange) return false;
                isAggressive = true;
                return true;
            };
        
        Func<bool> GoPassive() =>
            () =>
            {
                if (!isAggressive || playerProximity.IsInRange) return false;
                isAggressive = false;
                return true;
            };
        
        Func<bool> PlayerNotInRange() => 
            () => !playerProximity.IsInRange;
        
        Func<bool> PlayerNotInFront() => 
            () => Vector3.Dot(transform.forward, player.position - transform.position) < 0.3f;
        
        Func<bool> ChargeAttackSuccessful() => 
            () => !chargingAttack.IsEnabled();

        // Set initial state
        stateMachine.SetState(findGuardTarget);
    }

#if UNITY_EDITOR
    protected void OnDrawGizmosSelected()
    {
        using (new Handles.DrawingScope())
        {
            Handles.Label(player.position, "Player");
            if(guardTarget) Handles.Label(guardTarget.position, "Guarding");
           
            // Visualize chargePlayerRadius
            Handles.color = new Color(0.8f, 0.1f, 0.1f, 1.0f);
            Handles.DrawWireDisc(transform.position, Vector3.up, chargePlayerRadius);
            
            base.DrawBaseGizmos();
        }
    }
#endif
}

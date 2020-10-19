using System;
using EnemyBehaviours;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

[RequireComponent(typeof(RescueUnicorn))]
public class RescueBehaviour : EnemyBehaviour, IBehaviourWithRange
{
    public Transform player;
    public ProximityDetector playerProximity;
    private RescueUnicorn rescueAttack;

    private float attackCooldown;
    private bool isAttacking;

    [SerializeField] private float minAttackCooldown;
    [SerializeField] private float maxAttackCooldown;
    
    [Header("Evasive Mode Behaviours")]
    [SerializeField] private float minRangeToPlayer;
    [SerializeField] private float maxRangeToPlayer;

    [Header("Aggressive Mode Behaviours")]
    [SerializeField] private float chargeUpTime = 0.7f;
    [SerializeField] private float pauseAfterAttack = 0.5f;
    
    
    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindWithTag("Player").transform;
        rescueAttack = GetComponent<RescueUnicorn>();
        
        // Proximity Detector Setup
        playerProximity = GetComponent<ProximityDetector>();
        playerProximity.DetectionRadius = minRangeToPlayer;
        playerProximity.ThingToCheck = player;
    }
    
    protected override void OnEnable()
    {
        StartAttackCooldown();
        base.OnEnable();
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void StartAttackCooldown()
    {
        attackCooldown = Random.Range(minAttackCooldown, maxAttackCooldown);
    }
    
    protected override void FixedUpdate()
    {
        attackCooldown -= Time.deltaTime;
        base.FixedUpdate();
    }

    protected override void SetupStateMachine()
    {
        stateMachine = new StateMachine();

        var pickAPointAtASafeDistance = new MaintainRangeFromTransform(this, player, playerProximity);
        var walkToTarget = new MoveToTarget(this, agent, MoveState.Walking);
        var startRunningAway = new StartRunningAway(this, agent, playerProximity, maxRangeToPlayer + 2.0f);
        var runAwayABit = new RunAwayFromTransform(this, agent, playerProximity, MoveState.Running);
        
        var stareAtPlayer = new StareAtTransform(this, agent, player);
        var chargeAtPlayer = new ChargeAtPlayer(this, agent, player, rescueAttack);
        var toggleAttack = new ToggleAttack(rescueAttack);

        var aggressivelyWait = new StandAndWait();
        
        //////----
        // Set Transitions
        //
        
        // Passives
        
        // pickAPointAtASafeDistance
        stateMachine.AddTransition(pickAPointAtASafeDistance, walkToTarget, () => true);
        
        // walkToTarget
        stateMachine.AddTransition(walkToTarget, pickAPointAtASafeDistance, ReachedGoalPosition());
        stateMachine.AddTransition(walkToTarget, startRunningAway, TooCloseToPlayer());
        stateMachine.AddTransition(walkToTarget, pickAPointAtASafeDistance, GotStuck());
        
        // startRunningAway
        stateMachine.AddTransition(startRunningAway, runAwayABit, () => true);
        
        // runAwayABit
        stateMachine.AddTransition(runAwayABit, pickAPointAtASafeDistance, EscapedFromPlayer());


        
        
        // Aggressive State Transitions
        
        // stareAtPlayer
        stateMachine.AddTransition(stareAtPlayer, toggleAttack, WaitFor(stareAtPlayer, chargeUpTime));
        
        // toggleAttack
        stateMachine.AddTransition(toggleAttack, chargeAtPlayer, () => true);
        
        // chargeAtPlayer
        stateMachine.AddTransition(chargeAtPlayer, aggressivelyWait, ChargeAttackEnded());

        // aggressivelyWait
        stateMachine.AddTransition(aggressivelyWait, pickAPointAtASafeDistance, WaitFor(aggressivelyWait, pauseAfterAttack));

        //////----
        // Universal Transitions
        //
        
        // One-frame States
        stateMachine.AddUniversalTransition(stareAtPlayer, GoingAggressive());
        
        
        
        // Conditions
        Func<bool> ReachedGoalPosition() =>
            () => Vector3.Distance(transform.position, goalPosition) <= agent.stoppingDistance + 0.2f;
        
        Func<bool> GotStuck() =>
            () => timeSinceLastProgress > StuckTime;
        
        Func<bool> TooCloseToPlayer() => 
            () => playerProximity.IsInRange;
        
        Func<bool> EscapedFromPlayer() => 
            () => !playerProximity.IsInRange;
        
        Func<bool> WaitFor(IWaitableState state, float seconds) =>
            () => state.GetTimeElapsed() > seconds;
        
        Func<bool> ChargeAttackEnded() => 
            () => !rescueAttack.IsEnabled();

        Func<bool> GoingAggressive() =>
            () =>
            {
                if (attackCooldown < 0.0f)
                {
                    StartAttackCooldown();
                    return true;
                }

                return false;
            };
        
        
        // Set initial state
        stateMachine.SetState(pickAPointAtASafeDistance);
        
    }

#if UNITY_EDITOR
    protected void OnDrawGizmosSelected()
    {
        using (new Handles.DrawingScope())
        {
            var centered = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = {textColor = Color.white},
                fontSize = 14,
            };

            if(attackCooldown > 0.0f)
            {
                Handles.Label(transform.position,
                $"Current Mode: Attacking in: {attackCooldown:F1}", centered);
            
                Handles.color = new Color(0.4f, 0.9f, 0.2f, 1.0f);
                var playerPos = player.position;
                Handles.DrawWireDisc(playerPos, Vector3.up, minRangeToPlayer);
                Handles.DrawWireDisc(playerPos, Vector3.up, maxRangeToPlayer);
            }
            
            base.DrawBaseGizmos();
        }
    }
#endif
    public float GetMinRange()
    {
        return minRangeToPlayer;
    }

    public float GetMaxRange()
    {
        return maxRangeToPlayer;
    }
}

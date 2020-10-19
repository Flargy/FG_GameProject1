using System;
using EnemyBehaviours;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(ProximityDetector))]
public class EvadeBehaviour : EnemyBehaviour, IBehaviourWithRange, IBehaviourWithInitialPosition
{
    protected ProximityDetector playerProximity;
    
    [SerializeField, Tooltip("Number of seconds to wait between roaming.")] protected float IdleTime = 3.0f;

    [SerializeField, Tooltip("How far from our spawnpoint do we want to roam?")] public float SpawnPatrolRadius = 4.0f;
    [SerializeField, Tooltip("How close must the player be before we start running away?")] public float EvadePlayerRadius = 2.0f;
    [SerializeField, Tooltip("How far from the player do we need to be before we stop running?")] public float EscapedPlayerRadius = 20.0f;
    public Vector3 InitialPosition { get; protected set; }
    public Vector3 GetInitialPosition() => InitialPosition;

    protected override void Awake()
    {
        base.Awake();
        
        // Proximity Detector Setup
        playerProximity = GetComponent<ProximityDetector>();
        playerProximity.DetectionRadius = EvadePlayerRadius;
        if (!playerProximity.ThingToCheck)
        {
            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject)
                playerProximity.ThingToCheck = playerObject.transform;
            else
                Debug.LogWarning(
                    $"{gameObject.name} Can't find a GameObject with the Player tag set. Did you set up a player?");
        }
    }

    protected override void OnEnable()
    {
        var currentPos = transform.position;
        InitialPosition = currentPos;
        base.OnEnable();
    }
    
    protected override void SetupStateMachine()
    {
        stateMachine = new StateMachine();

        // Set Up States
        var walkRandomWithinCircle = new MoveToRandomWithinCircle(this, agent, SpawnPatrolRadius);
        var walkToTarget = new MoveToTarget(this, agent, MoveState.Walking);
        var standAndWait = new StandAndWait();
        var runAwayFromPlayer = new RunAwayFromTransform(this, agent, playerProximity, MoveState.Running);
        var startRunningAway = new StartRunningAway(this, agent, playerProximity, EscapedPlayerRadius);
        
        
        //////----
        // Set Transitions
        //
        
        // walkToTarget
        stateMachine.AddTransition(walkToTarget, standAndWait, ReachedGoalPosition());
        stateMachine.AddTransition(walkToTarget, standAndWait, GotStuck());
        
        // standAndWait
        stateMachine.AddTransition(standAndWait, walkRandomWithinCircle, WaitFor(standAndWait, IdleTime));
        
        // startRunningAway
        stateMachine.AddTransition(startRunningAway, runAwayFromPlayer, () => true);
        
        // runAwayFromPlayer
        stateMachine.AddTransition(runAwayFromPlayer, startRunningAway, GotStuck());
        stateMachine.AddTransition(runAwayFromPlayer, startRunningAway, WaitFor(runAwayFromPlayer, 1.0f));
        stateMachine.AddTransition(runAwayFromPlayer, walkRandomWithinCircle, EscapedFromPlayer());
        
        
        // One-frame States
        stateMachine.AddTransition(walkRandomWithinCircle, walkToTarget, () => true);
        
        //////----
        // Set Universal Transitions
        //
        
        // One-frame States
        stateMachine.AddUniversalTransition(startRunningAway, WantToRunFromPlayer());

        // Conditions
        Func<bool> ReachedGoalPosition() =>
            () => Vector3.Distance(transform.position, goalPosition) <= agent.stoppingDistance + 0.2f;
        
        Func<bool> GotStuck() =>
            () => timeSinceLastProgress > StuckTime;

        Func<bool> WaitFor(IWaitableState state, float seconds) =>
            () => state.GetTimeElapsed() > seconds;
        
        Func<bool> WantToRunFromPlayer() =>
            () => (stateMachine._curState != runAwayFromPlayer) && playerProximity.IsInRange;
        
        Func<bool> EscapedFromPlayer() => 
            () => !playerProximity.IsInRange;


        // Set initial state
        stateMachine.SetState(walkRandomWithinCircle);
    }
    
#if UNITY_EDITOR
    
    private void OnDrawGizmosSelected()
    {
        using (new Handles.DrawingScope())
        {
            var homeColor   = new Color(1.0f, 0.7f, 0.0f, 1.0f);
            var evadeColor  = new Color(1.0f ,0.3f, 0.3f, 1.0f);
            var escapeColor = new Color(0.2f ,0.9f, 0.7f, 1.0f);
            
            if (!Application.isPlaying)
            {
                InitialPosition = transform.position;
            }

            // Visualize SpawnPatrolRadius
            Handles.color = homeColor;
            Handles.DrawWireDisc(InitialPosition, Vector3.up, SpawnPatrolRadius);
            
            // Visualize EvadePlayerRadius
            Handles.color = evadeColor;
            Handles.DrawWireDisc(transform.position, Vector3.up, EvadePlayerRadius);
                
            // Visualize EscapedPlayerRadius
            Handles.color = escapeColor;
            Handles.DrawWireDisc(transform.position, Vector3.up, EscapedPlayerRadius);

            if (Application.isPlaying)
            {
                // Visualize InitialPosition
                Handles.color = homeColor;
                Handles.ArrowHandleCap(0, InitialPosition + (Vector3.up * 1.0f),
                    Quaternion.LookRotation(Vector3.down, Vector3.forward),
                    1.0f, EventType.Repaint);
               
            }
            
            base.DrawBaseGizmos();
        }
    }
#endif
    public float GetMinRange()
    {
        return EvadePlayerRadius;
    }

    public float GetMaxRange()
    {
        return EscapedPlayerRadius;
    }
}

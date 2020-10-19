using System;
using EnemyBehaviours;
using UnityEditor;
using UnityEngine;

public class LazyBehaviour : EnemyBehaviour, IBehaviourWithInitialPosition
{

    [SerializeField, Range(5.0f, 20.0f), Tooltip("How far do we want to go when we long-travel, at a minimum?")] private float minDistanceToNextLocation = 10.0f;
    [SerializeField, Range(1.0f, 10.0f), Tooltip("When we reach a location, how wide should our patrol circle be?")] private float locationPatrolRadius = 3.0f;
    [SerializeField, Range(5.0f, 30.0f), Tooltip("How long should we patrol an area before finding a new one?")] private float timeSpentAtEachLocation = 20.0f;
    [SerializeField, Range(0.1f, 3.0f), Tooltip("How long should we pause between short patrol walks?")] private float waitTimeBetweenSmallMoves = 2.0f;

    public Vector3 GetInitialPosition() => initialPosition;
    [HideInInspector] public Vector3  initialPosition;
    
    private float timeAtThisLocation;
    private bool isInTransit;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void OnEnable()
    {
        isInTransit = true;
        initialPosition = transform.position; // Not sure if this is needed, no time to test.
        base.OnEnable();
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void FixedUpdate()
    {
        if (!isInTransit) timeAtThisLocation += Time.deltaTime;
        base.FixedUpdate();
    }

    protected override void SetupStateMachine()
    {
        stateMachine = new StateMachine();
        
        // Set Up States
        var randomPointFarAway = new RandomOnNavMesh(this, transform, minDistanceToNextLocation);
        var walkRandomWithinCircle = new MoveToRandomWithinCircle(this, agent, locationPatrolRadius);
        var walkToTarget = new MoveToTarget(this, agent, MoveState.Walking);
        var standAndWait = new StandAndWait();
        
        
        //////----
        // Set Transitions
        //
        
        // randomPointFarAway
        stateMachine.AddTransition(randomPointFarAway, walkToTarget, RandomPointIsGood());
        
        // walkToTarget
        stateMachine.AddTransition(walkToTarget, standAndWait, ReachedGoalPosition());
        stateMachine.AddTransition(walkToTarget, randomPointFarAway, GotStuck());
        
        // standAndWait
        stateMachine.AddTransition(standAndWait, walkRandomWithinCircle, WaitFor(standAndWait, waitTimeBetweenSmallMoves));
        
        // walkRandomWithinCircle
        stateMachine.AddTransition(walkRandomWithinCircle, walkToTarget, () => true);

        //////----
        // Universal Transitions
        //

        stateMachine.AddUniversalTransition(randomPointFarAway, PatrolTimerExpired());
        
        // Conditions
        Func<bool> ReachedGoalPosition() =>
            () =>
            {
                if (!(Vector3.Distance(transform.position, goalPosition) <= agent.stoppingDistance + 0.2f))
                    return false;
                
                isInTransit = false;
                return true;
            };
        

        Func<bool> RandomPointIsGood() =>
            () =>
            {
                var vector = transform.position - initialPosition;
                return Vector3.Dot(vector, vector) > minDistanceToNextLocation * minDistanceToNextLocation;
            };
        
        Func<bool> PatrolTimerExpired() =>
            () =>
            {
                if (timeAtThisLocation > timeSpentAtEachLocation)
                {
                    timeAtThisLocation = 0.0f;
                    isInTransit = true;
                    return true;
                }

                return false;
            };
        
        Func<bool> GotStuck() =>
            () => timeSinceLastProgress > StuckTime;

        Func<bool> WaitFor(IWaitableState state, float seconds) =>
            () => state.GetTimeElapsed() > seconds;
        

        // Set initial state
        stateMachine.SetState(randomPointFarAway);
    }

#if UNITY_EDITOR
    
    private void OnDrawGizmosSelected()
    {
        using (new Handles.DrawingScope())
        {
            var homeColor   = new Color(1.0f, 0.7f, 0.0f, 1.0f);

            var centered = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = {textColor = Color.white},
                fontSize = 14,
            };

            Handles.Label(transform.position, $"Time until next long move {(timeSpentAtEachLocation - timeAtThisLocation):F1}", centered);

            // Visualize SpawnPatrolRadius
            Handles.color = homeColor;
            Handles.DrawWireDisc(initialPosition, Vector3.up, locationPatrolRadius);

            if (Application.isPlaying)
            {
                // Visualize initialPosition
                Handles.color = homeColor;
                Handles.ArrowHandleCap(0, initialPosition + (Vector3.up * 1.0f),
                    Quaternion.LookRotation(Vector3.down, Vector3.forward),
                    1.0f, EventType.Repaint);
               
            }
            
            base.DrawBaseGizmos();
        }
    }
#endif
}

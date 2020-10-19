using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBehaviour : MonoBehaviour, IMovingActor
{
    public enum MoveState
    {
        Running,
        Walking
    }
    
    protected NavMeshAgent agent;
    protected StateMachine stateMachine;

    [HideInInspector] public float timeSinceLastProgress = 0.0f;
    [HideInInspector] public Vector3 goalPosition;
    
    [SerializeField, Tooltip("In meters per second"),  Range(0.1f, 10.0f)] private float RunSpeed = 2.0f; 
    [SerializeField, Tooltip("In meters per second"),  Range(0.1f, 10.0f)] private float WalkSpeed = 1.0f;
    [SerializeField, Tooltip("In degrees per second"), Range(100.0f, 500.0f)] public float TurnRate = 120.0f;
    
    [SerializeField, Tooltip("Number of seconds we spend being stuck before giving up.")] protected float StuckTime = 1.5f;

    private float currentWalkSpeed;
    private float currentRunSpeed;
    
    protected virtual void Awake()
    {
        // Agent Setup
        agent = GetComponent<NavMeshAgent>();
        agent.angularSpeed = TurnRate;
    }
    
    protected virtual void OnEnable()
    {
        currentRunSpeed = RunSpeed;
        currentWalkSpeed = WalkSpeed;
        
        SetupStateMachine();
        agent.enabled = true;
    }
    
    protected virtual void OnDisable()
    {
        agent.enabled = false;
    }

    protected abstract void SetupStateMachine();

    protected virtual void Update() => stateMachine?.UpdateTick();
    protected virtual void FixedUpdate() => stateMachine?.FixedTick();
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        GetComponent<NavMeshAgent>().angularSpeed = TurnRate;
    }

    protected void DrawBaseGizmos()
    {
        if (Application.isPlaying)
        {
            // Print the state
            var centered = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = {textColor = Color.white},
                fontSize = 14,
            };
            Handles.Label(transform.position + Vector3.up * 2.0f,
                stateMachine?._curState.ToString() ?? "No Current State!", centered);
            
            // Visualize GoalPosition
            Handles.color = new Color(0.9f ,0.0f, 0.7f, 1.0f);
            Handles.ArrowHandleCap(0, goalPosition + (Vector3.up * 2.0f),
                Quaternion.LookRotation(Vector3.down, Vector3.forward),
                2.0f, EventType.Repaint);
        }

        stateMachine?.DrawGizmos();
    }
#endif
    
    public void SetSpeedModifier(float slowMultiplier)
    {
        currentRunSpeed = RunSpeed * slowMultiplier;
        currentWalkSpeed = WalkSpeed * slowMultiplier;
    }

    public float CurrentMoveSpeed(MoveState moveState)
    {
        switch (moveState)
        {
            case MoveState.Running:
                return currentRunSpeed;
            
            case MoveState.Walking:
                return currentWalkSpeed;

            default:
                // Default behaviour
                return currentRunSpeed;
        }
    }
}

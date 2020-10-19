using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static StateMachine;

namespace EnemyBehaviours {
public class StareAtTransform : IState, IWaitableState
{
    private EnemyBehaviour behaviour;
    private NavMeshAgent agent;
    private Transform stareTarget;
    private Transform ourTransform;
    private float elapsed;

    public StareAtTransform(EnemyBehaviour Behaviour, NavMeshAgent agent, Transform stareTarget)
    {
        this.behaviour = Behaviour;
        this.agent = agent;
        this.stareTarget = stareTarget;
        this.ourTransform = behaviour.transform;
    }
    
    // StateMachine.IState Implementation

    public void FixedTick()
    {
        if(Vector3.Dot(ourTransform.forward, (stareTarget.position - ourTransform.position).normalized) > 0.9f)
            elapsed += Time.deltaTime;
    }
    public void UpdateTick()
    {
        var currentRotation = ourTransform.rotation;
        var desiredLook = Quaternion.LookRotation(stareTarget.position - ourTransform.position, Vector3.up);
        behaviour.transform.rotation =
            Quaternion.RotateTowards(currentRotation, desiredLook, agent.angularSpeed * 2.0f * Time.deltaTime);

    }

    public void OnEnter()
    {
        agent.isStopped = true;
        elapsed = 0.0f;
    }

    public void OnExit()
    {
        agent.isStopped = false;
    }
    
#if UNITY_EDITOR
    public void DrawGizmos()
    {
        Handles.Label(behaviour.transform.position, $"Staring for {elapsed}");
    }
#endif
    public float GetTimeElapsed() => elapsed;
    // Methods
    
}
} // end namespace EnemyBehaviours

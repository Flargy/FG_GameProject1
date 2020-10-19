using UnityEngine;
using UnityEngine.AI;
using static StateMachine;

namespace EnemyBehaviours {
public class TargetPositionBetween : IState
{
    private ProtectiveBehaviour behaviour;
    private Transform player;

    public TargetPositionBetween(ProtectiveBehaviour behaviour, Transform player)
    {
        this.behaviour = behaviour;
        this.player = player;
    }
    
    // StateMachine.IState Implementation
    public void FixedTick() {}
    public void UpdateTick() {}

    public void OnEnter()
    {
        float t = 0.5f;
        Vector3 candidatePosition = Vector3.Lerp(player.position, behaviour.goalPosition, 0.5f);
        NavMeshHit hit;
        // Continually move our seek area towards the target and away from the player until we find something on mesh.
        while (!NavMesh.SamplePosition(candidatePosition, out hit, 5.0f, ~0) &&
               t < 1.0f)
        {
            t += 0.1f;
            Debug.Log($"t = {t}");
        }

        behaviour.goalPosition = hit.position;
    }

    public void OnExit() {}
#if UNITY_EDITOR
    public void DrawGizmos() {}
#endif
    
    // Methods
    
}
} // end namespace EnemyBehaviours

using UnityEngine;

namespace EnemyBehaviours {
public class FindGuardTarget : StateMachine.IState
{
    private ProtectiveBehaviour behaviour;

    public FindGuardTarget(ProtectiveBehaviour behaviour)
    {
        this.behaviour = behaviour;
    }
    
    // StateMachine.IState Implementation
    public void FixedTick()
    {
        var activeUnicorns = SpawnSystem.GetActiveUnicorns();
        if (activeUnicorns.Count == 0) return;

        int closestIndex = -1;
        float closestDistanceSq = float.PositiveInfinity;
        for (int i = 0; i < activeUnicorns.Count; i++)
        {
            var transform = activeUnicorns[i].transform;
            if (behaviour.transform == transform) break;
            var vecToUnicorn = transform.position - behaviour.transform.localPosition;
            var distSq = Vector3.Dot(vecToUnicorn, vecToUnicorn);
            if (distSq < closestDistanceSq)
            {
                closestIndex = i;
                closestDistanceSq = distSq;
            }
        }

        if (closestIndex != -1) behaviour.guardTarget = activeUnicorns[closestIndex].transform;
    }
    public void UpdateTick() {}


    public void OnEnter()
    {

        // No: Check for Inventory
        // Yes: Get related child
        // No: Pop back
        // No: Get closest Unicorn

        // Do we have a GuardTarget?
        // Yes: Find midpoint
    }

    public void OnExit() {}
#if UNITY_EDITOR
    public void DrawGizmos() {}
#endif
    
    // Methods
    private void FindUnicornOnPlayer()
    {

    }

    private void FindUnicornInWorld()
    {
        var activeUnicorns = SpawnSystem.GetActiveUnicorns();
        //activeUnicorns[0].GetComponent<UnicornStats>().colorType;
    }
            
}
} // end namespace EnemyBehaviours

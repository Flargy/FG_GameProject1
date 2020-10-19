using UnityEngine;

namespace EnemyBehaviours {
public class StandAndWait : StateMachine.IState, IWaitableState
{
    private float elapsed;
    
    // StateMachine.IState Implementation
    public void FixedTick() {}
    public void UpdateTick()
    {
        elapsed += Time.deltaTime;
    }

    public void OnEnter()
    {
        elapsed = 0.0f;
    }

    public void OnExit() {}
#if UNITY_EDITOR
    public void DrawGizmos() {}
#endif
    
    // Methods

    public float GetTimeElapsed() => elapsed;
}
} // end namespace EnemyBehaviours

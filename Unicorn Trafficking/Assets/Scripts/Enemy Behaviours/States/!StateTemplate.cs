using static StateMachine;

namespace EnemyBehaviours {
public class StateTemplate : IState
{
    
    public StateTemplate() { }
    
    // StateMachine.IState Implementation
    public void FixedTick() {}
    public void UpdateTick() {}

    public void OnEnter() {}

    public void OnExit() {}
#if UNITY_EDITOR
    public void DrawGizmos() {}
#endif
    
    // Methods
    
}
} // end namespace EnemyBehaviours

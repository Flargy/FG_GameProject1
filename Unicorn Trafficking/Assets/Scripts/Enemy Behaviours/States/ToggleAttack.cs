using static StateMachine;

namespace EnemyBehaviours {
public class ToggleAttack : IState
{
    private IEnemyAttack attack;
    public ToggleAttack(IEnemyAttack attack)
    {
        this.attack = attack;
    }
    
    // StateMachine.IState Implementation
    public void FixedTick() {}
    public void UpdateTick() {}

    public void OnEnter()
    {
        attack.Enable();
    }

    public void OnExit() {}
#if UNITY_EDITOR
    public void DrawGizmos() {}
#endif
    
    // Methods
    
}
} // end namespace EnemyBehaviours

using TacticsRace.Core;

public class MinigameState : IRaceState
{
    private RaceStateMachine _owner;
    private IMinigameLogic _logic;

    public MinigameState(RaceStateMachine owner, IMinigameLogic logic)
    {
        _owner = owner;
        _logic = logic;
    }

    public void Enter()
    {
        _owner.ToggleMinigameUI(true);
        _logic.OnStart();
    }

    public void Update()
    {
        _logic.OnUpdate();
        _owner.UpdateMinigameBar(_logic.GetProgress());
    }

    public void Exit()
    {
        _logic.OnFinish();
        _owner.ToggleMinigameUI(false);
    }

    public string GetStateName() => "DESAFIO";
}
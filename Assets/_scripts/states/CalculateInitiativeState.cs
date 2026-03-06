using System.Linq;
using System.Collections.Generic;
using TacticsRace.Core;

public class CalculateInitiativeState : IRaceState
{
    private RaceStateMachine _owner;
    public CalculateInitiativeState(RaceStateMachine owner) => _owner = owner;

    public void Enter()
    {
        var list = new List<(DriverData d, int score)>();
        foreach (var driver in _owner.participants)
        {
            int roll = driver.agility + UnityEngine.Random.Range(1, 4);
            if (driver == _owner.playerDriver) roll += _owner.lastMinigameBonus;
            list.Add((driver, roll));
        }

        _owner.sortedParticipants = list.OrderByDescending(x => x.score).Select(x => x.d).ToList();

        // Próxima etapa: Turno do primeiro da lista
        _owner.ChangeState(new PlayerTurnState(_owner));
    }

    public void Update() { }
    public void Exit() { }
    public string GetStateName() => "DEFININDO ORDEM";
}
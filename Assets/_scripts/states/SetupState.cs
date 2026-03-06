using UnityEngine;
using TacticsRace.Core;

public class SetupState : IRaceState
{
    private RaceStateMachine _owner;
    public SetupState(RaceStateMachine owner) => _owner = owner;

    public void Enter() => _owner.StartCoroutine(_owner.SpawnParticipantsRoutine());
    public void Update() { }
    public void Exit() { }
    public string GetStateName() => "PREPARANDO GRID";
}
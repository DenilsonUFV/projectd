using UnityEngine;
using TacticsRace.Core;

public class CountdownState : IRaceState
{
    private RaceStateMachine _owner;
    private float _timer;
    private int _lastSecond = -1;

    public CountdownState(RaceStateMachine owner)
    {
        _owner = owner;
        _timer = _owner.countdownDuration;
    }

    public void Enter() => Debug.Log("Iniciando Contagem...");

    public void Update()
    {
        _timer -= Time.deltaTime;
        int currentSecond = Mathf.CeilToInt(_timer);

        if (currentSecond != _lastSecond && currentSecond > 0)
        {
            _lastSecond = currentSecond;
            _owner.UpdateCountdownUI(currentSecond.ToString());
        }

        if (_timer <= 0)
        {
            _owner.UpdateCountdownUI("GO!");
            // Transição para o estado genérico de Minigame com a lógica de largada
            _owner.ChangeState(new MinigameState(_owner, new StartTimingMinigame(_owner)));
        }
    }

    public void Exit() { }
    public string GetStateName() => "LARGADA";
}
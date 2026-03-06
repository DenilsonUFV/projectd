using UnityEngine;
using TacticsRace.Core;
using System.Threading.Tasks;

public class StartTimingMinigame : IMinigameLogic
{
    private RaceStateMachine _owner;
    private float _barPos = 0f;
    private bool _forward = true;
    private bool _inputLocked = false;

    public StartTimingMinigame(RaceStateMachine owner) => _owner = owner;

    public void OnStart() => Debug.Log("Minigame Iniciado");

    public void OnUpdate()
    {
        if (_inputLocked) return;

        float speed = 2.5f;
        _barPos += (_forward ? 1 : -1) * Time.deltaTime * speed;
        if (_barPos >= 1f || _barPos <= 0f) _forward = !_forward;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    private async void HandleInput()
    {
        _inputLocked = true;

        float diff = Mathf.Abs(_barPos - 0.5f);
        // BUSCA A MARGEM REAL DEFINIDA NO MANAGER
        float perfectMargin = _owner.GetPerfectMargin();

        string resultText;
        Color resultColor;
        int bonus;

        if (diff <= perfectMargin) // PERFEITO: Dentro da zona verde
        {
            resultText = "PERFECT!!";
            resultColor = Color.cyan;
            bonus = 3;
        }
        else if (diff <= perfectMargin * 2.5f) // BOM: Um pouco fora
        {
            resultText = "GOOD!";
            resultColor = Color.green;
            bonus = 1;
        }
        else
        {
            resultText = "BAD START";
            resultColor = Color.red;
            bonus = 0;
        }

        _owner.lastMinigameBonus = bonus;
        _owner.SendMinigameResult(resultText, resultColor);

        await Task.Delay(1500);
        _owner.ChangeState(new CalculateInitiativeState(_owner));
    }

    public void OnFinish() { }
    public float GetProgress() => _barPos;
}
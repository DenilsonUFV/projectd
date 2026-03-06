using TacticsRace.Core;
using UnityEngine;

public class PlayerTurnState : IRaceState
{
    private RaceStateMachine _owner;
    public bool HasMoved { get; private set; } // Propriedade pública para o menu consultar

    public bool IsBonusMovement { get; private set; } // Nova flag

    public PlayerTurnState(RaceStateMachine owner, bool hasMoved = false, bool isBonus = false)
    {
        _owner = owner;
        HasMoved = hasMoved;
        IsBonusMovement = isBonus;
    }

    public void EnableBonusMovement()
    {
        // "Enganamos" o sistema: dizemos que ele não moveu ainda (para liberar o botão)
        // Mas marcamos que o próximo movimento é o bônus.
        this.HasMoved = false;
        this.IsBonusMovement = true;

        // Atualiza o menu imediatamente
        var menu = Object.FindFirstObjectByType<ActionMenuController>();
        if (menu != null) menu.RefreshVisuals(this);
    }

    public void Enter()
    {
        // Notifica o menu para atualizar as cores dos ícones
        var menu = Object.FindFirstObjectByType<ActionMenuController>();
        if (menu != null)
        {
            menu.RefreshVisuals(this);
            menu.RefreshIconsForMainMenu();
        }
    }

    public void Update() { }
    public void Exit() { }
    public string GetStateName() => HasMoved ? "AÇÃO" : "MOVIMENTO";
}
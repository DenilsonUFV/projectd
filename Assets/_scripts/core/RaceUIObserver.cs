using UnityEngine;
using TMPro; // Requisito: TextMeshPro
using TacticsRace.Core;

public class RaceUIObserver : MonoBehaviour
{
    public TextMeshProUGUI stateText;

    private void OnEnable()
    {
        RaceStateMachine.OnStateChanged += UpdateUI;
    }

    private void OnDisable()
    {
        RaceStateMachine.OnStateChanged -= UpdateUI;
    }

    private void UpdateUI(IRaceState newState)
    {
        if (stateText != null)
            stateText.text = newState.GetStateName();
    }
}
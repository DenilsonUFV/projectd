using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TacticsRace.Core;

public class RaceStateMachine : MonoBehaviour
{
    private IRaceState _currentState;

    [Header("Configurações de Pista")]
    public List<Transform> gridPositions;
    public float spawnDelay = 0.3f;
    public float countdownDuration = 3.0f;

    [Header("Participantes")]
    public DriverData playerDriver;
    public List<DriverData> participants = new List<DriverData>();

    [HideInInspector] public List<GameObject> activeCars = new List<GameObject>();
    [HideInInspector] public List<DriverData> sortedParticipants;
    [HideInInspector] public int lastMinigameBonus;

    // Eventos para UI e Sistemas Externos
    public static event Action<IRaceState> OnStateChanged;
    public static event Action<string> OnCountdownUpdated;
    public static event Action<bool> OnToggleMinigameUI;
    public static event Action<float> OnMinigameUpdate;

    void Start() => ChangeState(new SetupState(this));
    void Update() => _currentState?.Update();

    public void ChangeState(IRaceState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
        OnStateChanged?.Invoke(_currentState);
    }

    // Métodos de ponte para os Estados
    public void UpdateCountdownUI(string val) => OnCountdownUpdated?.Invoke(val);
    public void ToggleMinigameUI(bool active) => OnToggleMinigameUI?.Invoke(active);
    public void UpdateMinigameBar(float val) => OnMinigameUpdate?.Invoke(val);

    public static event Action<string, Color> OnMinigameResult; // Texto e Cor do resultado
    public void SendMinigameResult(string text, Color color) => OnMinigameResult?.Invoke(text, color);

    public IEnumerator SpawnParticipantsRoutine()
    {
        for (int i = 0; i < participants.Count; i++)
        {
            if (i >= gridPositions.Count) break;
            GameObject car = Instantiate(participants[i].carPrefab, gridPositions[i].position, gridPositions[i].rotation);
            activeCars.Add(car);
            StartCoroutine(AnimateCarEntry(car));
            yield return new WaitForSeconds(spawnDelay);
        }
        yield return new WaitForSeconds(0.5f);
        ChangeState(new CountdownState(this));
    }

    private IEnumerator AnimateCarEntry(GameObject car)
    {
        Vector3 finalScale = car.transform.localScale;
        car.transform.localScale = Vector3.zero;
        float elapsed = 0;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            car.transform.localScale = finalScale * Mathf.Sin((elapsed / 0.4f) * Mathf.PI * 0.7f) * 1.2f;
            yield return null;
        }
        car.transform.localScale = finalScale;
    }

    public IRaceState GetCurrentState()
    {
        if (_currentState == null)
        {
            Debug.LogWarning("FSM: Tentativa de ler estado, mas _currentState é null!");
        }
        return _currentState;
    }

    public float GetPerfectMargin()
    {
        // 0.05f base + 0.02f por ponto de agilidade (Total de 0 a 1)
        return 0.05f + (playerDriver.agility * 0.02f);
    }

}
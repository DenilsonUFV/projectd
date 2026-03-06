using UnityEngine;
using TMPro;
using TacticsRace.Core;
using System.Collections;

public class RaceUIManager : MonoBehaviour
{
    [Header("Painéis de Camada")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private GameObject minigamePanel;

    [Header("Textos de Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Textos de Status")]
    [SerializeField] private TextMeshProUGUI currentStateText;

    [Header("Textos de Minigame")]
    [SerializeField] private TextMeshProUGUI minigameResultText;

    private Coroutine _textEffectRoutine;
    private Coroutine _feedbackRoutine;

    private void OnEnable()
    {
        // Inscrição em todos os eventos do fluxo de jogo
        RaceStateMachine.OnStateChanged += HandleStateChanged;
        RaceStateMachine.OnCountdownUpdated += HandleCountdownUpdate;
        RaceStateMachine.OnToggleMinigameUI += HandleMinigameVisibility;
        RaceStateMachine.OnMinigameResult += ShowMinigameFeedback;
    }

    private void OnDisable()
    {
        // Limpeza de eventos
        RaceStateMachine.OnStateChanged -= HandleStateChanged;
        RaceStateMachine.OnCountdownUpdated -= HandleCountdownUpdate;
        RaceStateMachine.OnToggleMinigameUI -= HandleMinigameVisibility;
        RaceStateMachine.OnMinigameResult -= ShowMinigameFeedback;
    }

    // Gerencia a exibição do nome do estado atual (Top UI)
    private void HandleStateChanged(IRaceState newState)
    {
        if (currentStateText != null)
            currentStateText.text = newState.GetStateName();
    }

    // Gerencia a contagem regressiva (3, 2, 1, GO)
    private void HandleCountdownUpdate(string value)
    {
        if (countdownText == null) return;

        countdownPanel.SetActive(true);
        countdownText.text = value;

        // Efeito de punch no texto da contagem
        if (_textEffectRoutine != null) StopCoroutine(_textEffectRoutine);
        _textEffectRoutine = StartCoroutine(AnimateTextPunch(countdownText.gameObject, 1.5f, 0.2f));

        if (value == "GO!")
        {
            Invoke(nameof(HideCountdown), 1.0f);
        }
    }

    // Gerencia a visibilidade do painel do minigame
    private void HandleMinigameVisibility(bool isVisible)
    {
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(isVisible);

            // Quando o painel abrir ou fechar, resetamos o texto de resultado
            if (minigameResultText != null)
            {
                minigameResultText.text = "";
                minigameResultText.transform.localScale = Vector3.zero;
            }
        }
    }

    // Gerencia o feedback textual do resultado (Perfect, Good, etc)
    private void ShowMinigameFeedback(string text, Color color)
    {
        if (minigameResultText == null) return;

        minigameResultText.text = text;
        minigameResultText.color = color;

        // Inicia animação de popup específica para o resultado acima da barra
        if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
        _feedbackRoutine = StartCoroutine(AnimateResultPopup());
    }

    // Animação de "Punch" (Cresce e Volta ao original)
    private IEnumerator AnimateTextPunch(GameObject target, float maxScale, float duration)
    {
        Vector3 initialScale = Vector3.one;
        Vector3 targetScale = Vector3.one * maxScale;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.transform.localScale = Vector3.Lerp(targetScale, initialScale, elapsed / duration);
            yield return null;
        }

        target.transform.localScale = initialScale;
    }

    // Animação de "Popup" (Vem do zero com impacto)
    private IEnumerator AnimateResultPopup()
    {
        float elapsed = 0;
        float duration = 0.15f;
        Vector3 peakScale = new Vector3(1.3f, 1.3f, 1.3f);
        Vector3 finalScale = Vector3.one;

        // Surge e cresce
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            minigameResultText.transform.localScale = Vector3.Lerp(Vector3.zero, peakScale, elapsed / duration);
            yield return null;
        }

        // Assenta no tamanho final
        elapsed = 0;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            minigameResultText.transform.localScale = Vector3.Lerp(peakScale, finalScale, elapsed / 0.1f);
            yield return null;
        }

        minigameResultText.transform.localScale = finalScale;
    }

    private void HideCountdown()
    {
        if (countdownPanel != null) countdownPanel.SetActive(false);
    }
}
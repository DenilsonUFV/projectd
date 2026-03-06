using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TacticsRace.Core;

public class MinigameUIObserver : MonoBehaviour
{
    [Header("Referências de Movimento")]
    [SerializeField] private RectTransform cursor;
    [SerializeField] private RectTransform barContainer;
    [SerializeField] private RectTransform greenZone;

    [Header("Estética")]
    [SerializeField] private Color perfectColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Image cursorImage;

    private RaceStateMachine _manager;

    private void Awake()
    {
        _manager = FindFirstObjectByType<RaceStateMachine>();
    }

    private void OnEnable()
    {
        RaceStateMachine.OnMinigameUpdate += UpdateCursorPosition;
        RaceStateMachine.OnToggleMinigameUI += HandleVisibility;
    }

    private void OnDisable()
    {
        RaceStateMachine.OnMinigameUpdate -= UpdateCursorPosition;
        RaceStateMachine.OnToggleMinigameUI -= HandleVisibility;
    }

    private void HandleVisibility(bool isVisible)
    {
        if (isVisible)
        {
            // Iniciamos uma corrotina para garantir que a UI carregou antes de medir
            StartCoroutine(SetupVisualsRoutine());
        }
    }

    private IEnumerator SetupVisualsRoutine()
    {
        if (greenZone == null || barContainer == null || _manager == null) yield break;

        // Espera o final do frame para o RectTransform atualizar os tamanhos reais
        yield return new WaitForEndOfFrame();

        float totalWidth = barContainer.rect.width;
        float perfectMargin = _manager.GetPerfectMargin();

        // A largura visual é a porcentagem da margem total (margem é metade, então * 2)
        float visualWidth = totalWidth * (perfectMargin * 2f);

        // GARANTIA SÊNIOR: Forçamos as âncoras para middle-center via código 
        // para que o sizeDelta funcione como largura fixa (Width)
        greenZone.anchorMin = new Vector2(0.5f, 0.5f);
        greenZone.anchorMax = new Vector2(0.5f, 0.5f);
        greenZone.pivot = new Vector2(0.5f, 0.5f);

        // Define a largura baseada no cálculo
        greenZone.sizeDelta = new Vector2(visualWidth, greenZone.sizeDelta.y);

        Debug.Log($"[UI] Zona Verde ajustada para {visualWidth}px (Margem: {perfectMargin})");
    }

    private void UpdateCursorPosition(float progress)
    {
        if (cursor == null || barContainer == null) return;

        float width = barContainer.rect.width;
        // Mapeia 0-1 para a posição local da barra
        float newX = (progress * width) - (width / 2f);

        cursor.anchoredPosition = new Vector2(newX, cursor.anchoredPosition.y);

        if (cursorImage != null && _manager != null)
        {
            float diff = Mathf.Abs(progress - 0.5f);
            cursorImage.color = (diff <= _manager.GetPerfectMargin()) ? perfectColor : normalColor;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TacticsRace.Core;
using System.Collections;
using Unity.VisualScripting;
using TacticsRace.Skills;
using System.Collections.Generic;
using TacticsRace.Grid;
public enum MenuLevel { MAIN, OFFENSIVE, DEFENSIVE, SKILL_LEVEL
}

public class ActionMenuController : MonoBehaviour
{
    [Header("Estrutura")]
    [SerializeField] private GameObject menuRoot;

    [Header("Ícones (Ordem: Cima, Direita, Baixo, Esquerda)")]
    [SerializeField] private RectTransform[] iconSlots; // 0:Up, 1:Right, 2:Down, 3:Left
    [SerializeField] private Image[] iconImages;

    [Header("Configurações Visuais")]
    [SerializeField] private float selectedScale = 1f;
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private Color subMenuThemeColor = Color.cyan;

    private MenuLevel _currentMenuLevel = MenuLevel.MAIN;
    private RaceStateMachine _manager;
    private bool _isActive = false;
    private int _lastSelectedIndex = -1;

    private PlayerTurnState _currentTurnState;

    //SKILL MENU LOGIC
    private SkillSO _selectedSkill; // Habilidade que o jogador clicou

    [Header("Sprites de Navegação")]
    [SerializeField] private Sprite iconMainMove;      // Ícone de volante/seta
    [SerializeField] private Sprite iconMainOffensive; // Ícone de espada/chama
    [SerializeField] private Sprite iconMainDefensive; // Ícone de escudo
    [SerializeField] private Sprite iconMainEndTurn;   // Ícone de bandeira/check

    [Header("Sprites de Níveis")]
    [SerializeField] private Sprite[] levelIcons;      // Array com 4 sprites (Números 1, 2, 3, 4)

    [Header("Prefabs de Skills")]
    [SerializeField] private GameObject ghostPrefab;

    // Método chamado pelo PlayerTurnState ao entrar
    public void RefreshVisuals(PlayerTurnState state)
    {
        _currentTurnState = state;
        if (_currentTurnState == null) return;

        bool hasMoved = _currentTurnState.HasMoved;

        // Se já moveu, o ícone de Movimento (0) fica preto, senão fica Branco (Ativo)
        iconImages[0].color = hasMoved ? Color.black : Color.white;

        // O ícone de Terminar Turno (2) só fica Branco se já tiver movido
        iconImages[2].color = hasMoved ? Color.white : Color.black;

        // Ofensivo (1) e Defensivo (3) costumam estar sempre ativos na sua regra
        iconImages[1].color = Color.white;
        iconImages[3].color = Color.white;
    }

    // Método que troca as imagens dos ícones conforme o nível do menu
    // No ActionMenuController.cs, dentro do RefreshIconsForSkillLevels

    public void RefreshIconsForSkillLevels()
    {
        if (_selectedSkill == null) return;

        // Pegamos a posição atual do carro
        Vector3Int carPos = TilemapGridManager.Instance.WorldToCell(_manager.CurrentActiveCar.transform.position);
        int agility = PlayerDataManager.Instance.activeDriver.agility;

        // A habilidade pode ser usada aqui?
        bool canUseHere = _selectedSkill.CanUse(carPos, agility);

        for (int i = 0; i < iconImages.Length; i++)
        {
            iconImages[i].gameObject.SetActive(true);

            if (i < _selectedSkill.levelsAvailable)
            {
                iconImages[i].sprite = levelIcons[i];

                // Se não puder usar, o ícone do nível fica preto/desativado
                iconImages[i].color = canUseHere ? Color.white : Color.black;

                // Opcional: desativar o clique se não puder usar
                // (Você pode adicionar uma trava no ExecuteSkill também)
            }
            else
            {
                iconImages[i].gameObject.SetActive(false);
            }
        }
    }

    // Método para voltar ao normal
    public void RefreshIconsForMainMenu()
    {
        _currentMenuLevel = MenuLevel.MAIN;

        for (int i = 0; i < iconImages.Length; i++)
        {
            // Garante que TUDO esteja ativo antes de trocar os sprites
            iconImages[i].gameObject.SetActive(true);
            iconSlots[i].gameObject.SetActive(true);
            iconImages[i].color = Color.white;
        }

        iconImages[0].sprite = iconMainMove;
        iconImages[1].sprite = iconMainOffensive;
        iconImages[2].sprite = iconMainEndTurn;
        iconImages[3].sprite = iconMainDefensive;

        if (_currentTurnState != null)
        {
            RefreshVisuals(_currentTurnState);
        }
    }



    private void Awake()
    {
        _manager = FindFirstObjectByType<RaceStateMachine>();
        if (menuRoot != null) menuRoot.SetActive(false);
    }

    private void OnEnable() => RaceStateMachine.OnStateChanged += HandleStateChange;
    private void OnDisable() => RaceStateMachine.OnStateChanged -= HandleStateChange;

    private void HandleStateChange(IRaceState newState)
    {
        _isActive = (newState is PlayerTurnState);
        menuRoot.SetActive(_isActive);

        if (_isActive)
        {
            _currentMenuLevel = MenuLevel.MAIN;
            RefreshIconsForMainMenu();
            //ResetAllIcons();
        }
    }

    private void Update()
    {
        if (!_isActive) return;

        // 1. Voltar (ESC / X)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            if (_currentMenuLevel != MenuLevel.MAIN)
            {
                _currentMenuLevel = MenuLevel.MAIN;
                RefreshIconsForMainMenu();
                Debug.Log("Voltando para o Menu Principal");
                return;
            }
        }

        // 2. Navegação por Direção
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) ProcessInput(0);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) ProcessInput(1);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) ProcessInput(2);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) ProcessInput(3);
    }

    // Adicione estas variáveis ao seu ActionMenuController
    private bool _canMove = true;
    private bool _canAct = true;

    // Método para atualizar o estado do menu vindo do State Machine
    public void SetMenuCapabilities(bool canMove, bool canAct)
    {
        _canMove = canMove;
        _canAct = canAct;

        // Feedback Visual: Escurece o ícone de movimento (Cima) se não puder mover
        // iconImages[0] é o slot de Cima (Movimento)
        iconImages[0].color = _canMove ? normalColor : new Color(0.2f, 0.2f, 0.2f, 0.5f);

        // O botão de baixo (Passar Turno) agora só fica branco/ativo se canMove for falso
        // (Conforme seu pedido: terminar turno desativado antes de mover)
        iconImages[2].color = !_canMove ? normalColor : new Color(0.2f, 0.2f, 0.2f, 0.5f);
    }

    private void ProcessInput(int index)
    {
        switch (_currentMenuLevel)
        {
            case MenuLevel.MAIN:
                ProcessMainMenu(index);
                break;

            case MenuLevel.OFFENSIVE:
                // O jogador escolheu qual habilidade ofensiva quer (Cima, Direita, Baixo ou Esquerda)
                // Ex: index 0 pode ser "Vácuo", index 1 "Fechada", etc.
                HandleSkillSelection(index, true);
                break;

            case MenuLevel.SKILL_LEVEL:
                // O jogador agora escolhe o nível daquela habilidade específica
                // index 0 = Nível 1, index 1 = Nível 2...
                ExecuteSkill(index + 1);
                break;

            case MenuLevel.DEFENSIVE:
                HandleSkillSelection(index, false);
                break;
        }
    }

    private void HandleSkillSelection(int index, bool isOffensive)
    {
        List<SkillSO> skillsList = isOffensive
            ? PlayerDataManager.Instance.unlockedOffensiveSkills
            : PlayerDataManager.Instance.unlockedDefensiveSkills;

        if (index >= 0 && index < skillsList.Count)
        {
            SkillSO selected = skillsList[index];

            // --- A TRANCA REAL ---
            Vector3Int carPos = TilemapGridManager.Instance.WorldToCell(_manager.CurrentActiveCar.transform.position);
            int agility = PlayerDataManager.Instance.activeDriver.agility;

            if (!selected.CanUse(carPos, agility))
            {
                Debug.Log($"[Aviso] Você não pode usar {selected.skillName} aqui!");
                // Opcional: Tocar um som de "erro" aqui
                return; // Bloqueia a entrada no menu de níveis
            }
            // ---------------------

            _selectedSkill = selected;
            _currentMenuLevel = MenuLevel.SKILL_LEVEL;
            RefreshIconsForSkillLevels();
        }
    }

    private void ExecuteSkill(int level)
    {
        if (_selectedSkill == null) return;
        int cost = _selectedSkill.paCosts[level - 1];

        if (PlayerDataManager.Instance.ConsumePA(cost))
        {
            if (_selectedSkill is DriftSkillSO drift)
            {
                int boost = drift.boostAmount[level - 1];

                // Consome PA e muda de estado passando os dados do Drift
                if (PlayerDataManager.Instance.ConsumePA(_selectedSkill.paCosts[level - 1]))
                {
                    _currentMenuLevel = MenuLevel.MAIN;
                    RefreshIconsForMainMenu();

                    // AQUI ESTÁ A CHAVE: Passamos 'true' e o valor do 'boost'
                    _manager.ChangeState(new MovementSelectionState(_manager, true, boost));
                }
            }

            // No ActionMenuController.cs, dentro do ExecuteSkill

            if (_selectedSkill is FechadaSkillSO fechada)
            {
                // 1. Limpa fantasmas antigos (caso existam)
                PlayerDataManager.Instance.ClearGhosts();

                // 2. Pega a posição e direção do carro
                GameObject car = _manager.CurrentActiveCar;
                Vector3Int currentCell = TilemapGridManager.Instance.WorldToCell(car.transform.position);

                // 3. Spawna os fantasmas baseados no nível
                var offsets = fechada.GetBlockOffsets(level, car.transform);
                foreach (var offset in offsets)
                {
                    Vector3Int targetCell = currentCell + offset;

                    // Só coloca se o tile for caminhável (não spawnar fantasma dentro da parede)
                    if (TilemapGridManager.Instance.IsCellWalkable(targetCell))
                    {
                        GameObject ghost = Instantiate(ghostPrefab); // Arraste o prefab no Inspector
                        ghost.GetComponent<GhostBlocker>().Initialize(TilemapGridManager.Instance.CellToWorld(targetCell));
                        PlayerDataManager.Instance.RegisterGhost(ghost);
                    }
                }

                Debug.Log($"FECHADA Nível {level} ativada!");
            }

            // Reseta o menu para o principal
            _currentMenuLevel = MenuLevel.MAIN;
            RefreshIconsForMainMenu();
        }
    }

    private void BackToMain()
    {
        _currentMenuLevel = MenuLevel.MAIN;
        // Atualiza os ícones de volta para Mover, Ofensivo, etc.
    }

    private void SelectSkill(int index, bool isOffensive)
    {
        // Aqui buscaríamos no Driver do jogador as habilidades que ele destravou
        // Por enquanto, simulamos uma seleção:
        Debug.Log($"Habilidade {index} selecionada. Agora escolha o nível.");
        _currentMenuLevel = MenuLevel.SKILL_LEVEL;
        RefreshIconsForSkillLevels(); // Muda ícones para números 1, 2, 3, 4
    }

    // Atualize o ProcessMainMenu para respeitar essas travas
    private void ProcessMainMenu(int index)
    {
            bool hasMoved = (_currentTurnState != null && _currentTurnState.HasMoved);

            switch (index)
            {
                case 0: // CIMA: Movimento
                    if (!hasMoved) // BLOQUEIO REAL
                    {
                        StartCoroutine(HighlightSelectionRoutine(index));
                        _manager.ChangeState(new MovementSelectionState(_manager));
                    }
                    else
                    {
                        Debug.Log("Movimento bloqueado: Você já se moveu!");
                    }
                    break;

                case 1: // DIREITA: Ofensivo
                    StartCoroutine(HighlightSelectionRoutine(index));
                    EnterSubMenu(MenuLevel.OFFENSIVE);
                    break;

            case 2: // BAIXO: Terminar Turno
                if (hasMoved)
                {
                    StartCoroutine(HighlightSelectionRoutine(index));
                    // Em vez de mudar para um estado fixo, avançamos o turno na máquina
                    _manager.AdvanceTurn();
                }
                else
                {
                    Debug.Log("Bloqueado: Mova-se primeiro!");
                }
                break;

            case 3: // ESQUERDA: Defensivo
                    StartCoroutine(HighlightSelectionRoutine(index));
                    EnterSubMenu(MenuLevel.DEFENSIVE);
                    break;
            }
    }

    private void EnterSubMenu(MenuLevel level)
    {
        _currentMenuLevel = level;

        List<SkillSO> skillsToShow = (level == MenuLevel.OFFENSIVE)
            ? PlayerDataManager.Instance.unlockedOffensiveSkills
            : PlayerDataManager.Instance.unlockedDefensiveSkills;

        Vector3Int carPos = TilemapGridManager.Instance.WorldToCell(_manager.CurrentActiveCar.transform.position);
        int agility = PlayerDataManager.Instance.activeDriver.agility;

        for (int i = 0; i < iconImages.Length; i++)
        {
            if (i < skillsToShow.Count)
            {
                iconImages[i].gameObject.SetActive(true);
                iconImages[i].sprite = skillsToShow[i].icon;

                // CHECAGEM DE VIABILIDADE:
                // Se a habilidade disser que não pode ser usada aqui, fica preto.
                bool canUse = skillsToShow[i].CanUse(carPos, agility);
                iconImages[i].color = canUse ? Color.white : Color.black;
            }
            else
            {
                iconImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void ResetAllIcons()
    {
        for (int i = 0; i < iconSlots.Length; i++)
        {
            iconSlots[i].localScale = Vector3.one;
            iconImages[i].color = normalColor;
            // No futuro: iconImages[i].sprite = _manager.playerDriver.baseIcons[i];
        }
    }

    private IEnumerator HighlightSelectionRoutine(int index)
    {
        // Reseta escalas anteriores
        foreach (var rect in iconSlots) rect.localScale = Vector3.one;

        // Aumenta o selecionado
        iconSlots[index].localScale = Vector3.one * selectedScale;
        iconImages[index].color = highlightColor;

        yield return new WaitForSeconds(0.15f);

        // Se for um comando imediato (como Mover ou Passar), reseta. 
        // Se for entrada em submenu, o EnterSubMenu cuida do visual.
        if (_currentMenuLevel == MenuLevel.MAIN)
        {
            iconSlots[index].localScale = Vector3.one;
            iconImages[index].color = normalColor;
        }
    }
}
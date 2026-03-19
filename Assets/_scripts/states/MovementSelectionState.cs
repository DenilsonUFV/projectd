using UnityEngine;
using TacticsRace.Core;
using TacticsRace.Grid;
using System.Collections.Generic;
using System.Collections;

public class MovementSelectionState : IRaceState
{
    private RaceStateMachine _owner;
    private MovementOverlay _overlay;
    private HashSet<Vector3Int> _reachableCells = new HashSet<Vector3Int>();
    private bool _isMoving = false; 
    private bool _isBonusMovement;
    private int _bonusRange;

    public MovementSelectionState(RaceStateMachine owner, bool isBonus = false, int bonusRange = 0)
    {
        _owner = owner;
        _overlay = owner.GetComponent<MovementOverlay>();
        _isBonusMovement = isBonus;
        _bonusRange = bonusRange;
    }

    public void Enter()
    {
        _isMoving = false;
        CalculateFinalArcOnly();
        _overlay.ShowRange(_reachableCells);
    }

    private void CalculateFinalArcOnly()
    {
        _reachableCells.Clear();
        Transform carTransform = _owner.CurrentActiveCar.transform;
        Vector3Int startCell = TilemapGridManager.Instance.WorldToCell(carTransform.position);
        Vector2 startForward = carTransform.up;

        int range = _isBonusMovement ? _bonusRange : _owner.playerDriver.agility;

        Queue<(Vector3Int cell, int dist)> queue = new Queue<(Vector3Int, int)>();
        queue.Enqueue((startCell, 0));

        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        visited.Add(startCell);

        // 1. PRIMEIRO: Executa o BFS completo
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Adiciona ao arco final
            if (current.dist >= (range - 1) && current.dist <= range && current.dist > 0)
            {
                _reachableCells.Add(current.cell);
            }

            // Continua expandindo a busca
            if (current.dist < range)
            {
                Vector3Int[] neighbors = {
                current.cell + Vector3Int.up,
                current.cell + Vector3Int.down,
                current.cell + Vector3Int.left,
                current.cell + Vector3Int.right
            };

                foreach (var n in neighbors)
                {
                    if (visited.Contains(n)) continue;

                    Vector2 diff = new Vector2(n.x - startCell.x, n.y - startCell.y);
                    if (diff.sqrMagnitude < 0.1f) continue;

                    float dot = Vector2.Dot(startForward.normalized, diff.normalized);

                    // Tolerância de -0.4f conforme combinamos
                    if (dot < -0.4f) continue;

                    if (TilemapGridManager.Instance.IsCellWalkable(n))
                    {
                        visited.Add(n);
                        queue.Enqueue((n, current.dist + 1));
                    }
                }
            }
        }

        // 2. DEPOIS: Se após o BFS inteiro não achou nada, aplica o Fallback
        if (_reachableCells.Count == 0)
        {
            Debug.LogWarning("Nenhum caminho à frente! Ativando Manobra de Recuperação.");

            Vector3Int[] neighbors = {
            startCell + Vector3Int.up, startCell + Vector3Int.down,
            startCell + Vector3Int.left, startCell + Vector3Int.right
        };

            foreach (var n in neighbors)
            {
                // Adiciona vizinhos imediatos que não sejam paredes
                if (TilemapGridManager.Instance.IsCellWalkable(n))
                {
                    _reachableCells.Add(n);
                }
            }
        }
    }


    public void Update()
    {
        if (_isMoving) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector3Int clickedCell = TilemapGridManager.Instance.WorldToCell(mousePos);

            if (_reachableCells.Contains(clickedCell))
                _owner.StartCoroutine(MoveSequence(clickedCell));
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            _overlay.ClearRange();
            _owner.ChangeState(new PlayerTurnState(_owner));
        }
    }

    private IEnumerator MoveSequence(Vector3Int destination)
    {
        _isMoving = true;
        _overlay.ClearRange();

        GameObject car = _owner.CurrentActiveCar;
        Vector3 targetPos = TilemapGridManager.Instance.CellToWorld(destination);

        // Rotação suave para o destino final
        Vector3 moveDir = (targetPos - car.transform.position).normalized;
        float targetAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);

        float elapsed = 0;
        float duration = 0.5f; // Tempo do "drift" ou deslocamento
        Vector3 startPos = car.transform.position;
        Quaternion startRot = car.transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            car.transform.position = Vector3.Lerp(startPos, targetPos, percent);
            car.transform.rotation = Quaternion.Slerp(startRot, targetRot, percent);
            yield return null;
        }

        car.transform.position = targetPos;
        TilemapGridManager.Instance.UpdateOccupancy(destination, car);

        _isMoving = false; 
        _owner.ChangeState(new PlayerTurnState(_owner, hasMoved: true));
    }

    public void Exit()
    {
        _overlay.ClearRange();
        // Limpamos aqui para garantir que o próximo estado comece do zero
        PlayerDataManager.Instance.ClearBoost();
    }
    public string GetStateName() => "PILOTANDO";
}
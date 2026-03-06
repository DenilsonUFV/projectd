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
        Transform carTransform = _owner.activeCars[0].transform;
        Vector3Int startCell = TilemapGridManager.Instance.WorldToCell(carTransform.position);

        // IMPORTANTE: Use o forward do carro, mas vamos garantir que ele não seja zero
        Vector2 startForward = carTransform.up;

        // Se for bônus, usa o bonusRange passado no construtor
        int range = _isBonusMovement ? _bonusRange : _owner.playerDriver.agility;

        Queue<(Vector3Int cell, int dist)> queue = new Queue<(Vector3Int, int)>();
        queue.Enqueue((startCell, 0));

        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Só mostramos o arco final (últimos 2 passos)
            if (current.dist >= (range - 1) && current.dist <= range && current.dist > 0)
            {
                _reachableCells.Add(current.cell);
            }

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

                    // CÁLCULO DE DIREÇÃO CORRIGIDO:
                    // Em distâncias curtas (Drift), o vetor precisa ser preciso.
                    Vector2 diff = new Vector2(n.x - startCell.x, n.y - startCell.y);

                    // Se o vizinho for a própria célula de início (dist 0), ignoramos
                    if (diff.sqrMagnitude < 0.1f) continue;

                    float dot = Vector2.Dot(startForward.normalized, diff.normalized);

                    // No Drift, permitimos uma abertura levemente maior (0.0 em vez de 0.1) 
                    // para não bloquear curvas fechadas
                    if (dot < -0.1f) continue;

                    if (TilemapGridManager.Instance.IsCellWalkable(n))
                    {
                        visited.Add(n);
                        queue.Enqueue((n, current.dist + 1));
                    }
                }
            }
        }
        // REMOVIDO: PlayerDataManager.Instance.ClearBoost() daqui!
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

        GameObject car = _owner.activeCars[0];
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
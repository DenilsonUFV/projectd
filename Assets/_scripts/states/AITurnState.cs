using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TacticsRace.Grid;
using TacticsRace.Core;
using System.Linq;

public class AITurnState : IRaceState
{
    private RaceStateMachine _owner;
    private GameObject _aiCar;
    private int _agility = 5;

    public AITurnState(RaceStateMachine owner, GameObject aiCar)
    {
        _owner = owner;
        _aiCar = aiCar;
    }

    public void Enter() => _owner.StartCoroutine(AIRoutine());

    private IEnumerator AIRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        List<Vector3Int> possibleTiles = GetAvailableMovementTiles();

        if (possibleTiles.Count > 0)
        {
            Vector3Int bestTile = SelectTargetTile(possibleTiles);
            yield return _owner.StartCoroutine(AIMoveSequence(bestTile));
        }
        else
        {
            // --- MANOBRA DE EMERGÊNCIA DA IA ---
            Debug.Log($"{_aiCar.name} está de costas! Rotacionando...");
            yield return _owner.StartCoroutine(RecoverDirectionRoutine());
        }

        yield return new WaitForSeconds(0.5f);
        _owner.AdvanceTurn();
    }

    private IEnumerator RecoverDirectionRoutine()
    {
        Transform nextWaypoint = _owner.GetNextWaypoint(_aiCar);
        if (nextWaypoint == null) yield break;

        Vector3 targetDir = (nextWaypoint.position - _aiCar.transform.position).normalized;
        float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg - 90f;

        // IA "corrige" o volante instantaneamente ou muito rápido
        _aiCar.transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        yield return new WaitForSeconds(0.2f);
    }

    private Vector3Int SelectTargetTile(List<Vector3Int> tiles)
    {
        Transform nextWaypoint = _owner.GetNextWaypoint(_aiCar);
        if (nextWaypoint == null) return tiles[Random.Range(0, tiles.Count)];

        // Ordenamos os tiles para encontrar o "Campeão"
        var bestTile = tiles
            .Select(tile => {
                Vector3 worldPos = TilemapGridManager.Instance.CellToWorld(tile);

                // MÉTRICA 1: Distância absoluta até o objetivo (Foco Total)
                float distToWaypoint = Vector3.Distance(worldPos, nextWaypoint.position);

                // MÉTRICA 2: Alinhamento (Evitar que ela ande de lado se puder ir para frente)
                Vector2 dirToWaypoint = (nextWaypoint.position - _aiCar.transform.position).normalized;
                Vector2 moveDir = (worldPos - _aiCar.transform.position).normalized;
                float alignment = Vector2.Dot(dirToWaypoint, moveDir);

                // Penalidade extrema para tiles que não resultam em progresso real
                float penalty = (alignment < 0.2f) ? 1000f : 0f;

                return new
                {
                    Tile = tile,
                    // O score é: quanto menor a distância, melhor. O alinhamento serve de desempate.
                    Priority = distToWaypoint - (alignment * 2f) + penalty
                };
            })
            .OrderBy(x => x.Priority) // Queremos a MENOR distância/prioridade
            .First().Tile;


        Vector3Int best = bestTile;
        // Verifica se passamos pelo waypoint antes de sair do estado
        _owner.UpdateWaypoint(_aiCar);

        return best;
    }

    private List<Vector3Int> GetAvailableMovementTiles()
    {
        List<Vector3Int> reachable = new List<Vector3Int>();
        Vector3Int startCell = TilemapGridManager.Instance.WorldToCell(_aiCar.transform.position);
        Vector2 startForward = _aiCar.transform.up;

        Queue<(Vector3Int cell, int dist)> queue = new Queue<(Vector3Int, int)>();
        queue.Enqueue((startCell, 0));

        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.dist == _agility) // IA agora foca APENAS no limite máximo do seu alcance
            {
                if (TilemapGridManager.Instance.IsCellWalkable(current.cell))
                {
                    reachable.Add(current.cell);
                }
            }

            // Adiciona apenas se estiver no arco final (últimos 2 blocos)
           else  if (current.dist >= (_agility) && current.dist <= (_agility + 1) && current.dist > 0)
            {
                // REGRA CRÍTICA: Não pode parar no mesmo bloco do jogador ou de um Ghost
                // O IsCellWalkable já checa ocupação no seu TilemapGridManager
                if (TilemapGridManager.Instance.IsCellWalkable(current.cell))
                {
                    reachable.Add(current.cell);
                }
            }

            else if (current.dist < _agility)
            {
                Vector3Int[] neighbors = {
                    current.cell + Vector3Int.up, current.cell + Vector3Int.down,
                    current.cell + Vector3Int.left, current.cell + Vector3Int.right
                };

                foreach (var n in neighbors)
                {
                    if (visited.Contains(n)) continue;

                    Vector2 diff = new Vector2(n.x - startCell.x, n.y - startCell.y);
                    if (diff.sqrMagnitude < 0.1f) continue;

                    // ... dentro do loop de vizinhos ...
                    float dot = Vector2.Dot(startForward.normalized, diff.normalized);

                    // Bloqueio rigoroso: IA não considera tiles que estão "atrás" dela no movimento normal
                    // A menos que ela esteja no modo de Recuperação (que trataremos abaixo)
                    if (dot < -0.2f) continue;

                    // REGRA CRÍTICA: Não pode PASSAR por cima de obstáculos ou ocupantes (Ghosts/Cars)
                    if (TilemapGridManager.Instance.IsCellWalkable(n))
                    {
                        visited.Add(n);
                        queue.Enqueue((n, current.dist + 1));
                    }
                }
            }
        }
        return reachable;
    }

    private IEnumerator AIMoveSequence(Vector3Int destination)
    {
        Vector3 targetPos = TilemapGridManager.Instance.CellToWorld(destination);
        Vector3 startPos = _aiCar.transform.position;
        Quaternion startRot = _aiCar.transform.rotation;

        Vector3 moveDir = (targetPos - startPos).normalized;
        float targetAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);

        float elapsed = 0;
        float duration = 0.7f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _aiCar.transform.position = Vector3.Lerp(startPos, targetPos, t);
            _aiCar.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        _aiCar.transform.position = targetPos;
        _aiCar.transform.rotation = targetRot;
        TilemapGridManager.Instance.UpdateOccupancy(destination, _aiCar);
    }

    public void Update() { }
    public void Exit() { }
    public string GetStateName() => "IA_AGGRESSIVE";
}
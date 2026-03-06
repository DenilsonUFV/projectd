using UnityEngine;
using System.Collections.Generic;
using TacticsRace.Grid;

public class TilemapMovement : MonoBehaviour
{
    private TilemapGridManager _grid;

    // Armazena os nós que o jogador pode alcançar no turno atual
    public HashSet<Vector3Int> reachableCells { get; private set; } = new HashSet<Vector3Int>();

    void Start() => _grid = TilemapGridManager.Instance;

    public void CalculateMovementArea(Vector3 currentPos, int moveRange)
    {
        reachableCells.Clear();
        Vector3Int startCell = _grid.WorldToCell(currentPos);

        // Algoritmo BFS (Breadth-First Search) para alcance tático
        Queue<Vector3Int> cellsToVisit = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        cellsToVisit.Enqueue(startCell);
        distances.Add(startCell, 0);

        while (cellsToVisit.Count > 0)
        {
            Vector3Int current = cellsToVisit.Dequeue();
            int currentDist = distances[current];

            if (currentDist <= moveRange)
            {
                reachableCells.Add(current);

                // Checa os 4 vizinhos cardinais
                Vector3Int[] neighbors = {
                    current + new Vector3Int(1, 0, 0),
                    current + new Vector3Int(-1, 0, 0),
                    current + new Vector3Int(0, 1, 0), // No Tilemap 2D, Y é profundidade
                    current + new Vector3Int(0, -1, 0)
                };

                foreach (var neighbor in neighbors)
                {
                    if (!distances.ContainsKey(neighbor) && _grid.IsCellWalkable(neighbor))
                    {
                        distances.Add(neighbor, currentDist + 1);
                        cellsToVisit.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}
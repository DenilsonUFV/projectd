using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace TacticsRace.Grid
{
    public class TilemapGridManager : MonoBehaviour
    {
        public static TilemapGridManager Instance;

        [Header("Configuração 2D")]
        [SerializeField] private Tilemap baseTilemap;    // Asfalto
        [SerializeField] private Tilemap obstacleTilemap; // Paredes/Grama
        [Header("Camadas de Detecção")]
        [SerializeField] private Tilemap curveTilemap; // Arraste o novo Tilemap aqui no Inspector


        private Dictionary<Vector3Int, GameObject> occupiedCells = new Dictionary<Vector3Int, GameObject>();

        private void Awake() => Instance = this;

        public bool IsCellWalkable(Vector3Int cellCoords)
        {
            if (baseTilemap == null) return false;

            // No 2D, verificamos apenas se existe o Tile na posição X, Y
            bool hasGround = baseTilemap.HasTile(cellCoords);
            bool hasObstacle = obstacleTilemap != null && obstacleTilemap.HasTile(cellCoords);
            bool isOccupied = occupiedCells.ContainsKey(cellCoords);

            return hasGround && !hasObstacle && !isOccupied;
        }

        public void UpdateOccupancy(Vector3Int cell, GameObject car)
        {
            Vector3Int? oldKey = null;
            foreach (var pair in occupiedCells) { if (pair.Value == car) { oldKey = pair.Key; break; } }
            if (oldKey.HasValue) occupiedCells.Remove(oldKey.Value);
            if (car != null) occupiedCells[cell] = car;
        }

        public bool IsCurveTile(Vector3Int cell)
        {
            if (curveTilemap == null) return false;

            // Se houver qualquer tile pintado nessa posição no mapa de curvas, retorna true
            return curveTilemap.HasTile(cell);
        }

        public Vector3Int WorldToCell(Vector3 worldPos) => baseTilemap.WorldToCell(worldPos);
        public Vector3 CellToWorld(Vector3Int cellCoords) => baseTilemap.GetCellCenterWorld(cellCoords);
    }
}
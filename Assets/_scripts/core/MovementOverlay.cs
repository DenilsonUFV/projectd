using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MovementOverlay : MonoBehaviour
{
    [Header("Feedback Visual")]
    [SerializeField] private Tilemap overlayTilemap; // Um Tilemap extra só para o brilho
    [SerializeField] private TileBase moveTile;      // Arraste aqui o seu Tile azul semitransparente

    public void ShowRange(HashSet<Vector3Int> cells)
    {
        ClearRange();
        foreach (var cell in cells)
        {
            overlayTilemap.SetTile(cell, moveTile);
        }
    }

    public void ClearRange()
    {
        if (overlayTilemap != null)
            overlayTilemap.ClearAllTiles();
    }
}
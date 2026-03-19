using UnityEngine;
using TacticsRace.Grid;

public class GhostBlocker : MonoBehaviour
{
    private Vector3Int _myCell;
    private bool _initialized = false;

    public void Initialize(Vector3 worldPosition)
    {
        _myCell = TilemapGridManager.Instance.WorldToCell(worldPosition);
        transform.position = worldPosition;

        // Marca como ocupado no sistema de Grid
        TilemapGridManager.Instance.UpdateOccupancy(_myCell, gameObject);
        _initialized = true;
    }

    private void OnDestroy()
    {
        // Se o jogo fechar ou o objeto for destruído antes de inicializar, evita erro
        if (!_initialized) return;

        // Limpa a ocupação para que o caminho fique livre novamente
        TilemapGridManager.Instance.RemoveOccupancy(_myCell);
    }
}
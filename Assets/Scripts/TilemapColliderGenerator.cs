using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapColliderGenerator : MonoBehaviour
{
    [SerializeField]
    private Tilemap m_tilemap;

    private Tile m_tile;

    public Vector3 Center { get; private set;}

    private void Awake()
    {
        m_tile = ScriptableObject.CreateInstance<Tile>();
        m_tile.colliderType = Tile.ColliderType.Grid;
    }

    public void PrepareCollider(bool[][] pixelState)
    {
        for (int y = 0; y < pixelState.Length; y++)
        {
            for (int x = 0; x < pixelState[0].Length; x++)
            {
                if (pixelState[y][x])
                {
                    m_tilemap.SetTile(new Vector3Int(x, y, 0), m_tile);
                }
                else
                {
                    m_tilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
        Vector3Int centerTile = new Vector3Int(pixelState[0].Length / 2, pixelState.Length / 2, 0);
        Center = m_tilemap.CellToWorld(centerTile);
    }

    public void DestroyCollider(Vector2 originWorldSpace, List<Vector2Int> affectedTilesAsOffset)
    {
        Vector3Int originCell = m_tilemap.WorldToCell(originWorldSpace);
        foreach (Vector2Int offset in affectedTilesAsOffset)
        {
            Vector3Int tilePosition = originCell + (Vector3Int)offset;
            if(m_tilemap.HasTile(tilePosition))
                    m_tilemap.SetTile(tilePosition, null);
        }
    }
    
}

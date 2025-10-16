using System;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleTerrain : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_spriteRenderer;
    private ModifiableTexture m_modifiableTexture;

    [SerializeField] 
    private TilemapColliderGenerator m_tilemapColliderGenerator;
    private TilemapColliderGenerator m_nonChunkedCollider;

    [SerializeField]
    private Grid m_grid;

    [SerializeField]
    private ChunkManager m_chunkManager;

    [SerializeField]
    private Vector2Int m_chunkSize = new(300, 300);

    void Start()
    {
        m_modifiableTexture = ModifiableTexture.CreateFromSprite(m_spriteRenderer.sprite);
        m_spriteRenderer.sprite = m_modifiableTexture.Sprite;
        float pixelSize = 1f / m_modifiableTexture.Sprite.pixelsPerUnit;
        m_grid.cellSize = new Vector2(pixelSize, pixelSize); // Set grid cell size to match pixel size

        // Vector2 size = m_modifiableTexture.Sprite.bounds.size;
        Vector2 bottomLeftLocal = -m_modifiableTexture.Sprite.pivot;

        Vector2Int chunkGridSize = SplitTextureIntoChunks(m_modifiableTexture.Texture.width, m_modifiableTexture.Texture.height, m_chunkSize);

        bool[][] pixels = m_modifiableTexture.GetPixelState();

        PrepareColliderChunks(chunkGridSize, m_chunkSize, bottomLeftLocal, pixels);

        // Vector2 bottomLeftWorld = m_spriteRenderer.transform.TransformPoint(bottomLeftLocal);

        // m_nonChunkedCollider = Instantiate(m_tilemapColliderGenerator, bottomLeftWorld, Quaternion.identity, m_grid.transform);
        // m_nonChunkedCollider.PrepareCollider(m_modifiableTexture.GetPixelState());
    }

    private void PrepareColliderChunks(Vector2Int chunkGridSize, Vector2Int m_chunkSize, Vector2 bottomLeftLocal, bool[][] pixels)
    {
        for (int x = 0; x < chunkGridSize.x; x++)
        {
            for (int y = 0; y < chunkGridSize.y; y++)
            {
                Vector3Int offset = new Vector3Int(x * m_chunkSize.x, y * m_chunkSize.y, 0); // width, height
                Vector3 bottomLeftCorner = (Vector3)bottomLeftLocal + offset;
                bottomLeftCorner.Scale(m_grid.cellSize);
                bottomLeftCorner = m_spriteRenderer.transform.TransformPoint(bottomLeftCorner);

                TilemapColliderGenerator colliderGenerator = Instantiate(m_tilemapColliderGenerator, bottomLeftCorner, Quaternion.identity, m_grid.transform);

                colliderGenerator.gameObject.name = $"Chunk ({x}, {y})";
                m_chunkManager.AddChunk(colliderGenerator);
                bool[][] chunkPixels = SliceArray(pixels, offset.y, offset.x, m_chunkSize.y, m_chunkSize.x);
                colliderGenerator.PrepareCollider(chunkPixels);
            }
        }
    }

    private bool[][] SliceArray(bool[][] pixels, int startRow, int startCol, int numRows, int numCols)
    {
        int sourceHeight = pixels.Length;
        int sourceWidth = pixels[0].Length;

        int actualHeight = Mathf.Min(numRows, sourceHeight - startRow);
        int actualWidth = Mathf.Min(numCols, sourceWidth - startCol);

        actualHeight = Mathf.Max(actualHeight, 0);
        actualWidth = Mathf.Max(actualWidth, 0);

        bool[][] result = new bool[actualHeight][];
        for (int row = 0; row < actualHeight; row++)
        {
            result[row] = new bool[actualWidth];
            for (int col = 0; col < actualWidth; col++)
            {
                result[row][col] = pixels[startRow + row][startCol + col];
            }
        }
        return result;
    }

    private Vector2Int SplitTextureIntoChunks(int width, int height, Vector2Int m_chunkSize)
    {
        int chunkCountRight = Mathf.CeilToInt((float)width / m_chunkSize.x);
        int chunkCountUp = Mathf.CeilToInt((float)height / m_chunkSize.y);
        return new Vector2Int(chunkCountRight, chunkCountUp); // width, height
    }

    public void RemoveTerrainAt(Vector2 worldPosition, float radius)
    {
        float pixelSize = 1f / m_modifiableTexture.Sprite.pixelsPerUnit;
        int radiusInPixels = Mathf.RoundToInt(radius / pixelSize);
        List<Vector2Int> affectedPixelAsOffset = GetCircleOffsets(radiusInPixels);

        Vector2Int circleCenterInPixelSpace = m_modifiableTexture.WorldToTexturePosition(worldPosition, m_spriteRenderer.transform);

        ModifyTextureAt(circleCenterInPixelSpace, Color.clear, affectedPixelAsOffset);
        // m_nonChunkedCollider.DestroyCollider(worldPosition, affectedPixelAsOffset);
        List<TilemapColliderGenerator> chunksToModify = m_chunkManager.GetClosestChunks(worldPosition);
        foreach (TilemapColliderGenerator chunk in chunksToModify)
        {
            chunk.DestroyCollider(worldPosition, affectedPixelAsOffset);
        }
    }

    private void ModifyTextureAt(Vector2Int circleCenterInPixelSpace, Color clear, List<Vector2Int> affectedPixelAsOffset)
    {
        foreach (Vector2Int offset in affectedPixelAsOffset)
        {
            Vector2Int pixelPosition = circleCenterInPixelSpace + offset;
            m_modifiableTexture.SetPixel(pixelPosition, clear);
        }
        m_modifiableTexture.ApplyChanges();
    }

    private List<Vector2Int> GetCircleOffsets(int radiusInPixels)
    {
        List<Vector2Int> offsets = new List<Vector2Int>();
        for (int x = -radiusInPixels; x <= radiusInPixels; x++)
        {
            for (int y = -radiusInPixels; y <= radiusInPixels; y++)
            {
                if (x * x + y * y <= radiusInPixels * radiusInPixels)
                {
                    offsets.Add(new Vector2Int(x, y));
                }
            }
        }
        return offsets;
    }

    private void OnDrawGizmosSelected() {
        if(Application.isPlaying)
        {
           if(m_chunkManager != null)
            {
                Vector3 chunkSize = new(m_chunkSize.x * m_grid.cellSize.x, m_chunkSize.y * m_grid.cellSize.y, 0);
                Vector3 halfChunkSize = chunkSize / 2f;
                m_chunkManager.DrawGizmos(chunkSize, halfChunkSize);
            }
        }
    }
}

using System;
using UnityEngine;

public class ModifiableTexture
{
    private Texture2D m_texture;
    private Sprite m_sprite;
    private Vector2 m_pivot;
    private float m_pixelsPerUnit;
    public Texture2D Texture => m_texture;
    public Sprite Sprite => m_sprite;
    public Vector2 Pivot => m_pivot;

    public static ModifiableTexture CreateFromSprite(Sprite sprite)
    {
        Rect rect = sprite.rect;
        Texture2D texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color32[] pixels = sprite.texture.GetPixels32();
        texture.SetPixels32(pixels);
        texture.Apply();

        Vector2 normalizedPivot = new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height);
        return new ModifiableTexture(texture, normalizedPivot, sprite.pixelsPerUnit);
    }

    private ModifiableTexture(Texture2D texture, Vector2 pivot, float pixelsPerUnit)
    {
        m_texture = texture;
        m_pivot = pivot;
        m_pixelsPerUnit = pixelsPerUnit;
        RecreateSprite();
    }

    private void RecreateSprite()
    {
        m_sprite = Sprite.Create(m_texture, new Rect(0, 0, m_texture.width, m_texture.height), m_pivot, m_pixelsPerUnit, 0, SpriteMeshType.FullRect, Vector4.zero, false);
    }

    public Vector2Int WorldToTexturePosition(Vector2 worldPosition, Transform transform)
    {
        Vector2 localPosition = transform.InverseTransformPoint(worldPosition); // Convert to local space of the sprite renderer
        int x = Mathf.RoundToInt(localPosition.x * m_pixelsPerUnit + m_sprite.pivot.x);
        int y = Mathf.RoundToInt(localPosition.y * m_pixelsPerUnit + m_sprite.pivot.y);
        return new Vector2Int(x, y);
    }

    public bool IsValidTexturePosition(Vector2Int texturePosition)
    {
        return texturePosition.x >= 0 && texturePosition.x < m_texture.width &&
               texturePosition.y >= 0 && texturePosition.y < m_texture.height;
    }

    public bool SetPixel(Vector2Int texturePosition, Color color)
    {
        if (!IsValidTexturePosition(texturePosition))
            return false;

        m_texture.SetPixel(texturePosition.x, texturePosition.y, color);
        return true;
    }
    
    public void ApplyChanges()
    {
        m_texture.Apply();
    }

    public bool[][] GetPixelState()
    {
        int width = m_texture.width;
        int height = m_texture.height;
        Color[] pixels = m_texture.GetPixels();
        bool[][] pixelState = new bool[height][];
        for (int y = 0; y < height; y++)
        {
            pixelState[y] = new bool[width];
            for (int x = 0; x < width; x++)
            {
                Color pixel = pixels[y * width + x];
                pixelState[y][x] = pixel.a > 0.5f; // Consider pixel solid if alpha > 0.5
            }
        }
        return pixelState;
    }
}

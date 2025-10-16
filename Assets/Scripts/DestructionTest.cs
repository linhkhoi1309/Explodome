using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class DestructionTest : MonoBehaviour
{
    [SerializeField]
    private InputActionReference m_pointerInput, m_clickInput;
    
    [SerializeField]
    private DestructibleTerrain m_destructibleTerrain;

    [SerializeField, Min(0.1f)]
    private float m_radius = 1f;

    private void OnEnable()
    {
        m_clickInput.action.performed += OnClick;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = m_pointerInput.action.ReadValue<Vector2>();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        m_destructibleTerrain.RemoveTerrainAt(worldPosition, m_radius);
    }

    private void OnDisable() {
        m_clickInput.action.performed -= OnClick;
    }
}

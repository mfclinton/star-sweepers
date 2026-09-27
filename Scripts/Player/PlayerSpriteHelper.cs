using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Serializable class that contains 2 sprites
[System.Serializable]
public class SpritePair
{
    [SerializeField] private SpriteRenderer frontRenderer;
    [SerializeField] private SpriteRenderer backRenderer;
    [SerializeField] private int frontLayerIndex;
    [SerializeField] private int backLayerIndex;

    public void UpdateSortingOrder(bool movingLeft)
    {
        frontRenderer.sortingOrder = movingLeft ? backLayerIndex : frontLayerIndex;
        backRenderer.sortingOrder = movingLeft ? frontLayerIndex : backLayerIndex;
    }
}

public class PlayerSpriteHelper : MonoBehaviour
{
    #region Sprite Renderer References

    [Header("Character Sprite Renderers")]
    [SerializeField] private SpriteRenderer[] spritesToColor;
    [SerializeField] private SpriteRenderer[] flipSpriteRenderers;
    [SerializeField] private SpritePair[] frontBackSpriteRenderers;

    #endregion

    #region Update Sprite Methods

    public void UpdatePlayerFacingDir(Vector2 movementInput, bool isGrounded)
    {
        if(movementInput == Vector2.zero || !isGrounded)
            return;

        // Flip the player depending on direction
        bool movingLeft = movementInput.x < 0;
        foreach(SpriteRenderer sr in flipSpriteRenderers)
            sr.flipX = movingLeft;

        foreach(SpritePair sp in frontBackSpriteRenderers)
            sp.UpdateSortingOrder(movingLeft);
    }

    // Set the alpha of the player's color
    public void SetPlayerAlpha(float alpha)
    {
        foreach(SpriteRenderer sr in spritesToColor)
        {
            Color newColor = sr.color;
            newColor.a = alpha;
            sr.color = newColor;
        }
    }

    #endregion
}

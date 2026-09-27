using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHint : MonoBehaviour
{
    #region Input Variables

    [Header("Generic Sprite Backgrounds")]
    [SerializeField] private Sprite genericKeyboardBG;
    [SerializeField] private Sprite genericControllerBG;

    [Header("Hint UI Elements")]
    [SerializeField] private SpriteRenderer inputHintSpriteRenderer;
    [SerializeField] private TextMeshPro inputHintText;

    #endregion

    #region Component References

    private PlayerInput playerInput;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        inputHintSpriteRenderer.enabled = false;
    }

    #endregion

    #region Public Methods

    public string GetBindingForAction(string actionName)
    {
        // Get the current control scheme
        string currentControlScheme = playerInput.currentControlScheme;

        // Find the action in the action map
        InputAction action = playerInput.actions.FindAction(actionName);
        if (action == null) return null;

        // Find the first binding that matches the current control scheme
        foreach (var binding in action.bindings)
        {
            if (binding.groups.Contains(currentControlScheme))
            {
                return binding.path;
            }
        }

        return null;
    }

    private void SetSpriteForAction(string actionName)
    {
        if(actionName == null)
            return;

        // Checks BindingDict for Action
        Dictionary<string, Sprite> bindingSpriteDictionary = BindingSpriteMapHolder.Instance.BindingSpriteMap.BindingSpriteDictionary;
        string bindingPath = GetBindingForAction(actionName);
        bindingSpriteDictionary.TryGetValue(bindingPath, out Sprite sprite);

        if(sprite == null)
            // If not found, checks for generic sprite
            HandleSpriteNotFound(bindingPath);
        else
            ShowHint(sprite);
    }

    public void UpdateHint(string actionName)
    {
        // Invalid Action
        if(actionName == null)
        {
            HideHint();
            return;
        }

        SetSpriteForAction(actionName);
    }

    private void HandleSpriteNotFound(string bindingPath)
    {
        string lastPart = bindingPath.Substring(bindingPath.LastIndexOf('/') + 1);
        
        if (bindingPath.ToLower().Contains("keyboard"))
            ShowHint(genericKeyboardBG, lastPart);
        else if (bindingPath.ToLower().Contains("gamepad"))
            ShowHint(genericControllerBG, lastPart);
        else
            HideHint();
    }

    private void HideHint()
    {
        inputHintSpriteRenderer.enabled = false;
        inputHintText.text = "";
    }

    private void ShowHint(Sprite sprite, string text = "")
    {
        inputHintSpriteRenderer.sprite = sprite;
        inputHintSpriteRenderer.enabled = true;
        inputHintText.text = text;
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScrollerController : MonoBehaviour
{

    #region Component References

    private PlayerInput playerInput;
    private LetterNodeManager letterNodeManager;

    #endregion

    #region Internal Variables

    private Vector2 inputDir;
    private string prevActionMap;

    #endregion

    #region Unity Callbacks

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        letterNodeManager = GetComponentInChildren<LetterNodeManager>();
    }

    private void Update() {
        ProcessScrollDirection();
    }

    #endregion

    #region State Methods

    public bool EnterScrollerState() {
        prevActionMap = playerInput.currentActionMap.name;
        playerInput.SwitchCurrentActionMap("Scroller");

        return true;
    }

    public void ExitScrollerState() {
        playerInput.SwitchCurrentActionMap("UI");
    }

    #endregion

    #region Scroller Methods

    private void OnScrollDirection(InputValue value)
    {
        inputDir = value.Get<Vector2>();
    }

    private void OnBackOut()
    {
        ExitScrollerState();
        letterNodeManager.ExitLetterNodeEditMode();
    }

    #endregion

    #region Process Methods

    private void ProcessScrollDirection()
    {
        if(inputDir == Vector2.zero || inputDir.magnitude < 0.33f)
            return;

        if(Mathf.Abs(inputDir.x) <= Mathf.Abs(inputDir.y))
            letterNodeManager.NavigateCurNode(0f < inputDir.y);
        else
            letterNodeManager.NavigateToNextNode(0f < inputDir.x);
    }

    #endregion
}

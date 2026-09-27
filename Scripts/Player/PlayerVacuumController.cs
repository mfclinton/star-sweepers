using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVacuumController : MonoBehaviour
{
    #region Input Variables

    private Vector2 movementInput;

    #endregion

    #region Component References

    public Vacuum ControlledVacuum { get; private set;} 
    public PlayerController playerController { get; private set; }
    private PlayerInput playerInput;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
    }

    #endregion

    #region State Switching

    public void SwitchToVacuumControl(Vacuum vacuum)
    {
        ControlledVacuum = vacuum;
        playerInput.SwitchCurrentActionMap("Vacuum");
    }

    public void SwitchToPlayerControl()
    {
        ControlledVacuum.RemoveUser(this);
        ControlledVacuum = null;
        playerInput.SwitchCurrentActionMap("Player");
    }

    #endregion

    #region Input Handlers

    public void OnMoveVacuum(InputValue value)
    {
        movementInput = value.Get<Vector2>();
        ControlledVacuum?.OnMoveVacuum(movementInput);
    }

    public void OnExitVacuuming()
    {
        SwitchToPlayerControl();
        Debug.Log("Player exited vacuuming!");
    }

    public void OnSuck(InputValue value)
    {
        bool isPressed = value.isPressed;
        ControlledVacuum?.OnSuck(isPressed);
    }

    public void OnBlow(InputValue value)
    {
        bool isPressed = value.isPressed;
        ControlledVacuum?.OnBlow(isPressed);
    }

    #endregion
}

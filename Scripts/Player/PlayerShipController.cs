using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShipController : MonoBehaviour
{
    #region Input Variables

    // TODO
    private Vector2 movementInput;

    #endregion

    #region Component References

    public Ship ControlledShip { get; private set;} 
    private PlayerController playerController;
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

    public void SwitchToShipControl(Ship ship)
    {
        ControlledShip = ship;
        playerInput.SwitchCurrentActionMap("Ship");
    }

    public void SwitchToPlayerControl()
    {
        ControlledShip.RemovePilot(this);
        ControlledShip = null;
        playerInput.SwitchCurrentActionMap("Player");
    }

    #endregion

    #region Input Handlers

    public void OnSteer(InputValue value)
    {
        movementInput = value.Get<Vector2>();
        ControlledShip?.OnSteer(movementInput);
    }

    public void OnAccelerate(InputValue value)
    {
        // TODO
        Debug.Log("Accelerate!");
    }

    public void OnExitPiloting()
    {
        SwitchToPlayerControl();
        Debug.Log("Player exited piloting!");
    }

    #endregion
}

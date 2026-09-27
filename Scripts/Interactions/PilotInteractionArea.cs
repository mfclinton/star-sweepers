using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PilotInteractionArea : InteractionAreaBase
{
    #region Component References

    [SerializeField] private Ship ship;

    #endregion

    #region State Variables

    public override bool IsInteractable => !ship.IsBeingControlled;

    #endregion

    #region Interaction Overrides
    public override void ExecuteInteraction(PlayerController playerController)
    {
        bool result = SwitchPlayerToShip(playerController);

        if(result)
            Debug.Log("Player switched to ship!");
        else
            Debug.Log("Player failed to switch to ship!");
    }

    #endregion

    #region State Switching

    public bool SwitchPlayerToShip(PlayerController playerController)
    {
        PlayerShipController playerShipControl = playerController.playerShipControl;
        if(playerShipControl == null)
        {
            Debug.LogError("PlayerShipControl is null!");
            return false;
        }

        bool pilotSet = ship.TrySetNewPilot(playerShipControl);
        if(pilotSet)
            playerShipControl.SwitchToShipControl(ship);

        return pilotSet;
    }

    #endregion

    #region Unity Callbacks

    protected override void Awake()
    {
        base.Awake();
        ship.onPilotUpdated += (PlayerShipController newPilot) => UpdatePlayerInteractionAreas();
    }

    #endregion

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VacuumInteractionArea : InteractionAreaBase
{
    #region Component References

    [SerializeField] private Vacuum vacuum;

    #endregion

    #region State Variables

    public override bool IsInteractable => !vacuum.CurrentUser;

    #endregion

    #region Interaction Overrides
    public override void ExecuteInteraction(PlayerController playerController)
    {
        bool result = SwitchPlayerToVacuum(playerController);
    }

    #endregion

    #region State Switching

    public bool SwitchPlayerToVacuum(PlayerController playerController)
    {
        PlayerVacuumController playerVacuumControl = playerController.playerVacuumControl;
        if(playerVacuumControl == null)
        {
            Debug.LogError("PlayerVacuumController is null!");
            return false;
        }

        bool userSet = vacuum.TrySetNewUser(playerVacuumControl);
        if(userSet)
            playerVacuumControl.SwitchToVacuumControl(vacuum);

        return userSet;
    }

    #endregion

    #region Unity Callbacks

    protected override void Awake()
    {
        base.Awake();
        vacuum.onUserUpdated += (PlayerVacuumController newUser) => UpdatePlayerInteractionAreas();
    }

    #endregion
}

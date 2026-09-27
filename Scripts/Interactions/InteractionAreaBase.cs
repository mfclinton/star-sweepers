using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class InteractionAreaBase : MonoBehaviour
{

    #region State Variables

    public LinkedList<PlayerController> PlayersInArea { get; private set; }
    public virtual bool IsInteractable => true;

    #endregion

    #region Unity Callbacks

    protected virtual void Awake()
    {
        PlayersInArea = new LinkedList<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
        if (playerController)
            OnPlayerEnterArea(playerController);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
        if (playerController)
            OnPlayerExitArea(playerController);
    }

    #endregion

    #region Interaction Methods

    public virtual bool TryInteraction(PlayerController playerController)
    {
        if (!IsInteractable)
            return false;
        
        ExecuteInteraction(playerController);

        return true;
    }

    public abstract void ExecuteInteraction(PlayerController playerController);

    #endregion

    #region Player Methods

    public virtual void UpdatePlayerInteractionAreas()
    {
        foreach(PlayerController player in PlayersInArea)
            player.OnInteractionAreaUpdated(this);
    }

    public virtual void OnPlayerEnterArea(PlayerController playerController)
    {
        PlayersInArea.AddLast(playerController);
        playerController.EnterInteractionArea(this);
    }

    public virtual void OnPlayerExitArea(PlayerController playerController)
    {
        PlayersInArea.Remove(playerController);
        playerController.ExitInteractionArea(this);
    }

    #endregion
    
}

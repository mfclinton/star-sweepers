using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashEntity : SpaceEntity
{
    #region Configuration

    [Header("Configuration")]
    [SerializeField] private int shieldRecharge = 0;
    [SerializeField] private int healthRegain = 0;
    [SerializeField] private int points = 0;

    #endregion

    public virtual void OnValidate()
    {
        this.gameObject.layer = LayerMask.NameToLayer("TrashEntity");
    }

    public override void Awake()
    {
        base.Awake();
    }

    public void UpdateGameStats()
    {
        if(this.gameObject.tag == "OrganicTrash")
        {
            GameManager.Instance.organicTrashCollected++;
        }
        else if(this.gameObject.tag == "RecycableTrash")
        {
            GameManager.Instance.recycleableTrashCollected++;
        }
        else if(this.gameObject.tag == "EnergyTrash")
        {
            GameManager.Instance.energyTrashCollected++;
        }
    }

    // Trash is just this
    public override void OnFiltered(Vacuum vacuum, SpaceEntity trash)
    {
        UpdateGameStats();

        Ship ship = vacuum.ship;

        // Extracts trash data
        FilterType trashType = FilterType.Organic;
        if (CompareTag("OrganicTrash"))
            trashType = FilterType.Organic;
        else if (CompareTag("RecycableTrash"))
            trashType = FilterType.Recycle;
        else if (CompareTag("EnergyTrash"))
            trashType = FilterType.Energy;

        Sprite sprite = trash.GetComponent<SpriteRenderer>().sprite;

        // Adds trash to queue
        FilterProcessor.Instance.AddToQueue(trashType, points, sprite);
        ObjectPooler.Instance.ReturnToPool(this.gameObject.tag, this.gameObject);
    }

    public override void OnHitPlayer(PlayerController player)
    {
        throw new System.NotImplementedException();
    }

    public override void OnHitShip(Ship ship)
    {
        throw new System.NotImplementedException();
    }
}

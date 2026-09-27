using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardEntity : SpaceEntity
{
    [Header("Configuration")]
    [SerializeField] private int playerDamage = 1;
    [SerializeField] private int shipDamage = 1;
    [SerializeField] private bool destroyOnHit = true;

    public virtual void OnValidate()
    {
        this.gameObject.layer = LayerMask.NameToLayer("HazardEntity");
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnFiltered(Vacuum vacuum, SpaceEntity trash)
    {
        throw new System.NotImplementedException();
    }

    public override void OnHitPlayer(PlayerController player)
    {
        player.DamagePlayer(playerDamage);
        HandleHit();
    }

    public override void OnHitShip(Ship ship)
    {
        ship.DamageShip(shipDamage);
        HandleHit();
    }

    void HandleHit()
    {
        // TODO
        if (destroyOnHit)
        {
            ObjectPooler.Instance.ReturnToPool(this.gameObject.tag, this.gameObject);
        }
    }
}

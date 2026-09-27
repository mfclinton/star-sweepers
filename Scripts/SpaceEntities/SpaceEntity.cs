using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class SpaceEntity : MonoBehaviour
{
    #region Configuration

    [Header("Configuration")]
    [SerializeField] private bool isSuckable;
    [SerializeField] private bool isBlowable;
    [SerializeField] private bool canHitPlayer;
    [SerializeField] private bool canHitShip;
    [SerializeField] private bool canBeFiltered;

    public bool IsSuckable { get { return isSuckable; } protected set { isSuckable = value; } }
    public bool IsBlowable { get { return isBlowable; } protected set { isBlowable = value; } }
    public bool CanHitPlayer { get { return canHitPlayer; } protected set { canHitPlayer = value; } }
    public bool CanHitShip { get { return canHitShip; } protected set { canHitShip = value; } }
    public bool CanBeFiltered { get { return canBeFiltered; } protected set { canBeFiltered = value; } }

    #endregion

    #region Component References

    public Rigidbody2D rb { get; private set; }

    #endregion

    #region Entity Events

    public abstract void OnHitPlayer(PlayerController player);

    public abstract void OnHitShip(Ship ship);

    public abstract void OnFiltered(Vacuum vacuum, SpaceEntity trash);

    #endregion

    #region Unity Callbacks

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (canHitPlayer && other.gameObject.CompareTag("Player"))
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();

            if(player != null)
                OnHitPlayer(player);
        }
        else if (canHitShip && other.gameObject.CompareTag("Ship"))
        {
            Ship ship = other.gameObject.GetComponent<Ship>();
            
            if(ship != null)
                OnHitShip(ship);
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (canBeFiltered && other.gameObject.CompareTag("Vacuum"))
        {
            Vacuum vacuum = other.gameObject.GetComponentInParent<Vacuum>();

            if(vacuum != null && vacuum.isSucking)
                OnFiltered(vacuum, this);
        }
    }

    #endregion
}

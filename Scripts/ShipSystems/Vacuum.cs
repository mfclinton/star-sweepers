using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Vacuum : MonoBehaviour
{
    #region Movement Variables

    [Header("Movement")]
    [SerializeField] private float vacuumMoveSpeed = 5.0f;
    [SerializeField] private float maxPlayerFollowSpeed = 10f;
    [SerializeField] private float rotateUprightSpeed = 4f;

    #endregion

    #region Suck and Block Variables

    [Header("Suck and Blow")]
    [SerializeField] private LayerMask objectsToAffect;
    [SerializeField] private float raycastCircleRadius = 2f;
    [SerializeField] private float pullPower = 10f;
    [SerializeField] private float pushPower = 20f;
    
    public ParticleSystem suckParticleSystem;
    public ParticleSystem blowParticleSystem;

    #endregion

    #region User Variables

    [SerializeField] private Transform vacuumUserPosition;

    #endregion

    #region Input Variables

    // TODO
    private Vector2 movementInput;

    #endregion

    #region State Variables

    public PlayerVacuumController CurrentUser { get; private set; }
    public bool IsBeingControlled => CurrentUser != null;

    // TODO
    public bool isSucking { get; private set; }
    public bool isBlowing { get; private set; }

    #endregion

    #region Component References

    public Rigidbody2D rb { get; private set; }
    public Ship ship { get; private set; }

    #endregion

    #region Delegates

    public delegate void OnUserUpdated(PlayerVacuumController newUser);
    public OnUserUpdated onUserUpdated;

    #endregion

    #region State Switching

    public bool TrySetNewUser(PlayerVacuumController user)
    {
        if (IsBeingControlled)
        {
            Debug.Log("Vacuum is already being controlled!");
            return false;
        }

        CurrentUser = user;
        onUserUpdated?.Invoke(CurrentUser);
        
        return true;
    }

    public void RemoveUser(PlayerVacuumController user)
    {
        if (CurrentUser != user)
        {
            Debug.LogError("User is not the current user!");
            return;
        }

        movementInput = Vector2.zero;
        CurrentUser = null;
        onUserUpdated?.Invoke(CurrentUser);
        StopAction();
    }

    #endregion

    #region Input Handlers

    public void OnMoveVacuum(Vector2 movementInput)
    {
        this.movementInput = movementInput;
    }

    public void OnSuck(bool isActive)
    {
        if(isActive)
        {
            isSucking = true;
            isBlowing = false;
            suckParticleSystem.Play();
            blowParticleSystem.Stop();
        }
        else
            StopAction();
    }

    public void OnBlow(bool isActive)
    {
        if(isActive)
        {
            isSucking = false;
            isBlowing = true;
            suckParticleSystem.Stop();
            blowParticleSystem.Play();
        }
        else
            StopAction();

    }

    public void StopAction()
    {
        isSucking = false;
        isBlowing = false;
        suckParticleSystem.Stop();
        blowParticleSystem.Stop();
    }

    #endregion

    #region Movement Methods

    private void HandleMovement()
    {
        HandleMovePlayerWithVacuum();

        if(movementInput.magnitude <= 0.01f)
            return;

        rb.velocity = movementInput * vacuumMoveSpeed;
    }

    private void HandleMovePlayerWithVacuum()
    {
        if (CurrentUser == null)
            return;

        Vector2 targetPosition = vacuumUserPosition.position;
        Rigidbody2D playerRb = CurrentUser.playerController.rb;

        float maxDistanceDelta = maxPlayerFollowSpeed * Time.deltaTime;
        Vector2 newPosition = Vector2.MoveTowards(playerRb.position, targetPosition, maxDistanceDelta);
        playerRb.MovePosition(newPosition);

        float angle = Mathf.LerpAngle(playerRb.rotation, rb.rotation, rotateUprightSpeed * Time.deltaTime);
        playerRb.MoveRotation(angle);
    }

    #endregion

    #region Suck and Blow Methods

    private void HandleSuckOrBlow()
    {
        if (isSucking)
            Suck();
        else if (isBlowing)
            Blow();
        else
            AudioManager.Instance.StopEvent(AudioManager.Instance.vacuumSound);
    }

    private void Suck()
    {
        Vector2 raycastPosition = transform.position + transform.right * raycastCircleRadius;
        Collider2D[] objectsInRadius = Physics2D.OverlapCircleAll(raycastPosition, raycastCircleRadius, objectsToAffect);
        foreach (Collider2D col in objectsInRadius)
        {
            SpaceEntity spaceEntity = col.GetComponent<SpaceEntity>();
            if(spaceEntity != null && !spaceEntity.IsSuckable)
                continue;

            Rigidbody2D rb = spaceEntity?.rb;
            if(rb == null)
                rb = col.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 direction = (Vector2)transform.position - rb.position;
                rb.AddForce(direction.normalized * pullPower);
            }
        }

        AudioManager.Instance.PlayEvent(AudioManager.Instance.vacuumSound);
    }

    private void Blow()
    {
        Vector2 raycastPosition = transform.position + transform.right * raycastCircleRadius;
        Collider2D[] objectsInRadius = Physics2D.OverlapCircleAll(raycastPosition, raycastCircleRadius, objectsToAffect);
        foreach (Collider2D col in objectsInRadius)
        {
            SpaceEntity spaceEntity = col.GetComponent<SpaceEntity>();
            if(spaceEntity != null && !spaceEntity.IsBlowable)
                continue;

            Rigidbody2D rb = spaceEntity?.rb;
            if(rb == null)
                rb = col.GetComponent<Rigidbody2D>();
            
            if (rb != null)
            {
                Vector2 direction = rb.position - (Vector2)transform.position;
                rb.AddForce(direction.normalized * pushPower);
            }
        }

        AudioManager.Instance.PlayEvent(AudioManager.Instance.vacuumSound);
    }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ship = GetComponentInParent<Ship>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleSuckOrBlow();
    }

    private void OnDrawGizmos() {
        Vector2 raycastPosition = transform.position + transform.right * raycastCircleRadius;
        Gizmos.DrawWireSphere(raycastPosition, raycastCircleRadius);
    }

    #endregion
}

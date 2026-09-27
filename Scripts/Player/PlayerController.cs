using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    #region Input Variables

    private Vector2 movementInput;

    #endregion

    #region State Variables

    private LinkedList<InteractionAreaBase> interactionAreas;

    #endregion

    #region Player Info

    public int CurrentHealth { get; private set; }
    [SerializeField] private float hitImmunityTime = 1f;
    public bool IsImmuneToDamage { get; private set; }

    #endregion

    #region Delegates

    public delegate void OnHealthUpdated(int newHealth);
    public OnHealthUpdated onHealthUpdated;

    public delegate void OnPlayerDeath(PlayerController player);
    public OnPlayerDeath onPlayerDeath;

    #endregion

    #region Component References

    public PlayerInput playerInput {get; private set;}
    public PlayerInputHint playerInputHint {get; private set;}
    public PlayerShipController playerShipControl {get; private set;}
    public Rigidbody2D rb {get; private set;}
    public PlayerSpriteHelper playerSpriteHelper {get; private set;}
    public PlayerVacuumController playerVacuumControl {get; private set;}

    #endregion

    #region Movement Variables

    [SerializeField] private float speed = 5.0f;

    #endregion

    #region Artificial Gravity Variables

    [SerializeField] private float raycastDistance = 0.5f;
    [SerializeField] private LayerMask ArtificialGravityLayer;
    [SerializeField] private float rotateUprightSpeed = 4f;

    #endregion

    #region Movement Methods

    private Vector2 CalculateMovement()
    {
        // Calculate the movement vector
        return movementInput * speed * Time.fixedDeltaTime;
    }

    private ArtificialGravity OnArtificialGravity()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, ArtificialGravityLayer);

        return hit.collider?.gameObject?.GetComponent<ArtificialGravity>();
    }

    private void MovePlayer()
    {
        ArtificialGravity artificialGravity = OnArtificialGravity();
        bool isGrounded = artificialGravity != null;
        Vector2 movement = CalculateMovement();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if(artificialGravity != null)
        {
            Rigidbody2D shipRb = artificialGravity.Rb;
            Vector2 offset = rb.position - shipRb.position;

            Vector2 tangentialVelocity = Mathf.Deg2Rad * shipRb.angularVelocity * new Vector2(-offset.y, offset.x);
            rb.velocity = shipRb.velocity + tangentialVelocity + movement;

            float angle = Mathf.LerpAngle(rb.rotation, shipRb.rotation, rotateUprightSpeed * Time.deltaTime);
            rb.MoveRotation(angle);
        }
        else
        {
            rb.velocity = movement;
        }

        playerSpriteHelper?.UpdatePlayerFacingDir(movementInput, isGrounded);

        if(playerInput.currentActionMap.name == "Player")
        {
            if(isGrounded)
            {
                AudioManager.Instance.StopEvent(AudioManager.Instance.astroPushSound);
                if(movementInput != Vector2.zero)
                    AudioManager.Instance.PlayEvent(AudioManager.Instance.footstepsSound);
                else
                    AudioManager.Instance.StopEvent(AudioManager.Instance.footstepsSound);
            }
        }

    }

    #endregion

    #region Player Methods

    public void InitializePlayer()
    {
        CurrentHealth = GameManager.Instance.MaxPlayerHealth;
    }

    public void DamagePlayer(int amount)
    {
        if(IsImmuneToDamage)
            return;

        ModifyHealth(-amount);
        GameManager.Instance.numPlayersInjured++;
        // GameManager.Instance.LosePoints(amount);
        Debug.Log("Player Health: " + CurrentHealth + " / " + GameManager.Instance.MaxPlayerHealth);

        StartCoroutine(PlayerDamageDelay());
    }

    // CoRoutine delay for player damage
    private IEnumerator PlayerDamageDelay()
    {
        IsImmuneToDamage = true;
        playerSpriteHelper?.SetPlayerAlpha(0.5f);

        yield return new WaitForSeconds(hitImmunityTime);
        
        IsImmuneToDamage = false;
        playerSpriteHelper?.SetPlayerAlpha(1f);
    }

    public void HealPlayer(int amount)
    {
        ModifyHealth(amount);
    }

    void ModifyHealth(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, GameManager.Instance.MaxPlayerHealth);

        if (CurrentHealth == 0)
            HandlePlayerDeath();
        
        AudioManager.Instance.PlayEvent(AudioManager.Instance.lostAHeartSound, true);
    }

    public void HandlePlayerDeath()
    {
        if (playerVacuumControl.ControlledVacuum != null)
            playerVacuumControl.SwitchToPlayerControl();
        else if (playerShipControl.ControlledShip != null)
            playerShipControl.SwitchToPlayerControl();

        // Handle respawn
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = GameManager.Instance.FindAnyOpenSpawnpoint();
        CurrentHealth = GameManager.Instance.MaxPlayerHealth;

        onPlayerDeath?.Invoke(this);
    }
    
    #endregion

    #region Input Event Handlers

    private void OnPause()
    {
        UIManager.Instance.TogglePauseMenu();
    }

    private void OnMovement(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    private void OnInteract()
    {
        if (interactionAreas.Count == 0)
            return;

        InteractionAreaBase interactionArea = interactionAreas.First.Value;
        interactionArea.TryInteraction(this);

        AudioManager.Instance.PlayEvent(AudioManager.Instance.interactSound, true);
    }

    #endregion

    #region Interaction Area Handlers

    public void EnterInteractionArea(InteractionAreaBase interactionArea)
    {
        interactionAreas.AddLast(interactionArea);
        UpdateInteractionHint();
    }

    public void ExitInteractionArea(InteractionAreaBase interactionArea)
    {
        interactionAreas.Remove(interactionArea);
        UpdateInteractionHint();
    }

    public void OnInteractionAreaUpdated(InteractionAreaBase interactionArea)
    {
        UpdateInteractionHint();
    }

    void UpdateInteractionHint()
    {
        InteractionAreaBase nextInteractionArea = GetNextInteractionArea();
        if (nextInteractionArea != null)
            playerInputHint?.UpdateHint("Interact");
        else
            playerInputHint?.UpdateHint(null);
    }

    private InteractionAreaBase GetNextInteractionArea()
    {
        // Get the next IsInteractable InteractionArea using Linq
        InteractionAreaBase nextInteractionArea = interactionAreas.FirstOrDefault(ia => ia.IsInteractable);
        return nextInteractionArea;
    }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        // Get Component References
        playerInput = GetComponent<PlayerInput>();
        playerInputHint = GetComponent<PlayerInputHint>();
        playerShipControl = GetComponent<PlayerShipController>();
        rb = GetComponent<Rigidbody2D>();
        playerSpriteHelper = GetComponent<PlayerSpriteHelper>();
        playerVacuumControl = GetComponent<PlayerVacuumController>();

        // Initialize Variables
        movementInput = Vector2.zero;
        interactionAreas = new LinkedList<InteractionAreaBase>();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    #endregion
}

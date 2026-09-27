using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Ship : MonoBehaviour
{

    #region Movement Variables

    [SerializeField] private float speed = 5.0f;

    #endregion

    #region Tilt Variables

    [SerializeField] private float maxTiltAngle = 30f;
    [SerializeField] private float lerpSpeed = 1f;

    #endregion

    #region Input Variables

    // TODO
    private Vector2 movementInput;

    #endregion

    #region State Variables

    public PlayerShipController CurrentPilot { get; private set; }
    public bool IsBeingControlled => CurrentPilot != null;

    private float rotationVelocity; // Velocity reference for smooth damping
    private const float rotationSmoothTime = 0.2f; // You can adjust this value based on your needs

    #endregion

    #region System Info

    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int maxShield = 5;
    public int MaxHealth => maxHealth;
    public int MaxShield => maxShield;
    
    public int CurrentHealth { get; private set; }
    public int CurrentShield { get; private set; }

    public float HealthP{get {return ((float)CurrentHealth / (float)MaxHealth) * 100f;}}

    [SerializeField] private float hitImmunityTime = 1f;
    public bool IsImmuneToDamage { get; private set; }

    #endregion

    #region Component References

    public Rigidbody2D rb { get; private set; }
    public FilterSystem filterSystem { get; private set; }
    private SpriteRenderer[] shipSprites;

    #endregion

    #region Delegates

    public delegate void OnPilotUpdated(PlayerShipController newPilot);
    public OnPilotUpdated onPilotUpdated;

    public delegate void OnHealthUpdated(int newHealth);
    public OnHealthUpdated onHealthUpdated;

    public delegate void OnShieldUpdated(int newShield);
    public OnShieldUpdated onShieldUpdated;

    public delegate void OnShipDestroyed(Ship ship);
    public OnShipDestroyed onShipDestroyed;

    #endregion

    #region Particle Systems Config
    [SerializeField] GameObject slowThrusterParticlesParent;
    [SerializeField] GameObject fastThrusterParticlesParent;

    #endregion

    #region State Switching

    public bool TrySetNewPilot(PlayerShipController playerShipController)
    {
        if (IsBeingControlled)
        {
            Debug.Log("Ship is already being controlled!");
            return false;
        }

        CurrentPilot = playerShipController;
        onPilotUpdated?.Invoke(CurrentPilot);
        
        return true;
    }

    public void RemovePilot(PlayerShipController pilot)
    {
        if (CurrentPilot != pilot)
        {
            Debug.LogError("Pilot is not the current pilot!");
            return;
        }

        movementInput = Vector2.zero;
        CurrentPilot = null;
        onPilotUpdated?.Invoke(CurrentPilot);
    }

    #endregion

    #region Ship System Methods

    public void InitializeShip()
    {
        CurrentHealth = maxHealth;
        CurrentShield = maxShield;
    }

    public void DamageShip(int damage)
    {
        if (damage <= 0 || IsImmuneToDamage)
            return;

        int absoredShieldDamage = Mathf.Min(CurrentShield, damage);

        if(absoredShieldDamage > 0)
        {
            onShieldUpdated?.Invoke(CurrentShield - absoredShieldDamage);
            CurrentShield = Mathf.Clamp(CurrentShield - absoredShieldDamage, 0, maxShield);

            AudioManager.Instance.PlayEvent(AudioManager.Instance.collisionWithShieldSound, true);
            UIManager.Instance.UpdateShieldBar((float)CurrentShield / (float)maxShield);
        }
        
        int remainingDamage = damage - absoredShieldDamage;

        if(remainingDamage > 0)
        {
            onHealthUpdated?.Invoke(CurrentHealth - remainingDamage);
            CurrentHealth = Mathf.Clamp(CurrentHealth - remainingDamage, 0, maxHealth);

            AudioManager.Instance.PlayEvent(AudioManager.Instance.collisionNoShieldSound, true);
            UIManager.Instance.UpdateHealthBar((float)CurrentHealth / (float)maxHealth);
        }

        if(CurrentHealth <= 0)
        {
            HandleShipDestroyed();
            GameManager.Instance.GameOver(true);
        }
        
        WorldManager.Instance.TriggerShake();
        StartCoroutine(HitImmunity());
    }

    private IEnumerator HitImmunity()
    {
        foreach(SpriteRenderer sprite in shipSprites)
            sprite.color = new Color(1f, 1f, 1f, 0.5f);

        IsImmuneToDamage = true;
        yield return new WaitForSeconds(hitImmunityTime);
        IsImmuneToDamage = false;

        foreach(SpriteRenderer sprite in shipSprites)
            sprite.color = new Color(1f, 1f, 1f, 1f);
    }

    public void RepairShip(int repair)
    {
        if (repair <= 0)
            return;

        if(filterSystem.currentFilterType != FilterType.Recycle)
        {
            Debug.Log("FAILED TO REPAIR");
            AudioManager.Instance.PlayEvent(AudioManager.Instance.lostAHeartSound, true);
            return;
        }

        print("Repairing ship");

        CurrentHealth = Mathf.Clamp(CurrentHealth + repair, 0, maxHealth);
        onHealthUpdated?.Invoke(CurrentHealth);

        AudioManager.Instance.PlayEvent(AudioManager.Instance.healthRegenSound, true);
        UIManager.Instance.UpdateHealthBar((float)CurrentHealth / (float)maxHealth);
    }

    public void RechargeShield(int recharge)
    {
        if (recharge <= 0)
            return;

        if(filterSystem.currentFilterType != FilterType.Energy)
        {
            Debug.Log("FAILED TO RECHARGE");
            AudioManager.Instance.PlayEvent(AudioManager.Instance.lostAHeartSound, true);
            return;
        }

        print("Recharging shield");
        CurrentShield = Mathf.Clamp(CurrentShield + recharge, 0, maxShield);
        onShieldUpdated?.Invoke(CurrentShield);

        AudioManager.Instance.PlayEvent(AudioManager.Instance.shieldRegenSound, true);
        UIManager.Instance.UpdateShieldBar((float)CurrentShield / (float)maxShield);
    }

    void HandleShipDestroyed()
    {
        onShipDestroyed?.Invoke(this);
    }

    #endregion

    #region Input Handlers

    public void OnSteer(Vector2 movementInput)
    {
        this.movementInput = movementInput;
    }

    #endregion

    #region Movement Methods

    private void HandleMovement()
    {
        // if(movementInput.magnitude <= 0.01f)
        //     return;

        Vector2 naturalShipMovement = Vector2.right * 2f;

        rb.velocity = movementInput * speed + naturalShipMovement;
        HandleRotation();

        if(movementInput != Vector2.zero)
        {
            fastThrusterParticlesParent.SetActive(true);
            slowThrusterParticlesParent.SetActive(false);
            AudioManager.Instance.PlayThrustersSound(true);
        }
        else
        {
            slowThrusterParticlesParent.SetActive(true);
            fastThrusterParticlesParent.SetActive(false);
            AudioManager.Instance.StopThrustersSound();
        }
    }

    #endregion

    #region Tilt Methods

    private void HandleRotation()
    {
        float currentRotation = rb.rotation;

        float targetAngle = Mathf.Sign(movementInput.y) * Mathf.Lerp(0f, maxTiltAngle, Mathf.Abs(movementInput.y));
        
        float newRotation = Mathf.SmoothDamp(currentRotation, targetAngle, ref rotationVelocity, rotationSmoothTime);
        
        float angularVelocity = (newRotation - currentRotation) / Time.fixedDeltaTime;
        rb.angularVelocity = angularVelocity;
    }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        filterSystem = GetComponentInChildren<FilterSystem>();
        shipSprites = GetComponentsInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        print(filterSystem.currentFilterType);
    }

    #endregion

}

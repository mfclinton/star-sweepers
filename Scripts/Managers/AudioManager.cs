using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    #region FMOD Events

    [Header("FMOD Events")]
    public StudioEventEmitter cometSound;
    public StudioEventEmitter collisionNoShieldSound;
    public StudioEventEmitter collisionWithShieldSound;
    public StudioEventEmitter healthRegenSound;
    public StudioEventEmitter healthWarningSound;
    public StudioEventEmitter powerDownSound;
    public StudioEventEmitter shieldRegenSound;
    public StudioEventEmitter thrustersSound;
    public StudioEventEmitter trashCollectSound;
    public StudioEventEmitter mainThemeSound;
    public StudioEventEmitter astroPushSound;
    public StudioEventEmitter footstepsSound;
    public StudioEventEmitter interactSound;
    public StudioEventEmitter lostAHeartSound;
    public StudioEventEmitter gameOverTransSound;
    public StudioEventEmitter startMenuTransSound;
    public StudioEventEmitter vacuumSound;
    public StudioEventEmitter timeAlmostUpSound;

    #endregion

    // Helpers
    private bool thrustersPoweringDown;

    // Scheduled
    [Header("Scheduled Events")]
    [SerializeField, Range(0f,100f)] private float healthPTriggerLimit = 50f;
    [SerializeField] private float timeBetweenHealthPings = 5f;
    private float lastHealthPingTime = 0f;

    public static AudioManager Instance { get; private set; }
    private Ship ship;

    void Awake()
    {
        Instance = this;
        lastHealthPingTime = Time.time;
        ship = FindObjectOfType<Ship>();
    }

    private void Update() {
        if(Time.time - lastHealthPingTime >= timeBetweenHealthPings)
        {
            lastHealthPingTime = Time.time;
            float p = ship.HealthP;
            if(p <= healthPTriggerLimit)
                PlayHealthWarningSound(p, true);
        }
    }

    public void PlayThrustersSound(bool isFiring, bool overridePlaying = false)
    {
        overridePlaying = thrustersPoweringDown || overridePlaying;

        thrustersSound.Params[0].Value = isFiring ? 0f : 1f;
        print("Updated");

        // Check if sound is already playing
        if (!overridePlaying && thrustersSound.IsPlaying())
            return;

        // thrustersSound.SetParameter("StopFiring", isFiring ? 0f : 1f);
        thrustersSound.Play();
        print("Playing thrusters sound");

        thrustersPoweringDown = false;
    }

    public void StopThrustersSound()
    {
        thrustersPoweringDown = true;
        thrustersSound.Params[0].Value = 1f;
        thrustersSound.Stop();
    }

    public void PlayHealthWarningSound(float health, bool overridePlaying = false)
    {
        if(health <= 0f)
            return;

        // healthWarningSound.Params[0].Value = health;
        healthWarningSound.SetParameter("Health Low", health);

        if (!overridePlaying && healthWarningSound.IsPlaying())
            return;

        print(health);
        healthWarningSound.Play();
    }

    public void PlayMainThemeSound(bool isPlaying, bool overridePlaying = false)
    {
        ModifyMainThemeFinishLine(isPlaying);

        // TODO
        if (!overridePlaying && mainThemeSound.IsPlaying())
            return;

        mainThemeSound.Play();
    }

    public void ModifyMainThemeFinishLine(bool isPlaying)
    {
        mainThemeSound.SetParameter("FinishLine", isPlaying ? 0f : 1f);
        // mainThemeSound.Params[0].Value = isPlaying ? 0f : 1f;
    }

    public void SetGameOverParam(bool isEndGame)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameOver", isEndGame ? 1f : 0f);
    }

    public void SetPauseParam(bool isPaused)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GamePause", isPaused ? 0f : 1f);
    }

    public void SetMainThemeSpeedParam(float speed)
    {
        mainThemeSound.SetParameter("Speed", speed);
    }

    public void PlayEvent(StudioEventEmitter eventEmitter, bool overridePlaying = false)
    {
        if(!overridePlaying && eventEmitter.IsPlaying())
            return;

        // Debug.Log("Playing event: " + eventEmitter.name);
        eventEmitter.Play();
    }

    public void StopEvent(StudioEventEmitter eventEmitter)
    {
        // Debug.Log("Stopped event: " + eventEmitter.name);
        eventEmitter.Stop();
    }

    
}

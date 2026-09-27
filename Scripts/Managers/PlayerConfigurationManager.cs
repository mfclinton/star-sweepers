using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using FMODUnity;

public class PlayerConfigurationManager : MonoBehaviour
{
    #region Config

    [Header("Config")]
    [SerializeField] private int MaxPlayers = 2;
    [SerializeField] private float countdownTime = 5f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI numPlayersText;
    [SerializeField] private TextMeshProUGUI headerText;

    [Header("Audio")]
    [SerializeField] private StudioEventEmitter playerJoinsSound;
    [SerializeField] private StudioEventEmitter startSoundEmitter;
    [SerializeField] private StudioEventEmitter mainThemeEmitter;

    #endregion

    #region Internal Variables

    public static List<PlayerConfiguration> playerConfigs;
    public static PlayerConfigurationManager Instance { get; private set; }
    private Coroutine countdownCoroutine;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        Instance = this;
        playerConfigs = new List<PlayerConfiguration>();        
    }

    #endregion

    #region Player Input Manager Events

    public void OnPlayerJoined(PlayerInput pi)
    {
        Debug.Log("player joined " + pi.playerIndex);
        pi.transform.SetParent(transform);

        if(!playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
            playerConfigs.Add(new PlayerConfiguration(pi));

        pi.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(pi.playerIndex);
        
        // Update UI
        numPlayersText.text = $"{playerConfigs.Count} / {MaxPlayers}";
        if(playerConfigs.Count == MaxPlayers)
            numPlayersText.transform.parent.gameObject.SetActive(false);

        SetHeaderHint();

        playerJoinsSound.Play();
    }

    public void OnPlayerLeft(PlayerInput pi)
    {
        Debug.Log("player left " + pi.playerIndex);
        // PlayerConfiguration pc = playerConfigs.Find(p => p.PlayerIndex == pi.playerIndex);
        // playerConfigs.Remove(pc);
    }

    #endregion

    #region Public Methods

    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    public void SetPlayerCharacterName(int index, string characterName)
    {
        playerConfigs[index].CharacterName = characterName;
    }

    public void SetPlayerCharacterHeadSprite(int index, Sprite characterHeadSprite)
    {
        playerConfigs[index].CharacterHeadSprite = characterHeadSprite;
    }

    public bool TogglePlayerReady(int index)
    {
        playerConfigs[index].isReady = !playerConfigs[index].isReady;
        if (playerConfigs.Count == MaxPlayers && playerConfigs.All(p => p.isReady))
            StartCountdown();
        else
            StopCountdown();

        return playerConfigs[index].isReady;
    }

    #endregion

    #region Private Methods

    private void StartCountdown()
    {
        StopCountdown();
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private void StopCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        SetHeaderHint();
    }    

    private IEnumerator CountdownCoroutine()
    {
        float timer = countdownTime;
        float tStep = .1f;
        while (0 < timer)
        {
            string timeString = Mathf.Max(0, timer).ToString("F1");
            headerText.text = $"Starting in <color=green>{timeString}s</color>";
            yield return new WaitForSeconds(tStep);
            timer -= tStep;
        }

        foreach(PlayerConfiguration pc in playerConfigs)
            pc.Input.DeactivateInput();

        startSoundEmitter.Play();
        mainThemeEmitter.Stop();
        SceneLoader.Instance.LoadSceneWithFade("Scenes/Game");
    }

    private void SetHeaderHint()
    {
        if (playerConfigs.Count != MaxPlayers)
        {
            headerText.text = "Press Start to Join Game";
        }
        else
        {
            headerText.text = "Set User Tag, Player Model, and Ready Up!";
        }
    }

    #endregion
}

// Player Config Data Class
public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi)
    {
        Input = pi;

        PlayerIndex = pi.playerIndex;
        device = pi.devices[0];
    }

    // Player Input Data
    public PlayerInput Input { get; private set; }
    
    public int PlayerIndex { get; private set; }
    public InputDevice device { get; private set; }
    
    // Character Select Data
    public string CharacterName { get; set; }
    public Sprite CharacterHeadSprite { get; set; }

    // Menu Data
    public bool isReady { get; set; }
}
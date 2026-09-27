using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

[System.Serializable]
public class PlayerControllerVariantMap
{
    public Sprite spriteKey;
    public GameObject playerPrefab;
}

public class GameManager : MonoBehaviour
{
    #region Configuration Variables

    [SerializeField] private GameObject[] spawnPoints;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int maxPlayerHealth = 2;
    public int MaxPlayerHealth => maxPlayerHealth;
    
    [SerializeField] private float levelLengthMinutes = 4f;
    [SerializeField] private TextMeshProUGUI levelTimerText;
    private float timeRemainingInSeconds;
    private bool isGameEnded;
    private bool timeWarningSoundPlayed;
    
    [SerializeField] private PlayerControllerVariantMap[] playerControllerVariantMap;

    #endregion

    #region Game State Variables

    LinkedList<PlayerController> players = new LinkedList<PlayerController>();
    public LinkedList<Ship> ships = new LinkedList<Ship>();
    public int Points { get; private set; }
    private bool gameIsOver;

    #endregion

    #region Game Stats

    public int organicTrashCollected;
    public int recycleableTrashCollected;
    public int energyTrashCollected;
    public int TotalTrashCollected => organicTrashCollected + recycleableTrashCollected + energyTrashCollected;

    public int numSuccessfulFilters;
    // Check for 0
    public float FilterSuccessRate => TotalTrashCollected == 0 ? 0f : (float)numSuccessfulFilters / (float)TotalTrashCollected;
    public int numPlayersInjured;

    #endregion

    #region Component References

    public static GameManager Instance { get; private set;}
    public PlayerInputManager PlayerInputManager { get; private set; }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        Instance = this;
        PlayerInputManager = GetComponent<PlayerInputManager>();

        LeaderboardManager.Login(CreateTeamString());
    }

    private void Start() {
        StartGame();
        InitializeShips();
    }

    private void Update() {
        UpdateTimeRemaining();
    }

    #endregion

    #region Multiplayer Methods

    private void CreatePlayersFromConfig()
    {
        foreach (PlayerConfiguration playerConfig in PlayerConfigurationManager.playerConfigs)
        {
            GameObject newPrefab = playerControllerVariantMap.FirstOrDefault(x => x.spriteKey == playerConfig.CharacterHeadSprite).playerPrefab;
            PlayerInputManager.playerPrefab = newPrefab;
            PlayerInputManager.JoinPlayer(playerConfig.PlayerIndex, -1, null, playerConfig.device);
        }
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log("Player Joined");
        UIManager.Instance.playerInputs.Add(playerInput);
        InitializePlayer(playerInput);
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.Log("Player Left");
        PlayerController playerController = playerInput.GetComponent<PlayerController>();
        players.Remove(playerController);
        UIManager.Instance.playerInputs.Remove(playerInput);
    }

    #endregion

    #region Game State Methods

    void StartTimer()
    {
        timeRemainingInSeconds = levelLengthMinutes * 60f;
        timeWarningSoundPlayed = false;
    }

    public void StartGame()
    {
        CreatePlayersFromConfig();
        StartTimer();

        AudioManager.Instance.SetMainThemeSpeedParam(0f);
        AudioManager.Instance.SetGameOverParam(false);
        AudioManager.Instance.ModifyMainThemeFinishLine(true);
        AudioManager.Instance.SetPauseParam(false);
    }

    private void UpdateTimeRemaining()
    {
        if (isGameEnded)
            return;

        timeRemainingInSeconds -= Time.deltaTime;
        if (timeRemainingInSeconds <= 0f)
        {
            timeRemainingInSeconds = 0f;
            isGameEnded = true;
            GameOver(false);
        }

        int minutes = Mathf.FloorToInt(timeRemainingInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeRemainingInSeconds % 60f);
        levelTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if(timeRemainingInSeconds <= 30f && !timeWarningSoundPlayed)
        {
            AudioManager.Instance.SetMainThemeSpeedParam(1f);
            AudioManager.Instance.PlayEvent(AudioManager.Instance.timeAlmostUpSound);
            timeWarningSoundPlayed = true;
        }
    }

    public void FilteredWrong()
    {
        Debug.Log("Filtered Wrong");
        AudioManager.Instance.PlayEvent(AudioManager.Instance.lostAHeartSound, true);
    }

    public void GainPoints(int points)
    {
        if (points <= 0)
            return;
        
        GameManager.Instance.numSuccessfulFilters++;

        Points += points;
        Debug.Log("You gained " + points + " points!");

        AudioManager.Instance.PlayEvent(AudioManager.Instance.trashCollectSound);
        UIManager.Instance.UpdateScore(Points);
    }

    public void LosePoints(int points)
    {
        Points -= points;
        UIManager.Instance.UpdateScore(Points);
    }

    public void GameOver(bool died)
    {
        if(gameIsOver)
            return;

        gameIsOver = true;

        Debug.Log("The Game Is Over");

        AudioManager.Instance.SetMainThemeSpeedParam(0f);
        AudioManager.Instance.StopThrustersSound();
        if(died)
        {
            AudioManager.Instance.SetGameOverParam(true);
        }
        else
        {
            AudioManager.Instance.ModifyMainThemeFinishLine(false);
        }

        UIManager.Instance.OpenGameOverMenu();
        

        // Update All Leaderboard Stuff
        print("Sending Score: " + Points);
        LeaderboardManager.SendScore(Points);
        LeaderboardManager.GetTopNScores(3);
        LeaderboardManager.GetPlayerRank();
    }

    #endregion

    #region Player Methods

    public string CreateTeamString()
    {
        List<string> teamTags = new List<string>();
        foreach (PlayerConfiguration pc in PlayerConfigurationManager.playerConfigs)
            teamTags.Add(pc.CharacterName);

        teamTags.Sort();
        string teamString = string.Join(" & ", teamTags);

        Debug.Log("Team String: " + teamString);

        return teamString;
    }

    public void InitializePlayer(PlayerInput playerInput)
    {
        // Spawn Player
        playerInput.transform.position = spawnPoints[playerInput.playerIndex].transform.position;

        // Read in player data

        // Initialize Player Controller
        PlayerController playerController = playerInput.GetComponent<PlayerController>();
        playerController.InitializePlayer();

        // Add to list of players
        players.AddLast(playerController);
    }

    public Vector2 FindAnyOpenSpawnpoint()
    {
        foreach (GameObject spawnPoint in spawnPoints)
        {
            if (Physics2D.OverlapCircle(spawnPoint.transform.position, 1f, playerLayer) == null)
            {
                return spawnPoint.transform.position;
            }
        }

        return spawnPoints[0].transform.position;
    }

    #endregion

    #region Ship Methods

    public void InitializeShips()
    {
        ships = new LinkedList<Ship>(FindObjectsOfType<Ship>());
        foreach (Ship ship in ships)
            InitializeShip(ship);

    }

    public void InitializeShip(Ship ship)
    {
        ship.InitializeShip();
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PlayFab.ClientModels;

[System.Serializable]
public class LeaderboardUIEntry
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
}

public class UIManager : MonoBehaviour
{
    #region HUD

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image shieldBarFill;
    
    #endregion

    #region Pause Menu

    [Header("Menus")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject howToPlayMenu;
    [SerializeField] private GameObject leaderboardMenu;

    [SerializeField] private Button firstPauseMenuButton;
    [SerializeField] private Button firstGameOverMenuButton;
    [SerializeField] private Button firstHowToPlayMenuButton;
    [SerializeField] private Button firstLeaderboardMenuButton;

    [SerializeField] private Button openHowToPlayMenuButton;

    [SerializeField] private LeaderboardUIEntry[] topPlayerLeaderboardEntryUI;
    [SerializeField] private LeaderboardUIEntry thisRunLeaderboardEntryUI;

    #endregion

    #region Game Over Menu

    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI organicTrashCollectedText;
    [SerializeField] private TextMeshProUGUI recycleableTrashCollectedText;
    [SerializeField] private TextMeshProUGUI energyTrashCollectedText;
    [SerializeField] private TextMeshProUGUI filterSuccessRateText;
    [SerializeField] private TextMeshProUGUI playersInjuredText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private float disabledTime = 2f;

    #endregion

    #region Internal Variables

    // Internal References
    public List<PlayerInput> playerInputs;
    private List<string> currentActionMaps;
    
    public static UIManager Instance { get; private set; }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        Instance = this;
        
        // Initialize the list of player inputs
        playerInputs = new List<PlayerInput>();
        currentActionMaps = new List<string>();

        LeaderboardManager.onGetTopNScores += UpdateLeaderboard;
        LeaderboardManager.onGetPlayerRank += UpdatePlayerRank;
    }

    #endregion

    #region UI Updaters

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString("D4");
    }

    public void UpdateHealthBar(float healthT)
    {
        healthBarFill.fillAmount = healthT;
    }

    public void UpdateShieldBar(float shieldT)
    {
        shieldBarFill.fillAmount = shieldT;
    }

    #endregion

    #region Leaderboard Update Methods

    public void UpdateLeaderboard(List<PlayerLeaderboardEntry> updatedLeaderboard)
    {
        for (int i = 0; i < topPlayerLeaderboardEntryUI.Length; i++)
        {
            if (i < updatedLeaderboard.Count)
            {
                topPlayerLeaderboardEntryUI[i].nameText.text = StructureLeaderboardEntryName(updatedLeaderboard[i]);
                topPlayerLeaderboardEntryUI[i].scoreText.text = StructureLeaderboardEntryScore(updatedLeaderboard[i]);
            }
            else
            {
                topPlayerLeaderboardEntryUI[i].nameText.text = "-";
                topPlayerLeaderboardEntryUI[i].scoreText.text = "-";
            }
        }
    }

    public void UpdatePlayerRank(PlayerLeaderboardEntry player)
    {
        thisRunLeaderboardEntryUI.nameText.text = StructureLeaderboardEntryName(player);
        thisRunLeaderboardEntryUI.scoreText.text = StructureLeaderboardEntryScore(player);
    }

    private string StructureLeaderboardEntryName(PlayerLeaderboardEntry entry)
    {
        return $"{entry.Position + 1}. {entry.DisplayName}";
    }

    private string StructureLeaderboardEntryScore(PlayerLeaderboardEntry entry)
    {
        // 4 digits
        return $"{entry.StatValue.ToString("D4")}";
    }

    #endregion

    #region UI Mode + Navigation

    public void ToggleUIMode(bool isActive)
    {
        if (isActive)
        {
            // Store the current active action maps
            foreach (var playerInput in playerInputs)
            {
                currentActionMaps.Add(playerInput.currentActionMap.name);

                // Switch to the UI action map
                playerInput.SwitchCurrentActionMap("UI");
                print("Switching to UI action map");
            }

            // Pause the game
            Time.timeScale = 0f;

        }
        else
        {
            // Switch back to the previous action maps
            for (int i = 0; i < playerInputs.Count; i++)
            {
                playerInputs[i].SwitchCurrentActionMap(currentActionMaps[i]);
            }
            currentActionMaps.Clear();

            // Resume the game
            Time.timeScale = 1f;

        }
    }

    public void TogglePauseMenu()
    {
        bool isActive = !pauseMenu.activeSelf;
        pauseMenu.SetActive(isActive);
        ToggleUIMode(isActive);

        // Set the first button
        if (isActive)
        {
            firstPauseMenuButton.Select();
        }

        AudioManager.Instance.SetPauseParam(isActive);
    }

    public void OpenGameOverMenu()
    {
        gameOverMenu.SetActive(true);

        ToggleUIMode(true);
        Time.timeScale = 0f;

        firstGameOverMenuButton.Select();

        // Update the game over menu stats
        int organicTrashCollected = GameManager.Instance.organicTrashCollected;
        int recycleableTrashCollected = GameManager.Instance.recycleableTrashCollected;
        int energyTrashCollected = GameManager.Instance.energyTrashCollected;
        int filterSuccessRate = Mathf.RoundToInt(GameManager.Instance.FilterSuccessRate * 100f);

        organicTrashCollectedText.text = organicTrashCollected.ToString();
        recycleableTrashCollectedText.text = recycleableTrashCollected.ToString();
        energyTrashCollectedText.text = energyTrashCollected.ToString();
        filterSuccessRateText.text = filterSuccessRate.ToString() + "%";
        playersInjuredText.text = GameManager.Instance.numPlayersInjured.ToString();
    
        int finalScore = GameManager.Instance.Points;
        finalScoreText.text = finalScore.ToString("D4");

        ToggleLeaderboardMenu(true);
        SetButtonToInactive(firstLeaderboardMenuButton, disabledTime);
    }

    #endregion

    #region Basic UI Navigation

    // Set Button To Inactive for a certain amount of time
    public void SetButtonToInactive(Button button, float time)
    {
        button.interactable = false;
        StartCoroutine(EnableButtonAfterTime(button, time));
    }

    private IEnumerator EnableButtonAfterTime(Button button, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        button.interactable = true;
        button.Select();
    }

    public void ToggleHowToPlayMenu(bool isActive)
    {
        howToPlayMenu.SetActive(isActive);

        if (isActive)
            firstHowToPlayMenuButton.Select();
        else
            openHowToPlayMenuButton.Select();
    }

    public void ToggleLeaderboardMenu(bool isActive)
    {
        leaderboardMenu.SetActive(isActive);

        if (isActive)
            firstLeaderboardMenuButton.Select();
        else
            firstGameOverMenuButton.Select();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadSceneWithFade("Scenes/MainMenu");
    }

    public void ReturnToGame()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadSceneWithFade("Scenes/Game");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadSceneWithFade("Scenes/Game");
        AudioManager.Instance.SetPauseParam(false);
    }
    
    #endregion
}

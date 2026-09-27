using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DanielLochner.Assets.SimpleScrollSnap;

public class PlayerSetupMenuController : MonoBehaviour
{

    #region UI References

    [Header("UI References")]
    [SerializeField] private Button readyButton;
    public Button ReadyButton { get => readyButton; }

    #endregion

    #region Component References

    [Header("Component References")]
    [SerializeField] private SimpleScrollSnap characterSelectScroll;
    private LetterNodeManager usernameLetterNodeManager;

    #endregion

    #region Internal Variables

    private int playerIndex;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        usernameLetterNodeManager = GetComponentInChildren<LetterNodeManager>();
    }

    void Start()
    {
        usernameLetterNodeManager.EnterLetterNodeEditMode();
    }

    #endregion

    #region Public Methods

    public void SetPlayerIndex(int pi)
    {
        playerIndex = pi;
    }

    public void SetPlayerCharacterName()
    {
        name = usernameLetterNodeManager.GetCompleteText();
        PlayerConfigurationManager.Instance.SetPlayerCharacterName(playerIndex, name);
        // Debug.Log("Player " + playerIndex + "'s name is " + name);
    }

    public void SetPlayerCharacterHeadSprite()
    {
        Sprite sprite = characterSelectScroll.Panels[characterSelectScroll.CenteredPanel].GetComponent<Image>().sprite;
        PlayerConfigurationManager.Instance.SetPlayerCharacterHeadSprite(playerIndex, sprite);
        // Debug.Log("Player " + playerIndex + "'s head sprite is " + sprite.name);
    }

    public void ToggleReadyPlayer()
    {
        bool isReady = PlayerConfigurationManager.Instance.TogglePlayerReady(playerIndex);
        StartGame();

        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = isReady ? "Ready!" : "Ready?";
        readyButton.GetComponentInChildren<Image>().color = isReady ? Color.green : Color.white;
    }

    public void StartGame()
    {
        SetPlayerCharacterHeadSprite();
        SetPlayerCharacterName();
    }

    #endregion
}
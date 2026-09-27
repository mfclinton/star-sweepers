using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class LetterNodeManager : MonoBehaviour
{
    #region Setup
    
    [Header("Setup")]
    [SerializeField] private PlayerScrollerController playerScrollerController;
    [SerializeField] private StudioEventEmitter scrollUpEventSound, scrollDownEventSound, goToNextNodeEventSound;

    #endregion

    #region Configuration Variables

    [Header("Configuration Variables")]
    [SerializeField] private bool loopAround = true;
    [SerializeField] private float navigationDelay = 0.5f;

    #endregion

    #region Internal Variables

    private LetterNode[] letterNodes;
    private int currentLetterNodeIndex = 0;
    private float lastNavigationTime = 0f;

    #endregion

    #region Unity Callbacks

    private void Awake() {
        letterNodes = GetComponentsInChildren<LetterNode>();
        ResetUI();
    }

    #endregion

    #region Input Methods

    public void EnterLetterNodeEditMode() {
        bool success = playerScrollerController.EnterScrollerState();
        if(!success)
            return;

        currentLetterNodeIndex = 0;
        UpdateUI();
    }

    public void ExitLetterNodeEditMode() {
        playerScrollerController.ExitScrollerState();
        ResetUI();
    }

    #endregion

    #region Public Methods

    public string GetCompleteText() {
        
        string selectedText = "";
        foreach (LetterNode letterNode in letterNodes)
            selectedText += letterNode.SelectedText;

        return selectedText;
    }

    public void NavigateCurNode(bool isUp) {
        if(Time.time - lastNavigationTime < navigationDelay)
            return;

        if (isUp)
        {
            letterNodes[currentLetterNodeIndex].UseTopArrow();
            scrollUpEventSound.Play();
        }
        else
        {
            letterNodes[currentLetterNodeIndex].UseBottomArrow();
            scrollDownEventSound.Play();
        }

        lastNavigationTime = Time.time;
    }

    public void NavigateToNextNode(bool isRight) {
        if(Time.time - lastNavigationTime < navigationDelay)
            return;

        SelectLetterNode(isRight);
        goToNextNodeEventSound.Play();

        lastNavigationTime = Time.time;
    }

    #endregion

    #region Internal Methods

    private void SelectLetterNode(bool isNext) {
        int change = isNext ? 1 : -1;
        int edgeIndex = isNext ? letterNodes.Length - 1 : 0;
        
        if (currentLetterNodeIndex != edgeIndex)
            currentLetterNodeIndex += change;
        else if (loopAround)
            currentLetterNodeIndex = isNext ? 0 : letterNodes.Length - 1;

        UpdateUI();
    }

    private void UpdateLetterNodeColors() {
        for (int i = 0; i < letterNodes.Length; i++)
            letterNodes[i].SetBackgroundColor(i == currentLetterNodeIndex);
    }

    private void UpdateLetterNodeArrowVisibility() {
        for (int i = 0; i < letterNodes.Length; i++)
            letterNodes[i].SetArrowVisibility(i == currentLetterNodeIndex);
    }

    public void UpdateUI() {
        UpdateLetterNodeColors();
        UpdateLetterNodeArrowVisibility();
    }

    public void ResetUI() {
        currentLetterNodeIndex = -1;
        UpdateUI();
        currentLetterNodeIndex = 0;
    }

    #endregion
}

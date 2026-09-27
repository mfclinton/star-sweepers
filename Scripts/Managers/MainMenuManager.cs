using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Camera Panning")]
    [SerializeField] private float panSpeed = 0.25f;

    [Header("Audio")]
    [SerializeField] private StudioEventEmitter mainThemeEmitter; 

    [Header("UI")]
    [SerializeField] private GameObject howToPlayMenu;
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button closeHowToPlayButton;

    [Header("UI Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject setupMenu;

    private Camera mainCamera;

    #region Unity Callbacks

    private void Awake() {
        mainCamera = Camera.main;
    }

    private void FixedUpdate() {
        mainCamera.transform.position += Vector3.right * panSpeed * Time.fixedDeltaTime;
    }

    void Update() {
        if( FMODUnity.RuntimeManager.HasBankLoaded("Master") && !mainThemeEmitter.IsPlaying())
            mainThemeEmitter.Play();
    }

    #endregion

    #region Button Methods

    public void SetMenusState(bool isInSetupMenu)
    {
        mainMenu.SetActive(!isInSetupMenu);
        setupMenu.SetActive(isInSetupMenu);
    }

    public void ToggleHowToPlayMenu(bool isActive)
    {
        howToPlayMenu.SetActive(isActive);

        if(isActive)
            closeHowToPlayButton.Select();
        else
            howToPlayButton.Select();
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float scrollDelay = 0.5f;

    [Header("Setup")]
    [SerializeField] private Button button;

    // Components
    private MultiplayerEventSystem multiplayerEventSystem;
    private SimpleScrollSnap simpleScrollSnap;

    // Internal Variables
    private Vector2 inputDir;
    private float lastScrollTime = 0f;
    
    void Awake()
    {
        multiplayerEventSystem = GetComponent<PlayerInput>().uiInputModule.GetComponent<MultiplayerEventSystem>();
        simpleScrollSnap = button.GetComponentInChildren<SimpleScrollSnap>();
    }

    private void Update() {
        ProcessScrollDirection();
    }

    void OnNavigate(InputValue value)
    {
        if (button == null || !button.interactable || button.gameObject != multiplayerEventSystem.currentSelectedGameObject)
        {
            inputDir = Vector2.zero;
            return;
        }

        inputDir = value.Get<Vector2>();
    }

    void ProcessScrollDirection()
    {
        if (Mathf.Abs(inputDir.y) < Mathf.Abs(inputDir.x) && 0.5f < inputDir.magnitude)
            Scroll(0 < inputDir.x);
    }

    void Scroll(bool forward)
    {
        if(Time.time < lastScrollTime + scrollDelay)
            return;

        if (forward)
            simpleScrollSnap.GoToNextPanel();
        else
            simpleScrollSnap.GoToPreviousPanel();

        lastScrollTime = Time.time;
    }
}

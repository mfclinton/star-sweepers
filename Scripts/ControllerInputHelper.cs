using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem.UI;

public class ControllerInputHelper : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private PlayerInput playerInput;


    private EventSystem eventSystem;

    private GameObject lastSelectedGameObject;
    private InputAction navigateAction;

    private void Awake()
    {
        eventSystem = GetComponent<EventSystem>();

        if(playerInput == null)
            navigateAction = inputActions.FindActionMap("UI").FindAction("Navigate");
        else
            navigateAction = playerInput.actions.FindActionMap("UI").FindAction("Navigate");

        if (navigateAction != null)
            navigateAction.performed += HandleNavigation;
    }

    private void Update()
    {
        if (eventSystem.currentSelectedGameObject != null)
            lastSelectedGameObject = eventSystem.currentSelectedGameObject;
    }

    private void OnEnable()
    {
        navigateAction.Enable();
        eventSystem.SetSelectedGameObject(null);
        StartCoroutine(SelectButtonLater());
    }

    private void OnDisable()
    {
        navigateAction.Disable();
    }

    private void HandleNavigation(InputAction.CallbackContext context)
    {
        print(context.action.actionMap.asset);
        if (eventSystem.currentSelectedGameObject == null)
            eventSystem.SetSelectedGameObject(lastSelectedGameObject);
    }

    private IEnumerator SelectButtonLater()
    {
        yield return null; // Wait for one frame.
        if (lastSelectedGameObject != null)
        {
            eventSystem.SetSelectedGameObject(lastSelectedGameObject);
        }
        else
        {
            Selectable firstSelectable = FindFirstEnabledSelectable<Selectable>();
            if(firstSelectable != null)
                eventSystem.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }

    private T FindFirstEnabledSelectable<T>() where T : Selectable
    {
        T[] selectables = GameObject.FindObjectsOfType<T>();
        foreach (var selectable in selectables)
        {
            if (selectable.IsActive() && selectable.IsInteractable())
                return selectable;
        }
        return null;
    }
}

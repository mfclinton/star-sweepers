using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SharedPlayerUIHandler : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float navigationDelay = 0.1f;
    [SerializeField] private float submitDelay = 0.1f;
    [SerializeField] private float navigationMagnitudeThreshold = 0.5f;

    // Internal Variables
    private float lastNavigationTime = 0f;

    // Component References
    private PlayerInput playerInput;
    private EventSystem eventSystem;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        eventSystem = GameObject.FindObjectOfType<EventSystem>();
    }

    public void OnNavigate(InputValue value)
    {
        var input = value.Get<Vector2>();

        if (input.magnitude < navigationMagnitudeThreshold)
            return;

        var selectedObject = eventSystem.currentSelectedGameObject;
        if (selectedObject == null) // TODO
            return;

        var selectable = selectedObject.GetComponent<Selectable>();
        if (selectable == null)
            return;

        if (Time.unscaledTime - lastNavigationTime < navigationDelay)
            return;
        lastNavigationTime = Time.unscaledTime;

        Selectable nextSelectable = null;
        if (input.y > 0)
            nextSelectable = selectable.FindSelectableOnUp();
        else if (input.y < 0)
            nextSelectable = selectable.FindSelectableOnDown();
        else if (input.x < 0)
            nextSelectable = selectable.FindSelectableOnLeft();
        else if (input.x > 0)
            nextSelectable = selectable.FindSelectableOnRight();

        Debug.Log("Next Selectable: " + nextSelectable);

        if (nextSelectable != null)
            eventSystem.SetSelectedGameObject(nextSelectable.gameObject);
    }
    
    public void OnSubmit(InputValue value)
    {
        // Check if the Submit action was triggered
        if (value.isPressed)
        {
            // Check if a GameObject is currently selected
            if (eventSystem.currentSelectedGameObject != null)
            {
                var button = eventSystem.currentSelectedGameObject.GetComponent<Button>();
                if (button != null)
                    button.onClick.Invoke();
                else
                    Debug.LogWarning("The currently selected GameObject is not a button.");
            }
            else
                Debug.LogWarning("No GameObject is currently selected in the EventSystem.");
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#region Filter Helper DataStructs

public enum FilterType
{
    Recycle,
    Organic,
    Energy
}

[System.Serializable]
public class FilterVisualDetails
{
    [SerializeField] private Color filterColor;
    [SerializeField] private Vector2 direction;
    [SerializeField] private Sprite pipelineEndSprite;

    public Color FilterColor { get { return filterColor; } }
    public Vector2 Direction { get { return direction; } }
    public Sprite PipelineEndSprite { get { return pipelineEndSprite; } }

    public float GetDirectionAngle()
    {
        return Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg - 90;
    }
}

#endregion

#region Filter System

public class FilterSystem : MonoBehaviour
{
    #region System Details

    [SerializeField] private FilterVisualDetails recycleDetails;
    [SerializeField] private FilterVisualDetails organicDetails;
    [SerializeField] private FilterVisualDetails energyDetails;

    #endregion

    #region Sprite References

    [SerializeField] private SpriteRenderer arrow;
    [SerializeField] private SpriteRenderer arrowBG;

    [SerializeField] private Image pipelineEndImage;

    [Header("Rumble")]
    [SerializeField] private float rumbleTime = .2f;
    [SerializeField] private float rumbleMagnitude = 5f;

    #endregion

    #region State

    public FilterType currentFilterType { get; private set;}
    private Coroutine currentShakeCoroutine;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        SetCurrentFilter(FilterType.Recycle);
    }

    #endregion

    #region PublicMethods

    public void SetCurrentFilter(FilterType newFilterType)
    {
        currentFilterType = newFilterType;
        VisualizeFilterSystem();
    }

    public void CycleFilter()
    {
        // Use modulo and int cast to cycle through the enum
        int newFilterType = ((int)currentFilterType + 1) % System.Enum.GetValues(typeof(FilterType)).Length;
        SetCurrentFilter((FilterType)newFilterType);
    }

    #endregion

    #region PrivateMethods

    IEnumerator ShakeUIElement(RectTransform target, float duration, float magnitude)
    {
        Vector2 originalPos = target.anchoredPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);

            target.anchoredPosition = originalPos + new Vector2(x, y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.anchoredPosition = originalPos;
    }

    private void VisualizeFilterSystem()
    {
        FilterVisualDetails currentVisualDetails = GetCurrentVisualDetails();

        arrowBG.color = currentVisualDetails.FilterColor;
        arrow.transform.localRotation = Quaternion.Euler(0, 0, currentVisualDetails.GetDirectionAngle());

        pipelineEndImage.sprite = currentVisualDetails.PipelineEndSprite;

        if(currentShakeCoroutine != null)
            StopCoroutine(currentShakeCoroutine);
    
        // Start a new shake effect
        currentShakeCoroutine = StartCoroutine(ShakeUIElement(pipelineEndImage.rectTransform, rumbleTime, rumbleMagnitude));
    }

    private FilterVisualDetails GetCurrentVisualDetails()
    {
        switch(currentFilterType)
        {
            case FilterType.Recycle:
                return recycleDetails;

            case FilterType.Organic:
                return organicDetails;

            case FilterType.Energy:
                return energyDetails;

            default:
                Debug.LogError("Unknown filter type: " + currentFilterType);
                return null;
        }
    }

    #endregion
}

#endregion

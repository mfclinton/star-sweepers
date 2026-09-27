using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilterProcessor : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Transform pipeTransform;
    [SerializeField] private Image filterPrefab;
    public float pipelineTime = 5f;
    [SerializeField] private float bounceVisualSpeed = 15f;
    [SerializeField] private float bounceVisualAmplitude = .27f;

    public List<QueuedFilterElement> elementsToProcess;
    public List<Image> imagePool;
    FilterSystem filterSystem;
    public static FilterProcessor Instance { get; private set; }

    [System.Serializable]
    public class QueuedFilterElement
    {
        public Image image;
        public FilterType filter;
        public int points;
        public float t;
    }


    private void Awake() {
        Instance = this;
        elementsToProcess = new List<QueuedFilterElement>();
        filterSystem = FindObjectOfType<FilterSystem>();
    }

    void Update()
    {
        ProcessQueue();
    }

    private Image GetPooledImage()
    {
        foreach (var pooledImage in imagePool)
        {
            if (!pooledImage.gameObject.activeInHierarchy)
            {
                pooledImage.gameObject.SetActive(true);
                return pooledImage;
            }
        }

        // If no inactive image was found, create a new one, add it to the pool, and return it
        Image newPooledImage = Instantiate(filterPrefab, pipeTransform);
        imagePool.Add(newPooledImage);
        return newPooledImage;
    }

    public void AddToQueue(FilterType filter, int points, Sprite sprite)
    {
        Image image = GetPooledImage();
        image.sprite = sprite;

        QueuedFilterElement newElement = new QueuedFilterElement
        {
            image = image,
            filter = filter,
            points = points,
            t = 0
        };

        elementsToProcess.Add(newElement);
    }

    private void ProcessQueue()
    {
        float tIncrement = Time.deltaTime / pipelineTime;

        for (int i = elementsToProcess.Count - 1; i >= 0; i--)
            ProcessElementMovement(elementsToProcess[i], tIncrement);
    }

    private void ProcessElementMovement(QueuedFilterElement element, float tIncrement)
    {
        element.t += tIncrement;

        if (element.t >= 1)
        {
            ProcessElementData(element);
            RemoveElementFromQueue(element);
        }
        else
        {
            UpdateElementPosition(element);
        }
    }

    private void ProcessElementData(QueuedFilterElement element)
    {
        if (element.filter == filterSystem.currentFilterType)
        {
            GameManager.Instance.GainPoints(element.points);
        }
        else
        {
            GameManager.Instance.FilteredWrong();
        }
    }

    private void RemoveElementFromQueue(QueuedFilterElement element)
    {
        elementsToProcess.Remove(element);
        element.image.gameObject.SetActive(false);
    }

    private void UpdateElementPosition(QueuedFilterElement element)
    {
        RectTransform parentRect = element.image.rectTransform.parent as RectTransform;

        // Interpolate the x position from the left edge of the parent (-halfWidth) to the right edge (+halfWidth)
        float relativeXPos = Mathf.Lerp(-parentRect.rect.width / 2, parentRect.rect.width / 2, element.t);
        float relativeYPos = element.image.rectTransform.rect.height / 2f * Mathf.Cos(element.t * bounceVisualSpeed) * bounceVisualAmplitude;

        element.image.rectTransform.anchoredPosition = new Vector2(relativeXPos, relativeYPos);
    }

}

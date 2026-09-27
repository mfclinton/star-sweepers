using UnityEngine;
using TMPro;
using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LetterNode : MonoBehaviour
{
    #region Configuration Variables

    [Header("Content Configuration Variables")]
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private TextAsset contentTextAsset;
    [SerializeField] private bool startFromFirstPanel = true;

    [Header("Customization Variables")]
    [SerializeField] private Color selectedBackgroundColor;
    [SerializeField] private Color unselectedBackgroundColor;
    
    #endregion

    #region UI Component References

    [Header("UI Component References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button topArrowButton;
    [SerializeField] private Button bottomArrowButton;

    #endregion

    #region Internal Variables

    private SimpleScrollSnap simpleScrollSnap;

    #endregion

    #region Public Variables

    public string SelectedText => simpleScrollSnap.Panels[simpleScrollSnap.CenteredPanel].GetComponentInChildren<TextMeshProUGUI>().text;

    #endregion

    #region Unity Callbacks

    private void Awake() {
        simpleScrollSnap = GetComponentInChildren<SimpleScrollSnap>();
    }

    private void Start() {
        ParseAndInstantiateObjects();
    }

    #endregion

    #region Instantiation Methods

    private void ParseAndInstantiateObjects()
    {
        if(simpleScrollSnap == null || contentPrefab == null || contentTextAsset == null)
            return;

        DestroyPreviousInstances();

        string[] lines = contentTextAsset.text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            GameObject newContent = simpleScrollSnap.AddToFront(contentPrefab);
            newContent.GetComponentInChildren<TextMeshProUGUI>().SetText(line);
        }

        int index = startFromFirstPanel ? 0 : simpleScrollSnap.NumberOfPanels - 1;
        simpleScrollSnap.GoToPanel(index);
    }

    private void DestroyPreviousInstances()
    {
        if(simpleScrollSnap == null)
            return;

        int numChildren = simpleScrollSnap.NumberOfPanels;
        for (int i = numChildren - 1; i >= 0; i--)
        {
            simpleScrollSnap.Remove(i);
        }
    }

    #endregion

    #region Simple Scroll Methods

    public void OnPanelSelected() {
        // TODO
    }

    #endregion

    #region Interaction Methods

    public void UseTopArrow() {
        BaseEventData eventData = new BaseEventData(EventSystem.current);
        topArrowButton.OnSubmit(eventData);
    }

    public void UseBottomArrow() {
        BaseEventData eventData = new BaseEventData(EventSystem.current);
        bottomArrowButton.OnSubmit(eventData);
    }

    #endregion

    #region Customize Methods

    public void SetBackgroundColor(bool isSelected) {
        backgroundImage.color = isSelected ? selectedBackgroundColor : unselectedBackgroundColor;
    }

    public void SetArrowVisibility(bool isVisible) {
        topArrowButton.gameObject.SetActive(isVisible);
        bottomArrowButton.gameObject.SetActive(isVisible);
    }

    #endregion
}

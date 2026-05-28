using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents an encounter within the list of all encounters on the home screen.
/// </summary>
public class EncounterOptionUI : MonoBehaviour, IPointerClickHandler
{
    private EncounterInstance _encounter;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI difficultyLevelText;
    [SerializeField] private TextMeshProUGUI areaText;

    private Image image;
    private Hoverable hoverable;

    private EncounterListUI encounterListUI;

    /// <summary>Gets or sets the encounter represented by this object. Setting automatically updates the UI.</summary>
    public EncounterInstance Encounter
    {
        get => _encounter;
        set
        {
            _encounter = value;

            nameText.text = _encounter.EncounterData.Name;
            if (_encounter.IsCleared) nameText.fontStyle = FontStyles.Strikethrough;

            difficultyLevelText.text = $"Lvl {_encounter.EncounterData.DifficultyLevel}";
            areaText.text = _encounter.EncounterData.Area.Name;


            image.color = _encounter.EncounterData.Area.Color;
            hoverable.UpdateColors();
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        hoverable = GetComponent<Hoverable>();
    }

    private void Start()
    {
        encounterListUI = FindObjectsByType<EncounterListUI>(FindObjectsSortMode.None)[0];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        encounterListUI.EncounterOptionClicked(this);
    }
}

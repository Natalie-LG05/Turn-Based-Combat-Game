using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the encounter list UI on the home screen.
/// </summary>
public class EncounterListUI : MonoBehaviour
{
    [SerializeField] private GameObject encounterOptionPrefab;

    public EncounterOptionUI SelectedEncounterOption { get; private set; }

    [SerializeField] private HomeEncounterInfoUI encounterInfoUI;

    [SerializeField] private Transform encounterOptionContainer;
    private List<GameObject> encounterOptions;

    private void Awake()
    {
        encounterOptions = new List<GameObject>();
    }

    /// <summary>
    /// Set the list of encounters to display.
    /// </summary>
    /// <param name="encounters">The list of encounters to display.</param>
    public void SetEncounters(List<EncounterInstance> encounters)
    {
        // remove old encounter options (if any)
        foreach (GameObject obj in encounterOptions)
            Destroy(obj);
        encounterOptions.Clear();

        // create the encounter options
        foreach (EncounterInstance encounter in encounters)
            NewEncounterOption(encounter);

        // select the first option by default
        SelectEncounterOption(encounterOptions[0].GetComponent<EncounterOptionUI>());
    }

    /// <summary>
    /// When an encounter option is clicked, select it, deselecting the previously selected option, 
    /// <br/>selecting the clicked option, and displaying info about the newly selected option.
    /// </summary>
    /// <param name="encounterOption">The encounter option that was clicked.</param>
    public void EncounterOptionClicked(EncounterOptionUI encounterOption)
    {
        if (HomeManager.Instance.State == HomeState.EncounterListScreen)
            SelectEncounterOption(encounterOption);
    }

    /// <summary>
    /// Select an encounter option, deselecting the previously selected option and displaying info about the newly selected option.
    /// </summary>
    /// <param name="encounterOption">The encounter option to select.</param>
    private void SelectEncounterOption(EncounterOptionUI encounterOption)
    {
        // Deselect the previously selected encounter option and select the newly clicked one
        if (SelectedEncounterOption != null) SelectedEncounterOption.GetComponent<Hoverable>().Deselect();
        SelectedEncounterOption = encounterOption;
        SelectedEncounterOption.GetComponent<Hoverable>().Select();

        encounterInfoUI.Encounter = encounterOption.Encounter;
    }

    /// <summary>
    /// Create, initialize, and store a new encounter option.
    /// </summary>
    /// <param name="encounter">The encounter being represented by the new option.</param>
    private void NewEncounterOption(EncounterInstance encounter)
    {
        GameObject encounterOption = Instantiate(encounterOptionPrefab, encounterOptionContainer);
        encounterOption.GetComponent<EncounterOptionUI>().Encounter = encounter;
        encounterOptions.Add(encounterOption);
    }
}

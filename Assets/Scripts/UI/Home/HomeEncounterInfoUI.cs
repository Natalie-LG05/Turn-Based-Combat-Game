using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the UI on the home screen that displays information about the encounter that is currently selected in the encounter list UI
/// </summary>
public class HomeEncounterInfoUI : MonoBehaviour
{
    private EncounterInstance _encounter;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI areaText;
    [SerializeField] private TextMeshProUGUI difficultyLevelText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI timesClearedText;

    [SerializeField] private GameObject modifierIconPrefab;
    [SerializeField] private Transform modifierIconContainer;
    private List<GameObject> modifierIcons;

    [SerializeField] private GameObject fixedEncounterRewardUIPrefab;
    [SerializeField] private Transform fixedEncounterRewardUIContainer;
    private List<GameObject> fixedEncounterRewardUIs;

    [SerializeField] private GameObject randomEncounterRewardUIPrefab;
    [SerializeField] private Transform randomEncounterRewardUIContainer;
    private List<GameObject> randomEncounterRewardUIs;

    private void Awake()
    {
        modifierIcons = new List<GameObject>();
        fixedEncounterRewardUIs = new List<GameObject>();
        randomEncounterRewardUIs = new List<GameObject>();
    }

    /// <summary>Gets or sets the encounter for this UI to show info about. Setting automatically updates the UI.</summary>
    public EncounterInstance Encounter
    {
        get { return _encounter; }
        set
        {
            _encounter = value;

            nameText.text = _encounter.EncounterData.Name;
            areaText.text = $"Area: {_encounter.EncounterData.Area.Name}";
            difficultyLevelText.text = $"Difficulty: {_encounter.EncounterData.DifficultyLevel}";
            typeText.text = $"Type: {string.Concat(_encounter.EncounterData.Type.ToString().Select(x => char.IsUpper(x) ? " " + x : x.ToString())).Trim()}";
            timesClearedText.text = $"Times Cleared: {_encounter.TimesCleared}";

            UpdateModifierIcons();
            UpdateEncounterRewards();
        }
    }

    private void UpdateModifierIcons()
    {
        foreach (GameObject icon in modifierIcons)
            Destroy(icon);
        modifierIcons.Clear();

        foreach (EncounterModifierData modifier in _encounter.EncounterData.Modifiers)
            NewModifierIcon(modifier);
    }

    private void NewModifierIcon(EncounterModifierData modifier)
    {
        GameObject icon = Instantiate(modifierIconPrefab, modifierIconContainer);
        modifierIcons.Add(icon);
        icon.GetComponent<EncounterModifierIcon>().ModifierData = modifier;
    }

    private void UpdateEncounterRewards()
    {
        // clear old data
        foreach (GameObject fixedRewardUI in fixedEncounterRewardUIs)
            Destroy(fixedRewardUI);
        fixedEncounterRewardUIs.Clear();
        
        // set new data
        foreach (FixedEncounterReward encounterReward in Encounter.EncounterData.FirstClearRewards)
            NewFixedEncounterReward(encounterReward);

        // clear old data
        foreach (GameObject fixedRewardUI in randomEncounterRewardUIs)
            Destroy(fixedRewardUI);
        randomEncounterRewardUIs.Clear();

        // set new data
        foreach (RandomEncounterReward encounterReward in Encounter.EncounterData.SubsequentClearRewards)
            NewRandomEncounterReward(encounterReward);
    }

    private void NewFixedEncounterReward(FixedEncounterReward encounterReward)
    {
        GameObject rewardUI = Instantiate(fixedEncounterRewardUIPrefab, fixedEncounterRewardUIContainer);
        fixedEncounterRewardUIs.Add(rewardUI);
        rewardUI.GetComponent<FixedEncounterRewardUI>().Reward = encounterReward;
    }

    private void NewRandomEncounterReward(RandomEncounterReward encounterReward)
    {
        GameObject rewardUI = Instantiate(randomEncounterRewardUIPrefab, randomEncounterRewardUIContainer);
        randomEncounterRewardUIs.Add(rewardUI);
        rewardUI.GetComponent<RandomEncounterRewardUI>().Reward = encounterReward;
    }
}
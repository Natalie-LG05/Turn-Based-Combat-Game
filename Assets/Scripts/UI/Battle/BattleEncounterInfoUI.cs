using TMPro;
using UnityEngine;

/// <summary>
/// UI that displays information about the current encounter, including icons for each active encounter modifier.
/// </summary>
public class BattleEncounterInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject modifierIconPrefab;

    private EncounterData encounterData;

    [SerializeField] private TextMeshProUGUI encounterNameText;
    [SerializeField] private TextMeshProUGUI encounterDifficultyText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private Transform modifierIconContainer;

    /// <summary>
    /// Set the encounter and display info about it, including icons for each of its modifiers.
    /// </summary>
    /// <param name="data">The encounter data to display info about.</param>
    public void SetEncounter(EncounterData data)
    {
        encounterData = data;
        SetEncounterNameText(encounterData.Name);
        SetEncounterDifficultyText(encounterData.DifficultyLevel);

        foreach (EncounterModifierData modifier in encounterData.Modifiers)
        {
            GameObject icon = Instantiate(modifierIconPrefab, modifierIconContainer);
            icon.GetComponent<EncounterModifierIcon>().ModifierData = modifier;
        }
    }

    public void SetRound(int round)
    {
        SetRoundText(round);
    }

    private void SetEncounterNameText(string text)
    {
        encounterNameText.text = text;
    }

    private void SetEncounterDifficultyText(int difficultyLevel)
    {
        encounterDifficultyText.text = $"Lvl {difficultyLevel}";
    }

    private void SetRoundText(int round)
    {
        roundText.text = $"Round: {round}";
    }
}

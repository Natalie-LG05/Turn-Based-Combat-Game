using TMPro;
using UnityEngine;

public class EncounterInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject modifierIconPrefab;

    private EncounterData encounterData;

    [SerializeField] private TextMeshProUGUI encounterNameText;
    [SerializeField] private TextMeshProUGUI encounterDifficultyText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private Transform modifierIconContainer;

    public void SetEncounter(EncounterData data)
    {
        encounterData = data;
        SetEncounterNameText(encounterData.Name);
        SetEncounterDifficultyText(encounterData.DifficultyLevel);

        if (encounterData.Modifiers.Length > 0)
        {
            foreach (EncounterModifierData modifier in encounterData.Modifiers)
            {
                GameObject icon = Instantiate(modifierIconPrefab, modifierIconContainer);
                icon.GetComponent<EncounterModifierIcon>().ModifierData = modifier;
            }
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

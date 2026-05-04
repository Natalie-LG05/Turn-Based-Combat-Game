using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A tooltip that displays information about a character. It is a scrollable list containing the character's description 
/// <br/>followed by each of its abilities and status effects.
/// </summary>
public class CharacterInfoTooltip : Tooltip
{
    [SerializeField] private GameObject AbilityInfoUIPrefab;
    [SerializeField] private GameObject EffectInfoUIPrefab;

    private CharacterInstance character;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Transform infoContainer;
    private List<GameObject> abilityInfoUIs;
    private List<GameObject> effectInfoUIs;

    protected override void Awake()
    {
        base.Awake();
        abilityInfoUIs = new List<GameObject>();
        effectInfoUIs = new List<GameObject>();
    }

    /// <summary>
    /// Set the data displayed by this tooltip based on the character assigned to the UI that triggered it. 
    /// <br/>Should only be called by a CharacterUI with a character.
    /// </summary>
    /// <param name="sourceObject">The gameobject that triggered the display of this tooltip.</param>
    public override void SetTooltipData(GameObject sourceObject)
    {
        base.SetTooltipData(sourceObject);
        character = sourceObject.GetComponentInParent<CharacterUI>().Character;

        SetText(character);

        // clear any data from the previously hovered character
        foreach (GameObject obj in abilityInfoUIs)
            Destroy(obj);
        abilityInfoUIs.Clear();
        foreach (GameObject obj in effectInfoUIs)
            Destroy(obj);
        effectInfoUIs.Clear();

        SetAbilities(character);
        SetEffects(character);
    }

    /// <summary>
    /// Set the name, level, and description texts in this tooltip to reflect the cooresponding values of the specified character.
    /// </summary>
    /// <param name="character">The character to display info about.</param>
    private void SetText(CharacterInstance character)
    {
        nameText.text = character.CharacterData.name;
        levelText.text = $"Lvl {character.Level}";
        descriptionText.text = character.CharacterData.Description;
    }

    /// <summary>
    /// Show an entry for each ability the character has.
    /// </summary>
    /// <param name="character">The character to display info about.</param>
    private void SetAbilities(CharacterInstance character)
    {
        foreach (AbilityData ability in character.Abilities)
            NewAbilityInfo(ability);
    }

    /// <summary>
    /// Show an entry for each status effect the character has.
    /// </summary>
    /// <param name="character">The character to display info about.</param>
    private void SetEffects(CharacterInstance character)
    {
        foreach (StatusEffectInstance effect in character.StatusEffects)
            NewEffectInfo(effect);
    }

    /// <summary>
    /// Create a new entry in the tooltip to display info about an ability the character has, keep track of it, and set its data.
    /// </summary>
    /// <param name="ability">The ability to display info about.</param>
    private void NewAbilityInfo(AbilityData ability)
    {
        GameObject abilityInfo = Instantiate(AbilityInfoUIPrefab, infoContainer);
        abilityInfoUIs.Add(abilityInfo);
        abilityInfo.GetComponent<AbilityInfoUI>().Ability = ability;
    }

    /// <summary>
    /// Create a new entry in the tooltip to display info about a status effect the character has, keep track of it, and set its data.
    /// </summary>
    /// <param name="ability">The status effect to display info about.</param>
    private void NewEffectInfo(StatusEffectInstance effect)
    {
        GameObject effectInfo = Instantiate(EffectInfoUIPrefab, infoContainer);
        effectInfoUIs.Add(effectInfo);
        effectInfo.GetComponent<EffectInfoUI>().Effect = effect;
    }
}

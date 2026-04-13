using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterInfoTooltip : Tooltip
{
    [SerializeField] private GameObject AbilityInfoUIPrefab;
    [SerializeField] private GameObject EffectInfoUIPrefab;

    private CharacterInstance character;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Transform InfoContainer;
    private List<GameObject> abilityInfoUIs;
    private List<GameObject> effectInfoUIs;

    protected override void Awake()
    {
        base.Awake();
        abilityInfoUIs = new List<GameObject>();
        effectInfoUIs = new List<GameObject>();
    }

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

    private void SetText(CharacterInstance character)
    {
        nameText.text = character.CharacterData.name;
        levelText.text = $"Lvl {character.Level}";
        descriptionText.text = character.CharacterData.Description;
    }

    private void SetAbilities(CharacterInstance character)
    {
        if (character.Abilities.Count > 0)
        {
            foreach (AbilityData ability in character.Abilities)
                NewAbilityInfo(ability);
        }
    }

    private void SetEffects(CharacterInstance character)
    {
        if (character.StatusEffects.Count > 0)
        {
            foreach (StatusEffectInstance effect in character.StatusEffects)
                NewEffectInfo(effect);
        }
    }

    private void NewAbilityInfo(AbilityData ability)
    {
        GameObject abilityInfo = Instantiate(AbilityInfoUIPrefab, InfoContainer);
        abilityInfoUIs.Add(abilityInfo);
        abilityInfo.GetComponent<AbilityInfoUI>().Ability = ability;
    }

    private void NewEffectInfo(StatusEffectInstance effect)
    {
        GameObject effectInfo = Instantiate(EffectInfoUIPrefab, InfoContainer);
        effectInfoUIs.Add(effectInfo);
        effectInfo.GetComponent<EffectInfoUI>().Effect = effect;
    }
}

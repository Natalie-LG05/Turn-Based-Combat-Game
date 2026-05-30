using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleCharacterInfoUI : MonoBehaviour
{
    private CharacterInstance character;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image spriteImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private GameObject abilityInfoUIPrefab;
    [SerializeField] private Transform abilityInfoUIContainer;
    private List<GameObject> abilityInfoUIs;

    [SerializeField] private TextMeshProUGUI maxHPText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI supportText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;

    [SerializeField] private GameObject effectIconPrefab;
    [SerializeField] private Transform effectIconContainerRow1;
    [SerializeField] private Transform effectIconContainerRow2;
    private List<GameObject> effectIcons;

    private void Awake()
    {
        abilityInfoUIs = new List<GameObject>();
        effectIcons = new List<GameObject>();
    }

    public void SetCharacter(CharacterInstance character)
    {
        this.character = character;

        nameText.text = character.CharacterData.Name;
        levelText.text = $"Lvl {character.Level}";
        spriteImage.sprite = character.CharacterData.Sprite;
        descriptionText.text = character.CharacterData.Description;

        maxHPText.text = $"Max HP: {character.GetBaseStat(Stat.MaxHP)} ({character.MaxHP})";
        attackText.text = $"Attack: {character.GetBaseStat(Stat.Attack)} ({character.Attack})";
        supportText.text = $"Support: {character.GetBaseStat(Stat.Support)} ({character.Support})";
        defenseText.text = $"Defense: {character.GetBaseStat(Stat.Defense)} ({character.Defense})";
        speedText.text = $"Speed: {character.GetBaseStat(Stat.Speed)} ({character.Speed})";

        UpdateAbilities();
        UpdateEffects();
    }

    private void UpdateAbilities()
    {
        foreach (GameObject abilityUI in abilityInfoUIs)
            Destroy(abilityUI);
        abilityInfoUIs.Clear();

        foreach (AbilityData ability in character.Abilities)
            NewAbilityInfoUI(ability);
    }

    private void NewAbilityInfoUI(AbilityData ability)
    {
        GameObject abilityInfoUI = Instantiate(abilityInfoUIPrefab, abilityInfoUIContainer);
        abilityInfoUI.GetComponent<AbilityInfoUI>().Ability = ability;
        abilityInfoUIs.Add(abilityInfoUI);
    }

    private void UpdateEffects()
    {
        foreach (GameObject icon in effectIcons)
            Destroy(icon);
        effectIcons.Clear();

        // create the status effect icons, putting the first eight in row 1, the next 8 in row 2, then alternating for any effects after that
        int previousRow = 2;
        for (int i = 0; i < character.StatusEffects.Count; i++)
        {
            if (i < 16)
                NewEffectIcon(character.StatusEffects[i], i < 8 ? 1 : 2);
            else
            {
                if (previousRow == 2)
                {
                    NewEffectIcon(character.StatusEffects[i], 1);
                    previousRow = 1;
                } else
                {
                    NewEffectIcon(character.StatusEffects[i], 2);
                    previousRow = 2;
                }
            }
        }
    }

    private void NewEffectIcon(StatusEffectInstance status, int row)
    {
        GameObject icon = Instantiate(effectIconPrefab, row == 1 ? effectIconContainerRow1 : effectIconContainerRow2);
        icon.GetComponent<StatusEffectIcon>().StatusEffect = status;
        effectIcons.Add(icon);
    }
}

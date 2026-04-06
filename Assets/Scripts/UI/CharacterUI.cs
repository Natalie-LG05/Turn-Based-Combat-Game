using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private GameObject effectIconPrefab;

    private CharacterInstance character;
    
    [SerializeField] private Image characterSprite;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private Slider healthBar;

    [SerializeField] private Transform effectIconContainer;
    private List<GameObject> effectIcons;

    [SerializeField] private Hoverable chracterPlate;

    private void Awake()
    {
        effectIcons = new List<GameObject>();
    }

    public void SetCharacter(CharacterInstance characterInstance)
    {
        character = characterInstance;
        character.CharacterUI = this;
        SetCharacterSprite(character.CharacterData.Sprite);
        SetNameText(character.CharacterData.Name);
        SetLevelText(character.Level);
        SetHealthUI(character.CurrentHP, character.MaxHP);
        SetEffectIcons(character.StatusEffects);
    }

    private void SetCharacterSprite(Sprite sprite)
    {
        characterSprite.sprite = sprite;
    }

    private void SetNameText(string text)
    {
        nameText.text = text;
    }

    private void SetLevelText(int level)
    {
        levelText.text = $"Lvl {level}";
    }

    private void SetHealthText(int currentHP, int maxHP)
    {
        healthText.text = $"{currentHP}/{maxHP}";
    }

    private void SetHealthBar(int currentHP, int maxHP)
    {
        healthBar.maxValue = maxHP;
        healthBar.value = currentHP;
    }

    private void SetHealthUI(int currentHP, int maxHP)
    {
        SetHealthText(currentHP, maxHP);
        SetHealthBar(currentHP, maxHP);
    }

    private void SetEffectIcons(List<StatusEffectInstance> statusEffects)
    {
        // Delete current effect icons (if any)
        if (effectIcons.Count > 0)
        {
            effectIcons.RemoveRange(0, effectIcons.Count);
        }
        
        if (statusEffects.Count > 0)
        {
            foreach (StatusEffectInstance effect in statusEffects)
            {
                GameObject icon = Instantiate(effectIconPrefab, effectIconContainer);
                effectIcons.Add(icon);
                icon.GetComponent<StatusEffectIcon>().StatusEffect = effect;
            }
        }
    }
}

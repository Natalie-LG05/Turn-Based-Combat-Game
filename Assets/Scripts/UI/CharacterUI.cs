using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private GameObject effectIconPrefab;

    public CharacterInstance Character {  get; private set; }
    
    [SerializeField] private Image characterSprite;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private Slider healthBar;

    [SerializeField] private Transform effectIconContainer;
    private List<GameObject> effectIcons;

    [SerializeField] private Hoverable characterPlate;

    private void Awake()
    {
        effectIcons = new List<GameObject>();
    }

    public void SetCharacter(CharacterInstance characterInstance)
    {
        Character = characterInstance;
        Character.CharacterUI = this;
        SetCharacterSprite(Character.CharacterData.Sprite);
        SetNameText(Character.CharacterData.Name);
        SetLevelText(Character.Level);
        SetHealthUI(Character.CurrentHP, Character.MaxHP);
        SetEffectIcons(Character.StatusEffects);
    }

    public void AddStatusEffect(StatusEffectInstance effect)
    {
        NewEffectIcon(effect);
    }

    public void UpdateCurrentTurn(bool isTurn)
    {
        if (isTurn) characterPlate.Select(); 
        else characterPlate.Deselect();
    }

    public void HighlightCharacterPlate(bool highlight)
    {
        if (highlight) characterPlate.Hover();
        else characterPlate.Unhover();
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
                NewEffectIcon(effect);
            }
        }
    }

    private void NewEffectIcon(StatusEffectInstance effect)
    {
        GameObject icon = Instantiate(effectIconPrefab, effectIconContainer);
        effectIcons.Add(icon);
        icon.GetComponent<StatusEffectIcon>().StatusEffect = effect;
    }
}

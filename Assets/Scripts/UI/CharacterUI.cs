using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private GameObject effectIconPrefab;
    [SerializeField] private GameObject effectListIconPrefab;

    public CharacterInstance Character {  get; private set; }
    
    [SerializeField] private Image characterSprite;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private Slider healthBar;

    [SerializeField] private Transform effectIconContainer;
    private List<GameObject> effectIcons;
    private List<StatusEffectInstance> effects;

    [SerializeField] private Hoverable characterPlate;

    private void Awake()
    {
        effectIcons = new List<GameObject>();
        effects = new List<StatusEffectInstance>();
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

    public void UpdateHealth()
    {
        SetHealthUI(Character.CurrentHP, Character.MaxHP);
    }

    public IEnumerator UpdateHealthSmooth()
    {
        float curHPVal = healthBar.value;
        int newHP = Character.CurrentHP;
        int maxHP = Character.MaxHP;

        float changeAmt = curHPVal - newHP;
        while (curHPVal - newHP > Mathf.Epsilon)
        {
            curHPVal -= changeAmt * Time.deltaTime;
            SetHealthBar(curHPVal, maxHP);
            yield return null;
        }

        SetHealthBar(newHP, maxHP);
        SetHealthText(newHP, maxHP);
    }

    public void SetEffects(List<StatusEffectInstance> effectInstances)
    {
        SetEffectIcons(effectInstances);
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

    private void SetHealthBar(float currentHP, int maxHP)
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
            foreach (GameObject obj in effectIcons)
                Destroy(obj);
            effectIcons.Clear();
        }
        if (effects.Count > 0)
            effects.Clear();
        
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
        if (effects.Count < 5)
        {
            GameObject icon = Instantiate(effectIconPrefab, effectIconContainer);
            effectIcons.Add(icon);
            effects.Add(effect);
            icon.GetComponent<StatusEffectIcon>().StatusEffect = effect;
        } else
        {
            // If the 6th effect is being added, swap the 5th icon with the +1 icon
            if (effects.Count == 5)
            {
                // Remove the icon for the 5th effect
                GameObject oldIcon = effectIcons[4];
                effectIcons.Remove(oldIcon);
                Destroy(oldIcon);

                // Add the +1 icon
                GameObject newIcon = Instantiate(effectListIconPrefab, effectIconContainer);

                // Add the new effect to the list, and put all effects after the fourth into the +1 icon
                effects.Add(effect);
                effectIcons.Add(newIcon);
                newIcon.GetComponent<StatusEffectListIcon>().Effects = effects.GetRange(4, effects.Count - 4);
            } else
            {
                // There are already 6+ effects, so there should already be a +1 icon, so just update the effects shown by its tooltip
                effects.Add(effect);
                effectIcons[4].GetComponent<StatusEffectListIcon>().Effects = effects.GetRange(4, effects.Count - 4);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A UI object displaying information about a character. It contains the character's name, level, health, healthbar, sprite, and character plate.
/// </summary>
public class CharacterUI : MonoBehaviour
{
    [SerializeField] protected GameObject effectIconPrefab;
    [SerializeField] protected GameObject effectListIconPrefab;

    /// <summary>Gets or sets the character this UI is showing info about. Setting is protected.</summary>
    public CharacterInstance Character {  get; protected set; }
    
    [SerializeField] protected Image characterSprite;

    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected TextMeshProUGUI levelText;
    [SerializeField] protected TextMeshProUGUI healthText;

    [SerializeField] protected Slider healthBar;

    [SerializeField] protected Transform effectIconContainer;
    protected List<GameObject> effectIcons;
    protected List<StatusEffectInstance> effects;

    [SerializeField] protected Hoverable characterPlate;

    protected virtual void Awake()
    {
        effectIcons = new List<GameObject>();
        effects = new List<StatusEffectInstance>();
    }

    /// <summary>
    /// Set the character for this UI to display info about.
    /// </summary>
    /// <param name="characterInstance">The character for this UI to display info about.</param>
    public virtual void SetCharacter(CharacterInstance characterInstance)
    {
        Character = characterInstance;
        Character.CharacterUI = this;  // ensure characters have a reference to their UI (if any)

        // Set the UI
        SetCharacterSprite(Character.CharacterData.Sprite);
        SetNameText(Character.CharacterData.Name);
        SetLevelText(Character.Level);
        SetHealthUI(Character.CurrentHP, Character.MaxHP);
        SetEffectIcons(Character.StatusEffects);

        // Set the character reference of the cooresponding character plate
        characterPlate.gameObject.GetComponent<CharacterPlate>().Character = Character;
    }

    /// <summary>
    /// Update the healthbar. To be called after a change is made to the attached character's health.
    /// </summary>
    public void UpdateHealth()
    {
        SetHealthUI(Character.CurrentHP, Character.MaxHP);
    }

    /// <summary>
    /// Update the healthbar smoothly. To be called after a change is made to the attached character's health.
    /// </summary>
    public IEnumerator UpdateHealthSmooth()
    {
        float curHPVal = healthBar.value;
        int newHP = Character.CurrentHP;
        int maxHP = Character.MaxHP;

        // check if the character is taking damage or healing since they require slightly different code
        if (newHP < curHPVal)
        {
            float changeAmt = curHPVal - newHP;
            while (curHPVal - newHP > Mathf.Epsilon)
            {
                curHPVal -= changeAmt * Time.deltaTime;
                SetHealthBar(curHPVal, maxHP);
                yield return null;
            }
        } else
        {
            float changeAmt = newHP - curHPVal;
            while (newHP - curHPVal > Mathf.Epsilon)
            {
                curHPVal += changeAmt * Time.deltaTime;
                SetHealthBar(curHPVal, maxHP);
                yield return null;
            }
        }

        SetHealthUI(newHP, maxHP);
    }

    public void SetEffects(List<StatusEffectInstance> effectInstances)
    {
        SetEffectIcons(effectInstances);
    }

    /// <summary>
    /// Update the character plate based on if it is the attached character's turn or not.
    /// </summary>
    /// <param name="isTurn">Whether or not it is the attached character's turn.</param>
    public void UpdateCurrentTurn(bool isTurn)
    {
        if (isTurn) characterPlate.Select(); 
        else characterPlate.Deselect();
    }

    /// <summary>
    /// Highlight or unhighlight the character plate.
    /// </summary>
    /// <param name="highlight">Whether or not the character plate should be highlighted.</param>
    public void HighlightCharacterPlate(bool highlight)
    {
        if (highlight) characterPlate.Hover();
        else characterPlate.Unhover();
    }

    protected void SetCharacterSprite(Sprite sprite)
    {
        characterSprite.sprite = sprite;
    }

    protected void SetNameText(string text)
    {
        nameText.text = text;
    }

    protected void SetLevelText(int level)
    {
        levelText.text = $"Lvl {level}";
    }

    protected void SetHealthText(int currentHP, int maxHP)
    {
        healthText.text = $"{currentHP}/{maxHP}";
    }

    protected void SetHealthBar(float currentHP, int maxHP)
    {
        healthBar.maxValue = maxHP;
        healthBar.value = currentHP;
    }

    /// <summary>
    /// Set the health bar and health text.
    /// </summary>
    /// <param name="currentHP">The current hp value to display (usually that of the attached character)</param>
    /// <param name="maxHP">The max hp value to display (usually that of the attached character)</param>
    protected void SetHealthUI(int currentHP, int maxHP)
    {
        SetHealthText(currentHP, maxHP);
        SetHealthBar(currentHP, maxHP);
    }

    /// <summary>
    /// Update the effect icons based on the provided list of status effects.
    /// </summary>
    /// <param name="statusEffects">The list of status effects to display.</param>
    protected void SetEffectIcons(List<StatusEffectInstance> statusEffects)
    {
        // Delete current effect icons (if any)
        foreach (GameObject obj in effectIcons)
            Destroy(obj);
        effectIcons.Clear();
        effects.Clear();
        
        foreach (StatusEffectInstance effect in statusEffects)
            NewEffectIcon(effect);
    }

    /// <summary>
    /// Adds a new status effect to the display. 
    /// <br/>Only a max of 5 icons will be displayed, so if there are more than 5 effects, 
    /// <br/>a +x icon will be shown which displays the additional effects in a tooltip.
    /// </summary>
    /// <param name="effect">The status effect to display.</param>
    protected void NewEffectIcon(StatusEffectInstance effect)
    {
        if (effects.Count < 5)
        {
            // If there are less than 5 effects already, then add an icon along with the new effect
            GameObject icon = Instantiate(effectIconPrefab, effectIconContainer);
            effectIcons.Add(icon);
            effects.Add(effect);
            icon.GetComponent<StatusEffectIcon>().StatusEffect = effect;
        } else
        {
            // If the 6th effect is being added, swap the 5th icon with the +x icon
            if (effects.Count == 5)
            {
                // Remove the icon for the 5th effect
                GameObject oldIcon = effectIcons[4];
                effectIcons.Remove(oldIcon);
                Destroy(oldIcon);

                // Add the +1 icon
                GameObject newIcon = Instantiate(effectListIconPrefab, effectIconContainer);

                // Add the new effect to the list, and put all effects after the fourth into the +x icon
                effects.Add(effect);
                effectIcons.Add(newIcon);
                newIcon.GetComponent<StatusEffectListIcon>().Effects = effects.GetRange(4, effects.Count - 4);
            } else
            {
                // There are already 6+ effects, so there should already be a +x icon, so just update the effects shown by its tooltip
                effects.Add(effect);
                effectIcons[4].GetComponent<StatusEffectListIcon>().Effects = effects.GetRange(4, effects.Count - 4);
            }
        }
    }
}

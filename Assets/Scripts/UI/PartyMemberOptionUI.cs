using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A UI representing and showing information about a party member. Contains the same information as a CharacterUI. Additionaly, communicates back to the battle manager when its party member is clicked.
/// </summary>
public class PartyMemberOptionUI : CharacterUI, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoveredColor;

    private Image image;

    private bool active;

    protected override void Awake()
    {
        base.Awake();
        image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!active) image.color = hoveredColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!active) image.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!active) BattleManager.Instance.PartyMemberOptionClicked(Character);
    }

    /// <summary>
    /// Set the character for this UI to display info about.
    /// </summary>
    /// <param name="characterInstance">The character for this UI to display info about.</param>
    public void SetCharacter(CharacterInstance characterInstance, bool active)
    {
        if (effectIcons == null) effectIcons = new List<GameObject>();
        if (effects == null) effects = new List<StatusEffectInstance>();

        Character = characterInstance;
        SetCharacterSprite(Character.CharacterData.Sprite);
        SetNameText(Character.CharacterData.Name);
        SetLevelText(Character.Level);
        SetHealthUI(Character.CurrentHP, Character.MaxHP);
        SetEffectIcons(Character.StatusEffects);

        this.active = active;
    }
}

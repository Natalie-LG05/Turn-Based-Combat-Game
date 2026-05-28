using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A UI icon that cooresponds to a certain character, displaying its sprite as the icon and highlighting its character when the icon is hovered.
/// </summary>
public class CharacterIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CharacterInstance _character;
    private Image image;

    /// <summary>Gets or sets the character attached to this icon. Setting it automatically updates the sprite shown.</summary>
    public CharacterInstance Character
    {
        get { return _character; }
        set
        {
            _character = value;

            // update the UI whenever value is set
            image.sprite = _character.CharacterData.Sprite;
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_character.CharacterUI != null) _character.CharacterUI.HighlightCharacterPlate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeselectCharacter();
    }

    public void DeselectCharacter()
    {
        if (_character.CharacterUI != null) _character.CharacterUI.HighlightCharacterPlate(false);
    }
}
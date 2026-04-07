using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CharacterInstance _character;
    private Image image;

    public CharacterInstance Character
    {
        get { return _character; }
        set
        {
            _character = value;
            image.sprite = _character.CharacterData.Sprite;
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _character.CharacterUI.HighlightCharacterPlate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeselectCharacter();
    }

    public void DeselectCharacter()
    {
        _character.CharacterUI.HighlightCharacterPlate(false);
    }
}
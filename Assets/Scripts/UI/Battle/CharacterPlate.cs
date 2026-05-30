using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A component to be attached to a character plate. Communicates back to the battle manager when its character plate is clicked.
/// </summary>
public class CharacterPlate : MonoBehaviour, IPointerClickHandler
{
    /// <summary>Gets or sets the character that cooresponds to this character plate.</summary>
    public CharacterInstance Character { get; set; }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) BattleManager.Instance.TargetOptionClicked(Character);
        else if (eventData.button == PointerEventData.InputButton.Right) BattleManager.Instance.CharacterPlateRightClicked(Character);
    }
}

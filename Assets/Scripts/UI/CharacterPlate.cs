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
        BattleManager.Instance.TargetOptionClicked(Character);

#if UNITY_EDITOR
        Debug.Log($"{Character.CharacterData.Name} (lvl {Character.Level}) has\nMaxHP: {Character.MaxHP}\nAttack: {Character.Attack}\nSupport: {Character.Support}\nDefense: {Character.Defense}\nSpeed: {Character.Speed}");
#endif
    }
}

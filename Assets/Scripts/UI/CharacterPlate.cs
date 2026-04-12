using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterPlate : MonoBehaviour, IPointerClickHandler
{
    public CharacterInstance Character { get; set; }

    public void OnPointerClick(PointerEventData eventData)
    {
        BattleManager.Instance.TargetOptionClicked(Character);

        Debug.Log($"{Character.CharacterData.Name} (lvl {Character.Level}) has\nMaxHP: {Character.MaxHP}\nAttack: {Character.Attack}\nSupport: {Character.Support}\nDefense: {Character.Defense}\nSpeed: {Character.Speed}");
    }
}

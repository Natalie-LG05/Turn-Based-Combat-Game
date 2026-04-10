using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterPlate : MonoBehaviour, IPointerClickHandler
{
    public CharacterInstance Character { get; set; }

    public void OnPointerClick(PointerEventData eventData)
    {
        BattleManager.Instance.TargetOptionClicked(Character);
    }
}

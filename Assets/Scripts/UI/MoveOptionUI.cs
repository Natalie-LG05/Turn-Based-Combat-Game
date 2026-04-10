using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MoveOptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private MoveData _move;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color hoverColor;

    private Image image;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI elementText;
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private TextMeshProUGUI accuracyText;

    private MoveInfoUI moveInfoUI;

    public MoveData Move
    {
        get => _move;
        set
        {
            _move = value;
            nameText.text = _move.Name;
            elementText.text = _move.Element.ToString();
            powerText.text = _move.Power.ToString();
            accuracyText.text = _move.Accuracy.ToString();
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        moveInfoUI = transform.parent.parent.parent.GetComponentInParent<MoveSelectionUI>().MoveInfoUI_;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = hoverColor;
        moveInfoUI.Move = _move;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        BattleManager.Instance.MoveOptionClicked(_move);
    }
}

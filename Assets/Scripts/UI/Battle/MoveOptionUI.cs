using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A UI gives an overview of the move it cooresponds to and, when clicked, communicates back to the battle manager that its move is clicked.
/// </summary>
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

    /// <summary>Gets or sets the move that cooresponds to this move option UI. Setting automatically updates the UI.</summary>
    public MoveData Move
    {
        get => _move;
        set
        {
            _move = value;

            // update the UI
            nameText.text = _move.Name;
            elementText.text = _move.Element.ToString();
            powerText.text = _move.Power.ToString();
            accuracyText.text = _move.Accuracy.ToString();
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        moveInfoUI = transform.parent.parent.parent.GetComponentInParent<MoveSelectionUI>().MoveInfoUI;
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
        image.color = normalColor;
        BattleManager.Instance.MoveOptionClicked(_move);
    }
}

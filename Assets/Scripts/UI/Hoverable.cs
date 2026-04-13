using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color hoverColor;

    [SerializeField] private bool autoDetermineColors;
    [SerializeField] private float selectedColorMultiplier;
    [SerializeField] private float hoverColorMultiplier;

    [SerializeField] private string tooltipName;
    [SerializeField] private float tooltipShowDelay;
    private GameObject tooltip;

    private bool isSelected;

    private Image image;
    private Outline outline;

    private void Awake()
    {
        image = GetComponent<Image>();
        outline = GetComponent<Outline>();

        AutoDetermineColors();
    }

    private void Start()
    {
        tooltip = GameObject.Find("Tooltips").transform.Find(tooltipName).gameObject;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hover();

        // only show the tooltip when hovered for a certain amount of time as to not show it if the user is just moving their mouse past the hoverable
        if (tooltip != null) Invoke("ShowTooltip", tooltipShowDelay);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Unhover();

        if (tooltip != null) HideTooltip();
    }

    public void Select()
    {
        isSelected = true;
        SetColor(selectedColor);

        if (outline != null)
            outline.enabled = true;
    }

    public void Deselect()
    {
        isSelected = false;
        SetColor(normalColor);

        if (outline != null)
            outline.enabled = false;
    }

    public void Hover()
    {
        SetColor(hoverColor);
    }

    public void Unhover()
    {
        SetColor(isSelected ? selectedColor : normalColor);
    }

    public void UpdateColors()
    {
        AutoDetermineColors();
    }

    private void SetColor(Color color)
    {
        image.color = color;
    }

    private void AutoDetermineColors()
    {
        if (autoDetermineColors)
        {
            normalColor = image.color;
            selectedColor = image.color * selectedColorMultiplier;
            hoverColor = image.color * hoverColorMultiplier;
        }
    }

    private void ShowTooltip()
    {
        tooltip.SetActive(true);
        tooltip.GetComponent<Tooltip>().SetTooltipData(gameObject);
    }

    private void HideTooltip()
    {
        CancelInvoke("ShowTooltip");  // cancel showing the tooltip
        tooltip.SetActive(false);
    }
}

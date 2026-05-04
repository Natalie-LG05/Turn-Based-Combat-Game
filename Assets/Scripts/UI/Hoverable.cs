using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a UI object that changes color when it is hovered or selected. It may also show a tooltip when hovered.
/// </summary>
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

    /// <summary>
    /// Set this hoverable to selected, setting its color to its selected color and activating its border (if it has one).
    /// </summary>
    public void Select()
    {
        isSelected = true;
        SetColor(selectedColor);

        if (outline != null)
            outline.enabled = true;
    }

    /// <summary>
    /// Set this hoverable to not selected, setting its color to its normal color and deactivating its border (if it has one).
    /// </summary>
    public void Deselect()
    {
        isSelected = false;
        SetColor(normalColor);

        if (outline != null)
            outline.enabled = false;
    }

    /// <summary>
    /// Set this hoverable to hovered, setting its color to its hover color.
    /// </summary>
    public void Hover()
    {
        SetColor(hoverColor);
    }

    /// <summary>
    /// Set this hoverable to not hovered, setting its color back to either its normal or selected color (based on if it is selected or not).
    /// </summary>
    public void Unhover()
    {
        SetColor(isSelected ? selectedColor : normalColor);
    }

    /// <summary>
    /// Update the selected and hovered colors based on the current color of this object's image component.
    /// </summary>
    public void UpdateColors()
    {
        AutoDetermineColors();
    }

    private void SetColor(Color color)
    {
        image.color = color;
    }

    /// <summary>
    /// If this hoverable is set to automatically determine its selected and hovered colors, 
    /// <br/>calculate them based on its normal color (which will be the current color of its image component) 
    /// <br/>and its selected and hovered color multipliers.
    /// </summary>
    private void AutoDetermineColors()
    {
        if (autoDetermineColors)
        {
            normalColor = image.color;
            selectedColor = image.color * selectedColorMultiplier;
            hoverColor = image.color * hoverColorMultiplier;
        }
    }

    /// <summary>
    /// Show the tooltip associated with this hoverable and set its info by passing in the game object that triggered it.
    /// </summary>
    private void ShowTooltip()
    {
        tooltip.SetActive(true);
        tooltip.GetComponent<Tooltip>().SetTooltipData(gameObject);
    }

    /// <summary>
    /// Hide (or cancel showing) the tooltip associated with this hoverable.
    /// </summary>
    private void HideTooltip()
    {
        CancelInvoke("ShowTooltip");  // cancel showing the tooltip
        tooltip.SetActive(false);
    }
}

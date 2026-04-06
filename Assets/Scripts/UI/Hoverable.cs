using UnityEngine;
using UnityEngine.UI;

public class Hoverable : MonoBehaviour
{
    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color hoverColor;

    // SerializeField for the tooltip to display when this is hovered over

    private bool isSelected;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    // When hovered, change to hover color and display character info tooltip

    public void Select()
    {
        isSelected = true;
        SetColor(selectedColor);
    }

    public void Deselect()
    {
        isSelected = false;
        SetColor(normalColor);
    }

    public void Hover()
    {
        SetColor(hoverColor);
    }

    public void Unhover()
    {
        SetColor(isSelected ? selectedColor : normalColor);
    }

    private void SetColor(Color color)
    {
        image.color = color;
    }
}

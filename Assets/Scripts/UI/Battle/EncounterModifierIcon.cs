using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A UI icon that cooresponds to an encounter modifier, changing colors based on the modifier and displaying information about it when hovered.
/// </summary>
public class EncounterModifierIcon : MonoBehaviour
{
    private EncounterModifierData _modifierData;
    private Image image;
    private Hoverable hoverable;

    /// <summary>Gets or sets the encounter modifier represented by this icon. Setting automatically updates the icon's color.</summary>
    public EncounterModifierData ModifierData
    {
        get { return _modifierData; }
        set
        {
            _modifierData = value;
            image.color = _modifierData.Color;
            hoverable.UpdateColors();
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        hoverable = GetComponent<Hoverable>();
    }
}

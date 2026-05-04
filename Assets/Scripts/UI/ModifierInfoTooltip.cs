using TMPro;
using UnityEngine;

/// <summary>
/// A tooltip that displays information about an encounter modifier. 
/// </summary>
public class ModifierInfoTooltip : Tooltip
{
    private EncounterModifierData modifier;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    /// <summary>
    /// Set the data for the tooltip to show based on the object that triggered the tooltip.
    /// <br/>This tooltip is shown when an encounter modifier icon is hovered,
    /// <br/>so set the data based on the modifier attached to that icon.
    /// </summary>
    /// <param name="sourceObject">The game object that triggered the tooltip.</param>
    public override void SetTooltipData(GameObject sourceObject)
    {
        base.SetTooltipData(sourceObject);
        modifier = sourceObject.GetComponent<EncounterModifierIcon>().ModifierData;
        nameText.text = modifier.Name;
        descriptionText.text = modifier.Description;
    }
}

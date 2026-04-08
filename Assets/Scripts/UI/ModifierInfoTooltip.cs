using TMPro;
using UnityEngine;

public class ModifierInfoTooltip : Tooltip
{
    private EncounterModifierData modifier;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public override void SetTooltipData(GameObject sourceObject)
    {
        base.SetTooltipData(sourceObject);
        modifier = sourceObject.GetComponent<EncounterModifierIcon>().ModifierData;
        nameText.text = modifier.Name;
        descriptionText.text = modifier.Description;
    }
}

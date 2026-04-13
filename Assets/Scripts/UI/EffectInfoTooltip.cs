using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;

public class EffectInfoTooltip : Tooltip
{
    private StatusEffectInstance effect;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public override void SetTooltipData(GameObject sourceObject)
    {
        base.SetTooltipData(sourceObject);
        effect = sourceObject.GetComponent<StatusEffectIcon>().StatusEffect;

        nameText.text = effect.StatusEffectData.Name;
        // Permanent effects such as encounter modifiers simply say '-' as their duration instead of a number
        durationText.text = $"{(effect.StatusEffectData.IsPermanent ? "-" : effect.Duration)} Turns";

        string description = effect.StatusEffectData.Description;
        // Replace any '{i}' in the description which represent increases that can vary in power
        description = description.Replace("{i}", effect.BuffPower.ToString());
        // Replace any '{d}' in the description which represent decreases that can vary in power
        description = description.Replace("{d}", effect.DebuffPower.ToString());
        descriptionText.text = description;
    }
}

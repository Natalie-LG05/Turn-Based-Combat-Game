using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
/// A tooltip that displays information about a status effect.
/// </summary>
public class EffectInfoTooltip : Tooltip
{
    private StatusEffectInstance effect;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    /// <summary>
    /// Set the data for the tooltip to show based on the object that triggered the tooltip.
    /// <br/>This tooltip is shown when a status effect icon is hovered,
    /// <br/>so set the data based on the status effect attached to that icon.
    /// </summary>
    /// <param name="sourceObject">The game object that triggered the tooltip.</param>
    public override void SetTooltipData(GameObject sourceObject)
    {
        base.SetTooltipData(sourceObject);
        effect = sourceObject.GetComponent<StatusEffectIcon>().StatusEffect;

        nameText.text = effect.StatusEffectData.Name;
        // Permanent effects such as encounter modifiers simply say '-' as their duration instead of a number
        durationText.text = $"{(effect.StatusEffectData.IsPermanent ? "-" : effect.Duration)} Turns";

        string description = effect.StatusEffectData.Description;
            // Replace any '{i}' in the description which represent increases that can vary in power
            Queue<float> increasePowers = new Queue<float>();
            foreach (StatusEffectStatModifier increase in effect.StatIncreases)
                increasePowers.Enqueue(effect.GetModifierStrength(increase, true));
            description = Regex.Replace(description, "{i}", m => increasePowers.Dequeue().ToString());

            // Replace any '{d}' in the description which represent decreases that can vary in power
            Queue<float> decreasePowers = new Queue<float>();
            foreach (StatusEffectStatModifier decrease in effect.StatDecreases)
                decreasePowers.Enqueue(effect.GetModifierStrength(decrease, false));
        description = Regex.Replace(description, "{d}", m => decreasePowers.Dequeue().ToString());

            descriptionText.text = description;
    }
}

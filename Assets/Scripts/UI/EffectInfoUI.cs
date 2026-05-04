using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
/// A UI object that displays information about a status effect.
/// </summary>
public class EffectInfoUI : MonoBehaviour
{
    private StatusEffectInstance _effect;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    /// <summary>Gets or sets the status effect for this object to display info about. Setting it automatically updates the UI.</summary>
    public StatusEffectInstance Effect
    {
        get { return _effect; }
        set
        {
            _effect = value;

            // update the UI whenever value is set
            nameText.text = _effect.StatusEffectData.Name;
            // Permanent effects such as encounter modifiers simply say '-' as their duration instead of a number
            durationText.text = $"{(_effect.StatusEffectData.IsPermanent ? "-" : _effect.Duration)} Turns";

            string description = _effect.StatusEffectData.Description;
            // Replace any '{i}' in the description which represent increases that can vary in power
            Queue<float> increasePowers = new Queue<float>();
            foreach (StatusEffectStatModifier increase in _effect.StatIncreases)
                increasePowers.Enqueue(_effect.GetModifierStrength(increase, true));
            description = Regex.Replace(description, "{i}", m => increasePowers.Dequeue().ToString());

            // Replace any '{d}' in the description which represent decreases that can vary in power
            Queue<float> decreasePowers = new Queue<float>();
            foreach (StatusEffectStatModifier decrease in _effect.StatDecreases)
                decreasePowers.Enqueue(_effect.GetModifierStrength(decrease, false));
            description = Regex.Replace(description, "{d}", m => decreasePowers.Dequeue().ToString());

            descriptionText.text = description;
        }
    }
}

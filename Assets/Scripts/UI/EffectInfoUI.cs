using TMPro;
using UnityEngine;

public class EffectInfoUI : MonoBehaviour
{
    private StatusEffectInstance _effect;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private TextMeshProUGUI descriptionText;

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
            description = description.Replace("{i}", _effect.BuffPower.ToString());
            // Replace any '{d}' in the description which represent decreases that can vary in power
            description = description.Replace("{d}", _effect.DebuffPower.ToString());
            descriptionText.text = description;
        }
    }
}

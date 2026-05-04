using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A UI icon that contains references to one or more status effects. It contains text showing how many status effects it contains. When hovered, it shows information about all of its status effects.
/// </summary>
public class StatusEffectListIcon : MonoBehaviour
{
    private List<StatusEffectInstance> _effects;

    [SerializeField] private TextMeshProUGUI numberText;

    /// <summary>Gets or sets the status effects to be displayed by this icon. Setting automatically updates the UI.</summary>
    public List<StatusEffectInstance> Effects
    {
        get => _effects;
        set
        {
            _effects = value;

            // update the UI
            numberText.text = $"+{_effects.Count}";
        }
    }
}

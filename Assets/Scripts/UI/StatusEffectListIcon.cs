using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusEffectListIcon : MonoBehaviour
{
    private List<StatusEffectInstance> _effects;

    [SerializeField] private TextMeshProUGUI numberText;

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

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
            nameText.text = _effect.StatusEffectData.Name;
            durationText.text = $"{(_effect.StatusEffectData.IsPermanent ? "-" : _effect.Duration)} Turns";
            descriptionText.text = _effect.StatusEffectData.Description;
        }
    }
}

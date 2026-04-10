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
        durationText.text = $"{(effect.StatusEffectData.IsPermanent ? "-" : effect.Duration)} Turns";
        descriptionText.text = effect.StatusEffectData.Description.Replace("{x}", effect.Power.ToString());
    }
}

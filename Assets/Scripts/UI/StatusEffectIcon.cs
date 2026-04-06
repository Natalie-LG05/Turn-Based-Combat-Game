using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIcon : MonoBehaviour
{
    private StatusEffectInstance _statusEffect;
    private Image image;

    public StatusEffectInstance StatusEffect
    {
        get
        {
            return _statusEffect;
        }
        set
        {
            _statusEffect = value;
            image.color = _statusEffect.Data.IconColor;
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
    }
}

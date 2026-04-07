using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIcon : MonoBehaviour
{
    private StatusEffectInstance _statusEffect;
    private Image image;
    private Hoverable hoverable;

    public StatusEffectInstance StatusEffect
    {
        get { return _statusEffect; }
        set
        {
            _statusEffect = value;
            image.color = _statusEffect.Data.IconColor;
            hoverable.UpdateColors();
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        hoverable = GetComponent<Hoverable>();
    }
}

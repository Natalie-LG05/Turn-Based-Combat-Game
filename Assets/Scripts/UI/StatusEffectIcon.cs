using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A UI icon that cooresponds to a certain status effect, changing colors based on the effect and displaying information about it when hovered.
/// </summary>
public class StatusEffectIcon : MonoBehaviour
{
    private StatusEffectInstance _statusEffect;
    private Image image;
    private Hoverable hoverable;

    /// <summary>Gets or sets the status effect represented by this icon. Setting automatically updatest the icon's color.</summary>
    public StatusEffectInstance StatusEffect
    {
        get { return _statusEffect; }
        set
        {
            _statusEffect = value;

            // update the UI
            image.color = _statusEffect.StatusEffectData.IconColor;
            hoverable.UpdateColors();
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        hoverable = GetComponent<Hoverable>();
    }
}

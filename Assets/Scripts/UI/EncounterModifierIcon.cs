using UnityEngine;
using UnityEngine.UI;

public class EncounterModifierIcon : MonoBehaviour
{
    private EncounterModifierData _modifierData;
    private Image image;

    public EncounterModifierData ModifierData
    {
        get
        {
            return _modifierData;
        }
        set
        {
            _modifierData = value;
            image.color = _modifierData.Color;
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
    }
}

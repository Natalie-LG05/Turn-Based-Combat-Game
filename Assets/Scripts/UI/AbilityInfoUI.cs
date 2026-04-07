using TMPro;
using UnityEngine;

public class AbilityInfoUI : MonoBehaviour
{
    private AbilityData _ability;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public AbilityData Ability
    {
        get { return _ability; }
        set
        {
            _ability = value;
            nameText.text = _ability.Name;
            descriptionText.text = _ability.Description;
        }
    }
}

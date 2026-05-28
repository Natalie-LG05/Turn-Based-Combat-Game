using TMPro;
using UnityEngine;

/// <summary>
/// A UI object that shows information about an ability.
/// </summary>
public class AbilityInfoUI : MonoBehaviour
{
    private AbilityData _ability;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    /// <summary>Gets or sets the ability for this UI to show info about. Automatically updates the UI when ability is set.</summary>
    public AbilityData Ability
    {
        get { return _ability; }
        set
        {
            _ability = value;

            // update the UI whenever value is set
            nameText.text = _ability.Name;
            descriptionText.text = _ability.Description;
        }
    }
}

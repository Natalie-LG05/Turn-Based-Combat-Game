using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FixedEncounterRewardUI : MonoBehaviour
{
    private FixedEncounterReward _reward;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image icon;

    /// <summary>Gets or sets the encounter reward represented by this object. Setting automatically updates the UI.</summary>
    public FixedEncounterReward Reward
    {
        get => _reward;
        set
        {
            _reward = value;

            nameText.text = _reward.Item.Name;
            amountText.text = $"x{_reward.Amount}";

            icon.color = _reward.Item.IconColor;
        }
    }
}

using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomEncounterRewardUI : MonoBehaviour
{
    private RandomEncounterReward _reward;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image icon;

    /// <summary>Gets or sets the encounter reward represented by this object. Setting automatically updates the UI.</summary>
    public RandomEncounterReward Reward
    {
        get => _reward;
        set
        {
            _reward = value;

            nameText.text = _reward.Item.Name;

            amountText.text = _reward.RandomizationType ==
                RandomizationType.Range ? $"{_reward.MinAmount}-{_reward.MaxAmount}" :
                string.Join("\n", _reward.AmountChancePairs.Select(acp => $"{acp.Amount} : {(acp.Weight / (float)_reward.TotalWeight) * 100}%")).Trim();

            icon.color = _reward.Item.IconColor;
        }
    }
}

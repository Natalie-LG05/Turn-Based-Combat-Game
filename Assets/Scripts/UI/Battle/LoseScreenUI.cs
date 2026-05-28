using TMPro;
using UnityEngine;

public class LoseScreenUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI losePenaltyText;

    /// <summary>
    /// Set the amount of shells that the player lost for losing, and update the text shown accordingly.
    /// </summary>
    /// <param name="amount">The amount of shells that the player lost for losing.</param>
    public void SetPenaltyAmount(int amount)
    {
        losePenaltyText.text = $"You lost {amount} shells!";
    }
}

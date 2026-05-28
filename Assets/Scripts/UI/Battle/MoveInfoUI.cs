using TMPro;
using UnityEngine;

/// <summary>
/// A UI that displays information about a move.
/// </summary>
public class MoveInfoUI : MonoBehaviour
{
    private MoveData _move;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private TextMeshProUGUI elementText;
    [SerializeField] private TextMeshProUGUI hitsText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    /// <summary>Gets or sets the move for this UI to display info about. Setting automatically updates the UI.</summary>
    public MoveData Move
    {
        get => _move;
        set
        {
            _move = value;
            nameText.text = _move.Name;
            descriptionText.text = _move.Description;

            // Show a comma seperated list of these 4 values for each move effect of the selected move
            powerText.text = $"Power: {string.Join(", ", _move.Powers)}";
            accuracyText.text = $"Accuracy: {string.Join(", ", _move.Accuracies)}";
            elementText.text = $"Element: {string.Join(", ", _move.Elements)}";
            hitsText.text = $"Hits: {string.Join(", ", _move.HitsList)}";
        }
    }
}

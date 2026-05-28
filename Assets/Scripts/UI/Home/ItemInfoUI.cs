using TMPro;
using UnityEngine;

/// <summary>
/// Manages the UI that displays information about the item that is currently selected either on the home screen or during battle.
/// </summary>
public class ItemInfoUI : MonoBehaviour
{
    private ItemData _item;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public ItemData Item
    {
        get => _item;
        set
        {
            _item = value;

            nameText.text = _item.Name;
            descriptionText.text = _item.Description;
        }
    }
}

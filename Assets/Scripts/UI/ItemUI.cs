using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents an item within a list of items on the home screen or during battle.
/// </summary>
public class ItemUI : MonoBehaviour, IPointerClickHandler
{
    private ItemEntry<ItemData> _item;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image icon;

    private InventoryUI inventoryUI;

    /// <summary>Gets or sets the item represented by this object. Setting automatically updates the UI.</summary>
    public ItemEntry<ItemData> Item
    {
        get => _item;
        set
        {
            _item = value;

            nameText.text = _item.ItemData.Name;
            amountText.text = $"x{_item.Amount}";

            icon.color = _item.ItemData.IconColor;
        }
    }

    private void Start()
    {
        if (HomeManager.Instance != null && HomeManager.Instance.State == HomeState.InventoryScreen)
            inventoryUI = FindObjectsByType<InventoryUI>(FindObjectsSortMode.None)[0];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // if on the home screen, select that item in the inventory
        if (HomeManager.Instance != null && HomeManager.Instance.State == HomeState.InventoryScreen)
            inventoryUI.ItemOptionClicked(this);
    }
}

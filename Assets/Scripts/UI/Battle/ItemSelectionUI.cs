using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the UI for the item selection screen.
/// </summary>
public class ItemSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject itemOptionUIPrefab;

    private InventoryManager inventoryManager;

    [SerializeField] private Transform itemOptionContainer;
    private List<GameObject> itemOptionUIs;

    [SerializeField] private ItemInfoUI _itemInfoUI;

    /// <summary>Gets the item info UI assigned to this object.</summary>
    public ItemInfoUI ItemInfoUI { get => _itemInfoUI; }

    private void Awake()
    {
        itemOptionUIs = new List<GameObject>();

        inventoryManager = InventoryManager.Instance;
    }

    /// <summary>
    /// Updates the item options based on the battle items currently in the player's inventory.
    /// </summary>
    public void UpdateItemOptions()
    {
        // clear old data
        foreach (GameObject itemOption in itemOptionUIs)
            Destroy(itemOption);
        itemOptionUIs.Clear();

        foreach (ItemEntry<BattleItemData> itemEntry in inventoryManager.BattleItems)
            NewItemOption(itemEntry);

        // select the first item by default
        _itemInfoUI.Item = itemOptionUIs[0].GetComponent<ItemUI>().Item.ItemData;
    }

    /// <summary>
    /// Add a new interactable item option to the list.
    /// </summary>
    /// <param name="itemEntry">The battle item entry to be attached to that item option.</param>
    private void NewItemOption(ItemEntry<BattleItemData> itemEntry)
    {
        GameObject itemOption = Instantiate(itemOptionUIPrefab, itemOptionContainer);
        itemOption.GetComponent<ItemUI>().Item = itemEntry.GetGenericEntry();
        itemOptionUIs.Add(itemOption);
    }
}

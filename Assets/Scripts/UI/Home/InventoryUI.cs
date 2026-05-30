using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the inventory UI that is part of the inventory screen on the home screen.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject ItemUIPrefab;
    [SerializeField] private ItemData blankItem;

    public ItemUI SelectedItemOption { get; private set; }

    [SerializeField] private InventoryCategoryIcon defaultCategoryIcon;
    public InventoryCategoryIcon SelectedCategoryIcon { get; private set; }

    [SerializeField] private ItemInfoUI itemInfoUI;

    [SerializeField] private Transform itemUIContainer;
    private List<GameObject> itemUIs;

    private void Awake()
    {
        itemUIs = new List<GameObject>();
    }

    public void ShowDefaultCategory()
    {
        SelectCategoryIcon(defaultCategoryIcon);
        ShowItems(defaultCategoryIcon.Category);
    }

    public void UpdateInventory()
    {
        ShowItems(SelectedCategoryIcon.Category);
    }

    /// <summary>
    /// Show the list of items of a certain InventoryCategory.
    /// </summary>
    /// <param name="category">The inventory category of items to display.</param>
    public void ShowItems(InventoryCategory category)
    {
        // remove old item UIs (if any)
        foreach (GameObject obj in itemUIs)
            Destroy(obj);
        itemUIs.Clear();

        // get the list of items in the provided category
        List<ItemEntry<ItemData>> items = InventoryManager.Instance.GetItemsInInventoryCategory(category);

        // create the item UIs
        foreach (ItemEntry<ItemData> item in items)
            NewItemUI(item);

        // select the first option by default
        if (itemUIs.Count > 0) SelectItemOption(itemUIs[0].GetComponent<ItemUI>());
        else itemInfoUI.Item = blankItem;
    }

    /// <summary>
    /// When an item option is clicked, select it, deselecting the previously selected option, selecting the clicked option, and displaying info about the newly selected option..
    /// </summary>
    /// <param name="itemOption">The item option that was clicked.</param>
    public void ItemOptionClicked(ItemUI itemOption)
    {
        if (HomeManager.Instance.State == HomeState.InventoryScreen)
            SelectItemOption(itemOption);
    }

    /// <summary>
    /// Select an item option, deselecting the previously selected option and displaying info about the newly selected option.
    /// </summary>
    /// <param name="itemOption">The item option to select.</param>
    private void SelectItemOption(ItemUI itemOption)
    {
        // Deselect the previously selected item option and select the newly clicked one
        if (SelectedItemOption != null) SelectedItemOption.GetComponent<Hoverable>().Deselect();
        SelectedItemOption = itemOption;
        SelectedItemOption.GetComponent<Hoverable>().Select();

        itemInfoUI.Item = itemOption.Item.ItemData;
    }

    /// <summary>
    /// When an inventory category button is clicked, select it, selecting the clicked category and updating the UI accordingly.
    /// </summary>
    /// <param name="category">The category who's button was clicked.</param>
    public void CategoryButtonClicked(InventoryCategory category, InventoryCategoryIcon icon)
    {
        if (HomeManager.Instance.State == HomeState.InventoryScreen)
            ShowItems(category);
            SelectCategoryIcon(icon);
    }

    private void SelectCategoryIcon(InventoryCategoryIcon icon)
    {
        // Deselect the previously selected category icon and select the newly selected one
        if (SelectedCategoryIcon != null) SelectedCategoryIcon.GetComponent<Hoverable>().Deselect();
        SelectedCategoryIcon = icon;
        SelectedCategoryIcon.GetComponent<Hoverable>().Select();
    }

    private void NewItemUI(ItemEntry<ItemData> item)
    {
        GameObject itemUI = Instantiate(ItemUIPrefab, itemUIContainer);
        itemUI.GetComponent<ItemUI>().Item = item;
        itemUIs.Add(itemUI);
    }
}

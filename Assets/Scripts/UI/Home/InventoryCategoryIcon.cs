using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A class that handles the click behavior of the category icons on the inventory screen of the home screen.
/// </summary>
public class InventoryCategoryIcon : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private InventoryCategory _category;

    private InventoryUI inventoryUI;

    public InventoryCategory Category { get => _category; }

    private void Start()
    {
        inventoryUI = FindObjectsByType<InventoryUI>(FindObjectsSortMode.None)[0];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (HomeManager.Instance.State == HomeState.InventoryScreen)
            inventoryUI.CategoryButtonClicked(Category, this);
    }
}

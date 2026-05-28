using System.Collections.Generic;
using UnityEngine;

public class WinScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject itemUIPrefab;

    [SerializeField] private Transform itemUIContainer;

    /// <summary>
    /// Set the items to display on the win screen.
    /// </summary>
    /// <param name="items">The list of rewarded items.</param>
    public void SetRewardItems(List<ItemEntry<ItemData>> items)
    {
        foreach (ItemEntry<ItemData> item in items)
        {
            GameObject itemUI = Instantiate(itemUIPrefab, itemUIContainer);
            itemUI.GetComponent<ItemUI>().Item = item;
        }
    }
}

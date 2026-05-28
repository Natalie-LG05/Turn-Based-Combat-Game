using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum HomeState { EncounterListScreen, InventoryScreen, PartyMembersScreen, ShopScreen, SwappingPartyMembers, TargetSelection, Busy }

/// <summary>
/// Manages functionality of the homescreen, containing the main loop and handling much of the logic.
/// </summary>
public class HomeManager : MonoBehaviour
{
    public static HomeManager Instance { get; private set; }
    public static List<HomeState> BusyStates { get { return new List<HomeState>() { HomeState.SwappingPartyMembers, HomeState.TargetSelection, HomeState.Busy }; } }

    private EncounterListUI encounterListUI;
    [SerializeField] private HomePartyUI currentPartyMembersUI;

    private InventoryUI inventoryUI;
    [SerializeField] private TextMeshProUGUI useButtonText;

    private List<HomePartyMemberUI> selectedPartyMemberUIs;

    [SerializeField] private GameObject encounterListUIGO;
    [SerializeField] private GameObject encounterInfoUIGO;
    [SerializeField] private GameObject inventoryUIGO;
    [SerializeField] private GameObject itemInfoUIGO;

    private EncounterManager encounterManager;
    private PartyManager partyManager;
    private InventoryManager inventoryManager;

    public HomeState State { get; set; }
    public bool IsBusy { get { return BusyStates.Contains(State); } }

    private void Awake()
    {
        Instance = this;

        selectedPartyMemberUIs = new List<HomePartyMemberUI>();
    }

    private void Start()
    {
        encounterManager = EncounterManager.Instance;
        partyManager = PartyManager.Instance;
        inventoryManager = InventoryManager.Instance;

        encounterListUI = encounterListUIGO.GetComponent<EncounterListUI>();
        inventoryUI = inventoryUIGO.GetComponent<InventoryUI>();

        Setup();
    }

    private void Update()
    {
        if (State == HomeState.TargetSelection)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitTargetSelection();
            }
        }
    }

    /// <summary>
    /// When the play button is clicked, begin combat against the currently selected encounter.
    /// </summary>
    public void PlayButtonClicked()
    {
        if (State == HomeState.EncounterListScreen)
        {
            encounterManager.CurrentEncounter = encounterListUI.SelectedEncounterOption.Encounter;
            SceneManager.LoadScene(2);
        }
    }

    /// <summary>
    /// When the encounters button is clicked, if the current screen is not busy then change to the encounter list screen.
    /// </summary>
    public void EncountersButtonClicked()
    {
        if (!IsBusy)
        {
            State = HomeState.EncounterListScreen;
            ShowEncounterListScreen();
            currentPartyMembersUI.SetParty(partyManager.Party);
        }
    }

    /// <summary>
    /// When the inventory button is clicked, if the current screen is not busy then change to the inventory screen.
    /// </summary>
    public void InventoryButtonClicked()
    {
        if (!IsBusy)
        {
            State = HomeState.InventoryScreen;
            ShowInventoryScreen();

            // start with the currency category selected by default
            inventoryUI.ShowDefaultCategory();
        }
    }

    /// <summary>
    /// When the use button is clicked, use the selected item or enter target selection if needed.
    /// <br/>Only enters target selection if there is at least one valid target.
    /// <br/>During target selection, changes to a confirm button, which confirms using the item on the selected target.
    /// </summary>
    public void UseButtonClicked()
    {
        ItemData item = inventoryUI.SelectedItemOption.Item.ItemData;
        if (item.Category != ItemCategory.Consumable) return;
        ConsumableItemData consumable = (ConsumableItemData)item;

        if (State == HomeState.InventoryScreen)
        {
            if (consumable.Useable(partyManager.Party))
            {
                useButtonText.text = "CONFIRM";

                // TODO: change to party screen so that all party members are available as targets

                selectedPartyMemberUIs.Clear();
                State = HomeState.TargetSelection;

                foreach (HomePartyMemberUI partyMemberUI in currentPartyMembersUI.PartyMemberUIs)
                    partyMemberUI.gameObject.GetComponent<Hoverable>().SetHoverColorBehavior(true);
            }
        } else if (State == HomeState.TargetSelection)
        {
            if (selectedPartyMemberUIs.Count < 1) return;

            List<CharacterInstance> targets = selectedPartyMemberUIs.Select(partyMemberUI => (CharacterInstance)partyMemberUI.PartyMember).ToList();
            consumable.Effects?.ItemOnUse?.Invoke(null, consumable, targets);
            inventoryManager.RemoveItemPublic(item, 1);
            inventoryUI.UpdateInventory();
            currentPartyMembersUI.SetParty(partyManager.Party);

            ExitTargetSelection();
        }
    }

    /// <summary>
    /// Called when a party member UI is clicked while selecting the targets of an item that is about to be used.
    /// </summary>
    /// <param name="partyMemberUI">The party member UI that was clicked.</param>
    public void TargetOptionClicked(HomePartyMemberUI partyMemberUI)
    {
        ConsumableItemData item = (ConsumableItemData)inventoryUI.SelectedItemOption.Item.ItemData;
        if (selectedPartyMemberUIs.Contains(partyMemberUI))
        {
            partyMemberUI.gameObject.GetComponent<Hoverable>().Deselect();
            selectedPartyMemberUIs.Remove(partyMemberUI);
        } else if (selectedPartyMemberUIs.Count < item.NumberOfTargets)
        {
            if (item.Useable(partyMemberUI.PartyMember))
            {
                partyMemberUI.gameObject.GetComponent<Hoverable>().Select();
                selectedPartyMemberUIs.Add(partyMemberUI);
            }
        }
    }

    private void ExitTargetSelection()
    {
        State = HomeState.InventoryScreen;
        useButtonText.text = "USE";
        // TODO: hide party member screen
        foreach (HomePartyMemberUI partyMemberUI in currentPartyMembersUI.PartyMemberUIs)
        {
            partyMemberUI.gameObject.GetComponent<Hoverable>().Deselect();
            partyMemberUI.gameObject.GetComponent<Hoverable>().Unhover();
            partyMemberUI.gameObject.GetComponent<Hoverable>().SetHoverColorBehavior(false);
        }
    }

    private void Setup()
    {
        State = HomeState.EncounterListScreen;

        currentPartyMembersUI.SetParty(partyManager.Party);
        encounterListUI.SetEncounters(encounterManager.Encounters);
    }

    private void ShowEncounterListScreen()
    {
        SetEncounterListScreen(true);
        SetInventoryScreen(false);
    }

    private void ShowInventoryScreen()
    {
        SetInventoryScreen(true);
        SetEncounterListScreen(false);
    }

    /// <summary>
    /// Set whether or not the encounter list screen game objects are active.
    /// </summary>
    /// <param name="isActive">Whether or not the encounter list screen game objects are active.</param>
    private void SetEncounterListScreen(bool isActive)
    {
        encounterListUIGO.SetActive(isActive);
        encounterInfoUIGO.SetActive(isActive);
    }

    /// <summary>
    /// Set whether or not the inventory screen game objects are active.
    /// </summary>
    /// <param name="isActive">Whether or not the inventory screen game objects are active.</param>
    private void SetInventoryScreen(bool isActive)
    {
        inventoryUIGO.SetActive(isActive);
        itemInfoUIGO.SetActive(isActive);
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles the UI that shows and rearranges party members on the home screen.
/// </summary>
public class HomePartyUI : MonoBehaviour
{
    private GameObject selectedPartyMemberUI;

    [SerializeField] private GameObject partyMemberUIPrefab;

    private List<GameObject> partyMemberUIs;

    private PartyManager partyManager;

    private HomeState previousState;

    public List<HomePartyMemberUI> PartyMemberUIs
    {
        get { return partyMemberUIs.Select(partyMemberUI => partyMemberUI.GetComponent<HomePartyMemberUI>()).ToList(); }
    }

    private void Awake()
    {
        partyMemberUIs = new List<GameObject>();
    }

    private void Start()
    {
        partyManager = PartyManager.Instance;
    }

    /// <summary>
    /// When the swap button of a party member UI is clicked, select it and swap it with the next party member UI that is clicked.
    /// </summary>
    /// <param name="partyMemberUI">The party member UI whose swap button clicked.</param>
    public void SwapButtonClicked(GameObject partyMemberUI)
    {
        if (selectedPartyMemberUI != null) return;

        previousState = HomeManager.Instance.State;
        HomeManager.Instance.State = HomeState.SwappingPartyMembers;

        partyMemberUI.GetComponent<Hoverable>().Select();
        selectedPartyMemberUI = partyMemberUI;

        // give the user feedback by enabling hover colors on party member UIs when waiting for them to select one to swap with
        foreach (GameObject _partyMemberUI in partyMemberUIs)
            _partyMemberUI.GetComponent<Hoverable>().SetHoverColorBehavior(true);
    }

    /// <summary>
    /// When a party member UI is clicked, if there is currently a selected party member UI, swap it with the one that was just clicked.
    /// </summary>
    /// <param name="partyMemberUI">The party member UI that was clicked.</param>
    public void PartyMemberUIClicked(GameObject partyMemberUI)
    {
        if (selectedPartyMemberUI == null) return;

        foreach (GameObject _partyMemberUI in partyMemberUIs)
        {
            _partyMemberUI.GetComponent<Hoverable>().Unhover();
            _partyMemberUI.GetComponent<Hoverable>().SetHoverColorBehavior(false);
        }

        if (selectedPartyMemberUI != null)
        {
            if (selectedPartyMemberUI != partyMemberUI)
            {
                // swap the party member UIs in the list
                int selectedIndex = partyMemberUIs.IndexOf(selectedPartyMemberUI);
                int clickedIndex = partyMemberUIs.IndexOf(partyMemberUI);
                (partyMemberUIs[selectedIndex], partyMemberUIs[clickedIndex]) = (partyMemberUIs[clickedIndex], partyMemberUIs[selectedIndex]);

                // swap the party members in the party
                partyManager.SwapPartyMembers(partyMemberUI.GetComponent<HomePartyMemberUI>().PartyMember, selectedPartyMemberUI.GetComponent<HomePartyMemberUI>().PartyMember);

                UpdatePartyMemberUIOrder();
            }

            selectedPartyMemberUI.GetComponent<Hoverable>().Deselect();
            selectedPartyMemberUI = null;

            HomeManager.Instance.State = previousState;
        }
    }

    /// <summary>
    /// Set the party, which is a list of party members, to display.
    /// </summary>
    /// <param name="party">The current party as a list of party members.</param>
    public void SetParty(List<PartyMemberInstance> party)
    {
        // remove old party member UIs (if any)
        foreach (GameObject obj in partyMemberUIs)
            Destroy(obj);
        partyMemberUIs.Clear();

        // create the party member UIs
        foreach (PartyMemberInstance partyMember in party)
            NewPartyMemberUI(partyMember);
    }

    /// <summary>
    /// Create, initialize, and store a new party member UI.
    /// </summary>
    /// <param name="partyMember">The party member to display info about.</param>
    private void NewPartyMemberUI(PartyMemberInstance partyMember)
    {
        GameObject partyMemberUI = Instantiate(partyMemberUIPrefab, transform);
        partyMemberUI.GetComponent<HomePartyMemberUI>().PartyMember = partyMember;
        partyMemberUIs.Add(partyMemberUI);
    }

    /// <summary>
    /// Helper method that updates the order of the party member UI objects to match the order they are in the list.
    /// </summary>
    private void UpdatePartyMemberUIOrder()
    {
        for (int i = 0; i < partyMemberUIs.Count; i++)
            partyMemberUIs[i].transform.SetSiblingIndex(i);
    }
}

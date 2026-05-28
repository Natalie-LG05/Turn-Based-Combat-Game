using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the UI for choosing a party member to switch in (either when switching or when replacing a dead party member).
/// </summary>
public class PartyMemberSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject activePartyMemberUIPrefab;
    [SerializeField] private GameObject inactivePartyMemberUIPrefab;
    [SerializeField] private GameObject deadPartyMemberUIPrefab;

    [SerializeField] private Transform partyMemberUIContainer;
    private List<GameObject> partyMemberUIs;

    private void Awake()
    {
        partyMemberUIs = new List<GameObject>();
    }

    /// <summary>
    /// Set and display the player's current party. 
    /// <br/>Takes into account the current order of party members and does not show dead party members (for now).
    /// <br/>If a party member died this turn, shows only the first member as active, the 2nd as inactive, and the 3rd (if any) as inactive.
    /// <br/>instead of showing the first two as active and last two (if any) as inactive.
    /// </summary>
    /// <param name="alivePartyMembers">The current party.</param>
    /// <param name="partyMemberDiedThisTurn">Whether or not a party member died this turn.</param>
    public void SetParty(List<PartyMemberInstance> alivePartyMembers, List<PartyMemberInstance> deadPartyMembers, bool partyMemberDiedThisTurn)
    {
        // clear old data
        foreach (GameObject partyMemberUI in partyMemberUIs)
            Destroy(partyMemberUI);
        partyMemberUIs.Clear();

        NewPartyMemberUI(alivePartyMembers[0], true, false);
        // if a party member died this turn, only the first party member is active, otherwise the first two are active
        if (!partyMemberDiedThisTurn)
        {
            if (alivePartyMembers.Count > 1) NewPartyMemberUI(alivePartyMembers[1], true, false);
            if (alivePartyMembers.Count > 2) NewPartyMemberUI(alivePartyMembers[2], false, false);
            if (alivePartyMembers.Count > 3) NewPartyMemberUI(alivePartyMembers[3], false, false);
        } else
        {
            if (alivePartyMembers.Count > 1) NewPartyMemberUI(alivePartyMembers[1], false, false);
            if (alivePartyMembers.Count > 2) NewPartyMemberUI(alivePartyMembers[2], false, false);
        }

        foreach (PartyMemberInstance deadPartyMember in deadPartyMembers)
            NewPartyMemberUI(deadPartyMember, false, true);
    }

    /// <summary>
    /// Create a new party member UI and set its data.
    /// </summary>
    /// <param name="partyMember">The party member to attach to the UI and show info about.</param>
    /// <param name="isActive">Whether or not the provided party member is active.</param>
    private void NewPartyMemberUI(PartyMemberInstance partyMember, bool isActive, bool isDead)
    {
        GameObject partyMemberUIPrefab;
        if (isActive && !isDead) partyMemberUIPrefab = activePartyMemberUIPrefab;
        else if (!isActive && !isDead) partyMemberUIPrefab = inactivePartyMemberUIPrefab;
        else partyMemberUIPrefab = deadPartyMemberUIPrefab;

        GameObject partyMemberUI = Instantiate(partyMemberUIPrefab, partyMemberUIContainer);
        partyMemberUIs.Add(partyMemberUI);
        partyMemberUI.GetComponent<PartyMemberOptionUI>().SetCharacter(partyMember, !isActive && !isDead);
    }
}

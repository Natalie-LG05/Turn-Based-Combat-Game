using System.Collections.Generic;
using UnityEngine;

public class PartyMemberSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject activePartyMemberUIPrefab;
    [SerializeField] private GameObject inactivePartyMemberUIPrefab;

    [SerializeField] private Transform partyMemberUIContainer;
    private List<GameObject> partyMemberUIs;

    private void Awake()
    {
        partyMemberUIs = new List<GameObject>();
    }

    public void SetParty(List<PartyMemberInstance> party, bool partyMemberDiedThisTurn)
    {
        // clear old data
        foreach (GameObject partyMemberUI in partyMemberUIs)
            Destroy(partyMemberUI);
        partyMemberUIs.Clear();

        NewPartyMemberUI(party[0], true);
        // if a party member died this turn, only the first party member is active, otherwise the first two are active
        if (!partyMemberDiedThisTurn)
        {
            if (party.Count > 1) NewPartyMemberUI(party[1], true);
            if (party.Count > 2) NewPartyMemberUI(party[2], false);
            if (party.Count > 3) NewPartyMemberUI(party[3], false);
        } else
        {
            if (party.Count > 1) NewPartyMemberUI(party[1], false);
            if (party.Count > 2) NewPartyMemberUI(party[2], false);
        }
    }

    private void NewPartyMemberUI(PartyMemberInstance partyMember, bool active)
    {
        GameObject partyMemberUI = Instantiate(active ? activePartyMemberUIPrefab : inactivePartyMemberUIPrefab, partyMemberUIContainer);
        partyMemberUIs.Add(partyMemberUI);
        partyMemberUI.GetComponent<PartyMemberOptionUI>().SetCharacter(partyMember, active);
    }
}

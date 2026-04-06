using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    [SerializeField] private List<PartyMemberInstance> partyMembers;

    private void OnValidate()
    {
        if (partyMembers.Count > 4)
            partyMembers.RemoveRange(4, partyMembers.Count - 4);
    }

    private void Awake()
    {
        InitializePartyMembers();
    }

    private void InitializePartyMembers()
    {
        foreach (PartyMemberInstance partyMember in partyMembers)
        {
            partyMember.Init();
        }
    }
}
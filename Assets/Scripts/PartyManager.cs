using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [SerializeField] private List<PartyMemberInstance> _partyMembers;

    public List<PartyMemberInstance> PartyMembers { get => _partyMembers; }

    private void OnValidate()
    {
        // the party can only have up to four party members in it
        if (_partyMembers.Count > 4)
            _partyMembers.RemoveRange(4, _partyMembers.Count - 4);
    }

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePartyMembers();
    }

    private void InitializePartyMembers()
    {
        foreach (PartyMemberInstance partyMember in _partyMembers)
            partyMember.Init();
    }
}
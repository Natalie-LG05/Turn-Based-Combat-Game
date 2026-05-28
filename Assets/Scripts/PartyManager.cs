using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A singleton class that stores all the player's party members and manages the player's party. 
/// </summary>
[System.Serializable]
public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [SerializeField] private List<PartyMemberInstance> _party;
    [SerializeField] private List<PartyMemberInstance> _unlockedPartyMembers;
    [SerializeField] private List<PartyMemberInstance> _lockedPartyMembers;

    public List<PartyMemberInstance> Party { get => _party; }
    public List<PartyMemberInstance> UnlockedPartyMembers { get => _unlockedPartyMembers; }
    public List<PartyMemberInstance> LockedPartyMembers { get => _lockedPartyMembers; }

    public float AveragePartyMemberLevel
    {
        get
        {
            List<float> levels = Party.Select(partyMember => (float)partyMember.Level).ToList();
            return levels.Average();
        }
    }

    private void OnValidate()
    {
        // the party can only have up to four party members in it
        if (_party.Count > 4)
            _party.RemoveRange(4, _party.Count - 4);
    }

    private void Awake()
    {
        // If there is already an instance of this class, destroy new instances
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;  // Set the singleton instance
        DontDestroyOnLoad(gameObject);  // Set this object to persist between scenes

        InitializePartyMembers();
    }

    /// <summary>
    /// Swap the position of two party members in the party.
    /// </summary>
    /// <param name="partyMember1">The first party member.</param>
    /// <param name="partyMember2">The second party member.</param>
    public void SwapPartyMembers(PartyMemberInstance partyMember1, PartyMemberInstance partyMember2)
    {
        int index1 = Party.IndexOf(partyMember1);
        int index2 = Party.IndexOf(partyMember2);
        (_party[index1], _party[index2]) = (_party[index2], _party[index1]);
    }

    /// <summary>
    /// Initialize all party members, and add the first four unlocked party members (if any) into the party by default.
    /// </summary>
    private void InitializePartyMembers()
    {
        if (UnlockedPartyMembers.Count > 0)
        {
            foreach (PartyMemberInstance partyMember in UnlockedPartyMembers)
                partyMember.Init();
            foreach (PartyMemberInstance partyMember in LockedPartyMembers)
                partyMember.Init();

            // Add the first four unlocked party members into the party by default
            for (int i = 0; i < 4; i++)
                if (UnlockedPartyMembers.Count >= i+1) _party.Add(UnlockedPartyMembers[i]);
        } else
        {
            foreach (PartyMemberInstance partyMember in Party)
                partyMember.Init();
        }
    }
}
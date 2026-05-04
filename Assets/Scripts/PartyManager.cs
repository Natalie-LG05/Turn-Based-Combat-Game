using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A singleton class that stores all the player's party members and manages the player's party. 
/// </summary>
public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [SerializeField] private List<PartyMemberInstance> _partyMembers;

    /// <summary>Gets the player's party.</summary>
    public List<PartyMemberInstance> PartyMembers { get => _partyMembers; }

    private void OnValidate()
    {
        // the party can only have up to four party members in it
        if (_partyMembers.Count > 4)
            _partyMembers.RemoveRange(4, _partyMembers.Count - 4);
    }

    private void Awake()
    {
        // If there is already an instance of this class, destroy new instances
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;  // Set the singleton instance
        DontDestroyOnLoad(gameObject);  // Set this object to persist between scenes

        InitializePartyMembers();
    }

    private void InitializePartyMembers()
    {
        foreach (PartyMemberInstance partyMember in _partyMembers)
            partyMember.Init();
    }
}
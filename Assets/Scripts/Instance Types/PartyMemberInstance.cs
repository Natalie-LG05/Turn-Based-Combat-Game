using UnityEngine;

/// <summary>
/// Represents an instance of a party member, which is based off of a PartyMemberData.
/// </summary>
[System.Serializable]
public class PartyMemberInstance : CharacterInstance
{
    [SerializeField, Tooltip("The scriptable object to base this party member off of.")] private PartyMemberData _partyMemberData;

    //TODO: need to make equipable items first, then can uncomment the below code and expand on it
    //[SerializeField] private Weapon _weapon;
    //[SerializeField] private Focus _focus;
    //[SerializeField] private Armor _armor;
    //[SerializeField] private Charm[] _charms;
    //[SerializeField] private MoveBooster _moveBooster;

    // stat boosts

    /// <summary>Gets the party member data this party member instance is based off of.</summary>
    public PartyMemberData PartyMemberData { get => _partyMemberData; }

    /// <inheritdoc/>
    public override void Init()
    {
        // upcast party member data into character data and assign it before initializing to avoid errors
        _characterData = _partyMemberData;
        base.Init();
    }
}

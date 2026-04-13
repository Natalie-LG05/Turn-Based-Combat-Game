using UnityEngine;

[System.Serializable]
public class PartyMemberInstance : CharacterInstance
{
    [SerializeField] private PartyMemberData _partyMemberData;

    //TODO: need to make equipable items first, then can uncomment the below code and expand on it
    //[SerializeField] private Weapon _weapon;
    //[SerializeField] private Focus _focus;
    //[SerializeField] private Armor _armor;
    //[SerializeField] private Charm[] _charms;
    //[SerializeField] private MoveBooster _moveBooster;

    // stat boosts

    public PartyMemberData PartyMemberData { get => _partyMemberData; }

    public override void Init()
    {
        // upcast party member data into character data and assign it before initializing to avoid errors
        _characterData = _partyMemberData;
        base.Init();
    }
}

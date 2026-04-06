using UnityEngine;

[System.Serializable]
public class PartyMemberInstance : CharacterInstance
{
    [SerializeField] private PartyMemberData partyMemberData;

    //TODO: need to make equipable items first, then can uncomment the below code and expand on it
    //[SerializeField] private Weapon _weapon;
    //[SerializeField] private Focus _focus;
    //[SerializeField] private Armor _armor;
    //[SerializeField] private Charm[] _charms;
    //[SerializeField] private MoveBooster _moveBooster;

    //// stat boosts

    public override void Init()
    {
        _characterData = partyMemberData;
        base.Init();
    }
}

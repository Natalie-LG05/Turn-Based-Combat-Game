using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an instance of a party member, which is based off of a PartyMemberData.
/// </summary>
[System.Serializable]
public class PartyMemberInstance : CharacterInstance
{
    [SerializeField, Tooltip("The scriptable object to base this party member off of.")] private PartyMemberData _partyMemberData;

    [SerializeField] private EquipmentData _weapon;
    [SerializeField] private EquipmentData _focus;
    [SerializeField] private EquipmentData _armor;
    [SerializeField] private EquipmentData[] _charms = new EquipmentData[2];
    [SerializeField] private EquipmentData _moveBooster;

    private Dictionary<Stat, int> _krillStatUps;

    /// <summary>Gets the party member data this party member instance is based off of.</summary>
    public PartyMemberData PartyMemberData { get => _partyMemberData; }

    public EquipmentData Weapon { get => _weapon; }
    public EquipmentData Focus { get => _focus; }
    public EquipmentData Armor { get => _armor; }
    public EquipmentData[] Charms { get => _charms; }
    public EquipmentData MoveBooster { get => _moveBooster; }

    /// <summary>Gets a dictionary containing the current stat boosts from krill this party member has.</summary>
    public Dictionary<Stat, int> KrillStatUps { get => _krillStatUps; }

    /// <inheritdoc/>
    public override void Init()
    {
        // upcast party member data into character data and assign it before initializing to avoid errors
        _characterData = _partyMemberData;

        if (Charms == null) _charms = new EquipmentData[2];

        InitializeKrillStatUps();

        base.Init();
    }

    public void AddKrillStatUp(Stat stat, int amount)
    {
        _krillStatUps[stat] += amount;
        _additionalStats[stat] += amount;
    }

    private void InitializeKrillStatUps()
    {
        _krillStatUps = new Dictionary<Stat, int>()
        {
            { Stat.MaxHP, 0 },
            { Stat.Attack, 0 },
            { Stat.Support, 0 },
            { Stat.Defense, 0 },
            { Stat.Speed, 0 }
        };
    }
}

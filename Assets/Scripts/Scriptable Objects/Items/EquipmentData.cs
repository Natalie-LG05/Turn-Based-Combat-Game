using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for an equipable item.
/// </summary>
[CreateAssetMenu(fileName = "NewEquipmentData", menuName = "ScriptableObjects/Equipment Data", order = -1000)]
public class EquipmentData : ItemData
{
    [SerializeField] private EquipmentStats _stats;
    [SerializeField] private List<string> validPartyMemberIds;

    public EquipmentStats Stats { get => _stats; }
    public List<string> ValidPartyMemberIds { get => validPartyMemberIds; }
}

[System.Serializable]
public class EquipmentStats
{
    [SerializeField] private int _maxHP;
    [SerializeField] private int _attack;
    [SerializeField] private int _support;
    [SerializeField] private int _defense;
    [SerializeField] private int _speed;

    public int MaxHP { get => _maxHP; }
    public int Attack { get => _attack; }
    public int Support { get => _support; }
    public int Defense { get => _defense; }
    public int Speed { get => _speed; }
}
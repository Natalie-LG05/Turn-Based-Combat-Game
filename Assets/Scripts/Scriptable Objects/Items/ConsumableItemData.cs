using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for a consumable item.
/// </summary>
[CreateAssetMenu(fileName = "NewConsumableItemData", menuName = "ScriptableObjects/Consumable Item Data", order = -1000)]
public class ConsumableItemData : ItemData
{
    [SerializeField] private int _numberOfTargets;

    public int NumberOfTargets { get => _numberOfTargets; }

    public virtual bool Useable(PartyMemberInstance partyMember)
    {
        return false;
    }

    public virtual bool Useable(List<PartyMemberInstance> partyMembers)
    {
        foreach (PartyMemberInstance partyMember in partyMembers)
            if (Useable(partyMember)) return true;
        return false;
    }
}
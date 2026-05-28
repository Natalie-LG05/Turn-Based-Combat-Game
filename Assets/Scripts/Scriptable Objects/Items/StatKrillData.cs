using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for a stat krill.
/// </summary>
[CreateAssetMenu(fileName = "NewStatKrillData", menuName = "ScriptableObjects/Stat Krill Data", order = -1000)]
public class StatKrillData : ConsumableItemData
{
    [SerializeField] private int _amountGained;
    [SerializeField] private int _maxBoost;
    [SerializeField] private Stat _stat;

    public int AmountGained { get => _amountGained; }
    public int MaxBoost { get => _maxBoost; }
    public Stat Stat { get => _stat; }

    public override Effect Effects
    {
        get
        {
            return new Effect()
            {
                ItemOnUse = (CharacterInstance user, ItemData item, List<CharacterInstance> targets) =>
                {
                    foreach (CharacterInstance character in targets)
                    {
                        PartyMemberInstance partyMember = (PartyMemberInstance)character;
                        if (partyMember.KrillStatUps[Stat] < MaxBoost)
                        {
                            int amount = partyMember.KrillStatUps[Stat] + AmountGained <= MaxBoost ? AmountGained : MaxBoost - partyMember.KrillStatUps[Stat];
                            partyMember.AddKrillStatUp(Stat, amount);
                        }
                    }
                }
            };
        }
    }

    public override bool Useable(PartyMemberInstance partyMember)
    {
        return partyMember.KrillStatUps[Stat] < MaxBoost;
    }
}
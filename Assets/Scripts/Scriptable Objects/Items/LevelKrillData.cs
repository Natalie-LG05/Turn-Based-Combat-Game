using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for a level krill.
/// </summary>
[CreateAssetMenu(fileName = "NewLevelKrillData", menuName = "ScriptableObjects/Level Krill Data", order = -1000)]
public class LevelKrillData : ConsumableItemData
{
    [SerializeField] private int _amountGained;
    [SerializeField] private int _maxLevel;

    public int AmountGained { get => _amountGained; }
    public int MaxLevel { get => _maxLevel; }

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
                        if (character.Level >= MaxLevel) continue;

                        for (int i = 0; i < AmountGained; i++)
                            if (character.Level < MaxLevel) character.LevelUp();
                    }
                }
            };
        }
    }

    public override bool Useable(PartyMemberInstance partyMember)
    {
        return partyMember.Level < MaxLevel;
    }
}

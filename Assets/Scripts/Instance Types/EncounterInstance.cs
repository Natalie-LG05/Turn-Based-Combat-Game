using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an instance of an encounter, which is based off of an EncounterData.
/// </summary>
[System.Serializable]
public class EncounterInstance
{
    [SerializeField] private EncounterData _encounterData;

    private int _timesCleared;

    public EncounterData EncounterData { get => _encounterData; }

    /// <summary>Gets whether or not this encounter has been cleared at least once.</summary>
    public bool IsCleared { get => _timesCleared > 0; }
    public int TimesCleared { get => _timesCleared; }

    /// <summary>Gets the rewards for clearing this encounter in its current state.</summary>
    public List<ItemEntry<ItemData>> Rewards
    {
        get
        {
            List<ItemEntry<ItemData>> itemEntries = new List<ItemEntry<ItemData>>();
            if (!IsCleared)
            {
                foreach (FixedEncounterReward fixedReward in EncounterData.FirstClearRewards)
                    itemEntries.Add(new ItemEntry<ItemData>(fixedReward.Item, fixedReward.Amount));
            } else
            {
                foreach (RandomEncounterReward randomReward in EncounterData.SubsequentClearRewards)
                {
                    int amount = randomReward.GetRandomAmount();
                    if (amount > 0) itemEntries.Add(new ItemEntry<ItemData>(randomReward.Item, amount));
                }
            }
            return itemEntries;
        }
    }

    /// <summary>
    /// Create a new encounter instance and initialize it.
    /// </summary>
    /// <param name="encounterData">The encounter data to base the new encounter instance off of.</param>
    public EncounterInstance(EncounterData encounterData)
    {
        _encounterData = encounterData;
        _timesCleared = 0;
    }

    /// <summary>
    /// Signal to this encounter instance that it has been cleared so that it can keep track of how many times it has been cleared.
    /// </summary>
    public void Clear()
    {
        _timesCleared++;
    }
}

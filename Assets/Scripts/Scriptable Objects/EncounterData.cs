using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for an encounter.
/// </summary>
[CreateAssetMenu(fileName = "NewEncounterData", menuName = "ScriptableObjects/Encounter Data", order = -1000)]
public class EncounterData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField, Tooltip("The unique string id of this encounter.")] private string _id;
    [SerializeField, TextArea] private string _description;

    [SerializeField] private EncounterType _type;
    [SerializeField] private int _difficultyLevel;

    [SerializeField] private AreaData _area;
    [SerializeField] private EncounterModifierData[] _modifiers;
    [SerializeField] private EncounterWave[] _waves;

    [SerializeField] private List<FixedEncounterReward> _firstClearRewards;
    [SerializeField] private List<RandomEncounterReward> _subsequentClearRewards;

    public string Name { get => _name; }
    /// <summary>Gets the unique string id of this encounter</summary>
    public string Id { get => _id; }
    public string Description { get => _description; }

    public EncounterType Type { get => _type; }
    public int DifficultyLevel { get => _difficultyLevel; }

    public AreaData Area { get => _area; }
    public EncounterModifierData[] Modifiers { get => _modifiers; }
    public EncounterWave[] Waves { get => _waves; }

    public List<FixedEncounterReward> FirstClearRewards { get => _firstClearRewards; }
    public List<RandomEncounterReward> SubsequentClearRewards { get => _subsequentClearRewards; }

    private void OnValidate()
    {
        // ensure that no wave has more than five enemies
        foreach (EncounterWave wave in _waves)
            wave.OnValidate();
    }
}

public enum EncounterType { Normal, SingleWild, DoubleWild, HardWild, HardDoubleWild, Dungeon }

/// <summary>
/// A class that represents a wave in an encounter, containing a list of up to five enemies.
/// </summary>
[System.Serializable]
public class EncounterWave
{
    [SerializeField] private List<EnemyInstance> _enemies;

    public List<EnemyInstance> Enemies { get => _enemies; }

    public void OnValidate()
    {
        // ensure that no wave has more than five enemies
        if (_enemies.Count > 5)
            _enemies.RemoveRange(5, _enemies.Count - 5);
    }
}

/// <summary>
/// A class that represents an item reward from an encounter along with how much of it.
/// </summary>
[System.Serializable]
public class EncounterReward
{
    [SerializeField] protected ItemData _item;

    public ItemData Item { get => _item; }
}

/// <summary>
/// A type of encounter reward that always gives a specified amount of its item.
/// </summary>
[System.Serializable]
public class FixedEncounterReward : EncounterReward
{
    [SerializeField] private int _amount;

    public int Amount { get => _amount; }
}

/// <summary>
/// A type of encounter reward that give a random amount of its item or even none of its item.
/// </summary>
[System.Serializable]
public class RandomEncounterReward : EncounterReward
{
    [SerializeField] private RandomizationType _randomizationType;

    [SerializeField] private int _minAmount;
    [SerializeField] private int _maxAmount;

    [SerializeField] private List<AmountChancePair> _amountChancePairs;

    public RandomizationType RandomizationType { get => _randomizationType; }

    public int MinAmount { get => _minAmount; }
    public int MaxAmount { get => _maxAmount; }

    public List<AmountChancePair> AmountChancePairs { get => _amountChancePairs; }
    public int TotalWeight { get { return AmountChancePairs.Select(pair => pair.Weight).ToList().Sum(); } }

    /// <summary>
    /// Randomly determines how many items to give based on this object's internal logic and values.
    /// </summary>
    /// <returns>Returns the amount of the item to award.</returns>
    public int GetRandomAmount()
    {
        if (RandomizationType == RandomizationType.Weighted)
        {
            // weighted randomization logic
            int randNum = Random.Range(0, TotalWeight);
            foreach (AmountChancePair acp in AmountChancePairs)
            {
                if (randNum < acp.Weight)
                    return acp.Amount;
                randNum -= acp.Weight;
            }
        } else if (RandomizationType == RandomizationType.Range)
        {
            return Random.Range(MinAmount, MaxAmount + 1);
        }

        return 0;  // just here as a fallthrough so the code will compile with no errors, this line should never actually be reached during runtime
    }


    /// <summary>
    /// A class that contains two integers, the amount of an item to award, and the weight for that amount which is used when randomly determining the amount.
    /// </summary>
    [System.Serializable]
    public class AmountChancePair
    {
        [SerializeField] private int _amount;
        [SerializeField] private int _weight;

        public int Amount { get => _amount; }
        public int Weight { get => _weight; }
    }
}

public enum RandomizationType { Range, Weighted }
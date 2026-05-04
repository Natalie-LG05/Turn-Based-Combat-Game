using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for a character.
/// </summary>
[CreateAssetMenu(fileName = "NewCharacterData", menuName = "ScriptableObjects/Character Data", order = -1000)]
public class CharacterData : ScriptableObject
{
    [SerializeField] protected string _name;
    [SerializeField, Tooltip("The unique string id of this character.")] protected string _id;
    [SerializeField, TextArea] protected string _description;
    [SerializeField] protected Sprite _sprite;

    [SerializeField] protected List<CharacterCategory> _categories;

    [SerializeField, Tooltip("The amount of stat points this character has at level 1.")] protected int _baseStatPoints;
    [SerializeField, Tooltip("The amount of stat points this character gains when leveling up.")] protected int _levelupStatPoints;

    [SerializeField, Tooltip("The percentage of this character's stat points allocated to each stat.")] protected StatSpread _statSpread;
    [SerializeField, Tooltip("The moves this character learns and at what levels.")] protected MoveLevelPair[] _moveLearnset;
    [SerializeField, Tooltip("The abilities this character learns and at what levels.")] protected AbilityLevelPair[] _abilityLearnset;

    public string Name { get => _name; }
    /// <summary>Gets the unique string id of this character.</summary>
    public string Id { get => _id; }
    public string Description { get => _description; }
    public Sprite Sprite { get => _sprite; }

    public List<CharacterCategory> Categories { get => _categories; }

    /// <summary>Gets the amount of stat points this character has at level 1.</summary>
    public int BaseStatPoints { get => _baseStatPoints; }
    /// <summary>Gets the amount of stat points this character gains when leveling up.</summary>
    public int LevelupStatPoints { get => _levelupStatPoints; }

    /// <summary>Gets the percentage of this character's stat points allocated to each stat.</summary>
    public StatSpread StatSpread { get => _statSpread; }
    /// <summary>Gets the moves this character learns and at what levels.</summary>
    public MoveLevelPair[] MoveLearnset { get => _moveLearnset; }
    /// <summary>Gets the abilities this character learns and at what levels.</summary>
    public AbilityLevelPair[] AbilityLearnset { get => _abilityLearnset; }
}

public enum CharacterCategory { Crab, Bird }

/// <summary>
/// A class that contains the percentage of stat points to be allocated to each of the five main stats.
/// </summary>
[System.Serializable]
public class StatSpread
{
    [SerializeField, Range(0,100)] private int _maxHP;
    [SerializeField, Range(0,100)] private int _attack;
    [SerializeField, Range(0,100)] private int _support;
    [SerializeField, Range(0,100)] private int _defense;
    [SerializeField, Range(0,100)] private int _speed;

    public int MaxHP { get => _maxHP; }
    public int Attack { get => _attack; }
    public int Support { get => _support; }
    public int Defense { get => _defense; }
    public int Speed { get => _speed; }
}

/// <summary>
/// A class that contains a move and what level it should be learned at.
/// </summary>
[System.Serializable]
public class MoveLevelPair
{
    [SerializeField] private MoveData _moveData;
    [SerializeField] private int _level;

    public MoveData MoveData { get => _moveData; }
    public int Level { get => _level; }
}

/// <summary>
/// A class that contains an ability and what level it should be learned at.
/// </summary>
[System.Serializable]
public class AbilityLevelPair
{
    [SerializeField] private AbilityData _abilityData;
    [SerializeField] private int _level;

    public AbilityData AbilityData { get => _abilityData; }
    public int Level { get => _level; }
}
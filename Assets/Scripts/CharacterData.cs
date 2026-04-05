using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "ScriptableObjects/Character Data", order = -1000)]
public class CharacterData : ScriptableObject
{
    [SerializeField] protected string _name;
    [SerializeField] protected string _id;
    [SerializeField, TextArea] protected string _description;

    [SerializeField] protected int _baseStatPoints;
    [SerializeField] protected int _levelupStatPoints;

    [SerializeField] protected StatSpread _statSpread;
    [SerializeField] protected MoveLevelPair[] _moveLearnset;
    [SerializeField] protected AbilityLevelPair[] _abilityLearnset;

    public string Name { get => _name; }
    public string Id { get => _id; }
    public string Description { get => _description; }

    public int BaseStatPoints { get => _baseStatPoints; }
    public int LevelupStatPoints { get => _levelupStatPoints; }

    public StatSpread StatSpread { get => _statSpread; }
    public MoveLevelPair[] MoveLearnset { get => _moveLearnset; }
    public AbilityLevelPair[] AbilityLearnset { get => _abilityLearnset; }
}

[System.Serializable]
public class StatSpread
{
    [SerializeField, Range(0,100)] private int _maxHP;
    [SerializeField, Range(0,100)] private int _attack;
    [SerializeField, Range(0,100)] private int _support;
    [SerializeField, Range(0,100)] private int _defense;
    [SerializeField, Range(0, 100)] private int _speed;

    public int MaxHP { get; private set; }
    public int Attack { get; private set; }
    public int Support { get; private set; }
    public int Defense { get; private set; }
    public int Speed { get; private set; }
}

[System.Serializable]
public class MoveLevelPair
{
    [SerializeField] private MoveData _moveData;
    [SerializeField] private int _level;

    public MoveData MoveData { get => _moveData; }
    public int Level { get => _level; }
}

[System.Serializable]
public class AbilityLevelPair
{
    [SerializeField] private AbilityData _abilityData;
    [SerializeField] private int _level;

    public AbilityData AbilityData { get => _abilityData; }
    public int Level { get => _level; }
}

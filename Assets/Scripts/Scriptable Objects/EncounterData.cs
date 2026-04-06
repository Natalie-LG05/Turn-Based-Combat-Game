using UnityEngine;

[CreateAssetMenu(fileName = "NewEncounterData", menuName = "ScriptableObjects/Encounter Data", order = -1000)]
public class EncounterData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _id;
    [SerializeField, TextArea] private string _description;

    [SerializeField] private EncounterType _type;
    [SerializeField] private int _difficultyLevel;

    [SerializeField] private AreaData _area;
    [SerializeField] private EncounterModifierData[] _modifiers;
    [SerializeField] private EncounterWave[] _waves;
    //TODO: Encounter rewards

    public string Name { get => _name; }
    public string Id { get => _id; }
    public string Description { get => _description; }

    public EncounterType Type { get => _type; }
    public int DifficultyLevel { get => _difficultyLevel; }

    public AreaData Area { get => _area; }
    public EncounterModifierData[] Modifiers { get => _modifiers; }
    public EncounterWave[] Waves { get => _waves; }
}

public enum EncounterType { Normal, SingleWild, DoubleWild, Hard, Dungeon }

[System.Serializable]
public class EncounterWave
{
    [SerializeField] private EnemyInstance[] _enemies;

    public EnemyInstance[] Enemies { get => _enemies; }
}

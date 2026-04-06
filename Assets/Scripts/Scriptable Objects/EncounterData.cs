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
    [SerializeField] private EncounterModifierData[] modifiers;
    [SerializeField] private EncounterWave[] waves;
    //TODO: Encounter rewards
}

public enum EncounterType { Normal, SingleWild, DoubleWild, Hard, Dungeon }

[System.Serializable]
public class EncounterWave
{
    [SerializeField] private EnemyInstance[] _enemies;
}

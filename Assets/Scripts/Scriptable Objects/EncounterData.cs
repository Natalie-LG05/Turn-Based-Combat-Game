using System.Collections.Generic;
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
    //TODO: Encounter rewards

    public string Name { get => _name; }
    /// <summary>Gets the unique string id of this encounter</summary>
    public string Id { get => _id; }
    public string Description { get => _description; }

    public EncounterType Type { get => _type; }
    public int DifficultyLevel { get => _difficultyLevel; }

    public AreaData Area { get => _area; }
    public EncounterModifierData[] Modifiers { get => _modifiers; }
    public EncounterWave[] Waves { get => _waves; }

    private void OnValidate()
    {
        // ensure that no wave has more than five enemies
        foreach (EncounterWave wave in _waves)
        {
            wave.OnValidate();
        }
    }
}

public enum EncounterType { Normal, SingleWild, DoubleWild, Hard, Dungeon }

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

using UnityEngine;

/// <summary>
/// Represents an instance of an encounter, which is based off of an EncounterData.
/// </summary>
[System.Serializable]
public class EncounterInstance
{
    [SerializeField] private EncounterData _encounterData;

    private bool isCleared;
    private int timesCleared;

    public EncounterData EncounterData { get => _encounterData; }

    /// <summary>
    /// Initialize this encounter instance.
    /// </summary>
    public void Init()
    {
        isCleared = false;
        timesCleared = 0;
    }

    /// <summary>
    /// Signal to this encounter instance that it has been cleared so that it can keep track of how many times it has been cleared.
    /// </summary>
    public void Clear()
    {
        isCleared = true;
        timesCleared++;
    }
}

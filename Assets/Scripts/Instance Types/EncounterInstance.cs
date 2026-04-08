using UnityEngine;

[System.Serializable]
public class EncounterInstance
{
    [SerializeField] private EncounterData _encounterData;

    private bool isCleared;
    private int timesCleared;

    public EncounterData EncounterData { get => _encounterData; }
}

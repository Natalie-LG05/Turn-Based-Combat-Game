using UnityEngine;

[System.Serializable]
public class EncounterInstance
{
    [SerializeField] private EncounterData encounterData;

    private bool isCleared;
    private int timesCleared;
}

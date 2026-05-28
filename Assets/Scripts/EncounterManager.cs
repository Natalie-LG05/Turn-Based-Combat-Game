using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A singleton class that stores a list of all encounter instances. 
/// </summary>
[System.Serializable]
public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }

    [SerializeField] private List<EncounterData> encountersDatas;
    private List<EncounterInstance> _encounters;

    [SerializeField] private EncounterInstance _currentEncounter;

    /// <summary>Gets the ordered list of all encounters.</summary>
    public List<EncounterInstance> Encounters { get => _encounters; }

    /// <summary>Gets the current encounter, which is the encounter currently being attempted (if any).</summary>
    public EncounterInstance CurrentEncounter { get => _currentEncounter; set => _currentEncounter = value; }

    private void Awake()
    {
        // If there is already an instance of this class, destroy new instances
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;  // Set the singleton instance
        DontDestroyOnLoad(gameObject);  // Set this object to persist between scenes

        _encounters = new List<EncounterInstance>();

        foreach (EncounterData encounterData in encountersDatas)
            _encounters.Add(new EncounterInstance(encounterData));
    }
}

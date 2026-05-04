using UnityEngine;

/// <summary>
/// A singleton class that stores a list of all encounter instances. 
/// </summary>
public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }

    // list of all encounter instances

    [SerializeField] private EncounterData _currentEncounterData;

    /// <summary>Gets the current encounter, which is the encounter currently being attempted (if any).</summary>
    public EncounterData CurrentEncounter { get => _currentEncounterData; }

    private void Awake()
    {
        // If there is already an instance of this class, destroy new instances
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;  // Set the singleton instance
        DontDestroyOnLoad(gameObject);  // Set this object to persist between scenes
    }
}

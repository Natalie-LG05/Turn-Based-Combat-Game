using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance { get; private set; }

    // list of all encounter instances

    [SerializeField] private EncounterData _currentEncounterData;

    public EncounterData CurrentEncounter { get => _currentEncounterData; }

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

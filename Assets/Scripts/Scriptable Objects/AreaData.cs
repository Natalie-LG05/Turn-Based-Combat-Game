using UnityEngine;

[CreateAssetMenu(fileName = "NewAreaData", menuName = "ScriptableObjects/Area Data", order = -1000)]
public class AreaData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _id;
    [SerializeField, TextArea] private string _description;

    [SerializeField] private EncounterModifierData[] _modifiers;
    [SerializeField] private EnemyData[] _enemies;
}

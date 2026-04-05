using UnityEngine;

[CreateAssetMenu(fileName = "EncounterModifierData", menuName = "ScriptableObjects/Encounter Modifier Data", order = -1000)]
public class EncounterModifierData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _id;
    [SerializeField, TextArea] private string _description;

    [SerializeField] private StatusEffectData[] effects;
}

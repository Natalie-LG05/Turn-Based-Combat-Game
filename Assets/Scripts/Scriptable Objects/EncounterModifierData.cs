using UnityEngine;

[CreateAssetMenu(fileName = "EncounterModifierData", menuName = "ScriptableObjects/Encounter Modifier Data", order = -1000)]
public class EncounterModifierData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _id;
    [SerializeField, TextArea] private string _description;

    [SerializeField] private Color _iconColor;
    [SerializeField] private StatusEffectData[] _effects;

    public string Name { get => _name; }
    public string Id { get => _id; }
    public string Description { get => _description; }

    public Color Color { get => _iconColor; }
    public StatusEffectData[] Effects { get => _effects; }
}

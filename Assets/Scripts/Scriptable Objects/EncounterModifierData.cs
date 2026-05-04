using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for an encounter modifier.
/// </summary>
[CreateAssetMenu(fileName = "EncounterModifierData", menuName = "ScriptableObjects/Encounter Modifier Data", order = -1000)]
public class EncounterModifierData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField, Tooltip("The unique string id of this encounter modifier.")] private string _id;
    [SerializeField, TextArea] private string _description;

    [SerializeField] private Color _iconColor;
    [SerializeField, Tooltip("The status effects applied by this encounter modifier.")] private List<StatusEffectData> _effects;

    public string Name { get => _name; }
    /// <summary>Gets the unique string id of this encounter modifier.</summary>
    public string Id { get => _id; }
    public string Description { get => _description; }

    public Color Color { get => _iconColor; }
    /// <summary>Gets the status effects applied by this encounter modifier.</summary>
    public List<StatusEffectData> Effects { get => _effects; }
}

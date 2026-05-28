using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for an ability.
/// </summary>
[CreateAssetMenu(fileName = "NewAbilityData", menuName = "ScriptableObjects/Ability Data", order = -1000)]
public class AbilityData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField, Tooltip("The unique string id of this ability.")] private string _id;
    [SerializeField, TextArea] private string _description;

    [SerializeField, Tooltip("The status effect applied by this ability (if any).")] private StatusEffectData _statusEffect;

    public string Name { get => _name; }
    /// <summary>Gets the unique string id of this ability.</summary>
    public string Id { get => _id; }
    public string Description { get => _description; }

    /// <summary>Gets the status effect applied by this ability (if any).</summary>
    public StatusEffectData StatusEffect { get => _statusEffect; }

    /// <summary>Gets the effects of this ability, found by its id.</summary>
    public Effect Effects
    {
        get
        {
            if (EffectsDB.AbilityEffects.ContainsKey(_id)) return EffectsDB.AbilityEffects[_id];
            else return null;
        }
    }
}
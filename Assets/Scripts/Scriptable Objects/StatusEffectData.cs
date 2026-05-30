using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType { Buff, Debuff, Other }
public enum StatusEffectCategory { MaxHPUp, MaxHPDown, AttackUp, AttackDown, SupportUp, SupportDown, DefenseUp, DefenseDown, SpeedUp, SpeedDown, AccuracyUp, AccuracyDown, EvasionUp, EvasionDown }

/// <summary>
/// A scriptable object that contains the data for a status effect.
/// </summary>
[CreateAssetMenu(fileName = "NewStatusEffectData", menuName = "ScriptableObjects/Status Effect Data", order = -1000)]
public class StatusEffectData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField, Tooltip("The unique string id of this status effect.")] private string _id;
    [SerializeField, TextArea] private string _description;
    [SerializeField] private StatusEffectType _type;
    [SerializeField] private List<StatusEffectCategory> _categories;

    [SerializeField] private Color _iconColor;

    [SerializeField] private bool _isPermanent;

    [SerializeField] private List<StatusEffectStatModifier> _statIncreases;
    [SerializeField] private List<StatusEffectStatModifier> _statDecreases;

    public string Name { get => _name; }
    /// <summary>Gets the unique string id of this status effect.</summary>
    public string Id { get => _id; }
    public string Description { get => _description; }
    public StatusEffectType Type { get => _type; }
    public List<StatusEffectCategory> Categories { get => _categories; }

    public Color IconColor { get => _iconColor; }

    public bool IsPermanent { get => _isPermanent; }

    public List<StatusEffectStatModifier> StatIncreases { get => _statIncreases; }
    public List<StatusEffectStatModifier> StatDecreases { get => _statDecreases; }
}

/// <summary>
/// Represents a stat modifier applied by a status effect.
/// </summary>
[System.Serializable]
public class StatusEffectStatModifier
{
    [SerializeField] private Stat _stat;
    [SerializeField] private float _multiplier;
    [SerializeField] private bool _isStrengthFixed;
    [SerializeField] private float _fixedStrength;

    public Stat Stat { get => _stat; }
    public float Multiplier { get => _multiplier; set => _multiplier = value; }
    public bool IsStrengthFixed { get => _isStrengthFixed; }
    public float FixedStrength { get => _fixedStrength; }

    public StatusEffectStatModifier(Stat stat, float multiplier, float fixedStrength, bool isStrengthFixed)
    {
        _stat = stat;
        _multiplier = multiplier;
        _fixedStrength = fixedStrength;
        _isStrengthFixed = isStrengthFixed;
    }

    public StatusEffectStatModifier DeepCopy()
    {
        return new StatusEffectStatModifier(_stat, _multiplier, _fixedStrength, _isStrengthFixed);
    }
}
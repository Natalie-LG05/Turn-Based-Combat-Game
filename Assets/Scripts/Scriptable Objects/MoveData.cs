using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for a move.
/// </summary>
[CreateAssetMenu(fileName = "NewMoveData", menuName = "ScriptableObjects/Move Data", order = -1000)]
public class MoveData : ScriptableObject
{
    [SerializeField] protected string _name;
    [SerializeField, Tooltip("The unique string id of this move.")] protected string _id;
    [SerializeField, TextArea] protected string _description;

    [SerializeField] protected MoveType _type;
    [SerializeField] protected List<MoveCategory> _categories;
    [SerializeField] protected MoveElement _element;

    [SerializeField, Tooltip("The overall power to display for this move.")] protected int _power;
    [SerializeField, Tooltip("The overall accuracy to display for this move.")] protected int _accuracy;
    [SerializeField, Tooltip("The overall targeting type to display for this move.")] protected MoveTarget _target;

    [SerializeField] protected List<MoveDamageEffect> _damageEffects;
    [SerializeField] protected List<MoveStatusEffect> _moveStatusEffects;
    [SerializeField] protected List<MoveHealEffect> _healEffects;

    public string Name { get => _name; }
    /// <summary>Gets the unique string id of this move.</summary>
    public string Id { get => _id; }
    public string Description { get => _description; } 

    public MoveType Type { get => _type; }
    public MoveElement Element { get => _element; }
    public List<MoveCategory> Categories { get => _categories; }

    /// <summary>Gets the overall power of this move at a glance.</summary>
    public int Power { get => _power; }
    /// <summary>Gets the overall accuracy of this move at a glance.</summary>
    public int Accuracy { get => _accuracy; }
    /// <summary>Gets the overall targeting type of this move at a glance.</summary>
    public MoveTarget Target { get => _target; }

    public List<MoveDamageEffect> DamageEffects { get => _damageEffects; }
    public List<MoveStatusEffect> MoveStatusEffects { get => _moveStatusEffects; }
    public List<MoveHealEffect> HealEffects { get => _healEffects; }

    /// <summary>Gets a list of the elements of this move's effects.</summary>
    public List<MoveElement> Elements
    {
        get
        {
            List<MoveElement> elements = new List<MoveElement>();
            foreach (MoveDamageEffect effect in _damageEffects) elements.Add(effect.Element);
            foreach (MoveStatusEffect effect in _moveStatusEffects) elements.Add(effect.Element);
            foreach (MoveHealEffect effect in _healEffects) elements.Add(effect.Element);
            return elements;
        }
    }

    /// <summary>Gets a list of the powers of this move's effects.</summary>
    public List<int> Powers
    {
        get
        {
            List<int> powers = new List<int>();
            foreach (MoveDamageEffect effect in _damageEffects) powers.Add(effect.Power);
            foreach (MoveStatusEffect effect in _moveStatusEffects) powers.Add(effect.Power);
            foreach (MoveHealEffect effect in _healEffects) powers.Add(effect.Power);
            return powers;
        }
    }

    /// <summary>Gets a list of the accuracies of this move's effects.</summary>
    public List<int> Accuracies
    {
        get
        {
            List<int> accuracies = new List<int>();
            foreach (MoveDamageEffect effect in _damageEffects) accuracies.Add(effect.Accuracy);
            foreach (MoveStatusEffect effect in _moveStatusEffects) accuracies.Add(effect.Accuracy);
            foreach (MoveHealEffect effect in _healEffects) accuracies.Add(effect.Accuracy);
            return accuracies;
        }
    }

    /// <summary>Gets a list of the number of hits of this move's damage effects.</summary>
    public List<int> HitsList
    {
        get
        {
            List<int> hits = new List<int>();
            foreach (MoveDamageEffect effect in _damageEffects) hits.Add(effect.Hits);
            return hits;
        }
    }

    /// <summary>Gets a list of the targeting types of this move's effects.</summary>
    public List<MoveTarget> Targets
    {
        get
        {
            List<MoveTarget> targets = new List<MoveTarget>();
            foreach (MoveDamageEffect effect in _damageEffects) targets.Add(effect.Targets);
            foreach (MoveStatusEffect effect in _moveStatusEffects) targets.Add(effect.Targets);
            foreach (MoveHealEffect effect in _healEffects) targets.Add(effect.Targets);
            return targets;
        }
    }

    /// <summary>Gets the total power of damage effects of this move. Takes into account if any damage effects that hit multiple times.</summary>
    public int TotalAttackPower
    {
        get
        {
            int totalPower = 0;
            foreach (MoveDamageEffect effect in _damageEffects)
                totalPower += effect.Power * effect.Hits;
            return totalPower;
        }
    }

    /// <summary>Gets a list of all the status effects that can be applied by this move.</summary>
    public Dictionary<StatusEffectData, int> StatusEffects
    {
        get
        {
            Dictionary<StatusEffectData, int> statusEffects = new Dictionary<StatusEffectData, int>();
            foreach (MoveStatusEffect effect in _moveStatusEffects)
            {
                foreach (StatusEffectData status in effect.StatusEffects)
                {
                    statusEffects.Add(status, effect.Power);
                }
            }
            return statusEffects;
        }
    }

    /// <summary>Gets the total power of all healing effects of this move.</summary>
    public int TotalHealPower
    {
        get
        {
            int totalPower = 0;
            foreach (MoveHealEffect effect in _healEffects)
                totalPower += effect.Power;
            return totalPower;
        }
    }
}

public enum MoveType { MeleePhysical, MeleeEnhanced, RangedPhysical, RangedEnhanced, Spell }

public enum MoveElement { Normal, Poison, Twilight, Abyss }

public enum MoveCategory { Attack, BuffUser, BuffAlly, DebuffEnemy, DebuffUser, DamageUser, DebuffAlly, DamageAlly, BuffEnemy, HealEnemy, HealUser, HealAlly }

public enum MoveTarget { SingleOther, SingleAlly, SingleEnemy, SingleAny, User, UserTeam, EnemyTeam, AnyTeam, OppositeTeam, AllOther, All }

/// <summary>
/// A base class that represents an effect of a move.
/// </summary>
[System.Serializable]
public class MoveEffect
{
    [SerializeField] protected MoveTarget _targets;
    [SerializeField, Range(0, 100)] protected int _accuracy;
    [SerializeField] protected int _power;
    [SerializeField] protected MoveElement _element;

    [SerializeField] protected bool _alwaysHitsSelf;
    [SerializeField] protected bool _alwaysHits;

    public MoveTarget Targets { get => _targets; }
    public int Accuracy { get => _accuracy; }
    public int Power { get => _power; }
    public MoveElement Element { get => _element; }

    public bool AlwaysHitsSelf { get => _alwaysHitsSelf; }
    public bool AlwaysHits { get => _alwaysHits; }
}

/// <summary>
/// A type of move effect that deals damage to its target(s).
/// </summary>
[System.Serializable]
public class MoveDamageEffect : MoveEffect
{
    
    [SerializeField, Min(1), Tooltip("How many times this effect deals its damage.")] private int _hits;

    /// <summary>Gets how many times this effect deals its damage.</summary>
    public int Hits { get => _hits; }
}

/// <summary>
/// A type of move effect that applies status effects.
/// </summary>
[System.Serializable]
public class MoveStatusEffect : MoveEffect
{
    [SerializeField, Min(1)] private int _duration;
    [SerializeField] private List<StatusEffectData> _statusEffects;

    public int Duration { get => _duration; }
    public List<StatusEffectData> StatusEffects { get => _statusEffects; }
}

/// <summary>
/// A type of move effect that heals its target(s).
/// </summary>
[System.Serializable]
public class MoveHealEffect : MoveEffect
{
    [SerializeField] private bool _isPercentageHeal;

    public bool IsPercentageHeal { get => _isPercentageHeal; }
}
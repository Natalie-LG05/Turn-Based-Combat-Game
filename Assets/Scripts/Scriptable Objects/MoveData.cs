using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMoveData", menuName = "ScriptableObjects/Move Data", order = -1000)]
public class MoveData : ScriptableObject
{
    [SerializeField] protected string _name;
    [SerializeField] protected string _id;
    [SerializeField, TextArea] protected string _description;

    [SerializeField] protected MoveType _type;
    [SerializeField] protected List<MoveCategory> _categories;
    [SerializeField] protected MoveElement _element;

    [SerializeField] protected int _power;
    [SerializeField] protected int _accuracy;
    [SerializeField] protected MoveTarget _target;

    [SerializeField] protected List<MoveDamageEffect> _damageEffects;
    [SerializeField] protected List<MoveStatusEffect> _moveStatusEffects;
    [SerializeField] protected List<MoveHealEffect> _healEffects;

    public string Name { get => _name; }
    public string Id { get => _id; }
    public string Description { get => _description; } 

    public MoveType Type { get => _type; }
    public MoveElement Element { get => _element; }
    public List<MoveCategory> Categories { get => _categories; }

    public int Power { get => _power; }
    public int Accuracy { get => _accuracy; }
    public MoveTarget Target { get => _target; }

    public List<MoveDamageEffect> DamageEffects { get => _damageEffects; }
    public List<MoveStatusEffect> MoveStatusEffects { get => _moveStatusEffects; }
    public List<MoveHealEffect> HealEffects { get => _healEffects; }

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

    public List<int> HitsList
    {
        get
        {
            List<int> hits = new List<int>();
            foreach (MoveDamageEffect effect in _damageEffects) hits.Add(effect.Hits);
            return hits;
        }
    }

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

[System.Serializable]
public class MoveEffect
{
    [SerializeField] protected MoveTarget _targets;
    [SerializeField, Range(0, 100)] protected int _accuracy;
    [SerializeField, Tooltip("The base strength of this effect")] protected int _power;
    [SerializeField] protected MoveElement _element;

    [SerializeField] protected bool _alwaysHitsSelf;
    [SerializeField] protected bool _alwaysHits;

    public MoveTarget Targets { get => _targets; }
    public int Accuracy { get => _accuracy; }
    /// <summary>The base strength of this effect.</summary>
    public int Power { get => _power; }
    public MoveElement Element { get => _element; }

    public bool AlwaysHitsSelf { get => _alwaysHitsSelf; }
    public bool AlwaysHits { get => _alwaysHits; }
}

[System.Serializable]
public class MoveDamageEffect : MoveEffect
{
    
    [SerializeField, Min(1), Tooltip("How many times this effect deals its damage.")] private int _hits;

    /// <summary>How many times this effect deals its damage.</summary>
    public int Hits { get => _hits; }
}

[System.Serializable]
public class MoveStatusEffect : MoveEffect
{
    [SerializeField, Min(1)] private int _duration;
    [SerializeField] private List<StatusEffectData> _statusEffects;

    public int Duration { get => _duration; }
    public List<StatusEffectData> StatusEffects { get => _statusEffects; }
}

[System.Serializable]
public class MoveHealEffect : MoveEffect
{
    [SerializeField] private bool _isPercentageHeal;

    public bool IsPercentageHeal { get => _isPercentageHeal; }
}
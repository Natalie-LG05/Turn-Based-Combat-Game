using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMoveData", menuName = "ScriptableObjects/Move Data", order = -1000)]
public class MoveData : ScriptableObject
{
    [SerializeField] protected string _name;
    [SerializeField] protected string _id;
    [SerializeField, TextArea] protected string _description;

    [SerializeField] protected MoveType _type;
    [SerializeField] protected MoveElement _element;
    [SerializeField] protected List<MoveCategory> _categories;

    [SerializeField] protected List<MoveDamageEffect> _damageEffects;
    [SerializeField] protected List<MoveStatusEffect> _statusEffects;

    public string Name { get => _name; }
    public string Id { get => _id; }
    public string Description { get => _description; } 

    public MoveType Type { get => _type; }
    public MoveElement Element { get => _element; }
    public List<MoveCategory> Categories { get => _categories; }

    public List<MoveDamageEffect> DamageEffects { get => _damageEffects; }
    public List<MoveStatusEffect> StatusEffects { get => _statusEffects; }

    public List<MoveTarget> Targets
    {
        get
        {
            List<MoveTarget> targets = new List<MoveTarget>();
            foreach (MoveDamageEffect effect in _damageEffects) targets.Add(effect.Targets);
            foreach (MoveStatusEffect effect in _statusEffects) targets.Add(effect.Targets);
            return targets;
        }
    }
}

public enum MoveType { MeleePhysical, MeleeEnhanced, RangedPhysical, RangedEnhanced, Spell }

public enum MoveElement { Normal, Poison, Twilight, Abyss }

public enum MoveCategory { Attack, Buff, Debuff }

public enum MoveTarget { SingleOther, SingleEnemy, User, SingleAlly, EnemyTeam, UserTeam, AllOther, All }

[System.Serializable]
public class MoveEffect
{
    [SerializeField] protected MoveTarget _targets;
    [SerializeField, Range(0, 100)] protected int _accuracy;
    [SerializeField, Tooltip("The base strength of this effect")] protected int _power;
    [SerializeField] protected MoveElement _element;

    public MoveTarget Targets { get => _targets; }
    public int Accuracy { get => _accuracy; }
    /// <summary>The base strength of this effect.</summary>
    public int Power { get => _power; }
}

[System.Serializable]
public class MoveDamageEffect : MoveEffect
{
    /// <summary>How many times this effect deals its damage.</summary>
    [SerializeField, Min(1)] private int _hits;
}

[System.Serializable]
public class MoveStatusEffect : MoveEffect
{
    [SerializeField, Min(1)] private int _duration;
    [SerializeField] private StatusEffectData[] statusEffects;
}
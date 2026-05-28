using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for a battle item.
/// </summary>
[CreateAssetMenu(fileName = "NewBattleItemData", menuName = "ScriptableObjects/Battle Item Data", order = -1000)]
public class BattleItemData : ItemData
{
    [SerializeField] private List<BattleItemHealEffect> _healEffects;
    [SerializeField] private List<BattleItemStatusEffect> _itemStatusEffects;
    [SerializeField] private List<BattleItemReviveEffect> _reviveEffects;
    [SerializeField] private List<BattleItemCustomEffect> _customEffects;

    public List<BattleItemHealEffect> HealEffects { get => _healEffects; }
    public List<BattleItemStatusEffect> ItemStatusEffects { get => _itemStatusEffects; }
    public List<BattleItemReviveEffect> ReviveEffects { get => _reviveEffects; }
    public List<BattleItemCustomEffect> CustomEffects { get => _customEffects; }
}

/// <summary>
/// A class that contains the data for a battle item effect.
/// </summary>
[System.Serializable]
public class BattleItemEffect
{
    protected BattleTargettingType _targets;
    protected bool _doesUseUpTurn;

    public BattleTargettingType Targets { get => _targets; }
    public bool DoesUseUpTurn { get => _doesUseUpTurn; }
}

/// <summary>
/// A type of battle item effect that heals the target(s).
/// </summary>
[System.Serializable]
public class BattleItemHealEffect : BattleItemEffect
{
    [SerializeField] private int _healAmount;
    [SerializeField] private bool _isPercentageHeal;

    public int HealAmount { get => _healAmount; }
    public bool IsPercentageHeal { get => _isPercentageHeal; }
}

/// <summary>
/// A type of battle item effect that applies one or more status effects to the target(s).
/// </summary>
[System.Serializable]
public class BattleItemStatusEffect : BattleItemEffect
{
    [SerializeField, Min(1)] private int _duration;
    [SerializeField] private List<StatusEffectData> _statusEffects;

    public int Duration { get => _duration; }
    public List<StatusEffectData> StatusEffect { get => _statusEffects; }
}

/// <summary>
/// A type of battle item effect that revives one or more party members.
/// </summary>
[System.Serializable]
public class BattleItemReviveEffect : BattleItemEffect
{
    [SerializeField, Min(1), Tooltip("The amount of health for the target character(s) to revive with.")] private int _healAmount;
    [SerializeField] private bool _isPercentageHeal;

    /// <summary>Gets the amount of health for the target character(s) to revive with.</summary>
    public int HealAmount { get => _healAmount; }
    public bool IsPercentageHeal { get => _isPercentageHeal; }
}

/// <summary>
/// A class used to provide data for additional battle item effects to custom effects of items that are defined in EffectsDB.
/// </summary>
[System.Serializable]
public class BattleItemCustomEffect
{
    [SerializeField] private List<MoveDamageEffect> _healEffects;
    [SerializeField] private List<MoveStatusEffect> _statusEffects;
    [SerializeField] private List<MoveHealEffect> _reviveEffects;

    public List<MoveDamageEffect> HealEffects { get => _healEffects; }
    public List<MoveStatusEffect> StatusEffects { get => _statusEffects; }
    public List<MoveHealEffect> ReviveEffects { get => _reviveEffects; }
}
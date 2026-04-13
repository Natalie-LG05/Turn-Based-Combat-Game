using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CharacterInstance
{
    protected CharacterData _characterData;
    protected CharacterUI _characterUI;

    protected static int instanceCount;

    [SerializeField, Min(1)] protected int _level;

    protected bool _isPlayerTeam;

    protected int totalStatPoints;
    protected int additionalStatPoints;

    protected int _currentHP;

    public CharacterData CharacterData { get => _characterData; }
    public CharacterUI CharacterUI { get => _characterUI; set => _characterUI = value; }

    public int Level { get => _level; }

    public bool IsPlayerTeam { get => _isPlayerTeam; set => _isPlayerTeam = value; }

    public int CurrentHP { get => _currentHP; }

    public float SpeedTieBreaker { get => Random.value; }

    public int uniqueCharacterId { get; private set; }

    public List<StatusEffectInstance> StatusEffects { get; private set; }

    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, List<StatModifier>> StatModifiers { get; private set; }
    public Dictionary<Stat, int> AdditionalStats { get; private set; }

    public int MaxHP { get { return GetStat(Stat.MaxHP); } }
    public int Attack { get { return GetStat(Stat.Attack); } }
    public int Support { get { return GetStat(Stat.Support); } }
    public int Defense { get { return GetStat(Stat.Defense); } }
    public int Speed { get { return GetStat(Stat.Speed); } }

    public List<MoveData> Moveset { get; private set; }
    public List<AbilityData> Abilities { get; private set; }

    public virtual void Init()
    {
        InitializeAdditionalStats();

        CalculateTotalStatPoints();
        CalculateStartingStats();

        ResetStatModifiers();
        ResetStatusEffects();

        _currentHP = MaxHP;  // start at max health

        DetermineMoveset();
        DetermineAbilities();

        uniqueCharacterId = instanceCount++;
    }

    public void BattleStart()
    {
        SetHP(MaxHP);  // start battle at max health

        // trigger start of combat effects of abilities on this character
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnBattleStart?.Invoke(this, ability);
    }

    public void RoundStart()
    {
        // trigger start of round effects of status effects and abilities on this character
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnRoundStart?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnRoundStart?.Invoke(this, null, ability);
    }

    public void RoundEnd()
    {
        // trigger end of round effects of status effects and abilities on this character
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnRoundEnd?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnRoundEnd?.Invoke(this, null, ability);
    }

    public void TurnStart()
    {
        // trigger start of turn effects of status effects on this character
        foreach (StatusEffectInstance status in StatusEffects)
        {
            status.TurnStart();  // signal to each status effect that this character has begun a new turn
            status.Effects.OnTurnStart?.Invoke(this, status, null);
        }
        // trigger start of turn effects of abilities this character has
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnTurnStart?.Invoke(this, null, ability);
    }

    public void TurnEnd()
    {
        // trigger end of turn effects of status effects and abilities on this character
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnTurnEnd?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnTurnEnd?.Invoke(this, null, ability);

        // Signal to each status effect on this character that their turn has ended, possibly decreasing their duration and/or expiring them
        List<StatusEffectInstance> expiredEffects = new List<StatusEffectInstance>();
        foreach (StatusEffectInstance effect in StatusEffects)
            if (effect.TurnEnd()) expiredEffects.Add(effect);
        foreach (StatusEffectInstance effect in expiredEffects)  
            RemoveStatusEffect(effect, true); 

        _characterUI.SetEffects(StatusEffects);  // update UI
    }

    public void TakeAttackDamage(CharacterInstance user, MoveData move, MoveDamageEffect effect)
    {
        // calculate the basic damage based on the move effect power, user's level,
        // and ratio of the user's attack to the defender's defense (minimum of 1 damage)
        int damage = Mathf.RoundToInt((effect.Power / 7.5f) * (((user.Level - 1) / 10f) + 1) * ((float)user.Attack / Defense)) + 1;

        // triger effects of status effects and abilities on this character that happen before taking attack damage
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnBeforeAttackDamage?.Invoke(this, status, null, user);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnBeforeAttackDamage?.Invoke(this, null, ability, user);

        TakeDamage(damage);  // actually take the calculated damage

        // triger effects of status effects and abilities on this character that happen after taking attack damage
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnAfterAttackDamage?.Invoke(this, status, null, user);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnAfterAttackDamage?.Invoke(this, null, ability, user);
    }

    public void ApplyMoveStatusEffect(CharacterInstance user, MoveData move, MoveStatusEffect effect)
    {
        // calculate the power and duration of status effects applied by this effect
        float power = effect.Power * (1 + (user.Support / 100f));
        int duration = Mathf.FloorToInt(effect.Duration * (1 + (user.Support / 100f)));

        // attempt to apply each status effect
        foreach (StatusEffectData status in effect.StatusEffects)
        {
            bool wasApplied = ApplyStatusEffect(new StatusEffectInstance(status, duration, power, user, this), true);

            if (wasApplied)
            {
                // if the status was applied, show dialogue that depends on if it was a debuff or not
                string message = status.Type == StatusEffectType.Debuff ? $"{CharacterData.Name} (lvl {Level}) was inflicted with {status.Name}." : $"{CharacterData.Name} (lvl {Level}) gained {status.Name}.";
                BattleManager.Instance.QueueMessage(message);
            }
        }
    }

    public void ApplyMoveHeal(CharacterInstance user, MoveData move, MoveHealEffect effect)
    {
        // calculate the heal power based on the move effect power and user's support stat
        float power = effect.Power * (1 + (user.Support / 100f));

        // the calculated power is either the flat amount to heal, or percentage to heal depending on if the effect is a percentage heal or not
        int healAmount = effect.IsPercentageHeal ? Mathf.RoundToInt((power / 100f) * MaxHP) : Mathf.RoundToInt(power);
        Heal(healAmount);
    }

    public bool ApplyStatusEffect(StatusEffectInstance effect, bool procEffects)
    {
        StatusEffectInstance oldEffect = StatusEffects.Find(status => status.StatusEffectData.Id == effect.StatusEffectData.Id);
        if (oldEffect != null)
        {
            if (oldEffect.BuffPower < effect.BuffPower)
            {
                // if this character already has this status effect, but the old one is weaker, replace it with the new one
                RemoveStatusEffect(oldEffect, false);

                AddStatusEffect(effect, procEffects);
                return true;
            }
            else if (oldEffect.BuffPower == effect.BuffPower && oldEffect.Duration <= effect.Duration)
            {
                // if this character already has this status effect of the same power,
                // but the old one has less or equal duration, replace it with the new one
                RemoveStatusEffect(oldEffect, false);

                AddStatusEffect(effect, procEffects);
                return true;
            }
            else return false;
        } else
        {
            // this character doesn't have this status already, so add it
            AddStatusEffect(effect, procEffects);
            return true;
        }
    }

    protected void AddStatusEffect(StatusEffectInstance effect, bool procEffects)
    {
        StatusEffects.Add(effect);
        effect.Effects.OnApply?.Invoke(this, effect);  // trigger on apply effects of this status

        if (procEffects)
        {
            // triger effects of status effects and abilities on this character that happen when a status effect is gained
            foreach (StatusEffectInstance status in StatusEffects)
                status.Effects.OnStatusGained?.Invoke(this, status, null, effect);
            foreach (AbilityData ability in Abilities)
                ability.Effects.OnStatusGained?.Invoke(this, null, ability, effect);
        }

        if (CharacterUI != null) CharacterUI.SetEffects(StatusEffects);  // update UI
    }

    public void RemoveStatusEffect(StatusEffectInstance effect, bool procEffects)
    {
        // on expire effects of status effects and abilities are not triggered in
        // special cases such as encounter modifiers being added or abilities adding their statuses
        if (procEffects) effect.Effects.OnExpire?.Invoke(this, effect);

        effect.Effects.OnRemove?.Invoke(this, effect);  // trigger any effects of this status effect that trigger when it is removed
        StatusEffects.Remove(effect);

        if (procEffects)
        {
            // trigger effects of status effects and abilities on this character that happen when a status effect is removed
            foreach (StatusEffectInstance status in StatusEffects)
                status.Effects.OnStatusRemoved?.Invoke(this, status, null, effect);
            foreach (AbilityData ability in Abilities)
                ability.Effects.OnStatusRemoved?.Invoke(this, null, ability, effect);
        }

        if (CharacterUI != null) CharacterUI.SetEffects(StatusEffects);  // update UI
    }

    public void RemoveStatusEffect(string effectId, bool procEffects)
    {
        if (StatusEffects.Count > 0 && StatusEffects.Exists(status => status.StatusEffectData.Id == effectId))
        {
            // if this character has this status effect, remove it
            StatusEffectInstance effect = StatusEffects.Where(status => status.StatusEffectData.Id == effectId).First();
            if (effect != null) RemoveStatusEffect(effect, procEffects);
        }
    }

    public void AddModifier(Stat stat, float amount, string sourceId)
    {
        StatModifiers[stat].Add(new StatModifier(amount, sourceId));
    }

    public void RemoveModifier(Stat stat, string sourceId)
    {
        StatModifiers[stat].RemoveAll(mod => mod.SourceId == sourceId);
    }

    public void TakeDamage(int damage)
    {
        // trigger effects of status effects and abilities on this character that happen before taking any damage
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnBeforeDamage?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnBeforeDamage?.Invoke(this, null, ability);

        _currentHP = Mathf.Max(0, _currentHP - damage);  // keep health from going below zero

        // trigger effects of status effects and abilities on this character that happen after taking any damage or when hp is changed
        foreach (StatusEffectInstance status in StatusEffects)
        {
            status.Effects.OnAfterDamage?.Invoke(this, status, null);
            status.Effects.OnAfterHPChanged?.Invoke(this, status, null);
        }
        foreach (AbilityData ability in Abilities)
        {
            ability.Effects.OnAfterDamage?.Invoke(this, null, ability);
            ability.Effects.OnAfterHPChanged?.Invoke(this, null, ability);
        }
    }

    public void Heal(int healAmount)
    {
        _currentHP = Mathf.Min(_currentHP + healAmount, MaxHP);  // keep health from exceding MaxHP

        // trigger effects of status effects and abilities on this character that happen when their character's hp is changed
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnAfterHPChanged?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnAfterHPChanged?.Invoke(this, null, ability);
    }

    public bool HasAllMoveStatusEffects(MoveData move)
    {
        foreach (StatusEffectData status in move.StatusEffects.Keys)
        {
            if (!HasStatusEffect(status.Id)) return false;
        }
        return true;
    }

    public bool HasStatusEffect(string statusId)
    {
        return StatusEffects.Exists(status => status.StatusEffectData.Id == statusId);
    }

    // Helper methods for use inside this class
    protected void SetHP(int hp)
    {
        _currentHP = Mathf.Clamp(hp, 0, MaxHP);  // keep hp within range
        CharacterUI.UpdateHealth();  // update UI
    }

    protected void InitializeAdditionalStats()
    {
        AdditionalStats = new Dictionary<Stat, int>()
        {
            {Stat.MaxHP, 0},
            {Stat.Attack, 0},
            {Stat.Support, 0},
            {Stat.Defense, 0},
            {Stat.Speed, 0},
        };
    }

    protected void CalculateStartingStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.MaxHP, Mathf.RoundToInt(totalStatPoints * (_characterData.StatSpread.MaxHP / 100.0f)));
        Stats.Add(Stat.Attack, Mathf.RoundToInt(totalStatPoints * (_characterData.StatSpread.Attack / 100.0f)));
        Stats.Add(Stat.Support, Mathf.RoundToInt(totalStatPoints * (_characterData.StatSpread.Support / 100.0f)));
        Stats.Add(Stat.Defense, Mathf.RoundToInt(totalStatPoints * (_characterData.StatSpread.Defense / 100.0f)));
        Stats.Add(Stat.Speed, Mathf.RoundToInt(totalStatPoints * (_characterData.StatSpread.Speed / 100.0f)));
    }

    protected void ResetStatModifiers()
    {
        StatModifiers = new Dictionary<Stat, List<StatModifier>>()
        {
            {Stat.MaxHP, new List<StatModifier>()},
            {Stat.Attack, new List<StatModifier>()},
            {Stat.Support, new List<StatModifier>()},
            {Stat.Defense, new List<StatModifier>()},
            {Stat.Speed, new List<StatModifier>()},
            {Stat.DamageDealt, new List<StatModifier>()},
            {Stat.DamageTaken, new List<StatModifier>()},
            {Stat.Accuracy, new List<StatModifier>()},
            {Stat.Evasion, new List<StatModifier>()},
        };
    }

    protected void ResetStatusEffects()
    {
        StatusEffects = new List<StatusEffectInstance>();
    }

    protected void CalculateTotalStatPoints()
    {
        totalStatPoints = _characterData.BaseStatPoints + (_characterData.LevelupStatPoints * (_level - 1));
    }

    protected int GetStat(Stat stat)
    {
        int value = Stats[stat];

        // Multiply the base value by each active stat modifier for that stat (if there are any)
        if (StatModifiers[stat].Count > 0)
        {
            List<float> modifierVals = StatModifiers[stat].Select(mod => mod.Power).ToList();
            value = Mathf.RoundToInt(value * modifierVals.Aggregate(1f, (acc, next) => acc * next));
        }
        return value;
    }

    protected void DetermineMoveset()
    {
        Moveset = new List<MoveData>();
        foreach (MoveLevelPair mlp in _characterData.MoveLearnset)
        {
            if (mlp.Level <= _level)
            {
                Moveset.Add(mlp.MoveData);
            }
        }
    }

    protected void DetermineAbilities()
    {
        Abilities = new List<AbilityData>();
        foreach (AbilityLevelPair mlp in _characterData.AbilityLearnset)
        {
            if (mlp.Level <= _level)
            {
                Abilities.Add(mlp.AbilityData);
            }
        }
    }
}

public enum Stat { MaxHP, Attack, Support, Defense, Speed, DamageDealt, DamageTaken, Accuracy, Evasion }

[System.Serializable]
public class StatModifier
{
    public float Power { get; private set; }
    public string SourceId { get; private set; }

    public StatModifier(float power, string sourceId)
    {
        Power = power;
        SourceId = sourceId;
    }
}
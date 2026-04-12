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

        _currentHP = MaxHP;

        DetermineMoveset();
        DetermineAbilities();

        uniqueCharacterId = instanceCount++;
    }

    public void BattleStart()
    {
        SetHP(MaxHP);

        foreach (AbilityData ability in Abilities)
            ability.Effects.OnBattleStart?.Invoke(this, ability);
    }

    public void RoundStart()
    {
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnRoundStart?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnRoundStart?.Invoke(this, null, ability);
    }

    public void RoundEnd()
    {
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnRoundEnd?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnRoundEnd?.Invoke(this, null, ability);
    }

    public void TurnStart()
    {
        foreach (StatusEffectInstance status in StatusEffects)
        {
            status.TurnStart();
            status.Effects.OnTurnStart?.Invoke(this, status, null);
        }
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnTurnStart?.Invoke(this, null, ability);
    }

    public void TurnEnd()
    {
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnTurnEnd?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnTurnEnd?.Invoke(this, null, ability);

        // Decrease duration of status effects and update the UI accordingly
        List<StatusEffectInstance> expiredEffects = new List<StatusEffectInstance>();
        foreach (StatusEffectInstance effect in StatusEffects)
        {
            if (effect.TurnEnd()) expiredEffects.Add(effect);
        }
        foreach (StatusEffectInstance effect in expiredEffects)
            RemoveStatusEffect(effect, true);

        _characterUI.SetEffects(StatusEffects);
    }

    public void TakeAttackDamage(CharacterInstance user, MoveData move, MoveDamageEffect effect)
    {
        int damage = Mathf.RoundToInt((effect.Power / 7.5f) * (((user.Level - 1) / 10f) + 1) * ((float)user.Attack / Defense)) + 1;

        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnBeforeAttackDamage?.Invoke(this, status, null, user);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnBeforeAttackDamage?.Invoke(this, null, ability, user);

        TakeDamage(damage);

        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnAfterAttackDamage?.Invoke(this, status, null, user);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnAfterAttackDamage?.Invoke(this, null, ability, user);
    }

    public void ApplyMoveStatusEffect(CharacterInstance user, MoveData move, MoveStatusEffect effect)
    {
        float power = effect.Power * (1 + (user.Support / 100f));
        int duration = Mathf.FloorToInt(effect.Duration * (1 + (user.Support / 100f)));

        foreach (StatusEffectData status in effect.StatusEffects)
        {
            ApplyStatusEffect(new StatusEffectInstance(status, duration, power, user, this), true);

            string message = status.Type == StatusEffectType.Debuff ? $"{CharacterData.Name} (lvl {Level}) was inflicted with {status.Name}." : $"{CharacterData.Name} (lvl {Level}) gained {status.Name}.";
            BattleManager.Instance.QueueMessage(message);
        }
    }

    public void ApplyStatusEffect(StatusEffectInstance effect, bool procEffects)
    {
        StatusEffectInstance oldEffect = StatusEffects.Find(status => status.StatusEffectData.Id == effect.StatusEffectData.Id);
        if (oldEffect != null)
        {
            if (oldEffect.BuffPower < effect.BuffPower)
            {
                // If this character already has this status effect, but the old one is weaker, replace it with the new one
                RemoveStatusEffect(oldEffect, false);

                AddStatusEffect(effect, procEffects);
            } else if (oldEffect.BuffPower == effect.BuffPower && oldEffect.Duration < effect.Duration)
            {
                // If this character already has this status effect of the same power, but the old one has less duration, replace it with the new one
                RemoveStatusEffect(oldEffect, false);

                AddStatusEffect(effect, procEffects);
            }
        } else
        {
            AddStatusEffect(effect, procEffects);
        }
    }

    protected void AddStatusEffect(StatusEffectInstance effect, bool procEffects)
    {
        StatusEffects.Add(effect);
        effect.Effects.OnApply?.Invoke(this, effect);

        if (procEffects)
        {
            foreach (StatusEffectInstance status in StatusEffects)
                status.Effects.OnStatusGained?.Invoke(this, status, null, effect);
            foreach (AbilityData ability in Abilities)
                ability.Effects.OnStatusGained?.Invoke(this, null, ability, effect);
        }

        if (CharacterUI != null) CharacterUI.SetEffects(StatusEffects);
    }

    public void RemoveStatusEffect(StatusEffectInstance effect, bool procEffects)
    {
        if (procEffects) effect.Effects.OnExpire?.Invoke(this, effect);
        effect.Effects.OnRemove?.Invoke(this, effect);
        StatusEffects.Remove(effect);

        if (procEffects)
        {
            foreach (StatusEffectInstance status in StatusEffects)
                status.Effects.OnStatusRemoved?.Invoke(this, status, null, effect);
            foreach (AbilityData ability in Abilities)
                ability.Effects.OnStatusRemoved?.Invoke(this, null, ability, effect);
        }
    }

    public void RemoveStatusEffect(string effectId, bool procEffects)
    {
        if (StatusEffects.Count > 0 && StatusEffects.Exists(status => status.StatusEffectData.Id == effectId))
        {
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

    protected void SetHP(int hp)
    {
        _currentHP = hp >= 0 ? hp : 0;
        CharacterUI.UpdateHealth();
    }

    public void TakeDamage(int damage)
    {
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnBeforeDamage?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnBeforeDamage?.Invoke(this, null, ability);

        int newHP = _currentHP - damage;
        _currentHP = newHP >= 0 ? newHP : 0;

        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects.OnAfterDamage?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects.OnAfterDamage?.Invoke(this, null, ability);
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
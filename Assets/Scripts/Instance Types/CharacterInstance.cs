using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents an instance of a character, which is based off of an EnemyData or PartyMemberData.
/// </summary>
[System.Serializable]
public class CharacterInstance
{
    [SerializeField, HideInInspector] protected CharacterData _characterData;
    protected BattleCharacterUI _characterUI;

    [SerializeField, Min(1)] protected int _level;

    [SerializeField, HideInInspector] protected bool _isPlayerTeam;

    /// <summary>The base amount of stat points this character has at its current level (not to be confused with CharacterData.BaseStatPoints).</summary>
    [SerializeField, HideInInspector] protected int baseStatPoints;
    /// <summary>The amount of additional stat points this character has.</summary>
    [SerializeField, HideInInspector] protected int additionalStatPoints;

    protected int _currentHP;

    [SerializeField, HideInInspector] protected string _uniqueCharacterId;

    protected Dictionary<Stat, int> _baseStats;
    protected Dictionary<Stat, int> _additionalStats;

    protected List<MoveData> _moveset;
    protected List<AbilityData> _abilities;

    /// <summary>Gets the character data this character instance is based off of.</summary>
    public CharacterData CharacterData { get => _characterData; }
    /// <summary>Gets or sets the characterUI assigned to display info about this character.</summary>
    public BattleCharacterUI CharacterUI { get => _characterUI; set => _characterUI = value; }

    /// <summary>Gets the character's level.</summary>
    public int Level { get => _level; }

    /// <summary>Gets or sets whether or not the character is on the player's team.</summary>
    public bool IsPlayerTeam { get => _isPlayerTeam; set => _isPlayerTeam = value; }

    /// <summary>Gets the current HP of the character.</summary>
    public int CurrentHP { get => _currentHP; }

    /// <summary>Gets a random value. Used to break speed ties when determining turn order.</summary>
    public float SpeedTieBreaker { get => Random.value; }

    /// <summary>Gets the character's unique id.</summary>
    public string UniqueCharacterId { get => _uniqueCharacterId; }

    /// <summary>Gets the status effects that this character currently has on it.</summary>
    public List<StatusEffectInstance> StatusEffects { get; private set; }

    /// <summary>Gets a dictionary containing the current base stats of the character at its current level.</summary>
    public Dictionary<Stat, int> BaseStats { get => _baseStats; }
    /// <summary>Gets a dictionary containing the current additional stats of the character.</summary>
    public Dictionary<Stat, int> AdditionalStats { get => _additionalStats; }
    /// <summary>Gets a dictionary containing the temporary mid-combat stat modifiers the character has.</summary>
    public Dictionary<Stat, List<StatModifier>> StatModifiers { get; private set; }

    /// <summary>Gets the current MaxHP stat of the character, taking into account modifiers.</summary>
    public int MaxHP { get { return GetStat(Stat.MaxHP); } }
    /// <summary>Gets the current Attack stat of the character, taking into account modifiers.</summary>
    public int Attack { get { return GetStat(Stat.Attack); } }
    /// <summary>Gets the current Support stat of the character, taking into account modifiers.</summary>
    public int Support { get { return GetStat(Stat.Support); } }
    /// <summary>Gets the current Defense stat of the character, taking into account modifiers.</summary>
    public int Defense { get { return GetStat(Stat.Defense); } }
    /// <summary>Gets the current Speed stat of the character, taking into account modifiers.</summary>
    public int Speed { get { return GetStat(Stat.Speed); } }

    /// <summary>Gets the character's moveset, which is the list of moves it has available to use.</summary>
    public List<MoveData> Moveset { get => _moveset; }
    /// <summary>Gets a list of the abilities the character currently has.</summary>
    public List<AbilityData> Abilities { get => _abilities; }

    /// <summary>
    /// Initialize this character.
    /// </summary>
    public virtual void Init()
    {
        _uniqueCharacterId = System.Guid.NewGuid().ToString();

        InitializeAdditionalStats();

        CalculateBaseStatPoints();
        CalculateBaseStats();

        ResetStatusEffects();
        ResetStatModifiers();

        _currentHP = MaxHP;  // start at max health

        DetermineMoveset();
        DetermineAbilities();
    }

    /// <summary>
    /// Signal to this character that a battle has started.
    /// </summary>
    public void BattleStart()
    {
        SetHP(MaxHP);  // start battle at max health

        // trigger start of combat effects of abilities on this character
        foreach (AbilityData ability in Abilities)
            ability.Effects?.OnBattleStart?.Invoke(this, ability);
    }

    /// <summary>
    /// Signal to this character that a new round has started.
    /// </summary>
    public void RoundStart()
    {
        // trigger start of round effects of status effects and abilities on this character
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects?.OnRoundStart?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects?.OnRoundStart?.Invoke(this, null, ability);
    }

    /// <summary>
    /// Signal to this character that a round has ended.
    /// </summary>
    public void RoundEnd()
    {
        // trigger end of round effects of status effects and abilities on this character
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects?.OnRoundEnd?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects?.OnRoundEnd?.Invoke(this, null, ability);
    }

    /// <summary>
    /// Signal to this character that its turn has started.
    /// </summary>
    public void TurnStart()
    {
        // trigger start of turn effects of status effects on this character
        foreach (StatusEffectInstance status in StatusEffects)
        {
            status.TurnStart();  // signal to each status effect that this character has begun a new turn
            status.Effects?.OnTurnStart?.Invoke(this, status, null);
        }
        // trigger start of turn effects of abilities this character has
        foreach (AbilityData ability in Abilities)
            ability.Effects?.OnTurnStart?.Invoke(this, null, ability);
    }

    /// <summary>
    /// Signal to this character that its turn has ended.
    /// </summary>
    public void TurnEnd()
    {
        // trigger end of turn effects of status effects and abilities on this character
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects?.OnTurnEnd?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects?.OnTurnEnd?.Invoke(this, null, ability);

        // Signal to each status effect on this character that their turn has ended, possibly decreasing their duration and/or expiring them
        List<StatusEffectInstance> expiredEffects = new List<StatusEffectInstance>();
        foreach (StatusEffectInstance effect in StatusEffects)
            if (effect.TurnEnd()) expiredEffects.Add(effect);
        foreach (StatusEffectInstance effect in expiredEffects)  
            RemoveStatusEffect(effect, true); 

        _characterUI.SetEffects(StatusEffects);  // update UI
    }

    /// <summary>
    /// Signal to this character that a battle has started, resetting it back to its normal state.
    /// </summary>
    public void BattleEnd()
    {
        ResetStatusEffects();
        ResetStatModifiers();

        _currentHP = MaxHP;
    }

    /// <summary>
    /// Handle being targeted by a damaging effect of a move.
    /// </summary>
    /// <param name="user">The character that used the move.</param>
    /// <param name="move">The move that was used.</param>
    /// <param name="effect">The move effect causing the damage.</param>
    public void TakeAttackDamage(CharacterInstance user, MoveData move, MoveDamageEffect effect)
    {
        // calculate the basic damage based on the move effect power, user's level,
        // and ratio of the user's attack to the defender's defense (minimum of 1 damage)
        int damage = Mathf.FloorToInt((effect.Power / 7.5f) * (((user.Level - 1) / 10f) + 1) * ((float)user.Attack / Defense) + 0.5f) + 1;

        // triger effects of status effects and abilities on this character that happen before taking attack damage
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects?.OnBeforeAttackDamage?.Invoke(this, status, null, user);
        foreach (AbilityData ability in Abilities)
            ability.Effects?.OnBeforeAttackDamage?.Invoke(this, null, ability, user);

        TakeDamage(damage);  // actually take the calculated damage

        // triger effects of status effects and abilities on this character that happen after taking attack damage
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects?.OnAfterAttackDamage?.Invoke(this, status, null, user);
        foreach (AbilityData ability in Abilities)
            ability.Effects?.OnAfterAttackDamage?.Invoke(this, null, ability, user);
    }

    /// <summary>
    /// Handle being targeted by a status effect of a move.
    /// </summary>
    /// <param name="user">The character that used the move.</param>
    /// <param name="move">The move that was used.</param>
    /// <param name="effect">The move effect being applied.</param>
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

    /// <summary>
    /// Handle being targeted by a heal effect of a move.
    /// </summary>
    /// <param name="user">The character that used the move.</param>
    /// <param name="move">The move that was used.</param>
    /// <param name="effect">The move effect causing the healing.</param>
    public void ApplyMoveHeal(CharacterInstance user, MoveData move, MoveHealEffect effect)
    {
        // calculate the heal power based on the move effect power and user's support stat
        float power = effect.Power * (1 + (user.Support / 100f));

        // the calculated power is either the flat amount to heal, or percentage to heal depending on if the effect is a percentage heal or not
        int healAmount = effect.IsPercentageHeal ? Mathf.FloorToInt((power / 100f) * MaxHP + 0.5f) : Mathf.FloorToInt(power + 0.5f);
        Heal(healAmount);
    }

    /// <summary>
    /// Attempt to apply a status effect to this character. <br/>If this character already has a version of that status effect
    /// with higher power or duration, it won't be applied.
    /// </summary>
    /// <param name="effect">The status effect to apply.</param>
    /// <param name="procEffects">Whether or not to trigger effects of abilities and other status effects this character has.</param>
    /// <returns>Whether or not the status effect was applied.</returns>
    public bool ApplyStatusEffect(StatusEffectInstance effect, bool procEffects)
    {
        StatusEffectInstance oldEffect = StatusEffects.Find(status => status.StatusEffectData.Id == effect.StatusEffectData.Id);
        if (oldEffect != null)
        {
            if (oldEffect.Power < effect.Power)
            {
                // if this character already has this status effect, but the old one is weaker, replace it with the new one
                RemoveStatusEffect(oldEffect, false);

                AddStatusEffect(effect, procEffects);
                return true;
            }
            else if (oldEffect.Power == effect.Power && oldEffect.Duration <= effect.Duration)
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

    /// <summary>
    /// Add a status effect to this character.
    /// </summary>
    /// <param name="effect">The status effect to add.</param>
    /// <param name="procEffects">Whether or not to trigger effects of abilities and other status effects this character has.</param>
    protected void AddStatusEffect(StatusEffectInstance effect, bool procEffects)
    {
        StatusEffects.Add(effect);
        effect.OnApply();
        effect.Effects?.OnApply?.Invoke(this, effect);  // trigger on apply effects of this status

        if (procEffects)
        {
            // triger effects of status effects and abilities on this character that happen when a status effect is gained
            foreach (StatusEffectInstance status in StatusEffects)
                status.Effects?.OnStatusGained?.Invoke(this, status, null, effect);
            foreach (AbilityData ability in Abilities)
                ability.Effects?.OnStatusGained?.Invoke(this, null, ability, effect);
        }

        if (CharacterUI != null) CharacterUI.SetEffects(StatusEffects);  // update UI
    }

    /// <summary>
    /// Remove a status effect from this character.
    /// </summary>
    /// <param name="effect">The status effect to remove.</param>
    /// <param name="procEffects">Whether or not to trigger effects of abilities and other status effects this character has.</param>
    public void RemoveStatusEffect(StatusEffectInstance effect, bool procEffects)
    {
        // on expire effects of status effects and abilities are not triggered in
        // special cases such as encounter modifiers being added or abilities adding their statuses
        if (procEffects) effect.Effects?.OnExpire?.Invoke(this, effect);

        effect.OnRemove();
        effect.Effects?.OnRemove?.Invoke(this, effect);  // trigger any effects of this status effect that trigger when it is removed
        StatusEffects?.Remove(effect);

        if (procEffects)
        {
            // trigger effects of status effects and abilities on this character that happen when a status effect is removed
            foreach (StatusEffectInstance status in StatusEffects)
                status.Effects?.OnStatusRemoved?.Invoke(this, status, null, effect);
            foreach (AbilityData ability in Abilities)
                ability.Effects?.OnStatusRemoved?.Invoke(this, null, ability, effect);
        }

        if (CharacterUI != null) CharacterUI.SetEffects(StatusEffects);  // update UI
    }

    /// <summary>
    /// Remove a status effect from this character.
    /// </summary>
    /// <param name="effectId">The id of the status effect to remove.</param>
    /// <param name="procEffects">Whether or not to trigger effects of abilities and other status effects this character has.</param>
    public void RemoveStatusEffect(string effectId, bool procEffects)
    {
        if (StatusEffects.Count > 0 && StatusEffects.Exists(status => status.StatusEffectData.Id == effectId))
        {
            // if this character has this status effect, remove it
            StatusEffectInstance effect = StatusEffects.Where(status => status.StatusEffectData.Id == effectId).FirstOrDefault();
            if (effect != null) RemoveStatusEffect(effect, procEffects);
        }
    }

    /// <summary>
    /// Add a stat modifier to this character.
    /// </summary>
    /// <param name="stat">The stat to add a modifier for.</param>
    /// <param name="amount">The value of the modifier (float between 0 and 1).</param>
    /// <param name="sourceId">The id of the source of this modifier.</param>
    public void AddModifier(Stat stat, float amount, string sourceId)
    {
        StatModifiers[stat].Add(new StatModifier(amount, sourceId));
    }

    /// <summary>
    /// Remove a stat modifier from this character.
    /// </summary>
    /// <param name="stat">The stat to remove a modifier from.</param>
    /// <param name="sourceId">The id of the source of the modifier.</param>
    public void RemoveModifier(Stat stat, string sourceId)
    {
        StatModifiers[stat].RemoveAll(mod => mod.SourceId == sourceId);
    }

    /// <summary>
    /// Boost a move in this character's moveset, replacing it with its boosted version. Unboosts previously boosted move, replacing it with its normal version.
    /// </summary>
    /// <param name="move">The move to boost.</param>
    public void BoostMove(MoveData move)
    {
        if (move == null) return;

        // if there is already a boosted move, unboost it
        MoveData boostedMove = Moveset.Where(mv => mv.IsBoosted).FirstOrDefault();
        if (boostedMove != null) _moveset[Moveset.IndexOf(boostedMove)] = boostedMove.AlternateVersion;

        _moveset[Moveset.IndexOf(move)] = move.AlternateVersion;  // replace the provided move in this character's moveset with its boosted version
    }

    /// <summary>
    /// Make this character take some amount of damage. Health will not go below zero.
    /// </summary>
    /// <param name="damage">The amount of damage to take.</param>
    protected void TakeDamage(int damage)
    {
        // trigger effects of status effects and abilities on this character that happen before taking any damage
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects?.OnBeforeDamage?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects?.OnBeforeDamage?.Invoke(this, null, ability);

        _currentHP = Mathf.Max(0, _currentHP - damage);  // keep health from going below zero

        // trigger effects of status effects and abilities on this character that happen after taking any damage or when hp is changed
        foreach (StatusEffectInstance status in StatusEffects)
        {
            status.Effects?.OnAfterDamage?.Invoke(this, status, null);
            status.Effects?.OnAfterHPChanged?.Invoke(this, status, null);
        }
        foreach (AbilityData ability in Abilities)
        {
            ability.Effects?.OnAfterDamage?.Invoke(this, null, ability);
            ability.Effects?.OnAfterHPChanged?.Invoke(this, null, ability);
        }
    }

    /// <summary>
    /// Make this character heal some amount of health. Health will not go above this character's Max HP.
    /// </summary>
    /// <param name="healAmount">The amount of health to heal.</param>
    protected void Heal(int healAmount)
    {
        _currentHP = Mathf.Min(_currentHP + healAmount, MaxHP);  // keep health from exceding MaxHP

        // trigger effects of status effects and abilities on this character that happen when their character's hp is changed
        foreach (StatusEffectInstance status in StatusEffects)
            status.Effects?.OnAfterHPChanged?.Invoke(this, status, null);
        foreach (AbilityData ability in Abilities)
            ability.Effects?.OnAfterHPChanged?.Invoke(this, null, ability);
    }

    /// <summary>
    /// Determine if this character has all status effects of the provided types that can be applied by a move.
    /// </summary>
    /// <param name="move">The move to check.</param>
    /// <param name="effectTypes">A list of the status effect types to check for.</param>
    /// <returns>Whether or not this character already has every status effect that can be applied by the provided move.</returns>
    public bool HasAllMoveStatusEffects(MoveData move, List<StatusEffectType> effectTypes)
    {
        foreach (StatusEffectData status in move.StatusEffects.Keys)
        {
            if (effectTypes.Contains(status.Type))
                if (!HasStatusEffect(status.Id)) return false;
        }
        return true;
    }

    /// <summary>
    /// Determine if this character has all status effects of the provided type that can be applied by a move.
    /// </summary>
    /// <param name="move">The move to check.</param>
    /// <param name="effectType">The status effect types to check for.</param>
    /// <returns>Whether or not this character already has every status effect that can be applied by the provided move.</returns>
    public bool HasAllMoveStatusEffects(MoveData move, StatusEffectType effectType)
    {
        foreach (StatusEffectData status in move.StatusEffects.Keys)
        {
            if (effectType == status.Type)
                if (!HasStatusEffect(status.Id)) return false;
        }
        return true;
    }

    /// <summary>
    /// Determine if this character has a status effect.
    /// </summary>
    /// <param name="statusId">The id of the status effect to check for.</param>
    /// <returns>Whether or not this character has the provided status effect.</returns>
    public bool HasStatusEffect(string statusId)
    {
        return StatusEffects.Exists(status => status.StatusEffectData.Id == statusId);
    }

    // Helper methods for use inside this class
    /// <summary>
    /// Directly set this character's hp while keeping it within the range of zero to its Max HP. Directly updates this character's healthbar.
    /// </summary>
    /// <param name="hp">The amount to set the hp to.</param>
    protected void SetHP(int hp)
    {
        _currentHP = Mathf.Clamp(hp, 0, MaxHP);  // keep hp within range
        CharacterUI.UpdateHealth();  // update UI
    }

    /// <summary>
    /// Initialize the additional stats of this character with a zero in each stat. To be called when this character is first initialized.
    /// </summary>
    protected void InitializeAdditionalStats()
    {
        _additionalStats = new Dictionary<Stat, int>()
        {
            { Stat.MaxHP, 0 },
            { Stat.Attack, 0 },
            { Stat.Support, 0 },
            { Stat.Defense, 0 },
            { Stat.Speed, 0 }
        };
    }

    /// <summary>
    /// Calculate this character's base stats based on its total stat points and stat spread.
    /// </summary>
    protected void CalculateBaseStats()
    {
        _baseStats = new Dictionary<Stat, int>()
        {
            {Stat.MaxHP, Mathf.FloorToInt((baseStatPoints + additionalStatPoints) * (_characterData.StatSpread.MaxHP / 100.0f) + 0.5f)},
            {Stat.Attack, Mathf.FloorToInt((baseStatPoints + additionalStatPoints) * (_characterData.StatSpread.Attack / 100.0f) + 0.5f)},
            {Stat.Support, Mathf.FloorToInt((baseStatPoints + additionalStatPoints) * (_characterData.StatSpread.Support / 100.0f) + 0.5f)},
            {Stat.Defense, Mathf.FloorToInt((baseStatPoints + additionalStatPoints) * (_characterData.StatSpread.Defense / 100.0f) + 0.5f)},
            {Stat.Speed, Mathf.FloorToInt((baseStatPoints + additionalStatPoints) * (_characterData.StatSpread.Speed / 100.0f) + 0.5f)},
        };
    }

    /// <summary>
    /// Remove all stat modifiers on this character.
    /// </summary>
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

    /// <summary>
    /// Remove all status effects from this character.
    /// </summary>
    protected void ResetStatusEffects()
    {
        if (StatusEffects == null) StatusEffects = new List<StatusEffectInstance>();
        else
        {
            List<StatusEffectInstance> effects = new List<StatusEffectInstance>();
            foreach (StatusEffectInstance status in StatusEffects) effects.Add(status);
            foreach (StatusEffectInstance status in effects) RemoveStatusEffect(status, false);
        }
    }

    /// <summary>
    /// Calculate and set the total stat points this character has based on its base stat points and level.
    /// </summary>
    protected void CalculateBaseStatPoints()
    {
        baseStatPoints = _characterData.BaseStatPoints + (_characterData.LevelupStatPoints * (_level - 1));
    }

    /// <summary>
    /// Get the value of a stat on this character.
    /// </summary>
    /// <param name="stat">The stat to get the value of.</param>
    /// <returns>The current value of the provided stat.</returns>
    protected int GetStat(Stat stat)
    {
        // The value of a stat, before temporary modifiers, is the base stat plus any bonus permanent stat increases
        int value = _baseStats[stat] + AdditionalStats[stat];

        // Multiply the base value by each active stat modifier for that stat (if there are any)
        if (StatModifiers[stat].Count > 0)
        {
            List<float> modifierVals = StatModifiers[stat].Select(mod => mod.Power).ToList();
            value = Mathf.FloorToInt(value * modifierVals.Aggregate(1f, (acc, next) => acc * next) + 0.5f);
        }

        return value;
    }

    /// <summary>
    /// Set the moveset of this character based on its level.
    /// </summary>
    protected void DetermineMoveset()
    {
        MoveData moveToBoost = null;
        if (Moveset != null && Moveset.Count > 0)
        {
            MoveData boostedMove = Moveset.Where(mv => mv.IsBoosted).FirstOrDefault();
            if (boostedMove != null) moveToBoost = boostedMove.AlternateVersion;
        }

        _moveset = new List<MoveData>();
        foreach (MoveLevelPair mlp in _characterData.MoveLearnset)
        {
            if (mlp.Level <= _level)
                _moveset.Add(mlp.MoveData);
        }

        BoostMove(moveToBoost);
    }

    /// <summary>
    /// Set the abilities of this character based on its level.
    /// </summary>
    protected void DetermineAbilities()
    {
        _abilities = new List<AbilityData>();
        foreach (AbilityLevelPair mlp in _characterData.AbilityLearnset)
        {
            if (mlp.Level <= _level)
            {
                _abilities.Add(mlp.AbilityData);
            }
        }
    }

    /// <summary>
    /// Level up this character.
    /// </summary>
    public void LevelUp()
    {
        if (Level >= 100) return;  // max level is lvl 100

        _level++;

        CalculateBaseStatPoints();
        CalculateBaseStats();

        DetermineMoveset();
        DetermineAbilities();
    }
}

public enum Stat { MaxHP, Attack, Support, Defense, Speed, DamageDealt, DamageTaken, Accuracy, Evasion }

[System.Serializable]
public class StatModifier
{
    /// <summary>Gets the power of this modifier.</summary>
    public float Power { get; private set; }
    /// <summary>Gets the id of the source of this modifier.</summary>
    public string SourceId { get; private set; }

    /// <summary>
    /// Initialize and create a new stat modifier.
    /// </summary>
    /// <param name="power">The value of the modifier.</param>
    /// <param name="sourceId">The id of the source of this modifier.</param>
    public StatModifier(float power, string sourceId)
    {
        Power = power;
        SourceId = sourceId;
    }
}
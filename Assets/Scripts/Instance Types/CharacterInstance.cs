using System.Collections;
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
    public Dictionary<Stat, List<float>> StatModifiers { get; private set; }
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
        _currentHP = MaxHP;
    }

    public IEnumerator TakeAttackDamage(CharacterInstance user, MoveData move, MoveDamageEffect effect)
    {
        int damage = Mathf.RoundToInt((effect.Power / 5f) * ((user.Level / 10f) + 1) * ((float)user.Attack / user.Defense)) + 1;

        int newHP = _currentHP - damage;
        yield return SetHP(newHP, true);
    }

    public void AddStatusEffect(StatusEffectInstance effect)
    {
        StatusEffects.Add(effect);
        CharacterUI.SetEffects(StatusEffects);
    }

    protected IEnumerator SetHP(int hp, bool smooth)
    {
        _currentHP = hp >= 0 ? hp : 0;

        if (!smooth) CharacterUI.UpdateHealth();
        else yield return CharacterUI.UpdateHealthSmooth();
    }

    protected void InitializeAdditionalStats()
    {
        AdditionalStats = new Dictionary<Stat, int>();
        AdditionalStats.Add(Stat.MaxHP, 0);
        AdditionalStats.Add(Stat.Attack, 0);
        AdditionalStats.Add(Stat.Support, 0);
        AdditionalStats.Add(Stat.Defense, 0);
        AdditionalStats.Add(Stat.Speed, 0);
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
        StatModifiers = new Dictionary<Stat, List<float>>();
        StatModifiers.Add(Stat.MaxHP, new List<float>());
        StatModifiers.Add(Stat.Attack, new List<float>());
        StatModifiers.Add(Stat.Support, new List<float>());
        StatModifiers.Add(Stat.Defense, new List<float>());
        StatModifiers.Add(Stat.Speed, new List<float>());
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
            value = Mathf.RoundToInt(value * StatModifiers[stat].Aggregate((acc, next) => acc * next));
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

public enum Stat { MaxHP, Attack, Support, Defense, Speed }

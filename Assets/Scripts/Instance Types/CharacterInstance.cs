using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CharacterInstance
{
    protected CharacterData _characterData;
    protected CharacterUI _characterUI;

    [SerializeField, Min(1)] protected int _level;

    protected int totalStatPoints;
    protected int _currentHP;

    public CharacterData CharacterData { get => _characterData; }
    public CharacterUI CharacterUI { get => _characterUI; set => _characterUI = value; }

    public int Level { get => _level; }

    public int CurrentHP { get => _currentHP; }

    public List<StatusEffectInstance> StatusEffects { get; private set; }

    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, List<float>> StatModifiers { get; private set; }

    public int MaxHP { get { return GetStat(Stat.MaxHP); } }
    public int Attack { get { return GetStat(Stat.Attack); } }
    public int Support { get { return GetStat(Stat.Support); } }
    public int Defense { get { return GetStat(Stat.Defense); } }
    public int Speed { get { return GetStat(Stat.Speed); } }

    public List<MoveData> Moveset { get; private set; }
    public List<AbilityData> Abilities { get; private set; }

    public virtual void Init()
    {
        CalculateTotalStatPoints();
        CalculateStartingStats();
        ResetStatModifiers();
        ResetStatusEffects();
        _currentHP = MaxHP;

        DetermineMoveset();
        DetermineAbilities();
    }

    public void BattleStart()
    {
        _currentHP = MaxHP;
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

    public void AddStatusEffect(StatusEffectInstance effect)
    {
        StatusEffects.Add(effect);
    }
}

public enum Stat { MaxHP, Attack, Support, Defense, Speed }

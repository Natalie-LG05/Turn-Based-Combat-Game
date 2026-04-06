using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CharacterInstance
{
    protected CharacterData characterData;

    [SerializeField, Min(1)] protected int level;

    protected int totalStatPoints;
    protected int currentHP;

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
        currentHP = MaxHP;

        DetermineMoveset();
        DetermineAbilities();
    }

    protected void CalculateStartingStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.MaxHP, Mathf.RoundToInt(totalStatPoints * characterData.StatSpread.MaxHP));
        Stats.Add(Stat.Attack, Mathf.RoundToInt(totalStatPoints * characterData.StatSpread.Attack));
        Stats.Add(Stat.Support, Mathf.RoundToInt(totalStatPoints * characterData.StatSpread.Support));
        Stats.Add(Stat.Defense, Mathf.RoundToInt(totalStatPoints * characterData.StatSpread.Defense));
        Stats.Add(Stat.Speed, Mathf.RoundToInt(totalStatPoints * characterData.StatSpread.Speed));
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

    protected void CalculateTotalStatPoints()
    {
        totalStatPoints = characterData.BaseStatPoints + (characterData.LevelupStatPoints * level);
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
        
        foreach (MoveLevelPair mlp in characterData.MoveLearnset)
        {
            if (mlp.Level <= level)
            {
                Moveset.Add(mlp.MoveData);
            }
        }
    }

    protected void DetermineAbilities()
    {
        Abilities = new List<AbilityData>();

        foreach (AbilityLevelPair mlp in characterData.AbilityLearnset)
        {
            if (mlp.Level <= level)
            {
                Abilities.Add(mlp.AbilityData);
            }
        }
    }
}

public enum Stat { MaxHP, Attack, Support, Defense, Speed }

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyInstance : CharacterInstance
{
    [SerializeField] private EnemyData _enemyData;

    public EnemyData EnemyData { get => _enemyData;  }

    public List<MoveData> Attacks
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.Attack)).ToList(); }
    }

    public List<MoveData> BuffUserMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.BuffUser)).ToList(); }
    }

    public List<MoveData> BuffAllyMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.BuffAlly)).ToList(); }
    }

    public List<MoveData> BuffMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.BuffUser) || move.Categories.Contains(MoveCategory.BuffAlly)).ToList(); }
    }

    public List<MoveData> DebuffEnemyMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.DebuffEnemy)).ToList(); }
    }

    public List<MoveData> HealUserMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.HealUser)).ToList(); }
    }

    public List<MoveData> HealAllyMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.HealAlly)).ToList(); }
    }

    public List<MoveData> HealMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.HealUser) || move.Categories.Contains(MoveCategory.HealAlly)).ToList(); }
    }

    public override void Init()
    {
        // upcast enemy data into character data and assign it before initializing to avoid errors
        _characterData = _enemyData;
        base.Init();
    }

    public MoveData StrongestAttackMove()
    {
        MoveData strongestAttack = Attacks[0];
        foreach (MoveData move in Attacks)
        {
            if (move.Power > strongestAttack.Power)
                strongestAttack = move;
            else if (move.Power == strongestAttack.Power)
            {
                // Break ties randomly
                strongestAttack = Random.Range(0, 2) == 1 ? strongestAttack : move;
            }
        }
        return strongestAttack;
    }

    public List<MoveData> UsefulBuffMoves(List<EnemyInstance> enemies)
    {
        List<MoveData> usefulMoves = new List<MoveData>();
        if (BuffMoves.Count <= 0) return usefulMoves;  // if this enemy has no buff moves, return an empty list

        foreach (MoveData move in BuffUserMoves)
            if (!HasAllMoveStatusEffects(move)) usefulMoves.Add(move);

        foreach (MoveData move in BuffAllyMoves)
            if (AlliesNeedingBuff(enemies, move).Count > 0) usefulMoves.Add(move);

        return usefulMoves;
    }

    public List<EnemyInstance> AlliesNeedingBuff(List<EnemyInstance> enemies, MoveData move)
    {
        List<EnemyInstance> allies = new List<EnemyInstance>();
        foreach (EnemyInstance enemy in enemies)
            if (!enemy.HasAllMoveStatusEffects(move)) allies.Add(enemy);
        return allies;
    }

    public List<MoveData> UsefulDebuffMoves(List<CharacterInstance> playerCharacters)
    {
        List<MoveData> usefulMoves = new List<MoveData>();
        if (DebuffEnemyMoves.Count <= 0) return usefulMoves;  // if this enemy has no debuff moves, return an empty list

        foreach (MoveData move in DebuffEnemyMoves)
            if (PlayerCharactersNeedingDebuff(playerCharacters, move).Count > 0) usefulMoves.Add(move);

        return usefulMoves;
    }

    public List<CharacterInstance> PlayerCharactersNeedingDebuff(List<CharacterInstance> playerCharacters, MoveData move)
    {
        List<CharacterInstance> characters = new List<CharacterInstance>();
        foreach (CharacterInstance character in playerCharacters)
            if (!character.HasAllMoveStatusEffects(move)) characters.Add(character);
        return characters;
    }

    public StatusEffectData RandomUsefulStatusEffect(CharacterInstance target, MoveData move)
    {
        List<StatusEffectData> usefulStatuses = new List<StatusEffectData>();
        foreach (StatusEffectData status in move.StatusEffects.Keys) 
            if (!target.HasStatusEffect(status.Id)) usefulStatuses.Add(status);
        return usefulStatuses[Random.Range(0, usefulStatuses.Count)];
    }

    public MoveData StrongestStatusMove(CharacterInstance target, StatusEffectData status)
    {
        if (target.uniqueCharacterId == uniqueCharacterId)
        {
            // If the enemy is targetting itself, it must be using a buff so check all buff moves
            List<MoveData> usefulMoves = BuffMoves.Where(move => move.StatusEffects.ContainsKey(status)).ToList();
            return StrongestStatusMove(usefulMoves, status);
        } else if (!target.IsPlayerTeam)
        {
            // If the enemy is targetting another enemy, it must be using a buff so check buff moves that target allies
            List<MoveData> usefulMoves = BuffAllyMoves.Where(move => move.StatusEffects.ContainsKey(status)).ToList();
            return StrongestStatusMove(usefulMoves, status);
        } else
        {
            // The enemy is targetting a party member, so check its debuff moves
            List<MoveData> usefulMoves = DebuffEnemyMoves.Where(move => move.StatusEffects.ContainsKey(status)).ToList();
            return StrongestStatusMove(usefulMoves, status);
        }
    }

    private MoveData StrongestStatusMove(List<MoveData> moves, StatusEffectData status)
    {
        MoveData strongestMove = moves[0];
        foreach (MoveData move in moves)
        {
            if (move.StatusEffects[status] > strongestMove.StatusEffects[status])
                strongestMove = move;
            else if (move.StatusEffects[status] == strongestMove.StatusEffects[status])
            {
                // Break ties randomly
                strongestMove = Random.Range(0, 2) == 1 ? strongestMove : move;
            }
        }
        return strongestMove;
    }

    public List<MoveData> UsefulHealingMoves(List<EnemyInstance> enemies)
    {
        List<MoveData> usefulMoves = new List<MoveData>();
        if (HealMoves.Count <= 0) return usefulMoves;  // if this enemy has no healing moves, return an empty list

        foreach (EnemyInstance enemy in AlliesNeedingHealing(enemies))
        {
            if (enemy.uniqueCharacterId == uniqueCharacterId)
            {
                MoveData move = StrongestHealMove(enemy);
                if (!usefulMoves.Exists(move => move.Id == move.Id)) usefulMoves.Add(move);
            } else if (HealAllyMoves.Count > 0)
            {
                MoveData move = StrongestHealMove(enemy);
                if (!usefulMoves.Exists(move => move.Id == move.Id)) usefulMoves.Add(move);
            }
        }

        return usefulMoves;
    }

    public MoveData StrongestHealMove(EnemyInstance enemy)
    {
        if (enemy.uniqueCharacterId != uniqueCharacterId)
            return StrongestHealAllyMove();

        return StrongestHealMove();
    }

    private MoveData StrongestHealMove()
    {
        MoveData strongestHeal = HealMoves[0];
        foreach (MoveData move in HealMoves)
        {
            if (move.TotalHealPower > strongestHeal.TotalHealPower)
                strongestHeal = move;
            else if (move.TotalHealPower == strongestHeal.TotalHealPower)
            {
                // Break ties randomly
                strongestHeal = Random.Range(0, 2) == 1 ? strongestHeal : move;
            }
        }
        return strongestHeal;
    }

    private MoveData StrongestHealAllyMove()
    {
        MoveData strongestHeal = HealAllyMoves[0];
        foreach (MoveData move in HealAllyMoves)
        {
            if (move.TotalHealPower > strongestHeal.TotalHealPower)
                strongestHeal = move;
            else if (move.TotalHealPower == strongestHeal.TotalHealPower)
            {
                strongestHeal = Random.Range(0, 2) == 1 ? strongestHeal : move;
            }
        }
        return strongestHeal;
    }

    public List<EnemyInstance> AlliesNeedingHealing(List<EnemyInstance> enemies)
    {
        List<EnemyInstance> allies = new List<EnemyInstance>();
        foreach (EnemyInstance enemy in enemies)
            if (ShouldHeal(enemy)) allies.Add(enemy);
        return allies;
    }

    public bool ShouldHeal(EnemyInstance enemy)
    {
        float healthPercent = (float)enemy.CurrentHP / enemy.MaxHP;
        return healthPercent <= _enemyData.HealThreshold / 100f;
    }
}

public enum EnemyAction { Attack, Buff, Debuff, Heal }
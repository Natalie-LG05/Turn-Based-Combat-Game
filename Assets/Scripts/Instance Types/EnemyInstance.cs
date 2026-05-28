using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents an instance of an enemy, which is based off of an EnemyData.
/// </summary>
[System.Serializable]
public class EnemyInstance : CharacterInstance
{
    [SerializeField, Tooltip("The scriptable object to base this enemy off of.")] private EnemyData _enemyData;

    /// <summary>Gets the enemy data this enemy instance is based off of.</summary>
    public EnemyData EnemyData { get => _enemyData;  }

    /// <summary>Gets all attack moves in this enemy's moveset</summary>
    public List<MoveData> Attacks
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.Attack)).ToList(); }
    }

    /// <summary>Gets all moves in this enemy's moveset that apply a buff to the user.</summary>
    public List<MoveData> BuffUserMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.BuffUser)).ToList(); }
    }

    /// <summary>Gets all moves in this enemy's moveset that can apply a buff to any ally.</summary>
    public List<MoveData> BuffAllyMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.BuffAlly)).ToList(); }
    }

    /// <summary>Gets all moves in this enemy's moveset that can apply a buff to the user or an ally.</summary>
    public List<MoveData> BuffMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.BuffUser) || move.Categories.Contains(MoveCategory.BuffAlly)).ToList(); }
    }

    /// <summary>Gets all moves in this enemy's moveset that apply a debuff to an opposing character.</summary>
    public List<MoveData> DebuffEnemyMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.DebuffEnemy)).ToList(); }
    }

    /// <summary>Gets all moves in this enemy's moveset that heal the user.</summary>
    public List<MoveData> HealUserMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.HealUser)).ToList(); }
    }

    /// <summary>Gets all moves in this enemy's moveset that can heal any ally.</summary>
    public List<MoveData> HealAllyMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.HealAlly)).ToList(); }
    }

    /// <summary>Gets all moves in this enemy's moveset that can heal the user or an ally.</summary>
    public List<MoveData> HealMoves
    {
        get { return Moveset.Where(move => move.Categories.Contains(MoveCategory.HealUser) || move.Categories.Contains(MoveCategory.HealAlly)).ToList(); }
    }

    public EnemyInstance(EnemyData enemyData, int level)
    {
        _enemyData = enemyData;
        _level = level;
        Init();
    }

    /// <inheritdoc/>
    public override void Init()
    {
        // upcast enemy data into character data and assign it before initializing to avoid errors
        _characterData = _enemyData;
        base.Init();
    }

    /// <summary>
    /// Determine this enemy's strongest attack move. Ties are handled randomly.
    /// </summary>
    /// <returns>The strongest attack move this character has.</returns>
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

    /// <summary>
    /// Determines what buff moves in this enemy's moveset would be useful to use. 
    /// <br/>A useful buff move is one that has available enemy team targets that don't already have all the status effects applied by it.
    /// </summary>
    /// <param name="enemies">A list of all currently active enemies.</param>
    /// <returns>A list of useful buff moves this enemy has.</returns>
    public List<MoveData> UsefulBuffMoves(List<EnemyInstance> enemies)
    {
        List<MoveData> usefulMoves = new List<MoveData>();
        if (BuffMoves.Count <= 0) return usefulMoves;  // if this enemy has no buff moves, return an empty list

        foreach (MoveData move in BuffUserMoves)
            if (!HasAllMoveStatusEffects(move, new List<StatusEffectType> { StatusEffectType.Buff, StatusEffectType.Other })) usefulMoves.Add(move);

        foreach (MoveData move in BuffAllyMoves)
            if (AlliesNeedingBuff(enemies, move).Count > 0) usefulMoves.Add(move);

        return usefulMoves;
    }

    /// <summary>
    /// Determines which allies (if any) do not already have all the status effects applied by a certain move.
    /// </summary>
    /// <param name="enemies">A list of all currently active enemies.</param>
    /// <param name="move">The move to check.</param>
    /// <returns>A list of all allies that would benefit from the provided move.</returns>
    public List<EnemyInstance> AlliesNeedingBuff(List<EnemyInstance> enemies, MoveData move)
    {
        List<EnemyInstance> allies = new List<EnemyInstance>();
        foreach (EnemyInstance enemy in enemies)
            if (!enemy.HasAllMoveStatusEffects(move, new List<StatusEffectType> { StatusEffectType.Buff, StatusEffectType.Other })) allies.Add(enemy);
        return allies;
    }

    /// <summary>
    /// Determines what debuff moves in this enemy's moveset would be useful to use. 
    /// <br/>A useful debuff move is one that has available player team targets that don't already have all the status effects applied by it.
    /// </summary>
    /// <param name="playerCharacters">A list of all currently active characters on the player's team.</param>
    /// <returns>A list of useful debuff moves this enemy has.</returns>
    public List<MoveData> UsefulDebuffMoves(List<CharacterInstance> playerCharacters)
    {
        List<MoveData> usefulMoves = new List<MoveData>();
        if (DebuffEnemyMoves.Count <= 0) return usefulMoves;  // if this enemy has no debuff moves, return an empty list

        foreach (MoveData move in DebuffEnemyMoves)
            if (PlayerCharactersNeedingDebuff(playerCharacters, move).Count > 0) usefulMoves.Add(move);

        return usefulMoves;
    }

    /// <summary>
    /// Determines which player team character's (if any) do not already have all the status effects applied by a certain move.
    /// </summary>
    /// <param name="playerCharacters">A list of all currently active characters on the player's team.</param>
    /// <param name="move">The move to check.</param>
    /// <returns>A list of all player team characters that this move would hinder.</returns>
    public List<CharacterInstance> PlayerCharactersNeedingDebuff(List<CharacterInstance> playerCharacters, MoveData move)
    {
        List<CharacterInstance> characters = new List<CharacterInstance>();
        foreach (CharacterInstance character in playerCharacters)
            if (!character.HasAllMoveStatusEffects(move, new List<StatusEffectType> { StatusEffectType.Debuff, StatusEffectType.Other })) characters.Add(character);
        return characters;
    }

    /// <summary>
    /// Picks out a random useful status effect from the status effects that can be applied by a certain move.
    /// <br/>A useful status effect is one that the target does not already have.
    /// </summary>
    /// <param name="target">The character being targeted.</param>
    /// <param name="move">The move to check.</param>
    /// <returns>A random useful status effect of the provided move.</returns>
    public StatusEffectData RandomUsefulStatusEffect(CharacterInstance target, MoveData move)
    {
        List<StatusEffectData> usefulStatuses = new List<StatusEffectData>();
        foreach (StatusEffectData status in move.StatusEffects.Keys) 
            if (!target.HasStatusEffect(status.Id)) usefulStatuses.Add(status);
        return usefulStatuses[Random.Range(0, usefulStatuses.Count)];
    }

    /// <summary>
    /// Determine the move in this enemy's moveset that can provide the strongest version of a certain 
    /// status effect to a certain target. Ties are handled randomly.
    /// </summary>
    /// <param name="target">The character being targeted.</param>
    /// <param name="status">The target status effect.</param>
    /// <returns>Either the same move or a different move that applies a stronger version of the target status effect.</returns>
    public MoveData StrongestStatusMove(CharacterInstance target, StatusEffectData status)
    {
        if (target.UniqueCharacterId == UniqueCharacterId)
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

    /// <summary>
    /// Determines which move from a given list of moves applies the strongest version of a specified status effect.
    /// </summary>
    /// <param name="moves">The list of moves to check.</param>
    /// <param name="status">The target status effect.</param>
    /// <returns>The move from the provided list that appleis the strongest version of the provided status effect.</returns>
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

    /// <summary>
    /// Determines what healing moves this enemy has would be useful.
    /// <br/>A useful healing move is the strongest one that can target an enemy that needs healing.
    /// </summary>
    /// <param name="enemies">A list of all currently active enemies.</param>
    /// <returns>A list of useful healing moves in this enemy's moveset.</returns>
    public List<MoveData> UsefulHealingMoves(List<EnemyInstance> enemies)
    {
        List<MoveData> usefulMoves = new List<MoveData>();
        if (HealMoves.Count <= 0) return usefulMoves;  // if this enemy has no healing moves, return an empty list

        foreach (EnemyInstance enemy in AlliesNeedingHealing(enemies))
        {
            if (enemy.UniqueCharacterId == UniqueCharacterId)
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

    /// <summary>
    /// Determines the strongest heal move this enemy has that can target the given enemy.
    /// </summary>
    /// <param name="enemy">The target enemy.</param>
    /// <returns>The strongest healing move this enemy has that can target the provided enemy.</returns>
    public MoveData StrongestHealMove(EnemyInstance enemy)
    {
        if (enemy.UniqueCharacterId != UniqueCharacterId)
            return StrongestHealAllyMove();

        return StrongestHealMove();
    }

    /// <summary>
    /// Determines the strongest healing move this character has, regaredless of potential targets.
    /// </summary>
    /// <returns>The strongest healing move this enemy has.</returns>
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

    /// <summary>
    /// Determines the strongest healing move this character has that can target any ally.
    /// </summary>
    /// <returns>The strongest healing move this enemy has that can target any ally.</returns>
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

    /// <summary>
    /// Determines which allies (if any) need healing based on this enemies heal threshold.
    /// </summary>
    /// <param name="enemies">A list of all currently active enemies.</param>
    /// <returns>A list of enemies that need healing.</returns>
    public List<EnemyInstance> AlliesNeedingHealing(List<EnemyInstance> enemies)
    {
        List<EnemyInstance> allies = new List<EnemyInstance>();
        foreach (EnemyInstance enemy in enemies)
            if (ShouldHeal(enemy)) allies.Add(enemy);
        return allies;
    }

    /// <summary>
    /// Determines if this enemy should heal a given enemy. This is based on if the target enemy's health percentage is below this enemy's heal threshold.
    /// </summary>
    /// <param name="enemy">The target enemy.</param>
    /// <returns>If this enemy should heal that enemy or not.</returns>
    public bool ShouldHeal(EnemyInstance enemy)
    {
        float healthPercent = (float)enemy.CurrentHP / enemy.MaxHP;
        return healthPercent <= _enemyData.HealThreshold / 100f;
    }
}

public enum EnemyAction { Attack, Buff, Debuff, Heal }
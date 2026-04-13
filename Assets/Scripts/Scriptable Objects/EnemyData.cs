using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/Enemy Data", order = -1000)]
public class EnemyData : CharacterData
{
    [SerializeField] private EnemyRarity _rarity;

    [SerializeField, Range(0, 100)] private int _attackChance;
    [SerializeField, Range(0, 100)] private int _buffChance;
    [SerializeField, Range(0, 100)] private int _debuffChance;
    [SerializeField, Range(0, 100)] private int _healChance;
    [SerializeField, Range(0, 100)] private int _healThreshold;

    public EnemyRarity Rarity { get => _rarity; }

    public int AttackChance { get => _attackChance; }
    public int BuffChance { get => _buffChance; }
    public int DebuffChance { get => _debuffChance; }
    public int HealChance { get => _healChance; }
    public int HealThreshold { get => _healThreshold; }
}

public enum EnemyRarity { Common, Rare }
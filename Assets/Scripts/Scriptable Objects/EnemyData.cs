using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/Enemy Data", order = -1000)]
public class EnemyData : CharacterData
{
    [SerializeField] private EnemyRarity _rarity;

    public EnemyRarity Rarity { get => _rarity; }
}

public enum EnemyRarity { common, rare }
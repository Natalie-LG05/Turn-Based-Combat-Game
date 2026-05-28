using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for an area.
/// </summary>
[CreateAssetMenu(fileName = "NewAreaData", menuName = "ScriptableObjects/Area Data", order = -1000)]
public class AreaData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField, Tooltip("The unique string id of this area.")] private string _id;
    [SerializeField, TextArea, Tooltip("Enemy types that are normally found in this area.")] private string _description;

    [SerializeField] private List<EnemyData> _enemies;

    [SerializeField] private Color _color;
    [SerializeField] private Sprite _background;

    public string Name { get => _name; }
    /// <summary>Gets the unique string id of this area.</summary>
    public string Id { get => _id; }
    public string Description { get => _description; }

    /// <summary>Gets a list of the enemy types that are normally found in this area.</summary>
    public List<EnemyData> Enemies { get => _enemies; }

    /// <summary>Gets the color to use for certain UI elements related to this area.</summary>
    public Color Color { get => _color; }
    /// <summary>Gets the sprite to be used as the background during combat in encounters in this area.</summary>
    public Sprite Background { get => _background; }

    /// <summary>Gets a list of the common enemies in this area.</summary>
    public List<EnemyData> CommonEnemies
    {
        get { return Enemies.Where(enemy => enemy.Rarity == EnemyRarity.Common).ToList(); }
    }
    /// <summary>Gets a list of the rare enemies in this area.</summary>
    public List<EnemyData> RareEnemies
    {
        get { return Enemies.Where(enemy => enemy.Rarity == EnemyRarity.Rare).ToList(); }
    }

    /// <summary>Gets a random common enemy from this area.</summary>
    public EnemyData RandomCommonEnemy
    {
        get { return CommonEnemies[Random.Range(0, CommonEnemies.Count)]; }
    }
    /// <summary>Gets a random rare enemy from this area.</summary>
    public EnemyData RandomRareEnemy
    {
        get { return RareEnemies[Random.Range(0, RareEnemies.Count)]; }
    }
}

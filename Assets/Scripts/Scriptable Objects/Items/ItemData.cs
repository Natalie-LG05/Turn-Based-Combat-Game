using UnityEngine;

/// <summary>
/// A scriptable object that contains the data for an item.
/// </summary>
[CreateAssetMenu(fileName = "NewItemData", menuName = "ScriptableObjects/Items/Item Data", order = -1000)]
public class ItemData : ScriptableObject
{
    [SerializeField] protected string _name;
    [SerializeField, Tooltip("The unique string id of this item.")] protected string _id;
    [SerializeField, TextArea] protected string _description;

    [SerializeField] protected ItemCategory _category;
    [SerializeField] protected ItemType _type;

    [SerializeField] protected Color _iconColor;

    public string Name { get => _name; }
    /// <summary>Gets the unique string id of this item.</summary>
    public string Id { get => _id; }
    public string Description { get => _description; }

    public ItemCategory Category { get => _category; }
    public ItemType Type { get => _type; }

    public Color IconColor { get => _iconColor; }

    public virtual Effect Effects
    {
        get
        {
            if (EffectsDB.ItemEffects.ContainsKey(_id)) return EffectsDB.ItemEffects[_id];
            else return null;
        }
    }
}

public enum ItemCategory { Consumable, Equipable, Currency, Key }

public enum ItemType { BattleItem, Currency, Key, LevelKrill, StatKrill, Weapon, Focus, Armor, Charm, MoveBooster }
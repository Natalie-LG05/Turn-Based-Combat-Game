using System.Collections.Generic;
using UnityEngine;

public enum InventoryCategory { Currency, Consumables, BattleItems, Weapons, Focuses, Armor, Charms, MoveBoosters, KeyItems }

/// <summary>
/// A singleton class that stores and manages the player's inventory.
/// </summary>
[System.Serializable]
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private List<ItemEntry<ItemData>> _currencyItems;
    [SerializeField] private List<ItemEntry<LevelKrillData>> _levelKrillItems;
    [SerializeField] private List<ItemEntry<StatKrillData>> _statKrillItems;
    [SerializeField] private List<ItemEntry<BattleItemData>> _battleItems;
    [SerializeField] private List<ItemEntry<EquipmentData>> _weapons;
    [SerializeField] private List<ItemEntry<EquipmentData>> _focuses;
    [SerializeField] private List<ItemEntry<EquipmentData>> _armor;
    [SerializeField] private List<ItemEntry<EquipmentData>> _charms;
    [SerializeField] private List<ItemEntry<EquipmentData>> _moveBoosters;
    [SerializeField] private List<ItemEntry<ItemData>> _keyItems;

    public List<ItemEntry<ItemData>> CurrencyItems { get => _currencyItems; }
    public List<ItemEntry<LevelKrillData>> LevelKrillItems { get => _levelKrillItems; }
    public List<ItemEntry<StatKrillData>> StatKrillItems { get => _statKrillItems; }
    public List<ItemEntry<BattleItemData>> BattleItems { get => _battleItems; }
    public List<ItemEntry<EquipmentData>> Weapons {  get => _weapons; }
    public List<ItemEntry<EquipmentData>> Focuses { get => _focuses; }
    public List<ItemEntry<EquipmentData>> Armor { get => _armor; }
    public List<ItemEntry<EquipmentData>> Charms { get => _charms; }
    public List<ItemEntry<EquipmentData>> MoveBoosters { get => _moveBoosters; }
    public List<ItemEntry<ItemData>> KeyItems { get => _keyItems; }

    private void Awake()
    {
        // If there is already an instance of this class, destroy new instances
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;  // Set the singleton instance
        DontDestroyOnLoad(gameObject);  // Set this object to persist between scenes

        if (CurrencyItems == null) _currencyItems = new List<ItemEntry<ItemData>>();
        if (LevelKrillItems == null) _levelKrillItems = new List<ItemEntry<LevelKrillData>>();
        if (StatKrillItems == null) _statKrillItems = new List<ItemEntry<StatKrillData>>();
        if (BattleItems == null) _battleItems = new List<ItemEntry<BattleItemData>>();
        if (Weapons == null) _weapons = new List<ItemEntry<EquipmentData>>();
        if (Focuses == null) _focuses = new List<ItemEntry<EquipmentData>>();
        if (Armor == null) _armor = new List<ItemEntry<EquipmentData>>();
        if (Charms == null) _charms = new List<ItemEntry<EquipmentData>>();
        if (MoveBoosters == null) _moveBoosters = new List<ItemEntry<EquipmentData>>();
        if (KeyItems == null) _keyItems = new List<ItemEntry<ItemData>>();
    }

    public List<ItemEntry<ItemData>> GetItemsInInventoryCategory(InventoryCategory category)
    {
        List<ItemEntry<ItemData>> entries = new List<ItemEntry<ItemData>>();

        switch (category)
        {
            case InventoryCategory.Currency: 
                return CurrencyItems;
            case InventoryCategory.Consumables:
                entries.AddRange(LevelKrillItems.ConvertAll(entry => entry.GetGenericEntry()));
                entries.AddRange(StatKrillItems.ConvertAll(entry => entry.GetGenericEntry()));
                break;
            case InventoryCategory.BattleItems:
                return BattleItems.ConvertAll(entry => entry.GetGenericEntry());
            case InventoryCategory.Weapons:
                return Weapons.ConvertAll(entry => entry.GetGenericEntry());
            case InventoryCategory.Focuses:
                return Focuses.ConvertAll(entry => entry.GetGenericEntry());
            case InventoryCategory.Armor:
                return Armor.ConvertAll(entry => entry.GetGenericEntry());
            case InventoryCategory.Charms:
                return Charms.ConvertAll(entry => entry.GetGenericEntry());
            case InventoryCategory.MoveBoosters:
                return MoveBoosters.ConvertAll(entry => entry.GetGenericEntry());
            case InventoryCategory.KeyItems:
                return KeyItems;

        }

        return entries;
    }

    /// <summary>
    /// Add a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="amount">The amount to add.</param>
    public void AddItemPublic(ItemData item, int amount)
    {
        switch (item.Category)
        {
            case ItemCategory.Currency or ItemCategory.Key:
                AddItem(item, amount);
                break;
            case ItemCategory.Consumable:
                switch (item.Type)
                {
                    case ItemType.LevelKrill:
                        AddItem((LevelKrillData)item, amount);
                        break;
                    case ItemType.StatKrill:
                        AddItem((StatKrillData)item, amount);
                        break;
                    case ItemType.BattleItem:
                        AddItem((BattleItemData)item, amount);
                        break;
                }
                break;
            case ItemCategory.Equipable:
                AddItem((EquipmentData)item, amount);
                break;
        }
    }

    /// <summary>
    /// Add a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="amount">The amount to add.</param>
    private void AddItem(ItemData item, int amount)
    {
        if (item.Category == ItemCategory.Currency)
        {
            int index = CurrencyItems.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
                _currencyItems[index].UpdateItems(amount);
            else
                _currencyItems.Add(new ItemEntry<ItemData>(item, amount));
        } else if (item.Category == ItemCategory.Key)
        {
            int index = KeyItems.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
                _keyItems[index].UpdateItems(amount);
            else
                _keyItems.Add(new ItemEntry<ItemData>(item, amount));
        }
    }

    /// <summary>
    /// Add a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="amount">The amount to add.</param>
    private void AddItem(LevelKrillData item, int amount)
    {
        if (LevelKrillItems.Exists(entry => entry.ItemData == item))
            _levelKrillItems[LevelKrillItems.FindIndex(entry => entry.ItemData == item)].UpdateItems(amount);
        else
            _levelKrillItems.Add(new ItemEntry<LevelKrillData>(item, amount));
    }

    /// <summary>
    /// Add a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="amount">The amount to add.</param>
    private void AddItem(StatKrillData item, int amount)
    {
        if (_statKrillItems.Exists(entry => entry.ItemData == item))
            _statKrillItems[StatKrillItems.FindIndex(entry => entry.ItemData == item)].UpdateItems(amount);
        else
            _statKrillItems.Add(new ItemEntry<StatKrillData>(item, amount));
    }

    /// <summary>
    /// Add a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="amount">The amount to add.</param>
    private void AddItem(BattleItemData item, int amount)
    {
        int index = BattleItems.FindIndex(entry => entry.ItemData == item);
        if (index != -1)
            _battleItems[index].UpdateItems(amount);
        else
            _battleItems.Add(new ItemEntry<BattleItemData>(item, amount));
    }

    /// <summary>
    /// Add a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="amount">The amount to add.</param>
    private void AddItem(EquipmentData item, int amount)
    {
        if (item.Type == ItemType.Weapon)
        {
            int index = Weapons.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
                _weapons[index].UpdateItems(amount);
            else
                _weapons.Add(new ItemEntry<EquipmentData>(item, amount));
        } else if (item.Type == ItemType.Focus)
        {
            int index = Focuses.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
                _focuses[index].UpdateItems(amount);
            else
                _focuses.Add(new ItemEntry<EquipmentData>(item, amount));
        }
        else if (item.Type == ItemType.Armor)
        {
            int index = Armor.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
                _armor[index].UpdateItems(amount);
            else
                _armor.Add(new ItemEntry<EquipmentData>(item, amount));
        }
        else if (item.Type == ItemType.Charm)
        {
            int index = Charms.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
                _charms[index].UpdateItems(amount);
            else
                _charms.Add(new ItemEntry<EquipmentData>(item, amount));
        }
        else if (item.Type == ItemType.MoveBooster)
        {
            int index = MoveBoosters.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
                _moveBoosters[index].UpdateItems(amount);
            else
                _moveBoosters.Add(new ItemEntry<EquipmentData>(item, amount));
        }
    }

    /// <summary>
    /// Remove a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <param name="amount"></param>
    /// <returns>Returns if the amount of that item reached zero or not.</returns>
    public bool RemoveItemPublic(ItemData item, int amount)
    {
        switch (item.Category)
        {
            case ItemCategory.Currency or ItemCategory.Key:
                return RemoveItem(item, amount);
            case ItemCategory.Consumable:
                switch (item.Type)
                {
                    case ItemType.LevelKrill:
                        return RemoveItem((LevelKrillData)item, amount);
                    case ItemType.StatKrill:
                        return RemoveItem((StatKrillData)item, amount);
                    case ItemType.BattleItem:
                        return RemoveItem((BattleItemData)item, amount);
                }
                break;
            case ItemCategory.Equipable:
                return RemoveItem((EquipmentData)item, amount);
        }

        return false;  // should never reach this point, just here to prevent a compile error
    }

    /// <summary>
    /// Remove a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <param name="amount"></param>
    /// <returns>Returns if the amount of that item reached zero or not.</returns>
    private bool RemoveItem(ItemData item, int amount)
    {
        bool reachedZero = false;

        if (item.Category == ItemCategory.Currency)
        {
            int index = CurrencyItems.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
            {
                _currencyItems[index].UpdateItems(-amount);
                if (_currencyItems[index].Amount <= 0)
                {
                    reachedZero = true;
                    _currencyItems.RemoveAt(index);
                }
            }
        }
        else if (item.Category == ItemCategory.Key)
        {
            int index = KeyItems.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
            {
                _keyItems[index].UpdateItems(-amount);
                if (_keyItems[index].Amount <= 0)
                {
                    reachedZero = true;
                    _keyItems.RemoveAt(index);
                }
            }
        }

        return reachedZero;
    }

    /// <summary>
    /// Remove a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <param name="amount"></param>
    /// <returns>Returns if the amount of that item reached zero or not.</returns>
    private bool RemoveItem(LevelKrillData item, int amount)
    {
        bool reachedZero = false;

        int index = LevelKrillItems.FindIndex(entry => entry.ItemData == item);
        if (index != -1)
        {
            _levelKrillItems[index].UpdateItems(-amount);
            if (_levelKrillItems[index].Amount <= 0)
            {
                reachedZero = true;
                _levelKrillItems.RemoveAt(index);
            }
        }

        return reachedZero;
    }

    /// <summary>
    /// Remove a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <param name="amount"></param>
    /// <returns>Returns if the amount of that item reached zero or not.</returns>
    private bool RemoveItem(StatKrillData item, int amount)
    {
        bool reachedZero = false;

        int index = StatKrillItems.FindIndex(entry => entry.ItemData == item);
        if (index != -1)
        {
            _statKrillItems[index].UpdateItems(-amount);
            if (_statKrillItems[index].Amount <= 0)
            {
                reachedZero = true;
                _statKrillItems.RemoveAt(index);
            }
        }

        return reachedZero;
    }

    /// <summary>
    /// Remove a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <param name="amount"></param>
    /// <returns>Returns if the amount of that item reached zero or not.</returns>
    private bool RemoveItem(BattleItemData item, int amount)
    {
        bool reachedZero = false;

        int index = BattleItems.FindIndex(entry => entry.ItemData == item);
        if (index != -1)
        {
            _battleItems[index].UpdateItems(-amount);
            if (_battleItems[index].Amount <= 0)
            {
                reachedZero = true;
                _battleItems.RemoveAt(index);
            }
        }

        return reachedZero;
    }

    /// <summary>
    /// Remove a specified amount of an item to the inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <param name="amount"></param>
    /// <returns>Returns if the amount of that item reached zero or not.</returns>
    private bool RemoveItem(EquipmentData item, int amount)
    {
        bool reachedZero = false;

        if (item.Type == ItemType.Weapon)
        {
            int index = Weapons.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
            {
                _weapons[index].UpdateItems(-amount);
                if (_weapons[index].Amount <= 0)
                {
                    reachedZero = true;
                    _weapons.RemoveAt(index);
                }
            }
        }
        else if (item.Type == ItemType.Focus)
        {
            int index = Focuses.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
            {
                _focuses[index].UpdateItems(-amount);
                if (_focuses[index].Amount <= 0)
                {
                    reachedZero = true;
                    _focuses.RemoveAt(index);
                }
            }
        }
        else if (item.Type == ItemType.Armor)
        {
            int index = Armor.FindIndex(entry => entry.ItemData == item);
           if (index != -1)
            {
                _armor[index].UpdateItems(-amount);
                if (_armor[index].Amount <= 0)
                {
                    reachedZero = true;
                    _armor.RemoveAt(index);
                }
            }
        }
        else if (item.Type == ItemType.Charm)
        {
            int index = Charms.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
            {
                _charms[index].UpdateItems(-amount);
                if (_charms[index].Amount <= 0)
                {
                    reachedZero = true;
                    _charms.RemoveAt(index);
                }
            }
        }
        else if (item.Type == ItemType.MoveBooster)
        {
            int index = MoveBoosters.FindIndex(entry => entry.ItemData == item);
            if (index != -1)
            {
                _moveBoosters[index].UpdateItems(-amount);
                if (_moveBoosters[index].Amount <= 0)
                {
                    reachedZero = true;
                    _moveBoosters.RemoveAt(index);
                }
            }
        }

        return reachedZero;
    }
}

/// <summary>
/// Represents an inventory entry, storing what item this entry is for and the amount of that item present.
/// </summary>
/// <typeparam name="TData">An item data class.</typeparam>
[System.Serializable]
public class ItemEntry<TData> where TData : ItemData
{
    [SerializeField] protected TData _itemData;
    [SerializeField] protected int _amount;

    public TData ItemData { get => _itemData; protected set => _itemData = value; }
    public int Amount { get => _amount; protected set => _amount = value; }

    public ItemEntry(TData itemData, int amount)
    {
        _itemData = itemData;
        _amount = amount;
    }

    /// <summary>
    /// Change the amount of items in this entry. Positive amounts will add items and negative amounts will decrease the amount.
    /// </summary>
    /// <param name="amount">The amount to change by.</param>
    public void UpdateItems(int amount)
    {
        _amount += amount;
    }

    public ItemEntry<ItemData> GetGenericEntry()
    {
        return new ItemEntry<ItemData>(ItemData, Amount);
    }
}
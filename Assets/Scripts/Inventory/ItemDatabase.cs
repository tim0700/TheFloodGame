using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<InventoryItem> items = new List<InventoryItem>();

    public InventoryItem GetByID(string id) => items.Find(x => x.itemID == id);
    public InventoryItem GetByName(string name) => items.Find(x => x.itemName == name);
}

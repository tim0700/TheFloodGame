using UnityEngine;

public enum ItemType { Resource, Consumable, Equipment, Material, Weapon, Misc }
public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("기본")]
    public string itemName;
    [TextArea] public string description;

    public Sprite icon;
    public ItemType itemType = ItemType.Misc;
    public ItemRarity rarity = ItemRarity.Common;

    [Header("스택/식별")]
    public bool isStackable = true;
    public int maxStack = 99;

    [Header("월드 프리팹 (옵션)")]
    public GameObject worldPrefab; // 월드에 놓일 때 참조 (ItemPickup 등에서 사용)

    [HideInInspector] public string itemID; // 고유 ID (에디터에서 자동 생성)

    private void OnValidate()
    {
        if (maxStack < 1) maxStack = 1;
        if (!isStackable) maxStack = 1;

        // 에디터에서 한 번만 ID 생성
        if (string.IsNullOrEmpty(itemID))
            itemID = System.Guid.NewGuid().ToString();
    }
}

using UnityEngine;

public enum ItemType { Resource, Consumable, Equipment, Material, Weapon, Misc }
public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("�⺻")]
    public string itemName;
    [TextArea] public string description;

    public Sprite icon;
    public ItemType itemType = ItemType.Misc;
    public ItemRarity rarity = ItemRarity.Common;

    [Header("����/�ĺ�")]
    public bool isStackable = true;
    public int maxStack = 99;

    [Header("���� ������ (�ɼ�)")]
    public GameObject worldPrefab; // ���忡 ���� �� ���� (ItemPickup ��� ���)

    [HideInInspector] public string itemID; // ���� ID (�����Ϳ��� �ڵ� ����)

    private void OnValidate()
    {
        if (maxStack < 1) maxStack = 1;
        if (!isStackable) maxStack = 1;

        // �����Ϳ��� �� ���� ID ����
        if (string.IsNullOrEmpty(itemID))
            itemID = System.Guid.NewGuid().ToString();
    }
}

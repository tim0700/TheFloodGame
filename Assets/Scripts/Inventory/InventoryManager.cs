using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("�κ��丮 ���� ��")]
    public int maxSlots = 20;

    [Header("���� �κ��丮 ���� ���")]
    public List<InventorySlot> slots = new List<InventorySlot>();

    private void Awake()
    {
        // �̱��� ���� (���� �ν��Ͻ� ����)
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // ���� �ʱ�ȭ: maxSlots ��ŭ �� ���� ����
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    /// <summary>
    /// ������ �߰� �õ� (�����ϸ� true)
    /// </summary>
    public bool AddItem(InventoryItem item, int quantity)
    {
        // ���� ������ �������� ���� ���Կ� ������ ��ġ��
        if (item.isStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.item == item && slot.quantity < item.maxStack)
                {
                    int availableSpace = item.maxStack - slot.quantity;
                    int addAmount = Mathf.Min(availableSpace, quantity);
                    slot.quantity += addAmount;
                    quantity -= addAmount;

                    if (quantity <= 0)
                        return true; // �� ����
                }
            }
        }

        // ���� ������ �� ���Կ� �ֱ�
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == null)
            {
                int addAmount = Mathf.Min(quantity, item.maxStack);
                slots[i].item = item;
                slots[i].quantity = addAmount;
                quantity -= addAmount;

                if (quantity <= 0)
                    return true;
            }
        }

        // ���� ����
        Debug.LogWarning("�κ��丮�� ���� á���ϴ�!");
        return false;
    }

    /// <summary>
    /// ������ ���� (���� ��ŭ)
    /// </summary>
    public bool RemoveItem(InventoryItem item, int quantity)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                if (slots[i].quantity > quantity)
                {
                    slots[i].quantity -= quantity;
                    return true;
                }
                else if (slots[i].quantity == quantity)
                {
                    slots[i].Clear();
                    return true;
                }
                else
                {
                    quantity -= slots[i].quantity;
                    slots[i].Clear();
                }
            }
        }

        Debug.LogWarning("�����Ϸ��� �������� �����մϴ�.");
        return false;
    }
}

/// <summary>
/// �κ��丮 ���� (������ + ����)
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public InventoryItem item;
    public int quantity;

    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }

    public void Clear()
    {
        item = null;
        quantity = 0;
    }
}

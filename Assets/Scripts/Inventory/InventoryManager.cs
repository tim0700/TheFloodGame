using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("인벤토리 슬롯 수")]
    public int maxSlots = 20;

    [Header("현재 인벤토리 슬롯 목록")]
    public List<InventorySlot> slots = new List<InventorySlot>();

    private void Awake()
    {
        // 싱글톤 패턴 (다중 인스턴스 방지)
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // 슬롯 초기화: maxSlots 만큼 빈 슬롯 생성
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    /// <summary>
    /// 아이템 추가 시도 (성공하면 true)
    /// </summary>
    public bool AddItem(InventoryItem item, int quantity)
    {
        // 스택 가능한 아이템이 기존 슬롯에 있으면 합치기
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
                        return true; // 다 넣음
                }
            }
        }

        // 남은 수량을 빈 슬롯에 넣기
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

        // 공간 부족
        Debug.LogWarning("인벤토리가 가득 찼습니다!");
        return false;
    }

    /// <summary>
    /// 아이템 제거 (수량 만큼)
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

        Debug.LogWarning("제거하려는 아이템이 부족합니다.");
        return false;
    }
}

/// <summary>
/// 인벤토리 슬롯 (아이템 + 수량)
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

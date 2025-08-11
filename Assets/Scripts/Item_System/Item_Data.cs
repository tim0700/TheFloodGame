using UnityEngine;
using System;

/// <summary>
/// 로컬 플레이어 전용 아이템 데이터 구조체
/// Input: 아이템 ID, 수량, 추가 데이터
/// Output: 아이템 정보 저장/비교
/// Type: Local Struct
/// </summary>
[System.Serializable]
public struct Item_Data : System.IEquatable<Item_Data>
{
    [SerializeField] public int Item_ID;           // 아이템 고유 식별자
    [SerializeField] public int Quantity;          // 보유 수량
    [SerializeField] public int Additional_Data;   // 도구 내구도/기타 상태

    /// <summary>
    /// 새로운 아이템 데이터 생성자
    /// </summary>
    /// <param name="item_id">아이템 ID</param>
    /// <param name="quantity">수량</param>
    /// <param name="additional_data">추가 데이터 (도구 내구도 등)</param>
    public Item_Data(int item_id, int quantity, int additional_data = 0)
    {
        Item_ID = item_id;
        Quantity = quantity;
        Additional_Data = additional_data;
    }

    /// <summary>
    /// 아이템 데이터 동등성 비교 함수
    /// 모든 필드가 같은지 확인
    /// </summary>
    public bool Equals(Item_Data other)
    {
        return Item_ID == other.Item_ID && 
               Quantity == other.Quantity && 
               Additional_Data == other.Additional_Data;
    }

    public override bool Equals(object obj)
    {
        return obj is Item_Data other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Item_ID, Quantity, Additional_Data);
    }

    public static bool operator ==(Item_Data left, Item_Data right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Item_Data left, Item_Data right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 디버깅용 문자열 표현
    /// </summary>
    public override string ToString()
    {
        return $"Item[ID:{Item_ID}, Qty:{Quantity}, Data:{Additional_Data}]";
    }

    /// <summary>
    /// 빈 아이템 데이터 (아무것도 없음을 나타냄)
    /// </summary>
    public static Item_Data Empty => new Item_Data(0, 0, 0);

    /// <summary>
    /// 유효한 아이템인지 확인 (ID > 0이고 수량 > 0)
    /// </summary>
    public bool Is_Valid()
    {
        return Item_ID > 0 && Quantity > 0;
    }

    /// <summary>
    /// 같은 아이템 종류인지 확인 (ID만 비교, 수량 무시)
    /// </summary>
    public bool Is_Same_Item_Type(Item_Data other)
    {
        return Item_ID == other.Item_ID;
    }
}
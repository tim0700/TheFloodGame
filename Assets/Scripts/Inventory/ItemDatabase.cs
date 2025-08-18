using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    // 싱글톤 인스턴스 (ScriptableObject용)
    private static ItemDatabase _instance;
    
    /// <summary>
    /// ItemDatabase 싱글톤 인스턴스
    /// Resources 폴더에서 자동으로 로드하거나 수동으로 설정 가능
    /// </summary>
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                // Resources 폴더에서 ItemDatabase 찾기
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                
                if (_instance == null)
                {
                    Debug.LogError("[ItemDatabase] Instance를 찾을 수 없습니다. " +
                                 "Resources 폴더에 ItemDatabase ScriptableObject를 배치하거나 " +
                                 "ItemDatabase.SetInstance()를 호출하세요.");
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// ItemDatabase 인스턴스를 수동으로 설정
    /// Inspector에서 참조하거나 코드에서 직접 설정할 때 사용
    /// </summary>
    /// <param name="database">설정할 ItemDatabase 인스턴스</param>
    public static void SetInstance(ItemDatabase database)
    {
        _instance = database;
        
        if (_instance != null)
        {
            Debug.Log($"[ItemDatabase] Instance 설정됨: {_instance.name} ({_instance.items.Count}개 아이템)");
        }
    }
    
    /// <summary>
    /// 인스턴스 초기화 (게임 시작 시 호출)
    /// </summary>
    public static void Initialize()
    {
        if (Instance != null)
        {
            Debug.Log($"[ItemDatabase] 초기화 완료: {Instance.items.Count}개 아이템 로드됨");
        }
    }

    [Header("아이템 데이터베이스")]
    public List<InventoryItem> items = new List<InventoryItem>();

    public InventoryItem GetByID(string id) => items.Find(x => x.itemID == id);
    public InventoryItem GetByName(string name) => items.Find(x => x.itemName == name);
    
    /// <summary>
    /// 디버그용: 모든 아이템 이름 출력
    /// </summary>
    public void PrintAllItems()
    {
        Debug.Log($"[ItemDatabase] 등록된 아이템들:");
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
            {
                Debug.Log($"  {i}: {items[i].itemName} (ID: {items[i].itemID})");
            }
        }
    }
}

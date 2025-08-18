using UnityEngine;
using ResourceSystem;

/// <summary>
/// ResourceSystem과 기존 시스템(InventoryManager, Item_IDs) 간의 변환을 담당하는 유틸리티
/// 
/// Input: ResourceType, ToolType 등 자원 시스템 열거형
/// Output: InventoryItem, Item_IDs 등 기존 시스템 객체
/// Type: Static Utility Class
/// </summary>
public static class ResourceConverter
{
    #region ResourceType ↔ InventoryItem 변환

    /// <summary>
    /// ResourceType을 협업자의 InventoryItem으로 변환
    /// 안전한 ItemDatabase 접근 및 에러 처리 포함
    /// </summary>
    /// <param name="resourceType">변환할 자원 타입</param>
    /// <returns>해당하는 InventoryItem, 실패 시 null</returns>
    public static InventoryItem Convert_To_Inventory_Item(ResourceType resourceType)
    {
        string itemName = Get_Inventory_Item_Name(resourceType);
        
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogError($"[ResourceConverter] 알 수 없는 ResourceType: {resourceType}");
            return null;
        }

        // ItemDatabase 안전한 접근
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("[ResourceConverter] ItemDatabase.Instance가 null입니다. ItemDatabase가 초기화되었는지 확인하세요.");
            return null;
        }

        InventoryItem item = ItemDatabase.Instance.GetByName(itemName);
        
        if (item == null)
        {
            Debug.LogError($"[ResourceConverter] ItemDatabase에서 '{itemName}' 아이템을 찾을 수 없습니다.");
            Debug.LogWarning($"[ResourceConverter] 사용 가능한 아이템들: {Get_Available_Items_Debug_String()}");
        }
        else
        {
            Debug.Log($"[ResourceConverter] 변환 성공: {resourceType} → {itemName}");
        }

        return item;
    }

    /// <summary>
    /// ResourceType에 해당하는 InventoryItem의 이름 반환
    /// 협업자 시스템의 아이템명과 정확히 일치해야 함
    /// </summary>
    private static string Get_Inventory_Item_Name(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Wood => "Wood",         // 협업자 ItemDatabase의 정확한 이름
            ResourceType.Stone => "Stone",       // 협업자 ItemDatabase의 정확한 이름
            ResourceType.IronOre => "Iron Ore",  // 협업자 ItemDatabase의 정확한 이름
            ResourceType.Coal => "Coal",         // 협업자 ItemDatabase의 정확한 이름
            _ => ""
        };
    }

    #endregion

    #region ResourceType ↔ Item_IDs 변환

    /// <summary>
    /// ResourceType을 기존 Item_IDs로 변환 (호환성 유지용)
    /// </summary>
    /// <param name="resourceType">변환할 자원 타입</param>
    /// <returns>해당하는 Item_IDs 상수값</returns>
    public static int Convert_To_Item_ID(ResourceType resourceType)
    {
        int itemId = resourceType switch
        {
            ResourceType.Wood => Item_IDs.Wood,         // 1
            ResourceType.Stone => Item_IDs.Stone,       // 2
            ResourceType.IronOre => Item_IDs.Iron_Ore,  // 3
            ResourceType.Coal => Item_IDs.Coal,         // 4
            _ => 0  // 잘못된 ID
        };

        if (itemId == 0)
        {
            Debug.LogError($"[ResourceConverter] 지원되지 않는 ResourceType: {resourceType}");
        }

        return itemId;
    }

    /// <summary>
    /// Item_IDs를 ResourceType으로 역변환
    /// </summary>
    /// <param name="itemId">변환할 아이템 ID</param>
    /// <returns>해당하는 ResourceType, 실패 시 기본값</returns>
    public static ResourceType Convert_From_Item_ID(int itemId)
    {
        return itemId switch
        {
            Item_IDs.Wood => ResourceType.Wood,
            Item_IDs.Stone => ResourceType.Stone,
            Item_IDs.Iron_Ore => ResourceType.IronOre,
            Item_IDs.Coal => ResourceType.Coal,
            _ => ResourceType.Stone  // 기본값
        };
    }

    #endregion

    #region ToolType ↔ Item_IDs 변환

    /// <summary>
    /// ToolType을 기존 Item_IDs로 변환
    /// </summary>
    /// <param name="toolType">변환할 도구 타입</param>
    /// <returns>해당하는 Item_IDs 상수값, None이면 0</returns>
    public static int Convert_Tool_To_Item_ID(ToolType toolType)
    {
        return toolType switch
        {
            ToolType.None => 0,                                    // 도구 없음
            ToolType.WoodPickaxe => Item_IDs.Wooden_Pickaxe,      // 21
            ToolType.StonePickaxe => Item_IDs.Stone_Pickaxe,      // 22
            ToolType.IronPickaxe => Item_IDs.Iron_Pickaxe,        // 23
            ToolType.SteelPickaxe => Item_IDs.Steel_Pickaxe,      // 24
            _ => 0
        };
    }

    /// <summary>
    /// Item_IDs를 ToolType으로 역변환
    /// </summary>
    /// <param name="itemId">변환할 아이템 ID</param>
    /// <returns>해당하는 ToolType, 실패 시 None</returns>
    public static ToolType Convert_Tool_From_Item_ID(int itemId)
    {
        return itemId switch
        {
            Item_IDs.Wooden_Pickaxe => ToolType.WoodPickaxe,
            Item_IDs.Stone_Pickaxe => ToolType.StonePickaxe,
            Item_IDs.Iron_Pickaxe => ToolType.IronPickaxe,
            Item_IDs.Steel_Pickaxe => ToolType.SteelPickaxe,
            _ => ToolType.None
        };
    }

    #endregion

    #region 표시명 및 유틸리티 함수

    /// <summary>
    /// ResourceType의 한국어 표시명 반환
    /// UI에서 사용자에게 보여줄 때 사용
    /// </summary>
    /// <param name="resourceType">표시명을 얻을 자원 타입</param>
    /// <returns>한국어 표시명</returns>
    public static string Get_Resource_Display_Name(ResourceType resourceType)
    {
        return ResourceDefaults.Get_Resource_Display_Name(resourceType);
    }

    /// <summary>
    /// ToolType의 한국어 표시명 반환
    /// UI에서 사용자에게 보여줄 때 사용
    /// </summary>
    /// <param name="toolType">표시명을 얻을 도구 타입</param>
    /// <returns>한국어 표시명</returns>
    public static string Get_Tool_Display_Name(ToolType toolType)
    {
        return ResourceDefaults.Get_Tool_Display_Name(toolType);
    }

    /// <summary>
    /// 자원 타입이 유효한지 확인
    /// </summary>
    /// <param name="resourceType">확인할 자원 타입</param>
    /// <returns>정의된 ResourceType이면 true</returns>
    public static bool Is_Valid_Resource_Type(ResourceType resourceType)
    {
        return resourceType >= ResourceType.Wood && resourceType <= ResourceType.Coal;
    }

    /// <summary>
    /// 도구 타입이 유효한지 확인
    /// </summary>
    /// <param name="toolType">확인할 도구 타입</param>
    /// <returns>정의된 ToolType이면 true</returns>
    public static bool Is_Valid_Tool_Type(ToolType toolType)
    {
        return toolType >= ToolType.None && toolType <= ToolType.SteelPickaxe;
    }

    #endregion

    #region 디버그 및 검증 함수

    /// <summary>
    /// ItemDatabase에 있는 모든 아이템 목록을 디버그 문자열로 반환
    /// 아이템명 매핑 오류 시 참고용
    /// </summary>
    /// <returns>사용 가능한 아이템 목록 문자열</returns>
    private static string Get_Available_Items_Debug_String()
    {
        if (ItemDatabase.Instance == null || ItemDatabase.Instance.items == null)
        {
            return "ItemDatabase가 초기화되지 않음";
        }

        var items = ItemDatabase.Instance.items;
        if (items.Count == 0)
        {
            return "ItemDatabase가 비어있음";
        }

        string itemList = "";
        for (int i = 0; i < items.Count && i < 10; i++)  // 최대 10개만 표시
        {
            if (items[i] != null)
            {
                itemList += $"'{items[i].itemName}', ";
            }
        }

        return itemList.TrimEnd(',', ' ');
    }

    /// <summary>
    /// 자원 변환 시스템의 정상 동작 여부 검증
    /// 게임 시작 시 호출하여 시스템 무결성 확인
    /// </summary>
    /// <returns>모든 변환이 정상 동작하면 true</returns>
    public static bool Validate_Conversion_System()
    {
        Debug.Log("[ResourceConverter] 변환 시스템 검증 시작...");

        bool allValid = true;

        // ItemDatabase 존재 확인
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("[ResourceConverter] ItemDatabase.Instance가 null입니다!");
            allValid = false;
        }

        // 각 ResourceType별 변환 테스트
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            // InventoryItem 변환 테스트
            InventoryItem inventoryItem = Convert_To_Inventory_Item(resourceType);
            if (inventoryItem == null)
            {
                Debug.LogError($"[ResourceConverter] {resourceType} → InventoryItem 변환 실패");
                allValid = false;
            }

            // Item_IDs 변환 테스트
            int itemId = Convert_To_Item_ID(resourceType);
            if (itemId == 0)
            {
                Debug.LogError($"[ResourceConverter] {resourceType} → Item_IDs 변환 실패");
                allValid = false;
            }

            // 역변환 테스트
            ResourceType convertedBack = Convert_From_Item_ID(itemId);
            if (convertedBack != resourceType)
            {
                Debug.LogError($"[ResourceConverter] {resourceType} 역변환 실패: {convertedBack}");
                allValid = false;
            }
        }

        // 도구 변환 테스트
        foreach (ToolType toolType in System.Enum.GetValues(typeof(ToolType)))
        {
            if (toolType == ToolType.None) continue;

            int toolItemId = Convert_Tool_To_Item_ID(toolType);
            ToolType convertedBackTool = Convert_Tool_From_Item_ID(toolItemId);
            
            if (convertedBackTool != toolType)
            {
                Debug.LogError($"[ResourceConverter] {toolType} 도구 역변환 실패: {convertedBackTool}");
                allValid = false;
            }
        }

        if (allValid)
        {
            Debug.Log("[ResourceConverter] ✅ 변환 시스템 검증 완료 - 모든 변환 정상 동작");
        }
        else
        {
            Debug.LogError("[ResourceConverter] ❌ 변환 시스템 검증 실패 - 일부 변환에 문제 있음");
        }

        return allValid;
    }

    #endregion

    #region 디버그 전용 공개 메서드

    /// <summary>
    /// ResourceType에 해당하는 InventoryItem 이름 반환 (디버그용)
    /// 테스트 스크립트에서 사용
    /// </summary>
    /// <param name="resourceType">확인할 자원 타입</param>
    /// <returns>ItemDatabase에서 찾을 아이템 이름</returns>
    public static string Get_Inventory_Item_Name_Debug(ResourceType resourceType)
    {
        return Get_Inventory_Item_Name(resourceType);
    }

    #endregion
}
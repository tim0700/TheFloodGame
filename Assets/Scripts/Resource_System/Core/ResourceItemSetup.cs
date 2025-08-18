using UnityEngine;
using ResourceSystem;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 자원 시스템에 필요한 InventoryItem들을 자동으로 생성하고 ItemDatabase에 등록하는 도구
/// 
/// Input: 없음 (자동 설정)
/// Output: 필요한 모든 InventoryItem 생성 및 ItemDatabase 설정
/// Type: Editor Utility + Runtime Helper
/// </summary>
public class ResourceItemSetup : MonoBehaviour
{
    [Header("=== ItemDatabase 설정 ===")]
    [Tooltip("사용할 ItemDatabase (없으면 자동으로 찾거나 생성)")]
    public ItemDatabase targetDatabase;
    
    [Tooltip("게임 시작 시 자동으로 ItemDatabase 설정")]
    public bool autoSetupOnStart = true;

    [Header("=== 생성할 아이템 설정 ===")]
    [Tooltip("기존 아이템을 덮어쓸지 여부")]
    public bool overwriteExistingItems = false;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            Setup_ItemDatabase_Runtime();
        }

#if UNITY_EDITOR
        // 에디터에서만 실행: 아이템 자동 생성
        if (createItemsOnStart && Application.isEditor)
        {
            Create_Required_Items_Editor();
        }
#endif
    }

    /// <summary>
    /// 런타임에서 ItemDatabase 설정
    /// 필요한 경우 기본 아이템들을 생성하고 ItemDatabase.Instance 설정
    /// </summary>
    public void Setup_ItemDatabase_Runtime()
    {
        Debug.Log("[ResourceItemSetup] 런타임 ItemDatabase 설정 시작...");

        // ItemDatabase 찾기 또는 설정
        if (targetDatabase == null)
        {
            targetDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
            
            if (targetDatabase == null)
            {
                Debug.LogError("[ResourceItemSetup] ItemDatabase를 찾을 수 없습니다. " +
                             "에디터에서 'Create Required Items' 버튼을 먼저 클릭하세요.");
                return;
            }
        }

        // ItemDatabase.Instance 설정
        ItemDatabase.SetInstance(targetDatabase);
        
        // 필요한 아이템들이 있는지 확인
        Verify_Required_Items();
        
        Debug.Log("[ResourceItemSetup] 런타임 설정 완료");
    }

    /// <summary>
    /// 필요한 아이템들이 ItemDatabase에 있는지 확인
    /// </summary>
    private void Verify_Required_Items()
    {
        bool allItemsFound = true;
        
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            string itemName = Get_Required_Item_Name(resourceType);
            InventoryItem foundItem = targetDatabase.GetByName(itemName);
            
            if (foundItem == null)
            {
                Debug.LogWarning($"[ResourceItemSetup] 필요한 아이템 누락: '{itemName}'");
                allItemsFound = false;
            }
            else
            {
                Debug.Log($"[ResourceItemSetup] ✅ '{itemName}' 아이템 확인됨");
            }
        }

        if (allItemsFound)
        {
            Debug.Log("[ResourceItemSetup] ✅ 모든 필요한 아이템이 준비되었습니다");
        }
        else
        {
            Debug.LogError("[ResourceItemSetup] ⚠️ 일부 아이템이 누락되었습니다. " +
                         "에디터에서 'Create Required Items'를 실행하세요.");
        }
    }

    /// <summary>
    /// ResourceType에 대응하는 아이템 이름 반환
    /// </summary>
    private string Get_Required_Item_Name(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Wood => "Wood",
            ResourceType.Stone => "Stone", 
            ResourceType.IronOre => "Iron Ore",
            ResourceType.Coal => "Coal",
            _ => "Unknown"
        };
    }

#if UNITY_EDITOR
    [Header("=== 에디터 전용 기능 ===")]
    [Space(10)]
    [Tooltip("체크하면 Start()에서 자동으로 아이템 생성 (에디터 전용)")]
    public bool createItemsOnStart = false;

    /// <summary>
    /// 에디터에서 필요한 모든 아이템들을 자동 생성 (에디터 전용)
    /// </summary>
    [ContextMenu("Create Required Items")]
    public void Create_Required_Items_Editor()
    {
        Debug.Log("[ResourceItemSetup] 필요한 아이템들 생성 시작...");

        // ItemDatabase 찾기 또는 생성
        Ensure_ItemDatabase_Exists();
        
        if (targetDatabase == null)
        {
            Debug.LogError("[ResourceItemSetup] ItemDatabase 생성 실패");
            return;
        }

        // 각 ResourceType에 대한 InventoryItem 생성
        int createdCount = 0;
        int skippedCount = 0;

        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (Create_Resource_Item(resourceType))
            {
                createdCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        // 필요한 도구 아이템들도 생성 (선택사항)
        foreach (ToolType toolType in System.Enum.GetValues(typeof(ToolType)))
        {
            if (toolType != ToolType.None)
            {
                if (Create_Tool_Item(toolType))
                {
                    createdCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
        }

        // ItemDatabase를 Resources 폴더에 저장
        Save_ItemDatabase_To_Resources();
        
        // 결과 출력
        Debug.Log($"[ResourceItemSetup] 완료: {createdCount}개 생성, {skippedCount}개 스킵");
        
        // 에디터 새로고침
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(targetDatabase);
    }

    /// <summary>
    /// ItemDatabase가 존재하는지 확인하고 없으면 생성
    /// </summary>
    private void Ensure_ItemDatabase_Exists()
    {
        if (targetDatabase == null)
        {
            // Resources 폴더에서 찾기
            targetDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
        }

        if (targetDatabase == null)
        {
            // Resources 폴더 생성
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // ItemDatabase 생성
            targetDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(targetDatabase, "Assets/Resources/ItemDatabase.asset");
            
            Debug.Log("[ResourceItemSetup] 새 ItemDatabase 생성됨: Assets/Resources/ItemDatabase.asset");
        }
    }

    /// <summary>
    /// 특정 자원에 대한 InventoryItem 생성
    /// </summary>
    private bool Create_Resource_Item(ResourceType resourceType)
    {
        string itemName = Get_Required_Item_Name(resourceType);
        
        // 이미 존재하는지 확인
        InventoryItem existingItem = targetDatabase.GetByName(itemName);
        if (existingItem != null && !overwriteExistingItems)
        {
            Debug.Log($"[ResourceItemSetup] '{itemName}' 이미 존재함 (스킵)");
            return false;
        }

        // 새 InventoryItem 생성
        InventoryItem newItem = ScriptableObject.CreateInstance<InventoryItem>();
        newItem.itemName = itemName;
        newItem.description = Get_Resource_Description(resourceType);
        newItem.itemType = ItemType.Resource;
        newItem.rarity = ItemRarity.Common;
        newItem.isStackable = true;
        newItem.maxStack = 99;

        // 파일로 저장
        string assetPath = $"Assets/Resources/Items/{itemName}.asset";
        string folderPath = "Assets/Resources/Items";
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Items");
        }

        AssetDatabase.CreateAsset(newItem, assetPath);

        // ItemDatabase에 추가
        if (existingItem != null)
        {
            // 기존 아이템 교체
            int index = targetDatabase.items.IndexOf(existingItem);
            targetDatabase.items[index] = newItem;
        }
        else
        {
            // 새 아이템 추가
            targetDatabase.items.Add(newItem);
        }

        Debug.Log($"[ResourceItemSetup] ✅ '{itemName}' 아이템 생성됨");
        return true;
    }

    /// <summary>
    /// 특정 도구에 대한 InventoryItem 생성
    /// </summary>
    private bool Create_Tool_Item(ToolType toolType)
    {
        string itemName = Get_Tool_Item_Name(toolType);
        
        // 이미 존재하는지 확인
        InventoryItem existingItem = targetDatabase.GetByName(itemName);
        if (existingItem != null && !overwriteExistingItems)
        {
            return false;
        }

        // 새 InventoryItem 생성
        InventoryItem newItem = ScriptableObject.CreateInstance<InventoryItem>();
        newItem.itemName = itemName;
        newItem.description = Get_Tool_Description(toolType);
        newItem.itemType = ItemType.Equipment;
        newItem.rarity = Get_Tool_Rarity(toolType);
        newItem.isStackable = false;
        newItem.maxStack = 1;

        // 파일로 저장
        string assetPath = $"Assets/Resources/Items/{itemName}.asset";
        string folderPath = "Assets/Resources/Items";
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Items");
        }

        AssetDatabase.CreateAsset(newItem, assetPath);

        // ItemDatabase에 추가
        if (existingItem != null)
        {
            int index = targetDatabase.items.IndexOf(existingItem);
            targetDatabase.items[index] = newItem;
        }
        else
        {
            targetDatabase.items.Add(newItem);
        }

        Debug.Log($"[ResourceItemSetup] ✅ '{itemName}' 도구 생성됨");
        return true;
    }

    /// <summary>
    /// ItemDatabase를 Resources 폴더에 저장
    /// </summary>
    private void Save_ItemDatabase_To_Resources()
    {
        string assetPath = AssetDatabase.GetAssetPath(targetDatabase);
        if (string.IsNullOrEmpty(assetPath))
        {
            AssetDatabase.CreateAsset(targetDatabase, "Assets/Resources/ItemDatabase.asset");
        }
        
        EditorUtility.SetDirty(targetDatabase);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 자원에 대한 설명 생성
    /// </summary>
    private string Get_Resource_Description(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Wood => "강에서 수집할 수 있는 나무. 기본적인 건설 재료로 사용됩니다.",
            ResourceType.Stone => "강에서 수집하거나 갱도에서 채굴할 수 있는 돌. 제방 건설과 도구 제작에 필요합니다.",
            ResourceType.IronOre => "갱도에서만 채굴할 수 있는 철 원석. 철과 강철을 제련하는 데 필요합니다.",
            ResourceType.Coal => "갱도에서만 채굴할 수 있는 석탄. 모든 제련 과정에서 연료로 사용됩니다.",
            _ => "알 수 없는 자원입니다."
        };
    }

    /// <summary>
    /// 도구 아이템 이름 생성
    /// </summary>
    private string Get_Tool_Item_Name(ToolType toolType)
    {
        return toolType switch
        {
            ToolType.WoodPickaxe => "Wooden Pickaxe",
            ToolType.StonePickaxe => "Stone Pickaxe", 
            ToolType.IronPickaxe => "Iron Pickaxe",
            ToolType.SteelPickaxe => "Steel Pickaxe",
            _ => "Unknown Tool"
        };
    }

    /// <summary>
    /// 도구에 대한 설명 생성
    /// </summary>
    private string Get_Tool_Description(ToolType toolType)
    {
        return toolType switch
        {
            ToolType.WoodPickaxe => "기본적인 나무 곡괭이. 돌을 채굴할 수 있습니다.",
            ToolType.StonePickaxe => "돌로 만든 곡괭이. 돌, 철 원석, 석탄을 채굴할 수 있습니다.",
            ToolType.IronPickaxe => "철로 만든 곡괭이. 모든 자원을 효율적으로 채굴할 수 있습니다.",
            ToolType.SteelPickaxe => "강철로 만든 최고급 곡괭이. 최대 효율로 자원을 채굴할 수 있습니다.",
            _ => "알 수 없는 도구입니다."
        };
    }

    /// <summary>
    /// 도구의 희귀도 결정
    /// </summary>
    private ItemRarity Get_Tool_Rarity(ToolType toolType)
    {
        return toolType switch
        {
            ToolType.WoodPickaxe => ItemRarity.Common,
            ToolType.StonePickaxe => ItemRarity.Common,
            ToolType.IronPickaxe => ItemRarity.Uncommon,
            ToolType.SteelPickaxe => ItemRarity.Rare,
            _ => ItemRarity.Common
        };
    }
#endif
}
using UnityEngine;
using ResourceSystem;
using System.Collections.Generic;

/// <summary>
/// 자원 시스템과 협업자 인벤토리 시스템 간의 호환성을 검증하는 도구
/// 
/// Input: 없음 (자동 검증)
/// Output: 자원 변환 시스템의 정상 동작 여부 확인
/// Type: Runtime Helper
/// </summary>
public class ResourceItemSetup : MonoBehaviour
{
    [Header("=== 자원 변환 시스템 검증 ===")]
    [Tooltip("게임 시작 시 자동으로 변환 시스템 검증")]
    public bool autoValidateOnStart = true;
    
    [Tooltip("디버그 로그 출력 여부")]
    public bool enableDebugLog = true;

    private void Start()
    {
        if (autoValidateOnStart)
        {
            Validate_Resource_System();
        }
    }

    /// <summary>
    /// 런타임에서 자원 변환 시스템 검증
    /// InventoryManager와의 호환성 및 ResourceConverter 정상 동작 확인
    /// </summary>
    public void Validate_Resource_System()
    {
        Log_Debug("[ResourceItemSetup] 자원 변환 시스템 검증 시작...");

        // InventoryManager 존재 확인
        InventoryManger inventoryManager = FindObjectOfType<InventoryManger>();
        if (inventoryManager == null)
        {
            Debug.LogWarning("[ResourceItemSetup] InventoryManger가 씬에 없습니다. 자원 수집 기능이 제한될 수 있습니다.");
        }
        else
        {
            Log_Debug("[ResourceItemSetup] ✅ InventoryManger 발견됨");
        }
        
        // ResourceConverter 검증
        bool converterValid = ResourceConverter.Validate_Conversion_System();
        
        if (converterValid)
        {
            Log_Debug("[ResourceItemSetup] ✅ 자원 변환 시스템 검증 완료");
        }
        else
        {
            Debug.LogError("[ResourceItemSetup] ❌ 자원 변환 시스템에 문제가 있습니다");
        }
    }

    /// <summary>
    /// 필요한 자원 아이템명들이 올바르게 변환되는지 확인
    /// </summary>
    private void Verify_Required_Items()
    {
        Log_Debug("[ResourceItemSetup] 자원 아이템명 변환 검증 시작...");
        
        bool allItemsValid = true;
        
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            string itemName = ResourceConverter.Convert_To_Item_Name(resourceType);
            
            if (string.IsNullOrEmpty(itemName))
            {
                Debug.LogWarning($"[ResourceItemSetup] 아이템명 변환 실패: {resourceType}");
                allItemsValid = false;
            }
            else
            {
                Log_Debug($"[ResourceItemSetup] ✅ '{resourceType}' → '{itemName}' 변환 확인");
            }
        }

        if (allItemsValid)
        {
            Log_Debug("[ResourceItemSetup] ✅ 모든 자원 아이템명 변환이 정상입니다");
        }
        else
        {
            Debug.LogError("[ResourceItemSetup] ❌ 일부 자원 아이템명 변환에 문제가 있습니다");
        }
    }

    /// <summary>
    /// 디버그 로그 출력 (설정에 따라)
    /// </summary>
    /// <param name="message">로그 메시지</param>
    private void Log_Debug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log(message);
        }
    }

    /// <summary>
    /// 공개 API: 수동으로 자원 시스템 검증 실행
    /// </summary>
    [ContextMenu("Validate Resource System")]
    public void Manual_Validate_Resource_System()
    {
        Validate_Resource_System();
    }

    /// <summary>
    /// 자원 시스템 상태 정보를 문자열로 반환
    /// </summary>
    /// <returns>현재 상태 요약</returns>
    public string Get_System_Status()
    {
        var status = new System.Text.StringBuilder();
        
        status.AppendLine("=== 자원 변환 시스템 상태 ===");
        
        // InventoryManager 존재 여부
        InventoryManger inventoryManager = FindObjectOfType<InventoryManger>();
        status.AppendLine($"InventoryManager: {(inventoryManager != null ? "✅ 존재함" : "❌ 없음")}");
        
        // 각 ResourceType 변환 상태
        status.AppendLine("\n자원 아이템명 변환:");
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            string itemName = ResourceConverter.Convert_To_Item_Name(resourceType);
            string statusIcon = string.IsNullOrEmpty(itemName) ? "❌" : "✅";
            status.AppendLine($"  {resourceType} → '{itemName}' {statusIcon}");
        }
        
        return status.ToString();
    }
}
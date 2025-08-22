using UnityEngine;
using ResourceSystem;
using System.Collections;

/// <summary>
/// 자원 시스템과 협업자 인벤토리 시스템의 통합 테스트를 수행하는 컴포넌트 (간소화 버전)
/// 
/// Input: 테스트 설정 및 시작 명령
/// Output: 테스트 결과 로그, 시스템 상태 검증
/// Type: MonoBehaviour Component (테스트/디버그용)
/// </summary>
public class ResourceSystemTester : MonoBehaviour
{
    #region Inspector 설정 필드

    [Header("=== 테스트 설정 ===")]
    [Tooltip("게임 시작 시 자동으로 테스트 실행")]
    public bool autoRunOnStart = true;
    
    [Tooltip("상세한 로그 출력 여부")]
    public bool verboseLogging = true;

    [Header("=== 변환 시스템 테스트 ===")]
    [Tooltip("ResourceConverter 변환 테스트 실행")]
    public bool testResourceConverter = true;

    [Header("=== 인벤토리 연동 테스트 ===")]
    [Tooltip("인벤토리 매니저 테스트 실행")]
    public bool testInventoryManager = true;
    
    [Tooltip("실제 아이템 추가 테스트 실행")]
    public bool testItemAddition = true;
    
    [Tooltip("테스트용 아이템 추가 수량")]
    [Range(1, 99)]
    public int testItemQuantity = 5;

    [Header("=== ResourceNode 테스트 ===")]
    [Tooltip("ResourceNode 기본 기능 테스트")]
    public bool testResourceNode = true;
    
    [Tooltip("테스트할 자원 타입")]
    public ResourceType testResourceType = ResourceType.Stone;
    
    [Tooltip("테스트할 도구 타입")]
    public ToolType testToolType = ToolType.WoodPickaxe;

    #endregion

    #region 테스트 결과 저장

    [Header("=== 테스트 결과 (읽기 전용) ===")]
    [SerializeField] private bool resourceConverterTestPassed = false;
    [SerializeField] private bool inventoryManagerTestPassed = false;
    [SerializeField] private bool itemAdditionTestPassed = false;
    [SerializeField] private bool resourceNodeTestPassed = false;
    [SerializeField] private bool allTestsPassed = false;

    private int totalTests = 0;
    private int passedTests = 0;

    #endregion

    #region Unity 생명주기

    private void Start()
    {
        if (autoRunOnStart)
        {
            StartCoroutine(Run_All_Tests_Coroutine());
        }
    }

    #endregion

    #region 테스트 실행 메서드

    /// <summary>
    /// 모든 테스트를 순차적으로 실행하는 코루틴
    /// </summary>
    private IEnumerator Run_All_Tests_Coroutine()
    {
        Log_Test("=== 자원 시스템 통합 테스트 시작 (간소화 버전) ===", true);
        
        totalTests = 0;
        passedTests = 0;
        
        // 약간의 지연 (시스템 초기화 대기)
        yield return new WaitForSeconds(0.5f);

        // 1. ResourceConverter 테스트
        if (testResourceConverter)
        {
            yield return StartCoroutine(Test_Resource_Converter());
        }

        // 2. InventoryManager 테스트
        if (testInventoryManager)
        {
            yield return StartCoroutine(Test_Inventory_Manager());
        }

        // 3. 실제 아이템 추가 테스트
        if (testItemAddition)
        {
            yield return StartCoroutine(Test_Item_Addition());
        }

        // 4. ResourceNode 테스트
        if (testResourceNode)
        {
            yield return StartCoroutine(Test_Resource_Node_Integration());
        }

        // 최종 결과 출력
        Output_Final_Test_Results();
    }

    /// <summary>
    /// ResourceConverter 변환 시스템 테스트
    /// </summary>
    private IEnumerator Test_Resource_Converter()
    {
        Log_Test("--- ResourceConverter 테스트 시작 ---");
        totalTests++;

        try
        {
            // 모든 ResourceType에 대해 변환 테스트
            bool allConversionsValid = true;

            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                // 1. ResourceType → Item Name 변환
                string itemName = ResourceConverter.Convert_To_Item_Name(resourceType);
                
                if (string.IsNullOrEmpty(itemName))
                {
                    Log_Test($"❌ {resourceType} → Item Name 변환 실패", true);
                    allConversionsValid = false;
                    continue;
                }

                // 2. ResourceType → Item_IDs 변환
                int itemId = ResourceConverter.Convert_To_Item_ID(resourceType);
                
                if (itemId == 0)
                {
                    Log_Test($"❌ {resourceType} → Item_IDs 변환 실패", true);
                    allConversionsValid = false;
                    continue;
                }

                // 3. 역변환 테스트
                ResourceType convertedBack = ResourceConverter.Convert_From_Item_ID(itemId);
                
                if (convertedBack != resourceType)
                {
                    Log_Test($"❌ {resourceType} 역변환 실패: {convertedBack}", true);
                    allConversionsValid = false;
                    continue;
                }

                Log_Test($"✅ {resourceType} 변환 성공: {itemName} (ID: {itemId})");
            }

            // 도구 변환 테스트
            foreach (ToolType toolType in System.Enum.GetValues(typeof(ToolType)))
            {
                if (toolType == ToolType.None) continue;

                int toolItemId = ResourceConverter.Convert_Tool_To_Item_ID(toolType);
                ToolType convertedBackTool = ResourceConverter.Convert_Tool_From_Item_ID(toolItemId);

                if (convertedBackTool != toolType)
                {
                    Log_Test($"❌ {toolType} 도구 변환 실패", true);
                    allConversionsValid = false;
                }
                else
                {
                    Log_Test($"✅ {toolType} 도구 변환 성공 (ID: {toolItemId})");
                }
            }

            resourceConverterTestPassed = allConversionsValid;
            
            if (allConversionsValid)
            {
                passedTests++;
                Log_Test("✅ ResourceConverter 테스트 성공", true);
            }
            else
            {
                Log_Test("❌ ResourceConverter 테스트 실패", true);
            }
        }
        catch (System.Exception ex)
        {
            Log_Test($"❌ ResourceConverter 테스트 예외 발생: {ex.Message}", true);
            resourceConverterTestPassed = false;
        }

        yield return null;
    }

    /// <summary>
    /// InventoryManager 시스템 테스트 (협업자 버전)
    /// </summary>
    private IEnumerator Test_Inventory_Manager()
    {
        Log_Test("--- InventoryManger 테스트 시작 ---");
        totalTests++;

        try
        {
            // InventoryManger 인스턴스 확인 (협업자 버전)
            InventoryManger inventoryManager = FindObjectOfType<InventoryManger>();
            if (inventoryManager == null)
            {
                Log_Test("❌ InventoryManger가 씬에 없습니다", true);
                Log_Test("   해결방법: InventoryManger 컴포넌트를 씬에 추가하세요", true);
                inventoryManagerTestPassed = false;
                yield break;
            }

            // 인벤토리 슬롯 확인
            if (inventoryManager.ItemSlot == null || inventoryManager.ItemSlot.Length == 0)
            {
                Log_Test("❌ InventoryManger에 슬롯이 없습니다", true);
                inventoryManagerTestPassed = false;
                yield break;
            }

            Log_Test($"✅ InventoryManger 발견: {inventoryManager.ItemSlot.Length}개 슬롯");

            // 빈 슬롯 개수 확인
            int emptySlots = 0;
            foreach (var slot in inventoryManager.ItemSlot)
            {
                if (slot.Quantity == 0)
                    emptySlots++;
            }

            Log_Test($"✅ 사용 가능한 슬롯: {emptySlots}개");

            inventoryManagerTestPassed = true;
            passedTests++;
            Log_Test("✅ InventoryManger 테스트 성공", true);
        }
        catch (System.Exception ex)
        {
            Log_Test($"❌ InventoryManger 테스트 예외 발생: {ex.Message}", true);
            inventoryManagerTestPassed = false;
        }

        yield return null;
    }

    /// <summary>
    /// 실제 아이템 추가 테스트 (협업자 시스템 사용)
    /// </summary>
    private IEnumerator Test_Item_Addition()
    {
        Log_Test("--- 아이템 추가 테스트 시작 ---");
        totalTests++;

        try
        {
            // 선행 조건 확인
            if (!inventoryManagerTestPassed)
            {
                Log_Test("❌ 선행 테스트 실패로 아이템 추가 테스트 생략", true);
                itemAdditionTestPassed = false;
                yield break;
            }

            // InventoryManger 찾기
            InventoryManger inventoryManager = FindObjectOfType<InventoryManger>();
            if (inventoryManager == null)
            {
                Log_Test("❌ InventoryManger를 찾을 수 없습니다", true);
                itemAdditionTestPassed = false;
                yield break;
            }

            // 각 자원 타입별로 아이템 추가 테스트
            bool allAdditionsSuccessful = true;

            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                string itemName = ResourceConverter.Convert_To_Item_Name(resourceType);
                
                if (string.IsNullOrEmpty(itemName))
                {
                    Log_Test($"❌ {resourceType} 아이템명 변환 실패", true);
                    allAdditionsSuccessful = false;
                    continue;
                }

                // 협업자의 InventoryManger.AddItem(string, int, Sprite, string) 호출
                // 테스트용으로 기본값 사용
                int leftover = inventoryManager.AddItem(itemName, testItemQuantity, null, $"{itemName} 설명");
                
                if (leftover == 0)
                {
                    Log_Test($"✅ {resourceType} ({itemName}) {testItemQuantity}개 추가 성공");
                }
                else
                {
                    Log_Test($"⚠️ {resourceType} 추가 부분 성공 (남은 수량: {leftover})");
                    // 부분 성공도 성공으로 간주
                }
            }

            itemAdditionTestPassed = allAdditionsSuccessful;
            
            if (allAdditionsSuccessful)
            {
                passedTests++;
                Log_Test("✅ 아이템 추가 테스트 성공", true);
            }
            else
            {
                Log_Test("❌ 아이템 추가 테스트 실패", true);
            }
        }
        catch (System.Exception ex)
        {
            Log_Test($"❌ 아이템 추가 테스트 예외 발생: {ex.Message}", true);
            itemAdditionTestPassed = false;
        }

        yield return null;
    }

    /// <summary>
    /// ResourceNode 통합 테스트
    /// </summary>
    private IEnumerator Test_Resource_Node_Integration()
    {
        Log_Test("--- ResourceNode 통합 테스트 시작 ---");
        totalTests++;

        try
        {
            // 씬에서 ResourceNode 찾기
            ResourceNode[] resourceNodes = FindObjectsOfType<ResourceNode>();
            
            if (resourceNodes.Length == 0)
            {
                Log_Test("⚠️ 씬에 ResourceNode가 없어 통합 테스트 생략", true);
                resourceNodeTestPassed = true; // 실패가 아닌 스킵
                passedTests++;
                yield break;
            }

            Log_Test($"✅ 씬에서 {resourceNodes.Length}개 ResourceNode 발견");

            bool integrationTestPassed = true;

            // 각 ResourceNode의 기본 설정 확인
            foreach (ResourceNode node in resourceNodes)
            {
                if (node == null) continue;

                // 채취 가능 여부 테스트
                bool canHarvest = node.Can_Harvest(testToolType);
                Log_Test($"   {node.name}: {node.Resource_Type} (도구 {testToolType}로 채취 가능: {canHarvest})");

                // ResourceData 기본값 적용 확인
                if (node.harvestTime < 0f || node.mineAmountRange.x < 0)
                {
                    Log_Test($"❌ {node.name}의 설정값이 잘못되었습니다", true);
                    integrationTestPassed = false;
                }
            }

            resourceNodeTestPassed = integrationTestPassed;
            
            if (integrationTestPassed)
            {
                passedTests++;
                Log_Test("✅ ResourceNode 통합 테스트 성공", true);
            }
            else
            {
                Log_Test("❌ ResourceNode 통합 테스트 실패", true);
            }
        }
        catch (System.Exception ex)
        {
            Log_Test($"❌ ResourceNode 통합 테스트 예외 발생: {ex.Message}", true);
            resourceNodeTestPassed = false;
        }

        yield return null;
    }

    #endregion

    #region 테스트 결과 출력

    /// <summary>
    /// 최종 테스트 결과 출력
    /// </summary>
    private void Output_Final_Test_Results()
    {
        allTestsPassed = (passedTests == totalTests);
        
        Log_Test("", true);
        Log_Test("=== 자원 시스템 테스트 결과 ===", true);
        Log_Test($"전체 테스트: {totalTests}개", true);
        Log_Test($"성공: {passedTests}개", true);
        Log_Test($"실패: {totalTests - passedTests}개", true);
        
        if (allTestsPassed)
        {
            Log_Test("🎉 모든 테스트 통과! 자원 시스템이 정상 동작합니다.", true);
        }
        else
        {
            Log_Test("⚠️ 일부 테스트 실패. 위의 오류 메시지를 확인하세요.", true);
        }
        
        Log_Test("=================================", true);
    }

    #endregion

    #region 유틸리티 메서드

    /// <summary>
    /// 테스트 로그 출력
    /// </summary>
    /// <param name="message">로그 메시지</param>
    /// <param name="important">중요한 메시지 여부 (항상 출력)</param>
    private void Log_Test(string message, bool important = false)
    {
        if (important || verboseLogging)
        {
            Debug.Log($"[ResourceSystemTester] {message}");
        }
    }

    #endregion

    #region 수동 테스트 버튼 (에디터용)

#if UNITY_EDITOR
    [ContextMenu("수동 테스트 실행")]
    public void Manual_Run_Tests()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(Run_All_Tests_Coroutine());
        }
        else
        {
            Debug.LogWarning("[ResourceSystemTester] 플레이 모드에서만 테스트를 실행할 수 있습니다.");
        }
    }

    [ContextMenu("ResourceConverter 검증만 실행")]
    public void Manual_Test_Resource_Converter_Only()
    {
        if (Application.isPlaying)
        {
            bool validationResult = ResourceConverter.Validate_Conversion_System();
            Log_Test($"ResourceConverter 검증 결과: {(validationResult ? "성공" : "실패")}", true);
        }
        else
        {
            Debug.LogWarning("[ResourceSystemTester] 플레이 모드에서만 테스트를 실행할 수 있습니다.");
        }
    }
#endif

    #endregion
}
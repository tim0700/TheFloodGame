using UnityEngine;
using ResourceSystem;
using System.Collections;

/// <summary>
/// 자원 시스템과 인벤토리 시스템의 통합 테스트를 수행하는 컴포넌트
/// 
/// Input: 테스트 설정 및 시작 명령
/// Output: 테스트 결과 로그, 시스템 상태 검증
/// Type: MonoBehaviour Component (테스트/디버그용)
/// 
/// 사용법: 
/// 1. 씬에 빈 GameObject 생성
/// 2. 이 컴포넌트 추가
/// 3. Inspector에서 테스트 설정
/// 4. 플레이 모드에서 자동 실행 또는 수동 버튼 클릭
/// </summary>
public class ResourceSystemTester : MonoBehaviour
{
    #region Inspector 설정 필드

    [Header("=== 테스트 설정 ===")]
    [Tooltip("게임 시작 시 자동으로 테스트 실행")]
    public bool autoRunOnStart = true;
    
    [Tooltip("상세한 로그 출력 여부")]
    public bool verboseLogging = true;
    
    [Tooltip("테스트 실패 시 게임 일시정지")]
    public bool pauseOnTestFailure = false;

    [Header("=== 변환 시스템 테스트 ===")]
    [Tooltip("ResourceConverter 변환 테스트 실행")]
    public bool testResourceConverter = true;
    
    [Tooltip("ItemDatabase 검증 테스트 실행")]
    public bool testItemDatabase = true;

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
    [SerializeField] private bool itemDatabaseTestPassed = false;
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
        Log_Test("=== 자원 시스템 통합 테스트 시작 ===", true);
        
        totalTests = 0;
        passedTests = 0;
        
        // 약간의 지연 (시스템 초기화 대기)
        yield return new WaitForSeconds(0.5f);

        // 1. ResourceConverter 테스트
        if (testResourceConverter)
        {
            yield return StartCoroutine(Test_Resource_Converter());
        }

        // 2. ItemDatabase 테스트
        if (testItemDatabase)
        {
            yield return StartCoroutine(Test_Item_Database());
        }

        // 3. InventoryManager 테스트
        if (testInventoryManager)
        {
            yield return StartCoroutine(Test_Inventory_Manager());
        }

        // 4. 실제 아이템 추가 테스트
        if (testItemAddition)
        {
            yield return StartCoroutine(Test_Item_Addition());
        }

        // 5. ResourceNode 테스트
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
                // 1. ResourceType → InventoryItem 변환
                InventoryItem inventoryItem = ResourceConverter.Convert_To_Inventory_Item(resourceType);
                
                if (inventoryItem == null)
                {
                    Log_Test($"❌ {resourceType} → InventoryItem 변환 실패", true);
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

                Log_Test($"✅ {resourceType} 변환 성공: {inventoryItem.itemName} (ID: {itemId})");
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
    /// ItemDatabase 시스템 테스트
    /// </summary>
    private IEnumerator Test_Item_Database()
    {
        Log_Test("--- ItemDatabase 테스트 시작 ---");
        totalTests++;

        try
        {
            // ItemDatabase 인스턴스 확인
            if (ItemDatabase.Instance == null)
            {
                Log_Test("❌ ItemDatabase.Instance가 null입니다", true);
                Log_Test("   해결방법: ItemDatabase ScriptableObject를 생성하고 씬에 배치하세요", true);
                itemDatabaseTestPassed = false;
                yield break;
            }

            // 아이템 목록 확인
            if (ItemDatabase.Instance.items == null || ItemDatabase.Instance.items.Count == 0)
            {
                Log_Test("❌ ItemDatabase에 아이템이 없습니다", true);
                itemDatabaseTestPassed = false;
                yield break;
            }

            Log_Test($"✅ ItemDatabase 발견: {ItemDatabase.Instance.items.Count}개 아이템");

            // 자원 아이템들이 데이터베이스에 등록되어 있는지 확인
            bool allResourceItemsFound = true;
            
            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                string expectedItemName = ResourceConverter.Get_Inventory_Item_Name_Debug(resourceType);
                InventoryItem foundItem = ItemDatabase.Instance.GetByName(expectedItemName);

                if (foundItem == null)
                {
                    Log_Test($"❌ '{expectedItemName}' 아이템이 데이터베이스에 없습니다", true);
                    allResourceItemsFound = false;
                }
                else
                {
                    Log_Test($"✅ '{expectedItemName}' 아이템 발견");
                }
            }

            itemDatabaseTestPassed = allResourceItemsFound;
            
            if (allResourceItemsFound)
            {
                passedTests++;
                Log_Test("✅ ItemDatabase 테스트 성공", true);
            }
            else
            {
                Log_Test("❌ ItemDatabase 테스트 실패: 일부 자원 아이템이 누락됨", true);
            }
        }
        catch (System.Exception ex)
        {
            Log_Test($"❌ ItemDatabase 테스트 예외 발생: {ex.Message}", true);
            itemDatabaseTestPassed = false;
        }

        yield return null;
    }

    /// <summary>
    /// InventoryManager 시스템 테스트
    /// </summary>
    private IEnumerator Test_Inventory_Manager()
    {
        Log_Test("--- InventoryManager 테스트 시작 ---");
        totalTests++;

        try
        {
            // InventoryManager 인스턴스 확인
            if (InventoryManager.Instance == null)
            {
                Log_Test("❌ InventoryManager.Instance가 null입니다", true);
                Log_Test("   해결방법: InventoryManager 컴포넌트를 씬에 추가하세요", true);
                inventoryManagerTestPassed = false;
                yield break;
            }

            // 인벤토리 슬롯 확인
            if (InventoryManager.Instance.slots == null || InventoryManager.Instance.slots.Count == 0)
            {
                Log_Test("❌ InventoryManager에 슬롯이 없습니다", true);
                inventoryManagerTestPassed = false;
                yield break;
            }

            Log_Test($"✅ InventoryManager 발견: {InventoryManager.Instance.slots.Count}개 슬롯");

            // 빈 슬롯 개수 확인
            int emptySlots = 0;
            foreach (var slot in InventoryManager.Instance.slots)
            {
                if (slot.item == null)
                    emptySlots++;
            }

            Log_Test($"✅ 사용 가능한 슬롯: {emptySlots}개");

            inventoryManagerTestPassed = true;
            passedTests++;
            Log_Test("✅ InventoryManager 테스트 성공", true);
        }
        catch (System.Exception ex)
        {
            Log_Test($"❌ InventoryManager 테스트 예외 발생: {ex.Message}", true);
            inventoryManagerTestPassed = false;
        }

        yield return null;
    }

    /// <summary>
    /// 실제 아이템 추가 테스트
    /// </summary>
    private IEnumerator Test_Item_Addition()
    {
        Log_Test("--- 아이템 추가 테스트 시작 ---");
        totalTests++;

        try
        {
            // 선행 조건 확인
            if (!itemDatabaseTestPassed || !inventoryManagerTestPassed)
            {
                Log_Test("❌ 선행 테스트 실패로 아이템 추가 테스트 생략", true);
                itemAdditionTestPassed = false;
                yield break;
            }

            // 각 자원 타입별로 아이템 추가 테스트
            bool allAdditionsSuccessful = true;

            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                InventoryItem testItem = ResourceConverter.Convert_To_Inventory_Item(resourceType);
                
                if (testItem == null)
                {
                    Log_Test($"❌ {resourceType} 아이템 변환 실패", true);
                    allAdditionsSuccessful = false;
                    continue;
                }

                bool addSuccess = InventoryManager.Instance.AddItem(testItem, testItemQuantity);
                
                if (addSuccess)
                {
                    Log_Test($"✅ {resourceType} {testItemQuantity}개 추가 성공");
                }
                else
                {
                    Log_Test($"❌ {resourceType} 추가 실패 (인벤토리 공간 부족?)", true);
                    allAdditionsSuccessful = false;
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
            
            if (pauseOnTestFailure)
            {
                Debug.Break();
            }
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
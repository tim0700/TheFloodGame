using System.Collections;
using UnityEngine;
using ResourceSystem;

/// <summary>
/// 게임 월드에 배치되는 자원 노드 컴포넌트
/// 플레이어가 상호작용하여 자원을 채취할 수 있는 오브젝트
/// 
/// Input: 플레이어의 상호작용 (접근, 키 입력, 도구 확인)
/// Output: 자원 아이템, UI 이벤트, 오디오/비주얼 피드백
/// Type: MonoBehaviour Component
/// 
/// 필수 컴포넌트: Collider (IsTrigger = true)
/// 선택 컴포넌트: AudioSource, ParticleSystem
/// </summary>
[RequireComponent(typeof(Collider))]
public class ResourceNode : MonoBehaviour
{
    #region Inspector 설정 필드

    [Header("=== 자원 기본 정보 ===")]
    [Tooltip("이 노드에서 채취할 수 있는 자원 종류")]
    public ResourceType resourceType = ResourceType.Stone;
    
    [Tooltip("채취 방식 (갱도 채굴 / 강물 수집)")]
    public HarvestMethod harvestMethod = HarvestMethod.MineHarvesting;

    [Header("=== 채취 설정 ===")]
    [Tooltip("채취에 걸리는 시간 (초). 0이면 즉시 수집")]
    [Range(0f, 10f)]
    public float harvestTime = 2f;
    
    [Tooltip("채취에 필요한 최소 도구 레벨")]
    public ToolType requiredTool = ToolType.WoodPickaxe;

    [Header("=== 수량 설정 ===")]
    [Tooltip("갱도에서 채취 시 최소-최대 획득량")]
    public Vector2Int mineAmountRange = new Vector2Int(3, 8);
    
    [Tooltip("강물에서 수집 시 획득량 (Phase 2용)")]
    public int riverAmount = 1;

    [Header("=== 상호작용 UI ===")]
    [Tooltip("상호작용 가능할 때 표시할 UI 오브젝트")]
    public GameObject interactionIcon;
    
    [Tooltip("상호작용 안내 텍스트 (없으면 기본 텍스트 사용)")]
    public string customInteractionText = "";

    [Header("=== 비주얼 피드백 ===")]
    [Tooltip("채취 중일 때 재생할 파티클 시스템")]
    public ParticleSystem harvestParticle;
    
    [Tooltip("자원 상태별 모델 (0: 만땅, 1: 반, 2: 거의 없음)")]
    public GameObject[] resourceModels;

    [Header("=== 오디오 피드백 ===")]
    [Tooltip("오디오 소스 (없으면 자동으로 추가)")]
    public AudioSource audioSource;
    
    [Tooltip("채취 시작 소리")]
    public AudioClip harvestStartSound;
    
    [Tooltip("채취 완료 소리")]
    public AudioClip harvestCompleteSound;

    [Header("=== 디버그 정보 ===")]
    [Tooltip("디버그 로그 출력 여부")]
    public bool enableDebugLog = true;
    
    [Tooltip("Gizmo 표시 여부")]
    public bool showGizmo = true;

    #endregion

    #region 내부 상태 변수

    // 현재 상태
    private bool isBeingHarvested = false;
    private bool playerInRange = false;
    private Collider nodeCollider;
    
    // 자원 데이터 캐시
    private ResourceData cachedResourceData;
    
    // 컴포넌트 참조
    private AudioSource internalAudioSource;

    #endregion

    #region 이벤트 시스템

    /// <summary>
    /// 채취 진행률이 변경될 때 호출되는 이벤트 (0.0 ~ 1.0)
    /// UI 진행률 바 업데이트에 사용
    /// </summary>
    public System.Action<float> OnHarvestProgress;
    
    /// <summary>
    /// 채취가 완료될 때 호출되는 이벤트
    /// 매개변수: (자원 타입, 획득 수량)
    /// </summary>
    public System.Action<ResourceType, int> OnHarvestComplete;
    
    /// <summary>
    /// 플레이어에게 메시지를 표시해야 할 때 호출되는 이벤트
    /// 매개변수: (메시지 내용)
    /// </summary>
    public System.Action<string> OnInteractionMessage;
    
    /// <summary>
    /// 상호작용 UI를 업데이트해야 할 때 호출되는 이벤트
    /// 매개변수: (표시 여부, 상호작용 텍스트)
    /// </summary>
    public System.Action<bool, string> OnInteractionUIUpdate;

    #endregion

    #region Unity 생명주기

    private void Start()
    {
        Initialize_Resource_Node();
    }

    private void OnValidate()
    {
        // Inspector에서 값이 변경될 때 자동으로 기본값 적용
        if (Application.isPlaying)
        {
            Apply_Resource_Defaults();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Handle_Player_Enter();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Handle_Player_Exit();
        }
    }

    #endregion

    #region 초기화 및 설정

    /// <summary>
    /// ResourceNode 초기화
    /// 컴포넌트 확인, 기본값 설정, 유효성 검사 수행
    /// </summary>
    private void Initialize_Resource_Node()
    {
        Log_Debug("ResourceNode 초기화 시작...");

        // 필수 컴포넌트 확인
        Validate_Required_Components();
        
        // 자원별 기본값 적용
        Apply_Resource_Defaults();
        
        // 오디오 소스 설정
        Setup_Audio_Source();
        
        // 상호작용 UI 초기 상태 설정
        Setup_Interaction_UI();
        
        // 자원 모델 초기 상태 설정
        Update_Resource_Visual();

        Log_Debug($"ResourceNode 초기화 완료: {resourceType} ({harvestMethod})");
    }

    /// <summary>
    /// 필수 컴포넌트가 올바르게 설정되어 있는지 확인
    /// </summary>
    private void Validate_Required_Components()
    {
        // Collider 확인 및 설정
        nodeCollider = GetComponent<Collider>();
        if (nodeCollider == null)
        {
            Debug.LogError($"[ResourceNode] {gameObject.name}에 Collider가 없습니다!");
            return;
        }

        if (!nodeCollider.isTrigger)
        {
            Debug.LogWarning($"[ResourceNode] {gameObject.name}의 Collider가 Trigger로 설정되지 않았습니다. 자동으로 설정합니다.");
            nodeCollider.isTrigger = true;
        }

        Log_Debug("필수 컴포넌트 검증 완료");
    }

    /// <summary>
    /// 자원 타입에 따른 기본값 적용
    /// Inspector에서 설정되지 않은 값들을 자동으로 설정
    /// </summary>
    private void Apply_Resource_Defaults()
    {
        cachedResourceData = ResourceDefaults.Get_Default_Data(resourceType);
        
        // Inspector에서 기본값(0)으로 설정된 경우만 자동 적용
        if (harvestTime <= 0f && harvestMethod == HarvestMethod.MineHarvesting)
        {
            harvestTime = cachedResourceData.harvestTime;
        }
        
        if (mineAmountRange.x <= 0)
        {
            mineAmountRange = cachedResourceData.mineAmountRange;
        }
        
        if (riverAmount <= 0)
        {
            riverAmount = cachedResourceData.riverAmount;
        }

        Log_Debug($"기본값 적용 완료: 시간={harvestTime}초, 수량={mineAmountRange}개");
    }

    /// <summary>
    /// AudioSource 컴포넌트 설정
    /// </summary>
    private void Setup_Audio_Source()
    {
        if (audioSource == null)
        {
            // AudioSource가 지정되지 않은 경우 컴포넌트에서 찾기
            internalAudioSource = GetComponent<AudioSource>();
            
            if (internalAudioSource == null)
            {
                // AudioSource가 없으면 추가
                internalAudioSource = gameObject.AddComponent<AudioSource>();
                internalAudioSource.playOnAwake = false;
                Log_Debug("AudioSource 자동 추가");
            }
        }
        else
        {
            internalAudioSource = audioSource;
        }
    }

    /// <summary>
    /// 상호작용 UI 초기 설정
    /// </summary>
    private void Setup_Interaction_UI()
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }

    #endregion

    #region 플레이어 상호작용 처리

    /// <summary>
    /// 플레이어가 상호작용 범위에 진입했을 때 처리
    /// </summary>
    private void Handle_Player_Enter()
    {
        playerInRange = true;
        
        if (!isBeingHarvested)
        {
            Show_Interaction_UI();
        }
        
        Log_Debug("플레이어 진입 - 상호작용 UI 표시");
    }

    /// <summary>
    /// 플레이어가 상호작용 범위에서 벗어났을 때 처리
    /// </summary>
    private void Handle_Player_Exit()
    {
        playerInRange = false;
        Hide_Interaction_UI();
        
        Log_Debug("플레이어 이탈 - 상호작용 UI 숨김");
    }

    /// <summary>
    /// 상호작용 UI 표시
    /// </summary>
    private void Show_Interaction_UI()
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(true);
        }
        
        string interactionText = Get_Interaction_Text();
        OnInteractionUIUpdate?.Invoke(true, interactionText);
    }

    /// <summary>
    /// 상호작용 UI 숨김
    /// </summary>
    private void Hide_Interaction_UI()
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
        
        OnInteractionUIUpdate?.Invoke(false, "");
    }

    /// <summary>
    /// 상호작용 텍스트 생성
    /// </summary>
    private string Get_Interaction_Text()
    {
        if (!string.IsNullOrEmpty(customInteractionText))
        {
            return customInteractionText;
        }
        
        string resourceName = ResourceConverter.Get_Resource_Display_Name(resourceType);
        string actionText = harvestMethod == HarvestMethod.RiverCollection ? "수집" : "채취";
        
        return $"E키로 {resourceName} {actionText}";
    }

    #endregion

    #region 채취 가능 여부 검증

    /// <summary>
    /// 플레이어가 현재 이 자원을 채취할 수 있는지 확인
    /// </summary>
    /// <param name="playerTool">플레이어가 현재 장비하고 있는 도구</param>
    /// <returns>채취 가능하면 true</returns>
    public bool Can_Harvest(ToolType playerTool)
    {
        // 이미 채취 중인지 확인
        if (isBeingHarvested)
        {
            Send_Interaction_Message("이미 채취 중입니다!");
            return false;
        }

        // 도구 요구사항 확인
        if (!Is_Tool_Compatible(playerTool))
        {
            string requiredToolName = ResourceConverter.Get_Tool_Display_Name(requiredTool);
            Send_Interaction_Message($"{requiredToolName} 이상의 도구가 필요합니다!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 플레이어의 도구가 이 자원 채취에 적합한지 확인
    /// </summary>
    /// <param name="playerTool">플레이어 도구</param>
    /// <returns>적합하면 true</returns>
    private bool Is_Tool_Compatible(ToolType playerTool)
    {
        // 도구가 필요 없는 경우
        if (requiredTool == ToolType.None)
        {
            return true;
        }

        // 플레이어가 도구를 들고 있지 않은 경우
        if (playerTool == ToolType.None)
        {
            return false;
        }

        // 도구 레벨 비교 (숫자가 클수록 고급 도구)
        return (int)playerTool >= (int)requiredTool;
    }

    #endregion

    #region 유틸리티 및 피드백

    /// <summary>
    /// 자원 모델의 시각적 상태 업데이트
    /// 현재는 기본 상태만 표시, 향후 자원량에 따른 변화 가능
    /// </summary>
    private void Update_Resource_Visual()
    {
        if (resourceModels == null || resourceModels.Length == 0)
            return;

        // 현재는 모든 모델을 활성화 (기본 상태)
        // 향후 자원량에 따라 다른 모델 표시 가능
        for (int i = 0; i < resourceModels.Length; i++)
        {
            if (resourceModels[i] != null)
            {
                resourceModels[i].SetActive(i == 0); // 첫 번째 모델만 활성화
            }
        }
    }

    /// <summary>
    /// 플레이어에게 상호작용 메시지 전송
    /// </summary>
    /// <param name="message">전송할 메시지</param>
    private void Send_Interaction_Message(string message)
    {
        OnInteractionMessage?.Invoke(message);
        Log_Debug($"메시지 전송: {message}");
    }

    /// <summary>
    /// 오디오 재생
    /// </summary>
    /// <param name="clip">재생할 오디오 클립</param>
    private void Play_Audio(AudioClip clip)
    {
        if (internalAudioSource != null && clip != null)
        {
            internalAudioSource.PlayOneShot(clip);
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
            Debug.Log($"[ResourceNode:{gameObject.name}] {message}");
        }
    }

    #endregion

    #region 에디터 지원 (Gizmo)

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        // 상호작용 범위 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
        
        // 자원 타입 표시
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 2f,
            $"{ResourceConverter.Get_Resource_Display_Name(resourceType)}\n" +
            $"수량: {mineAmountRange.x}-{mineAmountRange.y}개\n" +
            $"도구: {ResourceConverter.Get_Tool_Display_Name(requiredTool)}\n" +
            $"시간: {harvestTime}초"
        );
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        // 자원 타입별 색상
        Gizmos.color = resourceType switch
        {
            ResourceType.Wood => Color.green,
            ResourceType.Stone => Color.gray,
            ResourceType.IronOre => Color.red,
            ResourceType.Coal => Color.black,
            _ => Color.white
        };

        // 작은 아이콘 표시
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.2f);
    }
#endif

    #endregion

    #region 공개 API (다음 단계에서 사용)

    /// <summary>
    /// 현재 채취 중인지 여부 반환
    /// </summary>
    public bool Is_Being_Harvested => isBeingHarvested;
    
    /// <summary>
    /// 플레이어가 상호작용 범위 내에 있는지 여부 반환
    /// </summary>
    public bool Is_Player_In_Range => playerInRange;
    
    /// <summary>
    /// 이 노드의 자원 타입 반환
    /// </summary>
    public ResourceType Resource_Type => resourceType;
    
    /// <summary>
    /// 이 노드의 채취 방식 반환
    /// </summary>
    public HarvestMethod Harvest_Method => harvestMethod;

    #endregion

    #region 채취 로직 구현

    /// <summary>
    /// 자원 채취 시작 (공개 API)
    /// PlayerResourceInteraction에서 호출될 메인 채취 함수
    /// </summary>
    /// <param name="playerTool">플레이어가 사용 중인 도구</param>
    /// <returns>채취 시작 성공 여부</returns>
    public bool Start_Harvest(ToolType playerTool)
    {
        // 채취 가능 여부 검증
        if (!Can_Harvest(playerTool))
        {
            return false;
        }

        // 이미 채취 중인 경우 처리
        if (isBeingHarvested)
        {
            Send_Interaction_Message("이미 채취 중입니다!");
            return false;
        }

        Log_Debug($"채취 시작: {resourceType} (도구: {playerTool})");

        // 채취 상태 설정
        isBeingHarvested = true;
        Hide_Interaction_UI();

        // 채취 시작 피드백
        Play_Audio(harvestStartSound);
        Start_Visual_Effects();

        // 채취 코루틴 시작
        StartCoroutine(Harvest_Coroutine());

        return true;
    }

    /// <summary>
    /// 채취 진행을 처리하는 코루틴
    /// 시간 기반 채취 진행률 업데이트 및 완료 처리
    /// </summary>
    private System.Collections.IEnumerator Harvest_Coroutine()
    {
        float elapsedTime = 0f;
        float targetTime = harvestTime;

        // 즉시 수집 처리 (강물에서 수집)
        if (targetTime <= 0f || harvestMethod == HarvestMethod.RiverCollection)
        {
            Complete_Harvest();
            yield break;
        }

        Log_Debug($"채취 진행 시작: {targetTime}초 예상");

        // 채취 진행 루프
        while (elapsedTime < targetTime)
        {
            // 프레임별 시간 누적
            elapsedTime += Time.deltaTime;

            // 진행률 계산 (0.0 ~ 1.0)
            float progress = Mathf.Clamp01(elapsedTime / targetTime);

            // 진행률 이벤트 호출
            OnHarvestProgress?.Invoke(progress);

            // 다음 프레임까지 대기
            yield return null;

            // 채취 중단 확인
            if (!isBeingHarvested)
            {
                Log_Debug("채취가 중단되었습니다");
                yield break;
            }

            // 플레이어가 범위를 벗어난 경우 채취 중단
            if (!playerInRange)
            {
                Send_Interaction_Message("범위를 벗어나 채취가 중단되었습니다!");
                Stop_Harvest();
                yield break;
            }
        }

        // 채취 완료 처리
        Complete_Harvest();
    }

    /// <summary>
    /// 채취 완료 처리
    /// 획득량 계산, 인벤토리 추가, 완료 피드백
    /// </summary>
    private void Complete_Harvest()
    {
        // 획득량 계산
        int harvestAmount = Calculate_Harvest_Amount();

        if (harvestAmount <= 0)
        {
            Send_Interaction_Message("채취에 실패했습니다!");
            Finish_Harvest();
            return;
        }

        // 인벤토리에 아이템 추가
        bool addSuccess = Add_To_Inventory(harvestAmount);

        if (addSuccess)
        {
            // 성공 메시지
            string resourceName = ResourceConverter.Get_Resource_Display_Name(resourceType);
            Send_Interaction_Message($"{resourceName} {harvestAmount}개를 획득했습니다!");

            // 완료 이벤트 호출
            OnHarvestComplete?.Invoke(resourceType, harvestAmount);

            // 완료 피드백
            Play_Audio(harvestCompleteSound);

            Log_Debug($"채취 완료: {resourceType} {harvestAmount}개 획득");
        }
        else
        {
            Send_Interaction_Message("인벤토리가 가득 차서 채취할 수 없습니다!");
        }

        // 채취 종료 처리
        Finish_Harvest();
    }

    /// <summary>
    /// 채취 중단 처리 (외부에서 호출 가능)
    /// </summary>
    public void Stop_Harvest()
    {
        if (!isBeingHarvested)
            return;

        Log_Debug("채취 중단됨");
        StopAllCoroutines(); // 진행 중인 채취 코루틴 중단
        Finish_Harvest();
    }

    /// <summary>
    /// 채취 종료 시 공통 처리
    /// 상태 초기화 및 UI 복원
    /// </summary>
    private void Finish_Harvest()
    {
        // 상태 초기화
        isBeingHarvested = false;

        // 비주얼 효과 중단
        Stop_Visual_Effects();

        // 진행률 초기화
        OnHarvestProgress?.Invoke(0f);

        // 플레이어가 여전히 범위 내에 있다면 상호작용 UI 복원
        if (playerInRange)
        {
            Show_Interaction_UI();
        }

        Log_Debug("채취 종료 처리 완료");
    }

    /// <summary>
    /// 채취량 계산
    /// 자원 타입, 채취 방식, 도구에 따른 획득량 결정
    /// </summary>
    /// <returns>실제 획득할 자원 수량</returns>
    private int Calculate_Harvest_Amount()
    {
        int baseAmount = 0;

        // 채취 방식에 따른 기본 수량
        switch (harvestMethod)
        {
            case HarvestMethod.RiverCollection:
                baseAmount = riverAmount;
                break;

            case HarvestMethod.MineHarvesting:
                // 갱도에서는 범위 내 랜덤 수량
                baseAmount = UnityEngine.Random.Range(mineAmountRange.x, mineAmountRange.y + 1);
                break;
        }

        // 도구 효율성 적용은 향후 확장 가능
        // 현재는 기본 수량만 반환
        return Mathf.Max(0, baseAmount);
    }

    /// <summary>
    /// 인벤토리에 획득한 자원 추가
    /// ResourceConverter를 통해 기존 시스템과 연동
    /// </summary>
    /// <param name="amount">추가할 수량</param>
    /// <returns>추가 성공 여부</returns>
    private bool Add_To_Inventory(int amount)
    {
        try
        {
            // ResourceConverter를 통해 InventoryItem으로 변환
            InventoryItem inventoryItem = ResourceConverter.Convert_To_Inventory_Item(resourceType);

            if (inventoryItem == null)
            {
                Debug.LogError($"[ResourceNode] {resourceType} 변환 실패");
                return false;
            }

            // InventoryManager 안전한 접근
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("[ResourceNode] InventoryManager.Instance가 null입니다");
                return false;
            }

            // 인벤토리에 아이템 추가
            bool success = InventoryManager.Instance.AddItem(inventoryItem, amount);

            if (success)
            {
                Log_Debug($"인벤토리 추가 성공: {inventoryItem.itemName} x{amount}");
            }
            else
            {
                Log_Debug($"인벤토리 추가 실패: 공간 부족 또는 오류");
            }

            return success;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ResourceNode] 인벤토리 추가 중 예외 발생: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 채취 시작 시 비주얼 효과 재생
    /// </summary>
    private void Start_Visual_Effects()
    {
        if (harvestParticle != null)
        {
            harvestParticle.Play();
        }
    }

    /// <summary>
    /// 채취 종료 시 비주얼 효과 중단
    /// </summary>
    private void Stop_Visual_Effects()
    {
        if (harvestParticle != null)
        {
            harvestParticle.Stop();
        }
    }

    #endregion
}
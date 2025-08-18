using System.Collections.Generic;
using UnityEngine;
using ResourceSystem;

/// <summary>
/// 플레이어가 자원 노드와 상호작용하는 시스템을 관리하는 컴포넌트
/// 플레이어 캐릭터에 붙어서 E키 입력을 감지하고 자원 채취를 실행
/// 
/// Input: 플레이어의 키 입력(E키), 근처 자원 노드 감지
/// Output: 자원 채취 실행, UI 메시지, 도구 호환성 검사
/// Type: MonoBehaviour Component
/// 
/// 필수 컴포넌트: None
/// 의존성: ResourceNode, InventoryManager, InputSystem
/// </summary>
public class PlayerResourceInteraction : MonoBehaviour
{
    #region Inspector 설정 필드

    [Header("=== 상호작용 설정 ===")]
    [Tooltip("자원 노드 감지 범위 (미터)")]
    [Range(1f, 10f)]
    public float interactionRange = 3f;
    
    [Tooltip("E키 입력 감지 여부")]
    public bool enableKeyInput = true;
    
    [Tooltip("상호작용 레이어 마스크 (ResourceNode가 있는 레이어)")]
    public LayerMask resourceLayerMask = -1;

    [Header("=== 도구 시스템 ===")]
    [Tooltip("현재 플레이어가 장비한 도구 (테스트용, 향후 인벤토리에서 가져올 예정)")]
    public ToolType currentTool = ToolType.WoodPickaxe;
    
    [Tooltip("도구 자동 감지 여부 (InventoryManager에서 현재 장비 확인)")]
    public bool autoDetectTool = true;

    [Header("=== UI 메시지 ===")]
    [Tooltip("상호작용 메시지를 표시할 시간 (초)")]
    [Range(0.5f, 5f)]
    public float messageDisplayTime = 2f;

    [Header("=== 디버그 ===")]
    [Tooltip("디버그 정보 출력 여부")]
    public bool enableDebugLog = true;
    
    [Tooltip("Gizmo 표시 여부 (상호작용 범위)")]
    public bool showGizmo = true;

    #endregion

    #region 내부 상태 변수

    // 근처 자원 노드들
    private List<ResourceNode> nearbyResourceNodes = new List<ResourceNode>();
    private ResourceNode currentTargetNode = null;
    
    // 상호작용 상태
    private bool isInteracting = false;
    private float lastInteractionTime = 0f;
    
    // 컴포넌트 참조
    private Camera playerCamera;
    
    // 메시지 표시 관련
    private string currentMessage = "";
    private float messageEndTime = 0f;

    #endregion

    #region 이벤트 시스템

    /// <summary>
    /// 플레이어에게 메시지를 표시해야 할 때 호출되는 이벤트
    /// UI 시스템에서 구독하여 메시지 표시
    /// </summary>
    public System.Action<string> OnPlayerMessage;
    
    /// <summary>
    /// 상호작용 가능한 자원 노드가 변경될 때 호출되는 이벤트
    /// 매개변수: (자원 노드, 상호작용 가능 여부)
    /// </summary>
    public System.Action<ResourceNode, bool> OnTargetNodeChanged;
    
    /// <summary>
    /// 자원 채취가 시작될 때 호출되는 이벤트
    /// 매개변수: (자원 타입, 예상 채취 시간)
    /// </summary>
    public System.Action<ResourceType, float> OnHarvestStarted;

    #endregion

    #region Unity 생명주기

    private void Start()
    {
        Initialize_Player_Interaction();
    }

    private void Update()
    {
        // 매 프레임 근처 자원 노드 감지
        Update_Nearby_Resource_Nodes();
        
        // 상호작용 중이면 범위 체크
        if (isInteracting)
        {
            Check_Interaction_Range();
        }
        
        // 가장 가까운 상호작용 대상 결정
        Update_Target_Node();
        
        // 키 입력 처리
        if (enableKeyInput)
        {
            Handle_Input();
        }
        
        // 메시지 표시 시간 관리
        Update_Message_Display();
    }

    #endregion

    #region 초기화

    /// <summary>
    /// PlayerResourceInteraction 초기화
    /// 필요한 컴포넌트 참조 및 설정 확인
    /// </summary>
    private void Initialize_Player_Interaction()
    {
        Log_Debug("PlayerResourceInteraction 초기화 시작...");

        // 카메라 참조 획득
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogWarning("[PlayerResourceInteraction] 플레이어 카메라를 찾을 수 없습니다. 상호작용 기능이 제한될 수 있습니다.");
            }
        }

        // 자원 노드 이벤트 구독 설정
        Setup_Resource_Node_Events();
        
        Log_Debug($"초기화 완료 - 상호작용 범위: {interactionRange}m, 현재 도구: {currentTool}");
    }

    /// <summary>
    /// 자원 노드 이벤트 구독 설정
    /// </summary>
    private void Setup_Resource_Node_Events()
    {
        // 기존 이벤트 구독 해제 (중복 방지)
        Unsubscribe_Resource_Node_Events();
        
        // 현재 활성화된 모든 ResourceNode 찾기
        ResourceNode[] allResourceNodes = FindObjectsOfType<ResourceNode>();
        
        foreach (ResourceNode node in allResourceNodes)
        {
            if (node != null)
            {
                // 메시지 이벤트 구독
                node.OnInteractionMessage += Handle_Resource_Node_Message;
                
                // 채취 완료 이벤트 구독
                node.OnHarvestComplete += Handle_Harvest_Complete;
            }
        }
        
        Log_Debug($"자원 노드 이벤트 구독 완료: {allResourceNodes.Length}개 노드");
    }

    /// <summary>
    /// 자원 노드 이벤트 구독 해제
    /// </summary>
    private void Unsubscribe_Resource_Node_Events()
    {
        ResourceNode[] allResourceNodes = FindObjectsOfType<ResourceNode>();
        
        foreach (ResourceNode node in allResourceNodes)
        {
            if (node != null)
            {
                node.OnInteractionMessage -= Handle_Resource_Node_Message;
                node.OnHarvestComplete -= Handle_Harvest_Complete;
            }
        }
    }

    /// <summary>
    /// 상호작용 중 범위 체크 (너무 멀어지면 강제 중단)
    /// </summary>
    private void Check_Interaction_Range()
    {
        if (currentTargetNode == null) return;
        
        float distance = Vector3.Distance(transform.position, currentTargetNode.transform.position);
        
        // 상호작용 범위를 벗어났으면 강제 중단
        if (distance > interactionRange * 1.2f) // 약간의 여유 범위
        {
            Log_Debug($"범위 이탈로 상호작용 강제 중단: {distance:F1}m > {interactionRange * 1.2f:F1}m");
            Force_Stop_Interaction();
        }
    }

    #endregion

    #region 자원 노드 감지 및 타겟팅

    /// <summary>
    /// 근처의 자원 노드들을 감지하여 목록 업데이트
    /// </summary>
    private void Update_Nearby_Resource_Nodes()
    {
        nearbyResourceNodes.Clear();

        // 플레이어 위치에서 상호작용 범위 내의 모든 Collider 감지
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, resourceLayerMask);

        foreach (Collider col in colliders)
        {
            ResourceNode resourceNode = col.GetComponent<ResourceNode>();
            if (resourceNode != null)
            {
                nearbyResourceNodes.Add(resourceNode);
            }
        }
    }

    /// <summary>
    /// 가장 가까운 상호작용 가능한 자원 노드를 타겟으로 설정
    /// </summary>
    private void Update_Target_Node()
    {
        // 현재 상호작용 중이라면 타겟 변경하지 않음
        if (isInteracting)
        {
            return;
        }

        ResourceNode newTarget = null;
        float closestDistance = float.MaxValue;

        // 가장 가까운 자원 노드 찾기
        foreach (ResourceNode node in nearbyResourceNodes)
        {
            if (node == null) continue;

            float distance = Vector3.Distance(transform.position, node.transform.position);
            
            // 현재 도구로 채취 가능한지 확인 (상호작용 중이 아닐 때만)
            ToolType playerTool = Get_Current_Player_Tool();
            bool canHarvest = Check_Can_Harvest_Silent(node, playerTool);

            // 채취 가능하고 더 가까운 노드면 타겟으로 설정
            if (canHarvest && distance < closestDistance)
            {
                newTarget = node;
                closestDistance = distance;
            }
        }

        // 타겟이 변경된 경우 이벤트 호출
        if (newTarget != currentTargetNode)
        {
            ResourceNode previousTarget = currentTargetNode;
            currentTargetNode = newTarget;
            
            // 이벤트 호출
            OnTargetNodeChanged?.Invoke(currentTargetNode, currentTargetNode != null);
            
            Log_Debug($"타겟 노드 변경: {(previousTarget?.name ?? "None")} → {(currentTargetNode?.name ?? "None")}");
        }
    }

    #endregion

    #region 입력 처리

    /// <summary>
    /// 키 입력 처리 (E키 감지)
    /// </summary>
    private void Handle_Input()
    {
        // E키 입력 감지 (Unity New Input System 호환)
        bool eKeyPressed = Input.GetKeyDown(KeyCode.E);
        
        if (eKeyPressed)
        {
            Attempt_Resource_Interaction();
        }
    }

    /// <summary>
    /// 자원 상호작용 시도
    /// 타겟 노드가 있으면 채취 시작
    /// </summary>
    private void Attempt_Resource_Interaction()
    {
        // 쿨다운 체크 (짧은 시간 내 중복 실행 방지)
        if (Time.time - lastInteractionTime < 0.5f)
        {
            return;
        }

        lastInteractionTime = Time.time;

        // 타겟 노드 확인
        if (currentTargetNode == null)
        {
            Show_Player_Message("상호작용할 수 있는 자원이 없습니다.");
            return;
        }

        // 이미 상호작용 중인지 확인
        if (isInteracting)
        {
            Show_Player_Message("이미 다른 작업을 진행 중입니다.");
            return;
        }

        // 자원 채취 시도
        Start_Resource_Harvest(currentTargetNode);
    }

    #endregion

    #region 자원 채취 실행

    /// <summary>
    /// 지정된 자원 노드에서 채취 시작
    /// </summary>
    /// <param name="targetNode">채취할 자원 노드</param>
    private void Start_Resource_Harvest(ResourceNode targetNode)
    {
        if (targetNode == null)
        {
            Log_Debug("채취 실패: 타겟 노드가 null입니다");
            return;
        }

        // 현재 플레이어 도구 확인
        ToolType playerTool = Get_Current_Player_Tool();
        
        Log_Debug($"채취 시도: {targetNode.Resource_Type} (도구: {playerTool})");

        // ResourceNode의 채취 시작 메서드 호출
        bool harvestStarted = targetNode.Start_Harvest(playerTool);

        if (harvestStarted)
        {
            // 채취 시작 상태 설정
            isInteracting = true;
            
            // 채취 시작 이벤트 호출
            OnHarvestStarted?.Invoke(targetNode.Resource_Type, targetNode.harvestTime);
            
            Log_Debug($"채취 시작됨: {targetNode.Resource_Type}");
        }
        else
        {
            Log_Debug($"채취 시작 실패: {targetNode.Resource_Type}");
        }
    }

    /// <summary>
    /// 메시지 출력 없이 채취 가능 여부만 확인 (내부용)
    /// </summary>
    /// <param name="node">확인할 자원 노드</param>
    /// <param name="playerTool">플레이어 도구</param>
    /// <returns>채취 가능 여부</returns>
    private bool Check_Can_Harvest_Silent(ResourceNode node, ToolType playerTool)
    {
        if (node == null) return false;
        
        // 해당 노드가 이미 채취 중인지 확인
        if (node.Is_Being_Harvested)
        {
            return false;
        }
        
        // 도구 호환성 확인 (간단 버전)
        if (node.requiredTool == ToolType.None)
        {
            return true;
        }
        
        if (playerTool == ToolType.None)
        {
            return false;
        }
        
        // 도구 레벨 비교
        return (int)playerTool >= (int)node.requiredTool;
    }

    #endregion

    #region 도구 시스템

    /// <summary>
    /// 현재 플레이어가 장비한 도구 반환
    /// autoDetectTool이 true면 InventoryManager에서 확인, false면 Inspector 설정값 사용
    /// </summary>
    /// <returns>현재 플레이어 도구</returns>
    private ToolType Get_Current_Player_Tool()
    {
        if (!autoDetectTool)
        {
            return currentTool;
        }

        // InventoryManager에서 현재 장비한 도구 확인
        // 향후 InventoryManager에 장비 슬롯 시스템이 구현되면 연동
        // 현재는 Inspector 설정값 반환
        return currentTool;
    }

    /// <summary>
    /// 플레이어 도구 변경 (외부에서 호출 가능)
    /// </summary>
    /// <param name="newTool">새로운 도구</param>
    public void Change_Player_Tool(ToolType newTool)
    {
        ToolType previousTool = currentTool;
        currentTool = newTool;
        
        Log_Debug($"도구 변경: {previousTool} → {newTool}");
        
        // 타겟 노드 재평가 (새 도구로 채취 가능 여부 변경됨)
        Update_Target_Node();
    }

    #endregion

    #region 이벤트 핸들러

    /// <summary>
    /// ResourceNode에서 오는 메시지 처리
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    private void Handle_Resource_Node_Message(string message)
    {
        Show_Player_Message(message);
    }

    /// <summary>
    /// 자원 채취 완료 처리
    /// </summary>
    /// <param name="resourceType">채취된 자원 타입</param>
    /// <param name="amount">채취된 수량</param>
    private void Handle_Harvest_Complete(ResourceType resourceType, int amount)
    {
        // 상호작용 상태 해제
        isInteracting = false;
        
        Log_Debug($"채취 완료: {resourceType} x{amount}");
        
        // 타겟 노드 재평가 (다시 상호작용 가능하도록)
        Update_Target_Node();
    }

    #endregion

    #region UI 메시지 시스템

    /// <summary>
    /// 플레이어에게 메시지 표시
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    private void Show_Player_Message(string message)
    {
        currentMessage = message;
        messageEndTime = Time.time + messageDisplayTime;
        
        // 외부 UI 시스템에 메시지 전달
        OnPlayerMessage?.Invoke(message);
        
        Log_Debug($"메시지 표시: {message}");
    }

    /// <summary>
    /// 메시지 표시 시간 관리
    /// </summary>
    private void Update_Message_Display()
    {
        // 메시지 표시 시간이 끝났으면 초기화
        if (Time.time >= messageEndTime && !string.IsNullOrEmpty(currentMessage))
        {
            currentMessage = "";
            OnPlayerMessage?.Invoke(""); // 빈 메시지로 UI 클리어
        }
    }

    #endregion

    #region 공개 API

    /// <summary>
    /// 현재 상호작용 중인지 여부 반환
    /// </summary>
    public bool Is_Interacting => isInteracting;
    
    /// <summary>
    /// 현재 타겟 자원 노드 반환
    /// </summary>
    public ResourceNode Current_Target_Node => currentTargetNode;
    
    /// <summary>
    /// 근처 자원 노드 수 반환
    /// </summary>
    public int Nearby_Resource_Count => nearbyResourceNodes.Count;

    /// <summary>
    /// 상호작용 강제 중단 (외부에서 호출 가능)
    /// </summary>
    public void Force_Stop_Interaction()
    {
        if (isInteracting && currentTargetNode != null)
        {
            currentTargetNode.Stop_Harvest();
            isInteracting = false;
            Log_Debug("상호작용 강제 중단됨");
            
            // 타겟 노드 재평가 (다시 상호작용 가능하도록)
            Update_Target_Node();
        }
    }

    #endregion

    #region 유틸리티

    /// <summary>
    /// 디버그 로그 출력 (설정에 따라)
    /// </summary>
    /// <param name="message">로그 메시지</param>
    private void Log_Debug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[PlayerResourceInteraction:{gameObject.name}] {message}");
        }
    }

    #endregion

    #region 에디터 지원 (Gizmo)

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        // 상호작용 범위 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // 타겟 노드 표시
        if (currentTargetNode != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTargetNode.transform.position);
            Gizmos.DrawWireCube(currentTargetNode.transform.position, Vector3.one * 0.5f);
        }
        
        // 근처 자원 노드들 표시
        Gizmos.color = Color.blue;
        foreach (ResourceNode node in nearbyResourceNodes)
        {
            if (node != null && node != currentTargetNode)
            {
                Gizmos.DrawWireCube(node.transform.position, Vector3.one * 0.3f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        // 항상 표시되는 상호작용 범위 (작고 연한 색)
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawSphere(transform.position, interactionRange);
    }
#endif

    #endregion

    #region 정리

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        Unsubscribe_Resource_Node_Events();
    }

    #endregion
}
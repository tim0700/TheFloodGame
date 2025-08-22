using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ResourceSystem;
using System.Collections;

public class MiningUIController : MonoBehaviour
{
    [Header("=== UI 요소 연결 ===")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private Image progressFill;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;

    [Header("=== 설정 ===")]
    [SerializeField] private float notificationDisplayTime = 3f;

    private ResourceType currentResourceType;

    private void Start()
    {
        // 모든 UI 요소 초기 상태로 설정
        InitializeUI();
        
        // 이벤트 구독
        SubscribeToEvents();
    }

    private void InitializeUI()
    {
        messageText.text = "";
        interactionPanel.SetActive(false);
        progressPanel.SetActive(false);
        notificationPanel.SetActive(false);
        
        if (progressFill != null)
        {
            progressFill.fillAmount = 0f;
        }
    }

    private void SubscribeToEvents()
    {
        // PlayerResourceInteraction 이벤트 구독
        PlayerResourceInteraction playerInteraction = FindObjectOfType<PlayerResourceInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.OnPlayerMessage += ShowMessage;
            playerInteraction.OnTargetNodeChanged += OnTargetChanged;
            playerInteraction.OnHarvestStarted += OnHarvestStarted;
            Debug.Log("[MiningUIController] PlayerResourceInteraction 이벤트 구독 완료");
        }
        else
        {
            Debug.LogError("[MiningUIController] PlayerResourceInteraction을 찾을 수 없습니다!");
        }

        // 모든 ResourceNode 이벤트 구독
        ResourceNode[] resourceNodes = FindObjectsOfType<ResourceNode>();
        foreach (ResourceNode node in resourceNodes)
        {
            if (node != null)
            {
                node.OnHarvestProgress += OnProgressUpdate;
                node.OnHarvestComplete += OnHarvestComplete;
                node.OnInteractionUIUpdate += OnInteractionUIUpdate;
            }
        }
        Debug.Log($"[MiningUIController] {resourceNodes.Length}개 ResourceNode 이벤트 구독 완료");
    }

    #region 이벤트 핸들러

    private void ShowMessage(string message)
    {
        if (messageText == null) return;

        if (string.IsNullOrEmpty(message))
        {
            messageText.text = "";
        }
        else
        {
            messageText.text = message;
            Debug.Log($"[UI] 메시지 표시: {message}");
        }
    }

    private void OnTargetChanged(ResourceNode node, bool canInteract)
    {
        if (interactionPanel == null) return;

        interactionPanel.SetActive(canInteract);

        if (canInteract && node != null && interactionText != null)
        {
            string resourceName = ResourceConverter.Convert_To_Item_Name(node.Resource_Type);
            interactionText.text = $"Use F to mining {resourceName}";
            Debug.Log($"[UI] 상호작용 UI 표시: {resourceName}");
        }
    }

    private void OnInteractionUIUpdate(bool show, string text)
    {
        if (interactionPanel == null) return;

        interactionPanel.SetActive(show);
        if (show && interactionText != null)
        {
            interactionText.text = text;
        }
    }

    private void OnHarvestStarted(ResourceType resourceType, float duration)
    {
        currentResourceType = resourceType;
        
        if (progressPanel != null)
        {
            progressPanel.SetActive(true);
            Debug.Log($"[UI] 진행률 바 시작: {resourceType}");
        }

        if (progressText != null)
        {
            string resourceName = ResourceConverter.Convert_To_Item_Name(resourceType);
            progressText.text = $"Mining {resourceName}... 0%";
        }
    }

    private void OnProgressUpdate(float progress)
    {
        if (progressFill != null)
        {
            progressFill.fillAmount = progress;
        }

        if (progressText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100);
            string resourceName = ResourceConverter.Convert_To_Item_Name(currentResourceType);
            progressText.text = $"Mining {resourceName}... {percentage}%";
        }
    }

    private void OnHarvestComplete(ResourceType resourceType, int amount)
    {
        // 진행률 바 숨기기
        if (progressPanel != null)
        {
            progressPanel.SetActive(false);
        }

        // 완료 알림 표시
        string resourceName = ResourceConverter.Convert_To_Item_Name(resourceType);
        ShowNotification($"Get {resourceName} {amount}!");
        
        Debug.Log($"[UI] 채굴 완료: {resourceName} x{amount}");
    }

    private void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null) return;

        notificationText.text = message;
        StartCoroutine(NotificationCoroutine());
    }

    private IEnumerator NotificationCoroutine()
    {
        notificationPanel.SetActive(true);
        yield return new WaitForSeconds(notificationDisplayTime);
        notificationPanel.SetActive(false);
    }

    #endregion

    private void OnDestroy()
    {
        // 이벤트 구독 해제는 자동으로 처리됨 (오브젝트 파괴시)
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("슬롯 부모 (GridLayoutGroup 등)")]
    public Transform slotParent;

    [Header("슬롯 프리팹")]
    public GameObject slotPrefab;

    private void Start()
    {
        RefreshUI();
    }

    /// <summary>
    /// 인벤토리 UI 갱신
    /// </summary>
    public void RefreshUI()
    {
        // 기존 슬롯 UI 전부 제거
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // 인벤토리 매니저 슬롯을 UI로 생성
        foreach (var slot in InventoryManager.Instance.slots)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotParent);

            Image icon = slotGO.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI qtyText = slotGO.transform.Find("Qty").GetComponent<TextMeshProUGUI>();

            if (slot.item != null)
            {
                icon.sprite = slot.item.icon;
                icon.enabled = true;

                qtyText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
            }
            else
            {
                icon.enabled = false;
                qtyText.text = "";
            }
        }
    }
}

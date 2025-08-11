using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("���� �θ� (GridLayoutGroup ��)")]
    public Transform slotParent;

    [Header("���� ������")]
    public GameObject slotPrefab;

    private void Start()
    {
        RefreshUI();
    }

    /// <summary>
    /// �κ��丮 UI ����
    /// </summary>
    public void RefreshUI()
    {
        // ���� ���� UI ���� ����
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // �κ��丮 �Ŵ��� ������ UI�� ����
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

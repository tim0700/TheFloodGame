//ItemSLot.cs
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    //====ITEM DATA====//
    public string ItemName;
    public int Quantity;
    public Sprite ItemSprite;
    public bool IsFull;

    //===ITEM SLOT=====//
    [SerializeField]
    private TMP_Text QuantityText;

    [SerializeField]
    private Image ItemImage;

    public GameObject SelectedShader;
    public bool ThisItemSelected;

    private InventoryManger inventoryManger;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        inventoryManger = GameObject.Find("InventoryCanvas").GetComponent<InventoryManger>();
    }

    public void AddItem(string ItemName, int Quantity, Sprite ItemSprite)
    {
        this.ItemName = ItemName;
        this.Quantity = Quantity;
        this.ItemSprite = ItemSprite;
        IsFull = true;

   
        QuantityText.text = Quantity.ToString();
        QuantityText.enabled = true;
        ItemImage.sprite = ItemSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("item LEFT BUTTON");
            OnLeftClick();
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("item Right BUTTON");
            OnRightClick();
        }
    }

    public void OnLeftClick()
    {
        inventoryManger.DeselectAllSlot();
        SelectedShader.SetActive(true);
        ThisItemSelected = true;
    }

    public void OnRightClick()
    {
        SelectedShader.SetActive(true);
        ThisItemSelected = true;
    }
}

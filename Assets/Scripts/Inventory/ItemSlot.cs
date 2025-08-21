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
    public string ItemDescription;
    public Sprite emptySprite;


    [SerializeField]
    private int MaxNumberOfItems;

    //===ITEM SLOT=====//
    [SerializeField]
    private TMP_Text QuantityText;

    [SerializeField]
    private Image ItemImage;

    //===ITEM DESCRIPTION SLOT===//
    public Image ItemDescriptionImage;
    public TMP_Text ItemDescriptionNameText;
    public TMP_Text ItemDescriptionText;

    public GameObject SelectedShader;
    public bool ThisItemSelected;

    private InventoryManger inventoryManger;

    // 🟢 버릴 아이템 프리팹 연결해두기
    public GameObject dropItemPrefab;

    // 🟢 플레이어 참조
    private Transform Player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        inventoryManger = GameObject.Find("InventoryCanvas").GetComponent<InventoryManger>();
    }

    public int AddItem(string ItemName, int Quantity, Sprite ItemSprite, string ItemDescription)
    {

        if (IsFull)
            return Quantity;

        //Update Name
        this.ItemName = ItemName;

        //Update image
        this.ItemSprite = ItemSprite;
        ItemImage.sprite = ItemSprite;

        //Update Description
        this.ItemDescription = ItemDescription;

        //Update Qunatity
        this.Quantity += Quantity;
        if (this.Quantity >= MaxNumberOfItems)
        {
            QuantityText.text = MaxNumberOfItems.ToString();
            QuantityText.enabled = true;
            IsFull = true;

            int ExtraItems = this.Quantity - MaxNumberOfItems;
            this.Quantity = MaxNumberOfItems;
            return ExtraItems;
        }
        QuantityText.text = this.Quantity.ToString();
        QuantityText.enabled = true;

        return 0;
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
        if (ThisItemSelected)
        {
            this.Quantity -= 1;
            QuantityText.text = this.Quantity.ToString();
            if (this.Quantity <= 0)
                EmptySlot();
        }

        else
        {
            inventoryManger.DeselectAllSlot();
            SelectedShader.SetActive(true);
            ThisItemSelected = true;
            ItemDescriptionNameText.text = ItemName;
            ItemDescriptionText.text = ItemDescription;
            ItemDescriptionImage.sprite = ItemSprite;
            if (ItemDescriptionImage.sprite == null)
                ItemDescriptionImage.sprite = emptySprite;
        }
    }

    private void EmptySlot()
    {
        QuantityText.enabled = false;
        ItemImage.sprite = emptySprite;

        ItemDescriptionNameText.text = "";
        ItemDescriptionText.text = "";
        ItemDescriptionImage.sprite = emptySprite;
    }

    public void OnRightClick()
    {
 
    }
}

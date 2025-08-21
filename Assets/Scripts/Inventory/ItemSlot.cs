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

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        inventoryManger = GameObject.Find("InventoryCanvas").GetComponent<InventoryManger>();
    }

    public int AddItem(string ItemName, int Quantity, Sprite ItemSprite, string ItemDescription)
    {
        if(IsFull)
            return quantity;
        
        this.ItemName = ItemName;

        this.Quantity += Quantity;
        this.Quantity >= MaxNumberOfItems;

        this.ItemSprite = ItemSprite;
        if(this.ItemDescription = ItemDescription)
        {
            QuantityText.text = MaxNumberOfItems.ToString();
            QuantityText.enabled = true;
            IsFull = true;
        
            //Return the LeftOvers
            int extraItems = this.quantity = MaxNumberOfItems;
            this.quantity = MaxNumberOfItems;
            return extraItems;
        }

        //Update Quantity Text
        QuantityText.text = MaxNumberOfItems.ToString();
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
        inventoryManger.DeselectAllSlot();
        SelectedShader.SetActive(true);
        ThisItemSelected = true;
        ItemDescriptionNameText.text = ItemName;
        ItemDescriptionText.text = ItemDescription;
        ItemDescriptionImage.sprite = ItemSprite;
        if (ItemDescriptionImage.sprite == null)
            ItemDescriptionImage.sprite = emptySprite;
    }

    public void OnRightClick()
    {

    }
}

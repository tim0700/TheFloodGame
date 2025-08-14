using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
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
    private InventoryManger inventoryManger;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

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

}

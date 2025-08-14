using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManger : MonoBehaviour
{
    public GameObject InventoryMenu;
    private bool MenuActivated;     //메뉴가 열려있는지 닫혀있는지 확인하는것

    void Start()
    {
        InventoryMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && MenuActivated) {
            Debug.Log("e 눌림");
            InventoryMenu.SetActive(false);
            MenuActivated = false;
        }
        else if (Input.GetKeyDown(KeyCode.E) && !MenuActivated)
        {
            Debug.Log("e 눌림");
            InventoryMenu.SetActive(true);
            MenuActivated = true;
        }
    }

    public void AddItem(string itemName, int quantity, Sprite itemSprite)
    {
        Debug.Log("ItemName = " + itemName +"Quantity = " + quantity + "ItemSprite = " + itemSprite);
    }
}

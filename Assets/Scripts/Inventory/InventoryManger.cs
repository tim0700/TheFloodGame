using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManger : MonoBehaviour
{
    public GameObject InventoryMenu;
    private bool MenuActivated;     //메뉴가 열려있는지 닫혀있는지 확인하는것
    public ItemSlot[] ItemSlot;

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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Input.GetKeyDown(KeyCode.E) && !MenuActivated)
        {
            Debug.Log("e 눌림");
            InventoryMenu.SetActive(true);
            MenuActivated = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void AddItem(string ItemName, int Quantity, Sprite ItemSprite, string ItemDescription)
    {
        Debug.Log("InventoryManager Add Item 발동");
        for (int i = 0; i < ItemSlot.Length; i++)
        {
            if (ItemSlot[i].IsFull == false)
            {
                ItemSlot[i].AddItem(ItemName, Quantity, ItemSprite, ItemDescription);
                return;
            }
        }
    }

    public void DeselectAllSlot()
    {
        for (int i = 0; i < ItemSlot.Length; i++)
        {
            ItemSlot[i].SelectedShader.SetActive(false);
            ItemSlot[i].ThisItemSelected = false;
        }
    }
}

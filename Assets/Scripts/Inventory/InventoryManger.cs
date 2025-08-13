using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManger : MonoBehaviour
{
    public GameObject InventoryMenu;
    private bool MenuActivated;     //메뉴켜져있는지 꺼져있는지 추적하는 함수

    void Start()
    {
        InventoryMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && MenuActivated) {
            Debug.Log("e 눌렀음");
            InventoryMenu.SetActive(false);
            MenuActivated = false;
        }
        else if (Input.GetKeyDown(KeyCode.E) && !MenuActivated)
        {
            Debug.Log("e 눌렀음");
            InventoryMenu.SetActive(true);
            MenuActivated = true;
        }
    }
}

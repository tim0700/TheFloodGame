using UnityEngine;


public class Item : MonoBehaviour {

    [SerializeField]
    private string ItemName;

    [SerializeField]
    private int quantity;

    [SerializeField]
    private Sprite sprite;

    [TextArea]
    [SerializeField]
    private string ItemDescription;

    private InventoryManger inventoryManger;


    void Start(){
       
        inventoryManger = GameObject.Find("InventoryCanvas").GetComponent<InventoryManger>();
    }

    private void OnCollisionEnter(Collision collision) {

        if(collision.gameObject.tag == "Player")
        {
            Debug.Log("item collison!");
            int LeftOverItems = inventoryManger.AddItem(ItemName, quantity, sprite, ItemDescription);
            if(LeftOverItem <= 0)
                Destroy(gameObject);
            else
                quantity = LeftOverItems;
                
        }
    }

}

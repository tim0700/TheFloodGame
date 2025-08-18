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
            Debug.Log("플레이와 아이템 충돌!");
            inventoryManger.AddItem(ItemName, quantity, sprite, ItemDescription);
            Destroy(gameObject);
        }
    }

}

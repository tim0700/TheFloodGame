using UnityEngine;

/// <summary>
/// 아이템 시스템 기본 테스트 클래스
/// 구현된 구조체들이 올바르게 작동하는지 확인
/// </summary>
public class Item_Test : MonoBehaviour
{
    void Start()
    {
        Test_Item_Creation();
        Test_Item_Categories();
        Test_Item_Utils();
        Debug.Log("[Item_Test] All tests completed successfully!");
    }

    void Test_Item_Creation()
    {
        // 기본 아이템 생성 테스트
        var wood = new Item_Data(Item_IDs.Wood, 10, 0);
        var iron_pickaxe = new Item_Data(Item_IDs.Iron_Pickaxe, 1, 200); // 내구도 200

        Debug.Log($"[Item_Test] Wood: {wood}");
        Debug.Log($"[Item_Test] Iron Pickaxe: {iron_pickaxe}");

        // 동등성 테스트
        var wood2 = new Item_Data(Item_IDs.Wood, 10, 0);
        Debug.Log($"[Item_Test] Wood equals Wood2: {wood.Equals(wood2)}");
        Debug.Log($"[Item_Test] Wood same type as Wood2: {wood.Is_Same_Item_Type(wood2)}");
    }

    void Test_Item_Categories()
    {
        // 카테고리 분류 테스트
        Debug.Log($"[Item_Test] Wood category: {Item_IDs.Get_Category_From_ID(Item_IDs.Wood)}");
        Debug.Log($"[Item_Test] Iron category: {Item_IDs.Get_Category_From_ID(Item_IDs.Iron)}");
        Debug.Log($"[Item_Test] Iron Pickaxe category: {Item_IDs.Get_Category_From_ID(Item_IDs.Iron_Pickaxe)}");
        Debug.Log($"[Item_Test] Stone Projectile category: {Item_IDs.Get_Category_From_ID(Item_IDs.Stone_Projectile)}");
        Debug.Log($"[Item_Test] Stone Dike Kit category: {Item_IDs.Get_Category_From_ID(Item_IDs.Stone_Dike_Kit)}");
    }

    void Test_Item_Utils()
    {
        // 유틸리티 함수 테스트
        Debug.Log($"[Item_Test] Is Wood valid ID: {Item_IDs.Is_Valid_Item_ID(Item_IDs.Wood)}");
        Debug.Log($"[Item_Test] Is 999 valid ID: {Item_IDs.Is_Valid_Item_ID(999)}");
        
        Debug.Log($"[Item_Test] Is Iron Pickaxe a tool: {Item_IDs.Is_Tool(Item_IDs.Iron_Pickaxe)}");
        Debug.Log($"[Item_Test] Is Wood a tool: {Item_IDs.Is_Tool(Item_IDs.Wood)}");
        
        Debug.Log($"[Item_Test] Is Iron a resource: {Item_IDs.Is_Resource(Item_IDs.Iron)}");
        Debug.Log($"[Item_Test] Is Stone Projectile consumable: {Item_IDs.Is_Consumable(Item_IDs.Stone_Projectile)}");
    }
}
/// <summary>
/// 게임 내 모든 아이템의 고유 식별자 정의
/// Input: 아이템 식별이 필요한 모든 시스템
/// Output: 고유한 정수 ID 값
/// Type: Static Constants Class
/// </summary>
public static class Item_IDs
{
    // ===== 기본 자원 (Raw_Resource) 1-10 =====
    
    /// <summary>
    /// 나무 - 강물에서 수집, 기본 건설 재료
    /// 획득: 강물 수집
    /// 용도: 제방, 건물, 탄약 제작
    /// </summary>
    public const int Wood = 1;
    
    /// <summary>
    /// 돌 - 강물/광맥 채취, 제방 및 도구 제작
    /// 획득: 강물 수집, 광맥 채취 (나무 곡괭이 이상)
    /// 용도: 도구 제작, 석재 가공, 제련 보조재
    /// </summary>
    public const int Stone = 2;
    
    /// <summary>
    /// 철 원석 - 철 광맥 채취, 철 제련용 원료
    /// 획득: 철 광맥 채취 (돌 곡괭이 이상)
    /// 용도: 철/강철 제련 주재료
    /// </summary>
    public const int Iron_Ore = 3;
    
    /// <summary>
    /// 석탄 - 석탄 광맥 채취, 제련 연료
    /// 획득: 석탄 광맥 채취 (돌 곡괭이 이상)
    /// 용도: 모든 제련 과정의 연료
    /// </summary>
    public const int Coal = 4;

    // ===== 가공 자원 (Processed_Resource) 11-20 =====
    
    /// <summary>
    /// 철 - 돌 제련소에서 철 원석 + 석탄으로 제련
    /// 제작: 돌 제련소 (철 원석 + 석탄)
    /// 용도: 중급 제방, 공격 건물, 도구 제작
    /// </summary>
    public const int Iron = 11;
    
    /// <summary>
    /// 강철 - 철 제련소에서 철 원석 + 돌 + 석탄으로 제련
    /// 제작: 철 제련소 (철 원석 + 돌 + 석탄)
    /// 용도: 최고급 제방, 고급 공격 건물, 최상급 도구
    /// </summary>
    public const int Steel = 12;
    
    /// <summary>
    /// 석재 - 석재 절삭대에서 돌 가공
    /// 제작: 석재 절삭대 (돌)
    /// 용도: 모든 제방 종류, 건물 건설, 탄약 제작
    /// </summary>
    public const int Cut_Stone = 13;

    // ===== 도구 (Tool) 21-30 =====
    
    /// <summary>
    /// 나무 곡괭이 - 채취량 적음, 돌만 채취 가능
    /// 성능: 채취량 1, 내구도 50회
    /// 채취 대상: 돌
    /// </summary>
    public const int Wooden_Pickaxe = 21;
    
    /// <summary>
    /// 돌 곡괭이 - 채취량 적음, 돌+철 원석 채취 가능
    /// 성능: 채취량 1, 내구도 100회
    /// 채취 대상: 돌, 철 원석, 석탄
    /// </summary>
    public const int Stone_Pickaxe = 22;
    
    /// <summary>
    /// 철 곡괭이 - 채취량 보통, 돌+철 원석 채취 가능
    /// 성능: 채취량 2, 내구도 200회
    /// 채취 대상: 돌, 철 원석, 석탄
    /// </summary>
    public const int Iron_Pickaxe = 23;
    
    /// <summary>
    /// 강철 곡괭이 - 채취량 많음, 돌+철 원석 채취 가능
    /// 성능: 채취량 3, 내구도 500회
    /// 채취 대상: 돌, 철 원석, 석탄
    /// </summary>
    public const int Steel_Pickaxe = 24;

    // ===== 탄약 (Ammunition) 31-40 =====
    
    /// <summary>
    /// 짱돌 - 투석기용 탄약, 석재+나무로 제작
    /// 제작: 짱돌 생산 건물 (석재 + 나무)
    /// 사용: 투석기 (피해량 50)
    /// </summary>
    public const int Stone_Projectile = 31;
    
    /// <summary>
    /// 60mm 포탄 - 60mm 박격포용 탄약, 철+석탄으로 제작
    /// 제작: 60mm 포탄 공장 (철 + 석탄)
    /// 사용: 60mm 박격포 (피해량 100)
    /// </summary>
    public const int Mortar_60mm = 32;
    
    /// <summary>
    /// 81mm 포탄 - 81mm 박격포용 탄약, 강철+석탄으로 제작
    /// 제작: 81mm 포탄 공장 (강철 + 석탄)
    /// 사용: 81mm 박격포 (피해량 200)
    /// </summary>
    public const int Mortar_81mm = 33;

    // ===== 건물 구성품 (Building_Component) 41-60 =====
    
    // 제방 재료 (41-43)
    /// <summary>
    /// 돌 제방 키트 - 석재+나무, 체력 100
    /// 제작: 석재 + 나무
    /// 건설: 1층 돌 제방 (체력 100, 높이 1m)
    /// </summary>
    public const int Stone_Dike_Kit = 41;
    
    /// <summary>
    /// 철 제방 키트 - 철+석재, 체력 200
    /// 제작: 철 + 석재
    /// 건설: 1층 철 제방 (체력 200, 높이 1m)
    /// </summary>
    public const int Iron_Dike_Kit = 42;
    
    /// <summary>
    /// 강철 제방 키트 - 강철+석재+나무, 체력 400
    /// 제작: 강철 + 석재 + 나무
    /// 건설: 1층 강철 제방 (체력 400, 높이 1m)
    /// </summary>
    public const int Steel_Dike_Kit = 43;
    
    // 공격 건물 (44-46)
    /// <summary>
    /// 투석기 키트 - 철+석재
    /// 제작: 철 + 석재
    /// 건설: 투석기 (체력 하, 공격력 하, 사용 탄약: 짱돌)
    /// </summary>
    public const int Catapult_Kit = 44;
    
    /// <summary>
    /// 60mm 박격포 키트 - 철+석재+나무
    /// 제작: 철 + 석재 + 나무
    /// 건설: 60mm 박격포 (체력 중, 공격력 중, 사용 탄약: 60mm 포탄)
    /// </summary>
    public const int Mortar_60mm_Kit = 45;
    
    /// <summary>
    /// 81mm 박격포 키트 - 강철+석재
    /// 제작: 강철 + 석재
    /// 건설: 81mm 박격포 (체력 상, 공격력 상, 사용 탄약: 81mm 포탄)
    /// </summary>
    public const int Mortar_81mm_Kit = 46;
    
    // 생산 건물 (47-54)
    /// <summary>
    /// 돌 제련소 키트 - 철 제련용
    /// 기능: 철 원석 + 석탄 → 철
    /// </summary>
    public const int Stone_Smelter_Kit = 47;
    
    /// <summary>
    /// 철 제련소 키트 - 강철 제련용
    /// 기능: 철 원석 + 돌 + 석탄 → 강철
    /// </summary>
    public const int Iron_Smelter_Kit = 48;
    
    /// <summary>
    /// 석재 절삭대 키트 - 돌 가공용
    /// 기능: 돌 → 석재
    /// </summary>
    public const int Stone_Cutter_Kit = 49;
    
    /// <summary>
    /// 짱돌 생산 건물 키트
    /// 기능: 석재 + 나무 → 짱돌
    /// </summary>
    public const int Stone_Projectile_Factory_Kit = 50;
    
    /// <summary>
    /// 60mm 포탄 공장 키트
    /// 기능: 철 + 석탄 → 60mm 포탄
    /// </summary>
    public const int Mortar_60mm_Factory_Kit = 51;
    
    /// <summary>
    /// 81mm 포탄 공장 키트
    /// 기능: 강철 + 석탄 → 81mm 포탄
    /// </summary>
    public const int Mortar_81mm_Factory_Kit = 52;

    // ===== 유틸리티 함수 =====
    
    /// <summary>
    /// 유효한 아이템 ID인지 확인
    /// </summary>
    /// <param name="item_id">검사할 아이템 ID</param>
    /// <returns>1-52 범위 내 유효한 ID면 true</returns>
    public static bool Is_Valid_Item_ID(int item_id)
    {
        return item_id >= 1 && item_id <= 52;
    }
    
    /// <summary>
    /// 아이템 ID를 통해 카테고리 추정
    /// ID 범위 기반 카테고리 자동 분류
    /// </summary>
    /// <param name="item_id">분류할 아이템 ID</param>
    /// <returns>해당 아이템의 카테고리</returns>
    public static Item_Category Get_Category_From_ID(int item_id)
    {
        return item_id switch
        {
            >= 1 and <= 10 => Item_Category.Raw_Resource,
            >= 11 and <= 20 => Item_Category.Processed_Resource,
            >= 21 and <= 30 => Item_Category.Tool,
            >= 31 and <= 40 => Item_Category.Ammunition,
            >= 41 and <= 60 => Item_Category.Building_Component,
            _ => Item_Category.None
        };
    }

    /// <summary>
    /// 도구 아이템인지 확인
    /// </summary>
    /// <param name="item_id">검사할 아이템 ID</param>
    /// <returns>도구 카테고리면 true</returns>
    public static bool Is_Tool(int item_id)
    {
        return Get_Category_From_ID(item_id) == Item_Category.Tool;
    }

    /// <summary>
    /// 자원 아이템인지 확인 (기본 + 가공 자원)
    /// </summary>
    /// <param name="item_id">검사할 아이템 ID</param>
    /// <returns>자원 카테고리면 true</returns>
    public static bool Is_Resource(int item_id)
    {
        var category = Get_Category_From_ID(item_id);
        return category == Item_Category.Raw_Resource || 
               category == Item_Category.Processed_Resource;
    }

    /// <summary>
    /// 소모품 아이템인지 확인 (탄약 + 건물 키트)
    /// </summary>
    /// <param name="item_id">검사할 아이템 ID</param>
    /// <returns>사용 시 소모되는 아이템이면 true</returns>
    public static bool Is_Consumable(int item_id)
    {
        var category = Get_Category_From_ID(item_id);
        return category == Item_Category.Ammunition || 
               category == Item_Category.Building_Component;
    }
}
/// <summary>
/// 아이템 분류 시스템
/// Input: 아이템 분류 필요 시점
/// Output: 아이템의 논리적 그룹 분류
/// Type: Enum
/// </summary>
public enum Item_Category
{
    /// <summary>
    /// 미정의 카테고리 (기본값)
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 기본 자원: 나무, 돌, 철 원석, 석탄
    /// 채취를 통해 직접 획득하는 원료
    /// 특징: 스택 가능, 가공 건물의 원료로 사용
    /// </summary>
    Raw_Resource = 1,
    
    /// <summary>
    /// 가공 자원: 철, 강철, 석재
    /// 제련소나 가공 건물을 통해 만드는 중간재
    /// 특징: 스택 가능, 고급 제작의 핵심 재료
    /// </summary>
    Processed_Resource = 2,
    
    /// <summary>
    /// 도구: 곡괭이 4종 (나무, 돌, 철, 강철)
    /// 내구도가 있으며 채취 효율을 결정
    /// 특징: 내구도 시스템, Additional_Data에 남은 사용 횟수 저장
    /// </summary>
    Tool = 3,
    
    /// <summary>
    /// 탄약: 짱돌, 60mm 포탄, 81mm 포탄
    /// 공격 건물에서 소모되는 아이템
    /// 특징: 스택 가능, 공격 시 소모됨
    /// </summary>
    Ammunition = 4,
    
    /// <summary>
    /// 건물 구성품: 제방 재료, 공격 건물 부품, 생산 건물 부품
    /// 건설 시 소모되어 구조물로 변환
    /// 특징: 일반적으로 스택 불가능, 건설 시 완전 소모
    /// </summary>
    Building_Component = 5
}
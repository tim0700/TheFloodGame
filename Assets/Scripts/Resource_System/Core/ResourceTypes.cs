using UnityEngine;

/// <summary>
/// 자원 채취 시스템의 핵심 열거형 및 데이터 구조 정의
/// 
/// Input: 없음 (정적 정의)
/// Output: 자원 시스템에서 사용할 열거형들
/// Type: 시스템 기반 구조 정의
/// </summary>
namespace ResourceSystem
{
    /// <summary>
    /// 자원의 종류를 정의하는 열거형
    /// 강물과 갱도에서 채취 가능한 모든 자원 타입
    /// </summary>
    public enum ResourceType
    {
        Wood = 0,       // 나무 - 강물 전용
        Stone = 1,      // 돌 - 강물(1개) + 갱도(5-8개)
        IronOre = 2,    // 철 원석 - 갱도 전용
        Coal = 3,       // 석탄 - 갱도 전용
    }

    /// <summary>
    /// 자원 채취 방식을 구분하는 열거형
    /// </summary>
    public enum HarvestMethod
    {
        RiverCollection = 0,    // 강에서 줍기 (즉시, 소량)
        MineHarvesting = 1,     // 갱도에서 채굴 (시간 소요, 대량)
    }

    /// <summary>
    /// 채취 도구의 종류 및 레벨을 정의하는 열거형
    /// 숫자가 클수록 고급 도구 (상위 호환)
    /// </summary>
    public enum ToolType
    {
        None = 0,           // 도구 없음 (맨손)
        WoodPickaxe = 1,    // 나무 곡괭이 (레벨 1)
        StonePickaxe = 2,   // 돌 곡괭이 (레벨 2)
        IronPickaxe = 3,    // 철 곡괭이 (레벨 3)
        SteelPickaxe = 4,   // 강철 곡괭이 (레벨 4)
    }

    /// <summary>
    /// 자원별 기본 설정 데이터를 담는 구조체
    /// Inspector에서 설정하거나 코드에서 기본값으로 사용
    /// </summary>
    [System.Serializable]
    public class ResourceData
    {
        [Header("기본 정보")]
        public ResourceType type;
        public string displayName;
        public string description;

        [Header("채취 설정")]
        [Tooltip("채취에 걸리는 시간 (초)")]
        public float harvestTime;
        
        [Tooltip("갱도에서의 최소-최대 획득량")]
        public Vector2Int mineAmountRange;
        
        [Tooltip("강물에서의 획득량")]
        public int riverAmount;

        [Header("도구 요구사항")]
        public ToolType requiredTool;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public ResourceData()
        {
            type = ResourceType.Stone;
            displayName = "";
            description = "";
            harvestTime = 2f;
            mineAmountRange = new Vector2Int(3, 8);
            riverAmount = 1;
            requiredTool = ToolType.WoodPickaxe;
        }

        /// <summary>
        /// 매개변수가 있는 생성자
        /// </summary>
        public ResourceData(ResourceType resourceType, string name, float time, Vector2Int mineRange, ToolType tool)
        {
            type = resourceType;
            displayName = name;
            harvestTime = time;
            mineAmountRange = mineRange;
            riverAmount = 1;
            requiredTool = tool;
            description = $"{name} 자원입니다.";
        }
    }

    /// <summary>
    /// 자원별 기본 설정을 제공하는 정적 클래스
    /// </summary>
    public static class ResourceDefaults
    {
        /// <summary>
        /// 자원 타입에 따른 기본 설정 반환
        /// </summary>
        public static ResourceData Get_Default_Data(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Wood => new ResourceData(
                    ResourceType.Wood, 
                    "나무", 
                    0f,                             // 강물에서는 즉시 수집
                    new Vector2Int(0, 0),           // 갱도에서는 채취 불가
                    ToolType.None                   // 도구 불필요
                ),
                
                ResourceType.Stone => new ResourceData(
                    ResourceType.Stone, 
                    "돌", 
                    2f,                             // 갱도에서 2초
                    new Vector2Int(5, 8),           // 갱도에서 5-8개
                    ToolType.WoodPickaxe            // 나무 곡괭이 필요
                ),
                
                ResourceType.IronOre => new ResourceData(
                    ResourceType.IronOre, 
                    "철 원석", 
                    3f,                             // 갱도에서 3초
                    new Vector2Int(2, 4),           // 갱도에서 2-4개
                    ToolType.StonePickaxe           // 돌 곡괭이 이상 필요
                ),
                
                ResourceType.Coal => new ResourceData(
                    ResourceType.Coal, 
                    "석탄", 
                    2.5f,                           // 갱도에서 2.5초
                    new Vector2Int(3, 5),           // 갱도에서 3-5개
                    ToolType.WoodPickaxe            // 나무 곡괭이 필요
                ),
                
                _ => new ResourceData()             // 기본값
            };
        }

        /// <summary>
        /// 도구 타입에 따른 표시명 반환
        /// </summary>
        public static string Get_Tool_Display_Name(ToolType toolType)
        {
            return toolType switch
            {
                ToolType.None => "맨손",
                ToolType.WoodPickaxe => "나무 곡괭이",
                ToolType.StonePickaxe => "돌 곡괭이",
                ToolType.IronPickaxe => "철 곡괭이",
                ToolType.SteelPickaxe => "강철 곡괭이",
                _ => "알 수 없는 도구"
            };
        }

        /// <summary>
        /// 자원 타입에 따른 표시명 반환
        /// </summary>
        public static string Get_Resource_Display_Name(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Wood => "나무",
                ResourceType.Stone => "돌",
                ResourceType.IronOre => "철 원석",
                ResourceType.Coal => "석탄",
                _ => "알 수 없는 자원"
            };
        }
    }
}
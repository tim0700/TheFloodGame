using UnityEngine;

/// <summary>
/// ScriptableObject 기반 게임 설정값 관리 시스템
/// Input: Unity Inspector를 통한 설정 조정
/// Output: 게임 전반에서 참조되는 설정값들
/// Type: ScriptableObject
/// </summary>

namespace TheFloodGame.Core.Data
{
    [CreateAssetMenu(fileName = "New_Game_Config", menuName = "TheFloodGame/Game Configuration")]
    public class Game_Config : ScriptableObject
    {
        [Header("Game Flow Settings")]
        [Tooltip("게임 시작 전 대기 시간 (초)")]
        [SerializeField] private float game_Start_Delay = 3.0f;
        
        [Tooltip("게임 종료 처리 시간 (초)")]
        [SerializeField] private float game_End_Duration = 5.0f;
        
        [Tooltip("자동 게임 종료 시간 (0이면 무제한)")]
        [SerializeField] private float maximum_Game_Duration = 0.0f;

        [Header("Water Level System")]
        [Tooltip("강물 수위 상승 주기 (초)")]
        [SerializeField] private float water_Level_Rise_Interval = 30.0f;
        
        [Tooltip("매번 상승하는 수위 양")]
        [SerializeField] private float water_Level_Rise_Amount = 0.5f;
        
        [Tooltip("게임 시작시 초기 강물 수위")]
        [SerializeField] private float initial_Water_Level = 0.5f;
        
        [Tooltip("강물 수위 상승 속도 가속 여부")]
        [SerializeField] private bool enable_Water_Acceleration = true;
        
        [Tooltip("가속 시작 시점 (게임 시작 후 경과 시간, 분)")]
        [SerializeField] private float acceleration_Start_Time = 10.0f;
        
        [Tooltip("가속 배율")]
        [SerializeField] private float acceleration_Multiplier = 1.2f;

        [Header("Player Settings")]
        [Tooltip("기본 제방 시작 높이")]
        [SerializeField] private float default_Dike_Height = 1.0f;
        
        [Tooltip("최대 제방 높이 한계")]
        [SerializeField] private float maximum_Dike_Height = 50.0f;
        
        [Tooltip("기본 제방 내구도")]
        [SerializeField] private int default_Dike_Durability = 100;
        
        [Tooltip("플레이어 이동 속도 (지상)")]
        [SerializeField] private float surface_Movement_Speed = 5.0f;
        
        [Tooltip("플레이어 이동 속도 (지하)")]
        [SerializeField] private float underground_Movement_Speed = 3.0f;

        [Header("Network Settings")]
        [Tooltip("최대 허용 네트워크 지연시간 (ms)")]
        [SerializeField] private float maximum_Network_Latency = 200.0f;
        
        [Tooltip("연결 시도 최대 시간 (초)")]
        [SerializeField] private float connection_Timeout = 10.0f;
        
        [Tooltip("재연결 시도 간격 (초)")]
        [SerializeField] private float reconnection_Interval = 2.0f;
        
        [Tooltip("최대 재연결 시도 횟수")]
        [SerializeField] private int maximum_Reconnection_Attempts = 5;

        [Header("Alert System")]
        [Tooltip("주의 단계 임계값 (제방 높이 대비 비율)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float caution_Threshold = 0.7f;
        
        [Tooltip("경고 단계 임계값 (제방 높이 대비 비율)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float warning_Threshold = 0.85f;
        
        [Tooltip("위험 단계 임계값 (제방 높이 대비 비율)")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float critical_Threshold = 0.95f;

        [Header("Performance Settings")]
        [Tooltip("시스템 업데이트 주기 (초, 0이면 매 프레임)")]
        [SerializeField] private float system_Update_Interval = 0.0f;
        
        [Tooltip("네트워크 동기화 주기 (초)")]
        [SerializeField] private float network_Sync_Interval = 0.1f;
        
        [Tooltip("자동 가비지 컬렉션 주기 (초)")]
        [SerializeField] private float gc_Interval = 60.0f;

        [Header("Debug Settings")]
        [Tooltip("디버그 모드 활성화")]
        [SerializeField] private bool enable_Debug_Mode = false;
        
        [Tooltip("디버그 로그 레벨")]
        [SerializeField] private LogType debug_Log_Level = LogType.Log;
        
        [Tooltip("네트워크 디버그 활성화")]
        [SerializeField] private bool enable_Network_Debug = false;

        // Properties for safe read-only access
        public float Game_Start_Delay => game_Start_Delay;
        public float Game_End_Duration => game_End_Duration;
        public float Maximum_Game_Duration => maximum_Game_Duration;
        public float Water_Level_Rise_Interval => water_Level_Rise_Interval;
        public float Water_Level_Rise_Amount => water_Level_Rise_Amount;
        public float Initial_Water_Level => initial_Water_Level;
        public bool Enable_Water_Acceleration => enable_Water_Acceleration;
        public float Acceleration_Start_Time => acceleration_Start_Time;
        public float Acceleration_Multiplier => acceleration_Multiplier;
        public float Default_Dike_Height => default_Dike_Height;
        public float Maximum_Dike_Height => maximum_Dike_Height;
        public int Default_Dike_Durability => default_Dike_Durability;
        public float Surface_Movement_Speed => surface_Movement_Speed;
        public float Underground_Movement_Speed => underground_Movement_Speed;
        public float Maximum_Network_Latency => maximum_Network_Latency;
        public float Connection_Timeout => connection_Timeout;
        public float Reconnection_Interval => reconnection_Interval;
        public int Maximum_Reconnection_Attempts => maximum_Reconnection_Attempts;
        public float Caution_Threshold => caution_Threshold;
        public float Warning_Threshold => warning_Threshold;
        public float Critical_Threshold => critical_Threshold;
        public float System_Update_Interval => system_Update_Interval;
        public float Network_Sync_Interval => network_Sync_Interval;
        public float GC_Interval => gc_Interval;
        public bool Enable_Debug_Mode => enable_Debug_Mode;
        public LogType Debug_Log_Level => debug_Log_Level;
        public bool Enable_Network_Debug => enable_Network_Debug;

        /// <summary>
        /// 강물 수위 상승 간격을 동적으로 계산 (가속 고려)
        /// </summary>
        /// <param name="elapsed_Game_Time">경과된 게임 시간 (초)</param>
        /// <returns>현재 수위 상승 간격</returns>
        public float Calculate_Current_Water_Rise_Interval(float elapsed_Game_Time)
        {
            if (!enable_Water_Acceleration)
                return water_Level_Rise_Interval;

            float elapsed_Minutes = elapsed_Game_Time / 60.0f;
            
            if (elapsed_Minutes >= acceleration_Start_Time)
            {
                float acceleration_Factor = Mathf.Pow(acceleration_Multiplier, 
                    (elapsed_Minutes - acceleration_Start_Time) / acceleration_Start_Time);
                return water_Level_Rise_Interval / acceleration_Factor;
            }
            
            return water_Level_Rise_Interval;
        }

        /// <summary>
        /// 현재 수위에 따른 경고 레벨 계산
        /// </summary>
        /// <param name="current_Water_Level">현재 강물 수위</param>
        /// <param name="Dike_Height">제방 높이</param>
        /// <returns>경고 레벨</returns>
        public Water_Level_Alert Calculate_Water_Alert_Level(float current_Water_Level, float Dike_Height)
        {
            if (Dike_Height <= 0)
                return Water_Level_Alert.Flooding;

            float ratio = current_Water_Level / Dike_Height;
            
            if (ratio >= 1.0f)
                return Water_Level_Alert.Flooding;
            else if (ratio >= critical_Threshold)
                return Water_Level_Alert.Critical;
            else if (ratio >= warning_Threshold)
                return Water_Level_Alert.Warning;
            else if (ratio >= caution_Threshold)
                return Water_Level_Alert.Caution;
            else
                return Water_Level_Alert.Safe;
        }

        /// <summary>
        /// 플레이어 위치에 따른 이동 속도 반환
        /// </summary>
        /// <param name="location">현재 플레이어 위치</param>
        /// <returns>해당 위치에서의 이동 속도</returns>
        public float Get_Movement_Speed_For_Location(Player_Location location)
        {
            return location switch
            {
                Player_Location.Surface_Area => surface_Movement_Speed,
                Player_Location.Underground_Mine => underground_Movement_Speed,
                Player_Location.Transition_Area => (surface_Movement_Speed + underground_Movement_Speed) * 0.5f,
                _ => surface_Movement_Speed
            };
        }

        /// <summary>
        /// 네트워크 연결이 안정적인지 확인
        /// </summary>
        /// <param name="latency">현재 지연시간 (ms)</param>
        /// <returns>안정적인 연결 시 true</returns>
        public bool Is_Network_Connection_Stable(float latency)
        {
            return latency <= maximum_Network_Latency;
        }

        /// <summary>
        /// 설정값 유효성 검사 (Unity Editor에서 호출)
        /// </summary>
        private void OnValidate()
        {
            // 음수 방지
            game_Start_Delay = Mathf.Max(0, game_Start_Delay);
            game_End_Duration = Mathf.Max(0, game_End_Duration);
            maximum_Game_Duration = Mathf.Max(0, maximum_Game_Duration);
            
            water_Level_Rise_Interval = Mathf.Max(0.1f, water_Level_Rise_Interval);
            water_Level_Rise_Amount = Mathf.Max(0.01f, water_Level_Rise_Amount);
            initial_Water_Level = Mathf.Max(0, initial_Water_Level);
            
            default_Dike_Height = Mathf.Max(0.1f, default_Dike_Height);
            maximum_Dike_Height = Mathf.Max(default_Dike_Height, maximum_Dike_Height);
            default_Dike_Durability = Mathf.Max(1, default_Dike_Durability);
            
            surface_Movement_Speed = Mathf.Max(0.1f, surface_Movement_Speed);
            underground_Movement_Speed = Mathf.Max(0.1f, underground_Movement_Speed);
            
            maximum_Network_Latency = Mathf.Max(10.0f, maximum_Network_Latency);
            connection_Timeout = Mathf.Max(1.0f, connection_Timeout);
            reconnection_Interval = Mathf.Max(0.5f, reconnection_Interval);
            maximum_Reconnection_Attempts = Mathf.Max(1, maximum_Reconnection_Attempts);
            
            // 임계값 순서 보장
            if (warning_Threshold <= caution_Threshold)
                warning_Threshold = caution_Threshold + 0.1f;
            if (critical_Threshold <= warning_Threshold)
                critical_Threshold = warning_Threshold + 0.05f;
            
            system_Update_Interval = Mathf.Max(0, system_Update_Interval);
            network_Sync_Interval = Mathf.Max(0.01f, network_Sync_Interval);
            gc_Interval = Mathf.Max(10.0f, gc_Interval);
        }

        /// <summary>
        /// 설정 정보를 문자열로 출력 (디버깅용)
        /// </summary>
        public override string ToString()
        {
            return $"Game Config: Water Rise={water_Level_Rise_Interval}s, " +
                   $"Initial Level={initial_Water_Level}m, " +
                   $"Dike Start={default_Dike_Height}m, " +
                   $"Max Latency={maximum_Network_Latency}ms";
        }
    }

    /// <summary>
    /// 게임 설정의 프리셋을 정의하는 구조체
    /// </summary>
    [System.Serializable]
    public struct Game_Config_Preset
    {
        public string preset_Name;
        public string preset_Description;
        public float water_Rise_Interval;
        public float water_Rise_Amount;
        public bool enable_Acceleration;
        public float maximum_Game_Duration;

        public Game_Config_Preset(string name, string description, 
            float riseInterval, float riseAmount, bool acceleration, float maxDuration)
        {
            preset_Name = name;
            preset_Description = description;
            water_Rise_Interval = riseInterval;
            water_Rise_Amount = riseAmount;
            enable_Acceleration = acceleration;
            maximum_Game_Duration = maxDuration;
        }

        // 기본 프리셋들
        public static Game_Config_Preset Easy => new("Easy", "느린 수위 상승, 긴 게임", 45.0f, 0.3f, false, 0.0f);
        public static Game_Config_Preset Normal => new("Normal", "표준 게임 설정", 30.0f, 0.5f, true, 0.0f);
        public static Game_Config_Preset Hard => new("Hard", "빠른 수위 상승, 도전적", 20.0f, 0.7f, true, 1200.0f);
        public static Game_Config_Preset Blitz => new("Blitz", "매우 빠른 게임", 10.0f, 1.0f, true, 600.0f);
    }
}
using System.Collections;
using UnityEngine;
using TheFloodGame.Core.Data;
using TheFloodGame.Core.Interfaces;

/// <summary>
/// 강물 수위 상승 타이머 시스템 - 게임 진행에 따른 수위 관리
/// Input: 게임 설정값, 게임 상태 변화
/// Output: 수위 상승 이벤트, 경고 단계 알림
/// Type: MonoBehaviour System
/// </summary>

namespace TheFloodGame.Core.GameManager
{
    public class Timer_System : MonoBehaviour, IGameSystem, IConfigurableGameSystem
    {
        [Header("Timer System Settings")]
        [SerializeField] private Game_Config current_Game_Config;
        [SerializeField] private bool auto_Start_On_Game_Begin = true;
        
        [Header("Current Water Level Status")]
        [SerializeField] private float current_Water_Level;
        [SerializeField] private float total_Elapsed_Time;
        [SerializeField] private float time_Until_Next_Rise;
        [SerializeField] private bool is_Timer_Running;

        [Header("Debug Information")]
        [SerializeField] private int total_Rise_Count;
        [SerializeField] private float current_Rise_Interval;
        [SerializeField] private bool acceleration_Active;

        // IGameSystem properties
        public string System_Name => "Timer_System";
        public System_Priority Priority => System_Priority.High;
        public bool Is_System_Active { get; private set; }

        // Timer coroutine reference
        private Coroutine water_Rise_Coroutine;

        // Events
        public event System.Action<IGameSystem, string> On_System_Error;
        public event System.Action<IGameSystem, bool> On_System_State_Changed;

        #region Unity Lifecycle

        private void Awake()
        {
            if (current_Game_Config == null)
            {
                Debug.LogError("[Timer_System] Game_Config not assigned! Timer system cannot function properly.");
                return;
            }

            Initialize_System();
        }

        private void Start()
        {
            Subscribe_To_Events();
        }

        private void OnDestroy()
        {
            Unsubscribe_From_Events();
            Cleanup_System();
        }

        private void OnValidate()
        {
            // 값 유효성 검사
            current_Water_Level = Mathf.Max(0, current_Water_Level);
            total_Elapsed_Time = Mathf.Max(0, total_Elapsed_Time);
            time_Until_Next_Rise = Mathf.Max(0, time_Until_Next_Rise);
        }

        #endregion

        #region IGameSystem Implementation

        public bool Is_System_Initialized()
        {
            return current_Game_Config != null && Is_System_Active;
        }

        public bool Initialize_System()
        {
            try
            {
                if (current_Game_Config == null)
                {
                    Trigger_System_Error("Game_Config is null during initialization");
                    return false;
                }

                Reset_Timer_Values();
                Is_System_Active = true;

                Game_Events.Trigger_System_Initialized(System_Name);
                Debug.Log($"[{System_Name}] System initialized successfully");
                
                return true;
            }
            catch (System.Exception ex)
            {
                Trigger_System_Error($"Initialization failed: {ex.Message}");
                return false;
            }
        }

        public void Start_System()
        {
            if (!Is_System_Initialized())
            {
                Trigger_System_Error("Cannot start system - not properly initialized");
                return;
            }

            if (auto_Start_On_Game_Begin)
            {
                Begin_Water_Rise_Timer();
            }

            On_System_State_Changed?.Invoke(this, true);
            Debug.Log($"[{System_Name}] System started");
        }

        public void Update_System(float delta_Time)
        {
            if (!is_Timer_Running) return;

            total_Elapsed_Time += delta_Time;
            
            // 가속도 상태 업데이트
            Update_Acceleration_Status();
            
            // 현재 수위 상승 간격 계산
            current_Rise_Interval = current_Game_Config.Calculate_Current_Water_Rise_Interval(total_Elapsed_Time);
        }

        public void Pause_System()
        {
            if (water_Rise_Coroutine != null)
            {
                StopCoroutine(water_Rise_Coroutine);
                water_Rise_Coroutine = null;
            }
            
            is_Timer_Running = false;
            Debug.Log($"[{System_Name}] System paused");
        }

        public void Resume_System()
        {
            if (Is_System_Active && !is_Timer_Running)
            {
                Begin_Water_Rise_Timer();
            }
            
            Debug.Log($"[{System_Name}] System resumed");
        }

        public void Stop_System()
        {
            Pause_System();
            Is_System_Active = false;
            
            On_System_State_Changed?.Invoke(this, false);
            Debug.Log($"[{System_Name}] System stopped");
        }

        public void Cleanup_System()
        {
            Stop_System();
            Unsubscribe_From_Events();
            
            Debug.Log($"[{System_Name}] System cleaned up");
        }

        public void Reset_System()
        {
            Stop_System();
            Reset_Timer_Values();
            
            if (Is_System_Active)
            {
                Start_System();
            }
            
            Debug.Log($"[{System_Name}] System reset");
        }

        public bool Check_System_Health()
        {
            return Is_System_Active && 
                   current_Game_Config != null && 
                   current_Water_Level >= 0 && 
                   total_Elapsed_Time >= 0;
        }

        #endregion

        #region IConfigurableGameSystem Implementation

        public void Apply_System_Configuration(object config_Data)
        {
            if (config_Data is Game_Config game_Config)
            {
                current_Game_Config = game_Config;
                
                // 설정이 변경되면 타이머 재시작
                if (is_Timer_Running)
                {
                    Reset_System();
                }
                
                Debug.Log($"[{System_Name}] Configuration applied");
            }
            else
            {
                Trigger_System_Error("Invalid configuration data type");
            }
        }

        public object Get_System_Configuration()
        {
            return current_Game_Config;
        }

        #endregion

        #region Water Level Management

        /// <summary>
        /// 강물 수위 상승 타이머 시작
        /// </summary>
        public void Begin_Water_Rise_Timer()
        {
            if (water_Rise_Coroutine != null)
            {
                StopCoroutine(water_Rise_Coroutine);
            }

            is_Timer_Running = true;
            time_Until_Next_Rise = current_Game_Config.Water_Level_Rise_Interval;
            water_Rise_Coroutine = StartCoroutine(Water_Rise_Timer_Coroutine());
            
            Debug.Log($"[{System_Name}] Water rise timer started");
        }

        /// <summary>
        /// 강물 수위 상승 타이머 중지
        /// </summary>
        public void Stop_Water_Rise_Timer()
        {
            if (water_Rise_Coroutine != null)
            {
                StopCoroutine(water_Rise_Coroutine);
                water_Rise_Coroutine = null;
            }
            
            is_Timer_Running = false;
            Debug.Log($"[{System_Name}] Water rise timer stopped");
        }

        /// <summary>
        /// 수위 상승 타이머 코루틴
        /// </summary>
        private IEnumerator Water_Rise_Timer_Coroutine()
        {
            while (is_Timer_Running)
            {
                // 현재 설정에 따른 대기 시간 계산
                float wait_Time = current_Game_Config.Calculate_Current_Water_Rise_Interval(total_Elapsed_Time);
                time_Until_Next_Rise = wait_Time;
                
                // 대기
                while (time_Until_Next_Rise > 0 && is_Timer_Running)
                {
                    yield return null;
                    time_Until_Next_Rise -= Time.deltaTime;
                }
                
                // 수위 상승 실행
                if (is_Timer_Running)
                {
                    Execute_Water_Level_Rise();
                }
            }
        }

        /// <summary>
        /// 실제 수위 상승 실행
        /// </summary>
        private void Execute_Water_Level_Rise()
        {
            float rise_Amount = current_Game_Config.Water_Level_Rise_Amount;
            current_Water_Level += rise_Amount;
            total_Rise_Count++;
            
            // 수위 상승 이벤트 발생
            Game_Events.Trigger_Water_Level_Risen(current_Water_Level);
            
            Debug.Log($"[{System_Name}] Water level risen to {current_Water_Level:F2}m (Rise #{total_Rise_Count})");
            
            // 각 플레이어에 대한 경고 레벨 체크는 Player_Manager에서 처리
        }

        /// <summary>
        /// 가속도 상태 업데이트
        /// </summary>
        private void Update_Acceleration_Status()
        {
            if (!current_Game_Config.Enable_Water_Acceleration) return;
            
            float elapsed_Minutes = total_Elapsed_Time / 60.0f;
            bool was_Acceleration_Active = acceleration_Active;
            acceleration_Active = elapsed_Minutes >= current_Game_Config.Acceleration_Start_Time;
            
            if (acceleration_Active && !was_Acceleration_Active)
            {
                Debug.Log($"[{System_Name}] Water level acceleration activated at {elapsed_Minutes:F1} minutes");
            }
        }

        /// <summary>
        /// 타이머 값들 초기화
        /// </summary>
        private void Reset_Timer_Values()
        {
            if (current_Game_Config != null)
            {
                current_Water_Level = current_Game_Config.Initial_Water_Level;
                current_Rise_Interval = current_Game_Config.Water_Level_Rise_Interval;
            }
            else
            {
                current_Water_Level = 0.5f; // 기본값
                current_Rise_Interval = 30.0f; // 기본값
            }
            
            total_Elapsed_Time = 0.0f;
            time_Until_Next_Rise = 0.0f;
            total_Rise_Count = 0;
            acceleration_Active = false;
            is_Timer_Running = false;
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// 게임 이벤트 구독
        /// </summary>
        private void Subscribe_To_Events()
        {
            Game_Events.On_Game_State_Changed += Handle_Game_State_Changed;
            Game_Events.On_Game_Paused += Handle_Game_Paused;
            Game_Events.On_Game_Reset += Handle_Game_Reset;
        }

        /// <summary>
        /// 게임 이벤트 구독 해제
        /// </summary>
        private void Unsubscribe_From_Events()
        {
            Game_Events.On_Game_State_Changed -= Handle_Game_State_Changed;
            Game_Events.On_Game_Paused -= Handle_Game_Paused;
            Game_Events.On_Game_Reset -= Handle_Game_Reset;
        }

        /// <summary>
        /// 게임 상태 변경 처리
        /// </summary>
        private void Handle_Game_State_Changed(Game_State old_State, Game_State new_State)
        {
            switch (new_State)
            {
                case Game_State.In_Progress:
                    if (auto_Start_On_Game_Begin)
                        Begin_Water_Rise_Timer();
                    break;
                    
                case Game_State.Game_Paused:
                    Pause_System();
                    break;
                    
                case Game_State.Game_Ending:
                case Game_State.Game_Over:
                    Stop_Water_Rise_Timer();
                    break;
            }
        }

        /// <summary>
        /// 게임 일시정지 처리
        /// </summary>
        private void Handle_Game_Paused(bool is_Paused)
        {
            if (is_Paused)
                Pause_System();
            else
                Resume_System();
        }

        /// <summary>
        /// 게임 리셋 처리
        /// </summary>
        private void Handle_Game_Reset()
        {
            Reset_System();
        }

        #endregion

        #region Public Properties and Methods

        /// <summary>
        /// 현재 강물 수위 반환
        /// </summary>
        public float Get_Current_Water_Level()
        {
            return current_Water_Level;
        }

        /// <summary>
        /// 다음 수위 상승까지 남은 시간 반환
        /// </summary>
        public float Get_Time_Until_Next_Rise()
        {
            return time_Until_Next_Rise;
        }

        /// <summary>
        /// 총 경과 시간 반환
        /// </summary>
        public float Get_Total_Elapsed_Time()
        {
            return total_Elapsed_Time;
        }

        /// <summary>
        /// 타이머 실행 상태 확인
        /// </summary>
        public bool Is_Timer_Running()
        {
            return is_Timer_Running;
        }

        /// <summary>
        /// 현재 수위 상승 간격 반환
        /// </summary>
        public float Get_Current_Rise_Interval()
        {
            return current_Rise_Interval;
        }

        /// <summary>
        /// 가속도 활성화 상태 확인
        /// </summary>
        public bool Is_Acceleration_Active()
        {
            return acceleration_Active;
        }

        /// <summary>
        /// 특정 시점으로 강제 수위 설정 (디버그/치트용)
        /// </summary>
        public void Set_Water_Level(float new_Level)
        {
            if (current_Game_Config.Enable_Debug_Mode)
            {
                current_Water_Level = Mathf.Max(0, new_Level);
                Game_Events.Trigger_Water_Level_Risen(current_Water_Level);
                Debug.Log($"[{System_Name}] Water level manually set to {current_Water_Level:F2}m");
            }
            else
            {
                Debug.LogWarning($"[{System_Name}] Cannot set water level - Debug mode is disabled");
            }
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// 시스템 오류 트리거
        /// </summary>
        private void Trigger_System_Error(string error_Message)
        {
            string full_Error = $"[{System_Name}] {error_Message}";
            Debug.LogError(full_Error);
            
            On_System_Error?.Invoke(this, error_Message);
            Game_Events.Trigger_System_Error(System_Name, error_Message);
        }

        #endregion

        #region Debug and Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || current_Game_Config == null) return;

            // 현재 수위를 시각화
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(
                new Vector3(0, current_Water_Level * 0.5f, 0), 
                new Vector3(20, current_Water_Level, 20)
            );

            // 초기 수위 표시
            Gizmos.color = Color.cyan;
            float initial_Level = current_Game_Config.Initial_Water_Level;
            Gizmos.DrawWireCube(
                new Vector3(0, initial_Level * 0.5f, 0), 
                new Vector3(22, initial_Level, 22)
            );
        }

        #endregion
    }
}
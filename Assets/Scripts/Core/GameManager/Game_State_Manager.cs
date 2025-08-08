using System.Collections;
using UnityEngine;
using TheFloodGame.Core.Data;
using TheFloodGame.Core.Interfaces;

/// <summary>
/// 게임 상태 전환 관리 시스템 - 게임의 전체적인 흐름 제어
/// Input: 게임 시작/종료/일시정지 요청, 플레이어 상태 변화
/// Output: 게임 상태 변경 이벤트, 상태 전환 관리
/// Type: MonoBehaviour System
/// </summary>

namespace TheFloodGame.Core.GameManager
{
    public class Game_State_Manager : MonoBehaviour, IGameSystem, ISaveableGameSystem
    {
        [Header("Game State Settings")]
        [SerializeField] private Game_Config current_Game_Config;
        [SerializeField] private Game_State current_Game_State = Game_State.Waiting_For_Players;
        [SerializeField] private Game_State previous_Game_State = Game_State.Waiting_For_Players;
        
        [Header("State Transition Settings")]
        [SerializeField] private bool allow_State_Transitions = true;
        [SerializeField] private float state_Transition_Delay = 0.5f;
        
        [Header("Auto-Progression Settings")]
        [SerializeField] private bool auto_Progress_To_Starting = true;
        [SerializeField] private bool auto_Progress_To_Playing = true;
        [SerializeField] private bool auto_Handle_Game_End = true;

        [Header("Debug Information")]
        [SerializeField] private float time_In_Current_State;
        [SerializeField] private int total_State_Changes;
        [SerializeField] private bool is_State_Locked;

        // IGameSystem properties
        public string System_Name => "Game_State_Manager";
        public System_Priority Priority => System_Priority.Critical;
        public bool Is_System_Active { get; private set; }

        // State transition coroutine
        private Coroutine state_Transition_Coroutine;

        // Events
        public event System.Action<IGameSystem, string> On_System_Error;
        public event System.Action<IGameSystem, bool> On_System_State_Changed;

        #region Unity Lifecycle

        private void Awake()
        {
            Initialize_System();
        }

        private void Start()
        {
            Subscribe_To_Events();
        }

        private void Update()
        {
            if (Is_System_Active)
            {
                time_In_Current_State += Time.deltaTime;
                
                // 자동 진행 체크
                Check_Auto_State_Progression();
            }
        }

        private void OnDestroy()
        {
            Unsubscribe_From_Events();
            Cleanup_System();
        }

        #endregion

        #region IGameSystem Implementation

        public bool Is_System_Initialized()
        {
            return Is_System_Active;
        }

        public bool Initialize_System()
        {
            try
            {
                current_Game_State = Game_State.Waiting_For_Players;
                previous_Game_State = Game_State.Waiting_For_Players;
                time_In_Current_State = 0.0f;
                total_State_Changes = 0;
                is_State_Locked = false;
                allow_State_Transitions = true;

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

            On_System_State_Changed?.Invoke(this, true);
            Debug.Log($"[{System_Name}] System started");
        }

        public void Update_System(float delta_Time)
        {
            time_In_Current_State += delta_Time;
        }

        public void Pause_System()
        {
            allow_State_Transitions = false;
            
            if (state_Transition_Coroutine != null)
            {
                StopCoroutine(state_Transition_Coroutine);
                state_Transition_Coroutine = null;
            }
            
            Debug.Log($"[{System_Name}] System paused");
        }

        public void Resume_System()
        {
            allow_State_Transitions = true;
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
            
            current_Game_State = Game_State.Waiting_For_Players;
            previous_Game_State = Game_State.Waiting_For_Players;
            time_In_Current_State = 0.0f;
            total_State_Changes = 0;
            is_State_Locked = false;
            
            if (Is_System_Active)
            {
                Start_System();
            }
            
            // 게임 리셋 이벤트 발생
            Game_Events.Trigger_Game_Reset();
            
            Debug.Log($"[{System_Name}] System reset");
        }

        public bool Check_System_Health()
        {
            return Is_System_Active && 
                   !is_State_Locked && 
                   System.Enum.IsDefined(typeof(Game_State), current_Game_State);
        }

        #endregion

        #region ISaveableGameSystem Implementation

        public string Serialize_System_Data()
        {
            var save_Data = new Game_State_Save_Data
            {
                current_State = current_Game_State,
                previous_State = previous_Game_State,
                time_In_State = time_In_Current_State,
                total_Changes = total_State_Changes,
                is_Locked = is_State_Locked
            };
            
            return JsonUtility.ToJson(save_Data);
        }

        public bool Deserialize_System_Data(string serialized_Data)
        {
            try
            {
                var save_Data = JsonUtility.FromJson<Game_State_Save_Data>(serialized_Data);
                
                current_Game_State = save_Data.current_State;
                previous_Game_State = save_Data.previous_State;
                time_In_Current_State = save_Data.time_In_State;
                total_State_Changes = save_Data.total_Changes;
                is_State_Locked = save_Data.is_Locked;
                
                Debug.Log($"[{System_Name}] System data deserialized successfully");
                return true;
            }
            catch (System.Exception ex)
            {
                Trigger_System_Error($"Failed to deserialize data: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Game State Management

        /// <summary>
        /// 게임 상태를 변경
        /// </summary>
        /// <param name="new_State">새로운 게임 상태</param>
        /// <param name="force_Change">강제 변경 여부</param>
        /// <returns>상태 변경 성공 시 true</returns>
        public bool Change_Game_State(Game_State new_State, bool force_Change = false)
        {
            if (!allow_State_Transitions && !force_Change)
            {
                Debug.LogWarning($"[{System_Name}] State transitions are currently disabled");
                return false;
            }

            if (is_State_Locked && !force_Change)
            {
                Debug.LogWarning($"[{System_Name}] State is currently locked");
                return false;
            }

            if (current_Game_State == new_State)
            {
                Debug.LogWarning($"[{System_Name}] Already in state {new_State}");
                return false;
            }

            if (!Is_Valid_State_Transition(current_Game_State, new_State) && !force_Change)
            {
                Debug.LogWarning($"[{System_Name}] Invalid state transition from {current_Game_State} to {new_State}");
                return false;
            }

            // 상태 전환 실행
            Execute_State_Change(new_State);
            return true;
        }

        /// <summary>
        /// 실제 상태 변경 실행
        /// </summary>
        private void Execute_State_Change(Game_State new_State)
        {
            Game_State old_State = current_Game_State;
            
            // 이전 상태 종료 처리
            Exit_Current_State();
            
            // 상태 업데이트
            previous_Game_State = current_Game_State;
            current_Game_State = new_State;
            time_In_Current_State = 0.0f;
            total_State_Changes++;
            
            // 새 상태 진입 처리
            Enter_New_State(new_State);
            
            // 이벤트 발생
            Game_Events.Trigger_Game_State_Changed(old_State, new_State);
            
            Debug.Log($"[{System_Name}] State changed from {old_State} to {new_State}");
        }

        /// <summary>
        /// 지연된 상태 변경
        /// </summary>
        public void Change_Game_State_Delayed(Game_State new_State, float delay)
        {
            if (state_Transition_Coroutine != null)
            {
                StopCoroutine(state_Transition_Coroutine);
            }
            
            state_Transition_Coroutine = StartCoroutine(Delayed_State_Change_Coroutine(new_State, delay));
        }

        /// <summary>
        /// 지연된 상태 변경 코루틴
        /// </summary>
        private IEnumerator Delayed_State_Change_Coroutine(Game_State new_State, float delay)
        {
            yield return new WaitForSeconds(delay);
            Change_Game_State(new_State);
            state_Transition_Coroutine = null;
        }

        /// <summary>
        /// 상태 전환의 유효성 검사
        /// </summary>
        private bool Is_Valid_State_Transition(Game_State from_State, Game_State to_State)
        {
            return from_State switch
            {
                Game_State.Waiting_For_Players => 
                    to_State is Game_State.Game_Starting,
                
                Game_State.Game_Starting => 
                    to_State is Game_State.In_Progress or Game_State.Waiting_For_Players,
                
                Game_State.In_Progress => 
                    to_State is Game_State.Game_Paused or Game_State.Game_Ending,
                
                Game_State.Game_Paused => 
                    to_State is Game_State.In_Progress or Game_State.Game_Ending,
                
                Game_State.Game_Ending => 
                    to_State is Game_State.Game_Over,
                
                Game_State.Game_Over => 
                    to_State is Game_State.Waiting_For_Players,
                
                _ => false
            };
        }

        /// <summary>
        /// 현재 상태 종료 처리
        /// </summary>
        private void Exit_Current_State()
        {
            switch (current_Game_State)
            {
                case Game_State.In_Progress:
                    // 게임 플레이 중 상태 종료
                    break;
                    
                case Game_State.Game_Paused:
                    // 일시정지 상태 종료
                    break;
            }
        }

        /// <summary>
        /// 새 상태 진입 처리
        /// </summary>
        private void Enter_New_State(Game_State new_State)
        {
            switch (new_State)
            {
                case Game_State.Game_Starting:
                    Handle_Game_Starting();
                    break;
                    
                case Game_State.In_Progress:
                    Handle_Game_Started();
                    break;
                    
                case Game_State.Game_Paused:
                    Handle_Game_Paused();
                    break;
                    
                case Game_State.Game_Ending:
                    Handle_Game_Ending();
                    break;
                    
                case Game_State.Game_Over:
                    Handle_Game_Over();
                    break;
            }
        }

        #endregion

        #region State-Specific Handlers

        /// <summary>
        /// 게임 시작 준비 상태 처리
        /// </summary>
        private void Handle_Game_Starting()
        {
            if (current_Game_Config != null && auto_Progress_To_Playing)
            {
                float delay = current_Game_Config.Game_Start_Delay;
                Change_Game_State_Delayed(Game_State.In_Progress, delay);
            }
        }

        /// <summary>
        /// 게임 진행 중 상태 처리
        /// </summary>
        private void Handle_Game_Started()
        {
            // 게임 시작 이벤트는 Game_Events에서 자동으로 발생
        }

        /// <summary>
        /// 게임 일시정지 상태 처리
        /// </summary>
        private void Handle_Game_Paused()
        {
            Game_Events.Trigger_Game_Paused(true);
        }

        /// <summary>
        /// 게임 종료 처리 중 상태 처리
        /// </summary>
        private void Handle_Game_Ending()
        {
            if (current_Game_Config != null && auto_Handle_Game_End)
            {
                float delay = current_Game_Config.Game_End_Duration;
                Change_Game_State_Delayed(Game_State.Game_Over, delay);
            }
        }

        /// <summary>
        /// 게임 완전 종료 상태 처리
        /// </summary>
        private void Handle_Game_Over()
        {
            // 게임 종료 후 정리 작업
        }

        #endregion

        #region Auto-Progression Logic

        /// <summary>
        /// 자동 상태 진행 체크
        /// </summary>
        private void Check_Auto_State_Progression()
        {
            switch (current_Game_State)
            {
                case Game_State.Waiting_For_Players:
                    Check_Players_Ready_For_Start();
                    break;
                    
                case Game_State.In_Progress:
                    Check_Game_End_Conditions();
                    break;
            }
        }

        /// <summary>
        /// 플레이어들이 게임 시작 준비되었는지 체크
        /// </summary>
        private void Check_Players_Ready_For_Start()
        {
            if (!auto_Progress_To_Starting) return;

            // Player_Manager에서 모든 플레이어가 준비되었는지 확인
            // 이는 Player_Manager 구현 후 완성될 예정
        }

        /// <summary>
        /// 게임 종료 조건 체크
        /// </summary>
        private void Check_Game_End_Conditions()
        {
            if (current_Game_Config != null && current_Game_Config.Maximum_Game_Duration > 0)
            {
                if (time_In_Current_State >= current_Game_Config.Maximum_Game_Duration)
                {
                    Change_Game_State(Game_State.Game_Ending);
                }
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// 게임 이벤트 구독
        /// </summary>
        private void Subscribe_To_Events()
        {
            Game_Events.On_Player_Victory += Handle_Player_Victory;
            Game_Events.On_Player_Defeat += Handle_Player_Defeat;
            Game_Events.On_Flooding_Occurred += Handle_Flooding_Occurred;
        }

        /// <summary>
        /// 게임 이벤트 구독 해제
        /// </summary>
        private void Unsubscribe_From_Events()
        {
            Game_Events.On_Player_Victory -= Handle_Player_Victory;
            Game_Events.On_Player_Defeat -= Handle_Player_Defeat;
            Game_Events.On_Flooding_Occurred -= Handle_Flooding_Occurred;
        }

        /// <summary>
        /// 플레이어 승리 처리
        /// </summary>
        private void Handle_Player_Victory(int player_ID, Victory_Condition condition)
        {
            if (current_Game_State == Game_State.In_Progress)
            {
                Change_Game_State(Game_State.Game_Ending);
            }
        }

        /// <summary>
        /// 플레이어 패배 처리
        /// </summary>
        private void Handle_Player_Defeat(int player_ID)
        {
            if (current_Game_State == Game_State.In_Progress)
            {
                Change_Game_State(Game_State.Game_Ending);
            }
        }

        /// <summary>
        /// 범람 발생 처리
        /// </summary>
        private void Handle_Flooding_Occurred(int player_ID)
        {
            if (current_Game_State == Game_State.In_Progress)
            {
                Change_Game_State(Game_State.Game_Ending);
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 현재 게임 상태 반환
        /// </summary>
        public Game_State Get_Current_Game_State()
        {
            return current_Game_State;
        }

        /// <summary>
        /// 이전 게임 상태 반환
        /// </summary>
        public Game_State Get_Previous_Game_State()
        {
            return previous_Game_State;
        }

        /// <summary>
        /// 현재 상태에서의 경과 시간 반환
        /// </summary>
        public float Get_Time_In_Current_State()
        {
            return time_In_Current_State;
        }

        /// <summary>
        /// 게임이 진행 중인지 확인
        /// </summary>
        public bool Is_Game_In_Progress()
        {
            return current_Game_State == Game_State.In_Progress;
        }

        /// <summary>
        /// 게임이 일시정지되었는지 확인
        /// </summary>
        public bool Is_Game_Paused()
        {
            return current_Game_State == Game_State.Game_Paused;
        }

        /// <summary>
        /// 게임이 종료되었는지 확인
        /// </summary>
        public bool Is_Game_Over()
        {
            return current_Game_State == Game_State.Game_Over;
        }

        /// <summary>
        /// 상태 잠금 설정
        /// </summary>
        public void Lock_State_Transitions(bool lock_State)
        {
            is_State_Locked = lock_State;
            Debug.Log($"[{System_Name}] State transitions {(lock_State ? "locked" : "unlocked")}");
        }

        /// <summary>
        /// 게임 일시정지/재개 토글
        /// </summary>
        public void Toggle_Game_Pause()
        {
            if (current_Game_State == Game_State.In_Progress)
            {
                Change_Game_State(Game_State.Game_Paused);
            }
            else if (current_Game_State == Game_State.Game_Paused)
            {
                Change_Game_State(Game_State.In_Progress);
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

        #region Data Structures

        /// <summary>
        /// 게임 상태 저장 데이터 구조
        /// </summary>
        [System.Serializable]
        private struct Game_State_Save_Data
        {
            public Game_State current_State;
            public Game_State previous_State;
            public float time_In_State;
            public int total_Changes;
            public bool is_Locked;
        }

        #endregion
    }
}
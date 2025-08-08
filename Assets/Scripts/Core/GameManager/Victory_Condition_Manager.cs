using System.Collections.Generic;
using UnityEngine;
using TheFloodGame.Core.Data;
using TheFloodGame.Core.Interfaces;

/// <summary>
/// 승리/패배 조건 검사 및 판정을 담당하는 시스템
/// Input: 플레이어 상태, 제방 상태, 강물 수위, 게임 시간
/// Output: 승리/패배 판정 이벤트, 게임 종료 신호
/// Type: MonoBehaviour System
/// </summary>

namespace TheFloodGame.Core.GameManager
{
    public class Victory_Condition_Manager : MonoBehaviour, IGameSystem, ISaveableGameSystem
    {
        [Header("Victory Condition Settings")]
        [SerializeField] private Game_Config current_Game_Config;
        [SerializeField] private bool auto_End_Game_On_Victory = true;
        [SerializeField] private bool enable_Time_Limit_Victory = true;
        [SerializeField] private bool enable_Surrender_Option = true;

        [Header("Current Game Status")]
        [SerializeField] private bool game_Has_Winner;
        [SerializeField] private int winner_Player_ID = -1;
        [SerializeField] private Victory_Condition winning_Condition;
        [SerializeField] private float game_Duration;
        [SerializeField] private bool victory_Conditions_Active;

        [Header("Victory Tracking")]
        [SerializeField] private Dictionary<int, bool> player_Defeat_Status = new Dictionary<int, bool>();
        [SerializeField] private Dictionary<int, float> player_Defeat_Times = new Dictionary<int, float>();
        [SerializeField] private List<Victory_Event> victory_History = new List<Victory_Event>();

        [Header("Time-based Victory")]
        [SerializeField] private bool time_Limit_Enabled;
        [SerializeField] private float time_Limit_Duration;
        [SerializeField] private float remaining_Time;
        [SerializeField] private bool overtime_Mode;

        [Header("Debug Information")]
        [SerializeField] private int total_Victory_Checks;
        [SerializeField] private float last_Check_Time;
        [SerializeField] private bool conditions_Met;

        // IGameSystem properties
        public string System_Name => "Victory_Condition_Manager";
        public System_Priority Priority => System_Priority.High;
        public bool Is_System_Active { get; private set; }

        // Player manager reference
        private Player_Manager player_Manager;
        private Timer_System timer_System;

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
            Find_Required_Systems();
        }

        private void Update()
        {
            if (Is_System_Active && victory_Conditions_Active)
            {
                Update_System(Time.deltaTime);
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
            return Is_System_Active && player_Defeat_Status != null && victory_History != null;
        }

        public bool Initialize_System()
        {
            try
            {
                player_Defeat_Status = new Dictionary<int, bool>();
                player_Defeat_Times = new Dictionary<int, float>();
                victory_History = new List<Victory_Event>();

                game_Has_Winner = false;
                winner_Player_ID = -1;
                winning_Condition = Victory_Condition.Enemy_Dike_Destroyed;
                game_Duration = 0.0f;
                victory_Conditions_Active = false;
                
                total_Victory_Checks = 0;
                last_Check_Time = 0.0f;
                conditions_Met = false;
                overtime_Mode = false;

                Setup_Time_Limit();

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

            victory_Conditions_Active = true;
            On_System_State_Changed?.Invoke(this, true);
            Debug.Log($"[{System_Name}] System started");
        }

        public void Update_System(float delta_Time)
        {
            if (!victory_Conditions_Active) return;

            game_Duration += delta_Time;
            last_Check_Time = Time.time;
            
            // 시간 제한 체크
            Update_Time_Limit(delta_Time);
            
            // 승리 조건 체크 (매 프레임은 비효율적이므로 간격을 둠)
            if (Time.time - last_Check_Time > 0.1f) // 0.1초마다 체크
            {
                Check_All_Victory_Conditions();
                total_Victory_Checks++;
            }
        }

        public void Pause_System()
        {
            victory_Conditions_Active = false;
            Debug.Log($"[{System_Name}] System paused");
        }

        public void Resume_System()
        {
            victory_Conditions_Active = true;
            Debug.Log($"[{System_Name}] System resumed");
        }

        public void Stop_System()
        {
            victory_Conditions_Active = false;
            Is_System_Active = false;
            
            On_System_State_Changed?.Invoke(this, false);
            Debug.Log($"[{System_Name}] System stopped");
        }

        public void Cleanup_System()
        {
            Stop_System();
            
            player_Defeat_Status.Clear();
            player_Defeat_Times.Clear();
            victory_History.Clear();
            
            Unsubscribe_From_Events();
            Debug.Log($"[{System_Name}] System cleaned up");
        }

        public void Reset_System()
        {
            Stop_System();
            
            game_Has_Winner = false;
            winner_Player_ID = -1;
            winning_Condition = Victory_Condition.Enemy_Dike_Destroyed;
            game_Duration = 0.0f;
            total_Victory_Checks = 0;
            conditions_Met = false;
            overtime_Mode = false;
            
            player_Defeat_Status.Clear();
            player_Defeat_Times.Clear();
            victory_History.Clear();
            
            Setup_Time_Limit();
            
            if (Is_System_Active)
            {
                Start_System();
            }
            
            Debug.Log($"[{System_Name}] System reset");
        }

        public bool Check_System_Health()
        {
            return Is_System_Active && 
                   victory_Conditions_Active && 
                   player_Defeat_Status != null && 
                   victory_History != null;
        }

        #endregion

        #region ISaveableGameSystem Implementation

        public string Serialize_System_Data()
        {
            var save_Data = new Victory_Manager_Save_Data
            {
                has_Winner = game_Has_Winner,
                winner_ID = winner_Player_ID,
                victory_Condition = winning_Condition,
                game_Duration = game_Duration,
                time_Limit = time_Limit_Duration,
                remaining_Time = remaining_Time,
                overtime_Mode = overtime_Mode,
                total_Checks = total_Victory_Checks
            };
            
            return JsonUtility.ToJson(save_Data);
        }

        public bool Deserialize_System_Data(string serialized_Data)
        {
            try
            {
                var save_Data = JsonUtility.FromJson<Victory_Manager_Save_Data>(serialized_Data);
                
                game_Has_Winner = save_Data.has_Winner;
                winner_Player_ID = save_Data.winner_ID;
                winning_Condition = save_Data.victory_Condition;
                game_Duration = save_Data.game_Duration;
                time_Limit_Duration = save_Data.time_Limit;
                remaining_Time = save_Data.remaining_Time;
                overtime_Mode = save_Data.overtime_Mode;
                total_Victory_Checks = save_Data.total_Checks;
                
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

        #region Victory Condition Checking

        /// <summary>
        /// 모든 승리 조건 체크
        /// </summary>
        private void Check_All_Victory_Conditions()
        {
            if (game_Has_Winner || !victory_Conditions_Active) return;

            // 1. 제방 파괴/범람 조건
            Check_Dike_Destruction_Victory();
            
            // 2. 플레이어 연결 끊김 조건
            Check_Disconnection_Victory();
            
            // 3. 시간 제한 조건
            if (enable_Time_Limit_Victory)
                Check_Time_Limit_Victory();
            
            // 4. 항복 조건
            if (enable_Surrender_Option)
                Check_Surrender_Victory();
        }

        /// <summary>
        /// 제방 파괴/범람 승리 조건 체크
        /// </summary>
        private void Check_Dike_Destruction_Victory()
        {
            if (player_Manager == null) return;

            var all_Players = player_Manager.Get_All_Players();
            
            foreach (var player in all_Players)
            {
                bool is_Player_Defeated = player.Current_Player_State == Player_State.Defeated;
                
                if (is_Player_Defeated && !player_Defeat_Status.ContainsKey(player.Player_ID))
                {
                    // 새로운 패배자 발견
                    Record_Player_Defeat(player.Player_ID, Victory_Condition.Enemy_Dike_Destroyed);
                    
                    // 상대방이 승리
                    var other_Player = Get_Other_Player(player.Player_ID);
                    if (other_Player != null)
                    {
                        Declare_Victory(other_Player.Player_ID, Victory_Condition.Enemy_Dike_Destroyed);
                    }
                    
                    return;
                }
            }
        }

        /// <summary>
        /// 연결 끊김 승리 조건 체크
        /// </summary>
        private void Check_Disconnection_Victory()
        {
            if (player_Manager == null) return;

            var all_Players = player_Manager.Get_All_Players();
            
            foreach (var player in all_Players)
            {
                if (player.Network_Status == Network_Status.Disconnected || 
                    player.Current_Player_State == Player_State.Disconnected)
                {
                    if (!player_Defeat_Status.ContainsKey(player.Player_ID))
                    {
                        Record_Player_Defeat(player.Player_ID, Victory_Condition.Enemy_Disconnected);
                        
                        var other_Player = Get_Other_Player(player.Player_ID);
                        if (other_Player != null)
                        {
                            Declare_Victory(other_Player.Player_ID, Victory_Condition.Enemy_Disconnected);
                        }
                        
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 시간 제한 승리 조건 체크
        /// </summary>
        private void Check_Time_Limit_Victory()
        {
            if (!time_Limit_Enabled || remaining_Time > 0) return;

            if (player_Manager == null) return;

            var all_Players = player_Manager.Get_All_Players();
            if (all_Players.Count < 2) return;

            // 제방이 더 높은 플레이어가 승리
            Player_Data highest_Dike_Player = null;
            float highest_Dike_Height = -1.0f;
            bool tie_Situation = false;

            foreach (var player in all_Players)
            {
                if (player.Current_Dike_Height > highest_Dike_Height)
                {
                    highest_Dike_Height = player.Current_Dike_Height;
                    highest_Dike_Player = player;
                    tie_Situation = false;
                }
                else if (Mathf.Approximately(player.Current_Dike_Height, highest_Dike_Height))
                {
                    tie_Situation = true;
                }
            }

            if (!tie_Situation && highest_Dike_Player != null)
            {
                Declare_Victory(highest_Dike_Player.Player_ID, Victory_Condition.Time_Limit_Reached);
            }
            else
            {
                // 무승부 상황 - 오버타임으로 이동
                Enter_Overtime_Mode();
            }
        }

        /// <summary>
        /// 항복 승리 조건 체크
        /// </summary>
        private void Check_Surrender_Victory()
        {
            // 항복 로직은 외부에서 호출되므로 여기서는 기본 검증만 수행
        }

        #endregion

        #region Victory Declaration

        /// <summary>
        /// 승리 선언
        /// </summary>
        /// <param name="winner_ID">승리자 ID</param>
        /// <param name="condition">승리 조건</param>
        public void Declare_Victory(int winner_ID, Victory_Condition condition)
        {
            if (game_Has_Winner) return;

            game_Has_Winner = true;
            winner_Player_ID = winner_ID;
            winning_Condition = condition;

            // 승리 이력 기록
            Record_Victory_Event(winner_ID, condition);

            // 플레이어 상태 업데이트
            if (player_Manager != null)
            {
                var winner = player_Manager.Get_All_Players().Find(p => p.Player_ID == winner_ID);
                winner?.Update_Player_State(Player_State.Victorious);
            }

            // 승리 이벤트 발생
            Game_Events.Trigger_Player_Victory(winner_ID, condition);

            // 게임 종료 처리
            if (auto_End_Game_On_Victory)
            {
                Game_Events.Trigger_Game_Ended(condition, winner_ID);
            }

            Debug.Log($"[{System_Name}] Player {winner_ID} wins by {condition}!");
        }

        /// <summary>
        /// 플레이어 패배 기록
        /// </summary>
        private void Record_Player_Defeat(int player_ID, Victory_Condition defeat_Reason)
        {
            player_Defeat_Status[player_ID] = true;
            player_Defeat_Times[player_ID] = game_Duration;
            
            Debug.Log($"[{System_Name}] Player {player_ID} defeated by {defeat_Reason} at {game_Duration:F1}s");
        }

        /// <summary>
        /// 승리 이벤트 기록
        /// </summary>
        private void Record_Victory_Event(int winner_ID, Victory_Condition condition)
        {
            var victory_Event = new Victory_Event
            {
                winner_ID = winner_ID,
                condition = condition,
                timestamp = game_Duration,
                game_Time = Time.time
            };
            
            victory_History.Add(victory_Event);
        }

        #endregion

        #region Time Limit Management

        /// <summary>
        /// 시간 제한 설정
        /// </summary>
        private void Setup_Time_Limit()
        {
            if (current_Game_Config != null && current_Game_Config.Maximum_Game_Duration > 0)
            {
                time_Limit_Enabled = true;
                time_Limit_Duration = current_Game_Config.Maximum_Game_Duration;
                remaining_Time = time_Limit_Duration;
            }
            else
            {
                time_Limit_Enabled = false;
                time_Limit_Duration = 0.0f;
                remaining_Time = 0.0f;
            }
        }

        /// <summary>
        /// 시간 제한 업데이트
        /// </summary>
        private void Update_Time_Limit(float delta_Time)
        {
            if (!time_Limit_Enabled) return;

            remaining_Time = Mathf.Max(0, remaining_Time - delta_Time);
            
            // 시간 경고 체크 (마지막 60초, 30초, 10초)
            Check_Time_Warnings();
        }

        /// <summary>
        /// 시간 경고 체크
        /// </summary>
        private void Check_Time_Warnings()
        {
            if (remaining_Time <= 60.0f && remaining_Time > 59.0f)
            {
                Debug.Log($"[{System_Name}] 1 minute remaining!");
            }
            else if (remaining_Time <= 30.0f && remaining_Time > 29.0f)
            {
                Debug.Log($"[{System_Name}] 30 seconds remaining!");
            }
            else if (remaining_Time <= 10.0f && remaining_Time > 9.0f)
            {
                Debug.Log($"[{System_Name}] 10 seconds remaining!");
            }
        }

        /// <summary>
        /// 오버타임 모드 진입
        /// </summary>
        private void Enter_Overtime_Mode()
        {
            overtime_Mode = true;
            remaining_Time = 300.0f; // 5분 연장
            
            Debug.Log($"[{System_Name}] Overtime mode activated! 5 minutes extension.");
        }

        #endregion

        #region Manual Victory Controls

        /// <summary>
        /// 수동 항복 처리
        /// </summary>
        /// <param name="surrendering_Player_ID">항복하는 플레이어 ID</param>
        public void Process_Player_Surrender(int surrendering_Player_ID)
        {
            if (game_Has_Winner || !enable_Surrender_Option) return;

            Record_Player_Defeat(surrendering_Player_ID, Victory_Condition.Enemy_Surrender);
            
            var other_Player = Get_Other_Player(surrendering_Player_ID);
            if (other_Player != null)
            {
                Declare_Victory(other_Player.Player_ID, Victory_Condition.Enemy_Surrender);
            }
            
            Debug.Log($"[{System_Name}] Player {surrendering_Player_ID} surrendered");
        }

        /// <summary>
        /// 강제 승리 선언 (디버그/관리자용)
        /// </summary>
        /// <param name="winner_ID">강제 승리자 ID</param>
        public void Force_Victory(int winner_ID)
        {
            if (current_Game_Config != null && current_Game_Config.Enable_Debug_Mode)
            {
                Declare_Victory(winner_ID, Victory_Condition.Enemy_Surrender);
                Debug.Log($"[{System_Name}] Forced victory for Player {winner_ID}");
            }
            else
            {
                Debug.LogWarning($"[{System_Name}] Cannot force victory - Debug mode is disabled");
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// 게임 이벤트 구독
        /// </summary>
        private void Subscribe_To_Events()
        {
            Game_Events.On_Player_State_Changed += Handle_Player_State_Changed;
            Game_Events.On_Flooding_Occurred += Handle_Flooding_Occurred;
            Game_Events.On_Dike_Destroyed += Handle_Dike_Destroyed;
            Game_Events.On_Player_Disconnected += Handle_Player_Disconnected;
            Game_Events.On_Game_State_Changed += Handle_Game_State_Changed;
        }

        /// <summary>
        /// 게임 이벤트 구독 해제
        /// </summary>
        private void Unsubscribe_From_Events()
        {
            Game_Events.On_Player_State_Changed -= Handle_Player_State_Changed;
            Game_Events.On_Flooding_Occurred -= Handle_Flooding_Occurred;
            Game_Events.On_Dike_Destroyed -= Handle_Dike_Destroyed;
            Game_Events.On_Player_Disconnected -= Handle_Player_Disconnected;
            Game_Events.On_Game_State_Changed -= Handle_Game_State_Changed;
        }

        /// <summary>
        /// 플레이어 상태 변경 이벤트 처리
        /// </summary>
        private void Handle_Player_State_Changed(Player_Data player_Data)
        {
            if (player_Data.Current_Player_State == Player_State.Defeated)
            {
                Check_Dike_Destruction_Victory();
            }
        }

        /// <summary>
        /// 범람 발생 이벤트 처리
        /// </summary>
        private void Handle_Flooding_Occurred(int player_ID)
        {
            var other_Player = Get_Other_Player(player_ID);
            if (other_Player != null)
            {
                Declare_Victory(other_Player.Player_ID, Victory_Condition.Enemy_Dike_Destroyed);
            }
        }

        /// <summary>
        /// 제방 파괴 이벤트 처리
        /// </summary>
        private void Handle_Dike_Destroyed(int player_ID)
        {
            var other_Player = Get_Other_Player(player_ID);
            if (other_Player != null)
            {
                Declare_Victory(other_Player.Player_ID, Victory_Condition.Enemy_Dike_Destroyed);
            }
        }

        /// <summary>
        /// 플레이어 연결 끊김 이벤트 처리
        /// </summary>
        private void Handle_Player_Disconnected(Player_Data player_Data)
        {
            var other_Player = Get_Other_Player(player_Data.Player_ID);
            if (other_Player != null)
            {
                Declare_Victory(other_Player.Player_ID, Victory_Condition.Enemy_Disconnected);
            }
        }

        /// <summary>
        /// 게임 상태 변경 이벤트 처리
        /// </summary>
        private void Handle_Game_State_Changed(Game_State old_State, Game_State new_State)
        {
            switch (new_State)
            {
                case Game_State.In_Progress:
                    victory_Conditions_Active = true;
                    break;
                    
                case Game_State.Game_Paused:
                    victory_Conditions_Active = false;
                    break;
                    
                case Game_State.Game_Ending:
                case Game_State.Game_Over:
                    victory_Conditions_Active = false;
                    break;
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// 필요한 시스템들 찾기
        /// </summary>
        private void Find_Required_Systems()
        {
            player_Manager = FindObjectOfType<Player_Manager>();
            timer_System = FindObjectOfType<Timer_System>();
            
            if (player_Manager == null)
                Debug.LogWarning($"[{System_Name}] Player_Manager not found!");
            if (timer_System == null)
                Debug.LogWarning($"[{System_Name}] Timer_System not found!");
        }

        /// <summary>
        /// 상대방 플레이어 가져오기
        /// </summary>
        private Player_Data Get_Other_Player(int player_ID)
        {
            if (player_Manager == null) return null;
            
            var all_Players = player_Manager.Get_All_Players();
            return all_Players.Find(p => p.Player_ID != player_ID);
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 게임에 승자가 있는지 확인
        /// </summary>
        public bool Has_Winner()
        {
            return game_Has_Winner;
        }

        /// <summary>
        /// 승리자 ID 반환
        /// </summary>
        public int Get_Winner_ID()
        {
            return winner_Player_ID;
        }

        /// <summary>
        /// 승리 조건 반환
        /// </summary>
        public Victory_Condition Get_Victory_Condition()
        {
            return winning_Condition;
        }

        /// <summary>
        /// 게임 지속 시간 반환
        /// </summary>
        public float Get_Game_Duration()
        {
            return game_Duration;
        }

        /// <summary>
        /// 남은 시간 반환
        /// </summary>
        public float Get_Remaining_Time()
        {
            return remaining_Time;
        }

        /// <summary>
        /// 오버타임 모드인지 확인
        /// </summary>
        public bool Is_Overtime_Mode()
        {
            return overtime_Mode;
        }

        /// <summary>
        /// 승리 이력 반환
        /// </summary>
        public List<Victory_Event> Get_Victory_History()
        {
            return new List<Victory_Event>(victory_History);
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
        /// 승리 이벤트 데이터 구조
        /// </summary>
        [System.Serializable]
        public struct Victory_Event
        {
            public int winner_ID;
            public Victory_Condition condition;
            public float timestamp;
            public float game_Time;
        }

        /// <summary>
        /// 승리 매니저 저장 데이터 구조
        /// </summary>
        [System.Serializable]
        private struct Victory_Manager_Save_Data
        {
            public bool has_Winner;
            public int winner_ID;
            public Victory_Condition victory_Condition;
            public float game_Duration;
            public float time_Limit;
            public float remaining_Time;
            public bool overtime_Mode;
            public int total_Checks;
        }

        #endregion
    }
}
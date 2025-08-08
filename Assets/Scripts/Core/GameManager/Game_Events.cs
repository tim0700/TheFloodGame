using System;
using UnityEngine;
using TheFloodGame.Core.Data;

/// <summary>
/// 게임 시스템 간 이벤트 기반 통신을 담당하는 이벤트 시스템
/// Input: 다양한 게임 시스템에서 발생하는 이벤트
/// Output: 시스템 간 느슨한 결합을 통한 이벤트 전파
/// Type: Static Event Manager
/// </summary>

namespace TheFloodGame.Core.GameManager
{
    public static class Game_Events
    {
        #region Game State Events
        
        /// <summary>
        /// 게임 상태가 변경될 때 호출되는 이벤트
        /// </summary>
        public static event Action<Game_State, Game_State> On_Game_State_Changed;
        
        /// <summary>
        /// 게임이 시작될 때 호출되는 이벤트
        /// </summary>
        public static event Action On_Game_Started;
        
        /// <summary>
        /// 게임이 일시정지될 때 호출되는 이벤트
        /// </summary>
        public static event Action<bool> On_Game_Paused;
        
        /// <summary>
        /// 게임이 종료될 때 호출되는 이벤트
        /// </summary>
        public static event Action<Victory_Condition, int> On_Game_Ended;
        
        /// <summary>
        /// 게임이 리셋될 때 호출되는 이벤트
        /// </summary>
        public static event Action On_Game_Reset;

        #endregion

        #region Player Events
        
        /// <summary>
        /// 플레이어 상태가 변경될 때 호출되는 이벤트
        /// </summary>
        public static event Action<Player_Data> On_Player_State_Changed;
        
        /// <summary>
        /// 플레이어가 연결되었을 때 호출되는 이벤트
        /// </summary>
        public static event Action<Player_Data> On_Player_Connected;
        
        /// <summary>
        /// 플레이어가 연결이 끊어졌을 때 호출되는 이벤트
        /// </summary>
        public static event Action<Player_Data> On_Player_Disconnected;
        
        /// <summary>
        /// 플레이어가 위치를 변경했을 때 호출되는 이벤트
        /// </summary>
        public static event Action<Player_Data, Player_Location, Player_Location> On_Player_Location_Changed;
        
        /// <summary>
        /// 플레이어가 준비 완료했을 때 호출되는 이벤트
        /// </summary>
        public static event Action<Player_Data> On_Player_Ready;

        #endregion

        #region Water Level Events
        
        /// <summary>
        /// 강물 수위가 상승했을 때 호출되는 이벤트
        /// </summary>
        public static event Action<float> On_Water_Level_Risen;
        
        /// <summary>
        /// 수위 경고 단계가 변경될 때 호출되는 이벤트
        /// </summary>
        public static event Action<int, Water_Level_Alert> On_Water_Alert_Changed;
        
        /// <summary>
        /// 범람이 발생했을 때 호출되는 이벤트
        /// </summary>
        public static event Action<int> On_Flooding_Occurred;

        #endregion

        #region Dike Events
        
        /// <summary>
        /// 제방 높이가 변경될 때 호출되는 이벤트
        /// </summary>
        public static event Action<int, float> On_Dike_Height_Changed;
        
        /// <summary>
        /// 제방이 손상될 때 호출되는 이벤트
        /// </summary>
        public static event Action<int, int> On_Dike_Damaged;
        
        /// <summary>
        /// 제방이 수리될 때 호출되는 이벤트
        /// </summary>
        public static event Action<int, int> On_Dike_Repaired;
        
        /// <summary>
        /// 제방이 파괴될 때 호출되는 이벤트
        /// </summary>
        public static event Action<int> On_Dike_Destroyed;

        #endregion

        #region System Events
        
        /// <summary>
        /// 시스템이 초기화될 때 호출되는 이벤트
        /// </summary>
        public static event Action<string> On_System_Initialized;
        
        /// <summary>
        /// 시스템에 오류가 발생했을 때 호출되는 이벤트
        /// </summary>
        public static event Action<string, string> On_System_Error;
        
        /// <summary>
        /// 시스템 상태가 변경될 때 호출되는 이벤트
        /// </summary>
        public static event Action<string, bool> On_System_State_Changed;

        #endregion

        #region Network Events
        
        /// <summary>
        /// 네트워크 상태가 변경될 때 호출되는 이벤트
        /// </summary>
        public static event Action<Network_Status> On_Network_Status_Changed;
        
        /// <summary>
        /// 네트워크 지연시간이 변경될 때 호출되는 이벤트
        /// </summary>
        public static event Action<float> On_Network_Latency_Changed;
        
        /// <summary>
        /// 네트워크 데이터를 수신했을 때 호출되는 이벤트
        /// </summary>
        public static event Action<byte[]> On_Network_Data_Received;

        #endregion

        #region Victory Events
        
        /// <summary>
        /// 플레이어가 승리했을 때 호출되는 이벤트
        /// </summary>
        public static event Action<int, Victory_Condition> On_Player_Victory;
        
        /// <summary>
        /// 플레이어가 패배했을 때 호출되는 이벤트
        /// </summary>
        public static event Action<int> On_Player_Defeat;

        #endregion

        #region Event Trigger Methods

        /// <summary>
        /// 게임 상태 변경 이벤트 발생
        /// </summary>
        public static void Trigger_Game_State_Changed(Game_State old_State, Game_State new_State)
        {
            try
            {
                On_Game_State_Changed?.Invoke(old_State, new_State);
                
                // 특별한 상태 변경에 대한 추가 이벤트 발생
                if (new_State == Game_State.In_Progress)
                    On_Game_Started?.Invoke();
            }
            catch (Exception ex)
            {
                Log_Event_Error("Game_State_Changed", ex);
            }
        }

        /// <summary>
        /// 게임 일시정지 이벤트 발생
        /// </summary>
        public static void Trigger_Game_Paused(bool is_Paused)
        {
            try
            {
                On_Game_Paused?.Invoke(is_Paused);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Game_Paused", ex);
            }
        }

        /// <summary>
        /// 게임 종료 이벤트 발생
        /// </summary>
        public static void Trigger_Game_Ended(Victory_Condition condition, int winner_ID)
        {
            try
            {
                On_Game_Ended?.Invoke(condition, winner_ID);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Game_Ended", ex);
            }
        }

        /// <summary>
        /// 게임 리셋 이벤트 발생
        /// </summary>
        public static void Trigger_Game_Reset()
        {
            try
            {
                On_Game_Reset?.Invoke();
            }
            catch (Exception ex)
            {
                Log_Event_Error("Game_Reset", ex);
            }
        }

        /// <summary>
        /// 플레이어 상태 변경 이벤트 발생
        /// </summary>
        public static void Trigger_Player_State_Changed(Player_Data player_Data)
        {
            try
            {
                On_Player_State_Changed?.Invoke(player_Data);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Player_State_Changed", ex);
            }
        }

        /// <summary>
        /// 플레이어 연결 이벤트 발생
        /// </summary>
        public static void Trigger_Player_Connected(Player_Data player_Data)
        {
            try
            {
                On_Player_Connected?.Invoke(player_Data);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Player_Connected", ex);
            }
        }

        /// <summary>
        /// 플레이어 연결 끊김 이벤트 발생
        /// </summary>
        public static void Trigger_Player_Disconnected(Player_Data player_Data)
        {
            try
            {
                On_Player_Disconnected?.Invoke(player_Data);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Player_Disconnected", ex);
            }
        }

        /// <summary>
        /// 플레이어 위치 변경 이벤트 발생
        /// </summary>
        public static void Trigger_Player_Location_Changed(Player_Data player_Data, Player_Location old_Location, Player_Location new_Location)
        {
            try
            {
                On_Player_Location_Changed?.Invoke(player_Data, old_Location, new_Location);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Player_Location_Changed", ex);
            }
        }

        /// <summary>
        /// 강물 수위 상승 이벤트 발생
        /// </summary>
        public static void Trigger_Water_Level_Risen(float new_Water_Level)
        {
            try
            {
                On_Water_Level_Risen?.Invoke(new_Water_Level);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Water_Level_Risen", ex);
            }
        }

        /// <summary>
        /// 수위 경고 변경 이벤트 발생
        /// </summary>
        public static void Trigger_Water_Alert_Changed(int player_ID, Water_Level_Alert alert_Level)
        {
            try
            {
                On_Water_Alert_Changed?.Invoke(player_ID, alert_Level);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Water_Alert_Changed", ex);
            }
        }

        /// <summary>
        /// 범람 발생 이벤트 발생
        /// </summary>
        public static void Trigger_Flooding_Occurred(int player_ID)
        {
            try
            {
                On_Flooding_Occurred?.Invoke(player_ID);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Flooding_Occurred", ex);
            }
        }

        /// <summary>
        /// 제방 높이 변경 이벤트 발생
        /// </summary>
        public static void Trigger_Dike_Height_Changed(int player_ID, float new_Height)
        {
            try
            {
                On_Dike_Height_Changed?.Invoke(player_ID, new_Height);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Dike_Height_Changed", ex);
            }
        }

        /// <summary>
        /// 제방 손상 이벤트 발생
        /// </summary>
        public static void Trigger_Dike_Damaged(int player_ID, int remaining_Durability)
        {
            try
            {
                On_Dike_Damaged?.Invoke(player_ID, remaining_Durability);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Dike_Damaged", ex);
            }
        }

        /// <summary>
        /// 시스템 초기화 이벤트 발생
        /// </summary>
        public static void Trigger_System_Initialized(string system_Name)
        {
            try
            {
                On_System_Initialized?.Invoke(system_Name);
            }
            catch (Exception ex)
            {
                Log_Event_Error("System_Initialized", ex);
            }
        }

        /// <summary>
        /// 시스템 오류 이벤트 발생
        /// </summary>
        public static void Trigger_System_Error(string system_Name, string error_Message)
        {
            try
            {
                On_System_Error?.Invoke(system_Name, error_Message);
            }
            catch (Exception ex)
            {
                Log_Event_Error("System_Error", ex);
            }
        }

        /// <summary>
        /// 네트워크 상태 변경 이벤트 발생
        /// </summary>
        public static void Trigger_Network_Status_Changed(Network_Status status)
        {
            try
            {
                On_Network_Status_Changed?.Invoke(status);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Network_Status_Changed", ex);
            }
        }

        /// <summary>
        /// 플레이어 승리 이벤트 발생
        /// </summary>
        public static void Trigger_Player_Victory(int player_ID, Victory_Condition condition)
        {
            try
            {
                On_Player_Victory?.Invoke(player_ID, condition);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Player_Victory", ex);
            }
        }

        /// <summary>
        /// 플레이어 패배 이벤트 발생
        /// </summary>
        public static void Trigger_Player_Defeat(int player_ID)
        {
            try
            {
                On_Player_Defeat?.Invoke(player_ID);
            }
            catch (Exception ex)
            {
                Log_Event_Error("Player_Defeat", ex);
            }
        }

        #endregion

        #region Event Management

        /// <summary>
        /// 모든 이벤트 리스너 제거 (게임 종료 시 호출)
        /// </summary>
        public static void Clear_All_Events()
        {
            // Game State Events
            On_Game_State_Changed = null;
            On_Game_Started = null;
            On_Game_Paused = null;
            On_Game_Ended = null;
            On_Game_Reset = null;

            // Player Events
            On_Player_State_Changed = null;
            On_Player_Connected = null;
            On_Player_Disconnected = null;
            On_Player_Location_Changed = null;
            On_Player_Ready = null;

            // Water Level Events
            On_Water_Level_Risen = null;
            On_Water_Alert_Changed = null;
            On_Flooding_Occurred = null;

            // Dike Events
            On_Dike_Height_Changed = null;
            On_Dike_Damaged = null;
            On_Dike_Repaired = null;
            On_Dike_Destroyed = null;

            // System Events
            On_System_Initialized = null;
            On_System_Error = null;
            On_System_State_Changed = null;

            // Network Events
            On_Network_Status_Changed = null;
            On_Network_Latency_Changed = null;
            On_Network_Data_Received = null;

            // Victory Events
            On_Player_Victory = null;
            On_Player_Defeat = null;
        }

        /// <summary>
        /// 이벤트 발생 시 오류 로깅
        /// </summary>
        private static void Log_Event_Error(string event_Name, Exception exception)
        {
            Debug.LogError($"[Game_Events] Error in {event_Name}: {exception.Message}");
            Debug.LogException(exception);
        }

        /// <summary>
        /// 이벤트 리스너 개수 확인 (디버깅용)
        /// </summary>
        public static int Get_Event_Listener_Count(string event_Name)
        {
            return event_Name switch
            {
                nameof(On_Game_State_Changed) => On_Game_State_Changed?.GetInvocationList().Length ?? 0,
                nameof(On_Player_State_Changed) => On_Player_State_Changed?.GetInvocationList().Length ?? 0,
                nameof(On_Water_Level_Risen) => On_Water_Level_Risen?.GetInvocationList().Length ?? 0,
                nameof(On_Dike_Height_Changed) => On_Dike_Height_Changed?.GetInvocationList().Length ?? 0,
                nameof(On_System_Error) => On_System_Error?.GetInvocationList().Length ?? 0,
                nameof(On_Network_Status_Changed) => On_Network_Status_Changed?.GetInvocationList().Length ?? 0,
                _ => -1
            };
        }

        #endregion
    }

    /// <summary>
    /// 이벤트 데이터를 담는 구조체들
    /// </summary>
    [Serializable]
    public struct Game_Event_Data
    {
        public string event_Name;
        public float timestamp;
        public string additional_Info;

        public Game_Event_Data(string name, string info = "")
        {
            event_Name = name;
            timestamp = Time.time;
            additional_Info = info;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheFloodGame.Core.Data;
using TheFloodGame.Core.Interfaces;

/// <summary>
/// 두 플레이어의 상태 추적 및 관리를 담당하는 시스템
/// Input: 플레이어 연결/해제, 상태 변화, 제방 정보 업데이트
/// Output: 플레이어 상태 이벤트, 승리/패배 판정
/// Type: MonoBehaviour System
/// </summary>

namespace TheFloodGame.Core.GameManager
{
    public class Player_Manager : MonoBehaviour, IGameSystem, INetworkGameSystem, ISaveableGameSystem
    {
        [Header("Player Manager Settings")]
        [SerializeField] private Game_Config current_Game_Config;
        [SerializeField] private int maximum_Player_Count = 2;
        [SerializeField] private bool auto_Assign_Player_IDs = true;

        [Header("Player Data")]
        [SerializeField] private List<Player_Data> active_Players = new List<Player_Data>();
        [SerializeField] private Player_Data local_Player;
        [SerializeField] private Player_Data remote_Player;

        [Header("Player Status Tracking")]
        [SerializeField] private int connected_Player_Count;
        [SerializeField] private int ready_Player_Count;
        [SerializeField] private bool all_Players_Connected;
        [SerializeField] private bool all_Players_Ready;

        [Header("Water Level Monitoring")]
        [SerializeField] private float current_Water_Level;
        [SerializeField] private Dictionary<int, Water_Level_Alert> player_Alert_Levels = new Dictionary<int, Water_Level_Alert>();

        [Header("Debug Information")]
        [SerializeField] private float last_Update_Time;
        [SerializeField] private int total_Player_State_Changes;
        [SerializeField] private bool is_Monitoring_Active;

        // IGameSystem properties
        public string System_Name => "Player_Manager";
        public System_Priority Priority => System_Priority.High;
        public bool Is_System_Active { get; private set; }

        // Network status tracking
        private Network_Status current_Network_Status = Network_Status.Disconnected;

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
            if (Is_System_Active && is_Monitoring_Active)
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
            return Is_System_Active && active_Players != null;
        }

        public bool Initialize_System()
        {
            try
            {
                active_Players = new List<Player_Data>();
                player_Alert_Levels = new Dictionary<int, Water_Level_Alert>();
                
                connected_Player_Count = 0;
                ready_Player_Count = 0;
                all_Players_Connected = false;
                all_Players_Ready = false;
                current_Water_Level = 0.0f;
                total_Player_State_Changes = 0;
                is_Monitoring_Active = true;

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

            is_Monitoring_Active = true;
            On_System_State_Changed?.Invoke(this, true);
            Debug.Log($"[{System_Name}] System started");
        }

        public void Update_System(float delta_Time)
        {
            last_Update_Time = Time.time;
            
            // 플레이어 상태 업데이트
            Update_Player_Statistics(delta_Time);
            
            // 제방 상태 모니터링
            Monitor_Dike_Status();
            
            // 연결 상태 확인
            Update_Connection_Status();
        }

        public void Pause_System()
        {
            is_Monitoring_Active = false;
            Debug.Log($"[{System_Name}] System paused");
        }

        public void Resume_System()
        {
            is_Monitoring_Active = true;
            Debug.Log($"[{System_Name}] System resumed");
        }

        public void Stop_System()
        {
            is_Monitoring_Active = false;
            Is_System_Active = false;
            
            On_System_State_Changed?.Invoke(this, false);
            Debug.Log($"[{System_Name}] System stopped");
        }

        public void Cleanup_System()
        {
            Stop_System();
            
            // 모든 플레이어 정리
            foreach (var player in active_Players)
            {
                Disconnect_Player(player.Player_ID);
            }
            
            active_Players.Clear();
            player_Alert_Levels.Clear();
            
            Unsubscribe_From_Events();
            Debug.Log($"[{System_Name}] System cleaned up");
        }

        public void Reset_System()
        {
            Stop_System();
            
            // 모든 플레이어 데이터 리셋
            foreach (var player in active_Players)
            {
                player.Reset_Player_Data();
            }
            
            connected_Player_Count = 0;
            ready_Player_Count = 0;
            all_Players_Connected = false;
            all_Players_Ready = false;
            current_Water_Level = 0.0f;
            total_Player_State_Changes = 0;
            
            player_Alert_Levels.Clear();
            
            if (Is_System_Active)
            {
                Start_System();
            }
            
            Debug.Log($"[{System_Name}] System reset");
        }

        public bool Check_System_Health()
        {
            return Is_System_Active && 
                   is_Monitoring_Active && 
                   active_Players != null && 
                   active_Players.Count <= maximum_Player_Count;
        }

        #endregion

        #region INetworkGameSystem Implementation

        public void Synchronize_Network_Data()
        {
            // 네트워크 동기화 로직 (향후 Unity Netcode 통합 시 구현)
            if (current_Network_Status == Network_Status.Connected)
            {
                foreach (var player in active_Players)
                {
                    // 플레이어 데이터 동기화
                }
            }
        }

        public void Handle_Network_Data(byte[] network_Data)
        {
            // 네트워크 데이터 처리 (향후 구현)
            Debug.Log($"[{System_Name}] Received network data: {network_Data.Length} bytes");
        }

        public Network_Status Get_Network_Status()
        {
            return current_Network_Status;
        }

        #endregion

        #region ISaveableGameSystem Implementation

        public string Serialize_System_Data()
        {
            var save_Data = new Player_Manager_Save_Data
            {
                player_Count = active_Players.Count,
                players = active_Players.ToArray(),
                water_Level = current_Water_Level,
                total_State_Changes = total_Player_State_Changes
            };
            
            return JsonUtility.ToJson(save_Data);
        }

        public bool Deserialize_System_Data(string serialized_Data)
        {
            try
            {
                var save_Data = JsonUtility.FromJson<Player_Manager_Save_Data>(serialized_Data);
                
                active_Players = new List<Player_Data>(save_Data.players);
                current_Water_Level = save_Data.water_Level;
                total_Player_State_Changes = save_Data.total_State_Changes;
                
                Update_Player_Counts();
                
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

        #region Player Management

        /// <summary>
        /// 새 플레이어 연결
        /// </summary>
        /// <param name="player_Name">플레이어 이름</param>
        /// <param name="is_Local">로컬 플레이어 여부</param>
        /// <param name="is_Host">호스트 플레이어 여부</param>
        /// <returns>생성된 플레이어 데이터</returns>
        public Player_Data Connect_Player(string player_Name, bool is_Local = false, bool is_Host = false)
        {
            if (active_Players.Count >= maximum_Player_Count)
            {
                Debug.LogWarning($"[{System_Name}] Cannot connect player - maximum capacity reached");
                return null;
            }

            int player_ID = auto_Assign_Player_IDs ? Get_Next_Available_Player_ID() : active_Players.Count;
            
            Player_Data new_Player = new Player_Data(player_ID, player_Name, is_Local);
            
            // 기본 설정 적용
            if (current_Game_Config != null)
            {
                new_Player.Increase_Dike_Height(current_Game_Config.Default_Dike_Height - 1.0f); // -1.0f는 기본값 보정
            }

            active_Players.Add(new_Player);
            
            // 로컬/원격 플레이어 할당
            if (is_Local)
            {
                local_Player = new_Player;
            }
            else if (remote_Player == null)
            {
                remote_Player = new_Player;
            }

            // 경고 레벨 초기화
            player_Alert_Levels[player_ID] = Water_Level_Alert.Safe;
            
            // 플레이어 이벤트 구독
            Subscribe_To_Player_Events(new_Player);
            
            Update_Player_Counts();
            
            // 연결 이벤트 발생
            Game_Events.Trigger_Player_Connected(new_Player);
            
            Debug.Log($"[{System_Name}] Player {player_ID} ({player_Name}) connected");
            return new_Player;
        }

        /// <summary>
        /// 플레이어 연결 해제
        /// </summary>
        /// <param name="player_ID">플레이어 ID</param>
        /// <returns>연결 해제 성공 시 true</returns>
        public bool Disconnect_Player(int player_ID)
        {
            Player_Data player = Get_Player_By_ID(player_ID);
            if (player == null)
            {
                Debug.LogWarning($"[{System_Name}] Cannot disconnect player {player_ID} - not found");
                return false;
            }

            // 플레이어 상태 업데이트
            player.Update_Player_State(Player_State.Disconnected);
            
            // 이벤트 구독 해제
            Unsubscribe_From_Player_Events(player);
            
            // 로컬/원격 플레이어 참조 제거
            if (player == local_Player)
                local_Player = null;
            if (player == remote_Player)
                remote_Player = null;

            // 플레이어 제거
            active_Players.Remove(player);
            player_Alert_Levels.Remove(player_ID);
            
            Update_Player_Counts();
            
            // 연결 해제 이벤트 발생
            Game_Events.Trigger_Player_Disconnected(player);
            
            Debug.Log($"[{System_Name}] Player {player_ID} disconnected");
            return true;
        }

        /// <summary>
        /// 플레이어 준비 상태 설정
        /// </summary>
        /// <param name="player_ID">플레이어 ID</param>
        /// <param name="is_Ready">준비 상태</param>
        public void Set_Player_Ready(int player_ID, bool is_Ready)
        {
            Player_Data player = Get_Player_By_ID(player_ID);
            if (player == null) return;

            player.Set_Player_Ready(is_Ready);
            Update_Player_Counts();
            
            if (is_Ready)
            {
                // Game_Events.Trigger_Player_Ready(player); (미진행)
                Debug.Log($"[{System_Name}] Player {player_ID} is ready");
            }
            else
            {
                Debug.Log($"[{System_Name}] Player {player_ID} is not ready");
            }
        }

        /// <summary>
        /// 플레이어 위치 업데이트
        /// </summary>
        /// <param name="player_ID">플레이어 ID</param>
        /// <param name="new_Location">새로운 위치</param>
        /// <param name="world_Position">월드 좌표</param>
        public void Update_Player_Location(int player_ID, Player_Location new_Location, Vector3 world_Position)
        {
            Player_Data player = Get_Player_By_ID(player_ID);
            if (player == null) return;

            Player_Location old_Location = player.Current_Location;
            player.Update_Player_Location(new_Location, world_Position);
            
            Game_Events.Trigger_Player_Location_Changed(player, old_Location, new_Location);
        }

        #endregion

        #region Dike Management

        /// <summary>
        /// 플레이어 제방 높이 증가
        /// </summary>
        /// <param name="player_ID">플레이어 ID</param>
        /// <param name="height_Increase">증가할 높이</param>
        /// <returns>실제 증가된 높이</returns>
        public float Increase_Player_Dike_Height(int player_ID, float height_Increase)
        {
            Player_Data player = Get_Player_By_ID(player_ID);
            if (player == null) return 0.0f;

            float actual_Increase = player.Increase_Dike_Height(height_Increase);
            
            if (actual_Increase > 0)
            {
                Game_Events.Trigger_Dike_Height_Changed(player_ID, player.Current_Dike_Height);
                Update_Player_Alert_Level(player);
            }
            
            return actual_Increase;
        }

        /// <summary>
        /// 플레이어 제방에 대미지 적용
        /// </summary>
        /// <param name="player_ID">플레이어 ID</param>
        /// <param name="damage_Amount">대미지 양</param>
        /// <returns>제방이 파괴되었는지 여부</returns>
        public bool Apply_Dike_Damage(int player_ID, int damage_Amount)
        {
            Player_Data player = Get_Player_By_ID(player_ID);
            if (player == null) return false;

            bool is_Destroyed = player.Apply_Dike_Damage(damage_Amount);
            
            Game_Events.Trigger_Dike_Damaged(player_ID, player.Dike_Durability);
            
            if (is_Destroyed)
            {
                // Game_Events.Trigger_Dike_Destroyed(player_ID); (게임 오버 체크, 개발 예정)
                Handle_Player_Defeat(player_ID, Victory_Condition.Enemy_Dike_Destroyed);
            }
            
            return is_Destroyed;
        }

        /// <summary>
        /// 플레이어 제방 수리
        /// </summary>
        /// <param name="player_ID">플레이어 ID</param>
        /// <param name="repair_Amount">수리량</param>
        public void Repair_Player_Dike(int player_ID, int repair_Amount)
        {
            Player_Data player = Get_Player_By_ID(player_ID);
            if (player == null) return;

            player.Repair_Dike(repair_Amount);
            // Game_Events.Trigger_Dike_Repaired(player_ID, player.Dike_Durability); 건설 파트. 머리아프다.
        }

        #endregion

        #region Water Level Monitoring

        /// <summary>
        /// 제방 상태 모니터링
        /// </summary>
        private void Monitor_Dike_Status()
        {
            foreach (var player in active_Players)
            {
                // 범람 체크
                bool is_Defeated = player.Check_Player_Defeat(current_Water_Level);
                
                if (is_Defeated && player.Current_Player_State != Player_State.Defeated)
                {
                    Handle_Flooding(player.Player_ID);
                }
                
                // 경고 레벨 업데이트
                Update_Player_Alert_Level(player);
            }
        }

        /// <summary>
        /// 플레이어 경고 레벨 업데이트
        /// </summary>
        /// <param name="player">플레이어 데이터</param>
        private void Update_Player_Alert_Level(Player_Data player)
        {
            if (current_Game_Config == null) return;

            Water_Level_Alert new_Alert = current_Game_Config.Calculate_Water_Alert_Level(
                current_Water_Level, player.Current_Dike_Height);
            
            if (player_Alert_Levels.TryGetValue(player.Player_ID, out Water_Level_Alert old_Alert))
            {
                if (old_Alert != new_Alert)
                {
                    player_Alert_Levels[player.Player_ID] = new_Alert;
                    Game_Events.Trigger_Water_Alert_Changed(player.Player_ID, new_Alert);
                }
            }
            else
            {
                player_Alert_Levels[player.Player_ID] = new_Alert;
                Game_Events.Trigger_Water_Alert_Changed(player.Player_ID, new_Alert);
            }
        }

        /// <summary>
        /// 범람 처리
        /// </summary>
        /// <param name="player_ID">범람된 플레이어 ID</param>
        private void Handle_Flooding(int player_ID)
        {
            Game_Events.Trigger_Flooding_Occurred(player_ID);
            Handle_Player_Defeat(player_ID, Victory_Condition.Enemy_Dike_Destroyed);
        }

        #endregion

        #region Victory/Defeat Handling

        /// <summary>
        /// 플레이어 패배 처리
        /// </summary>
        /// <param name="defeated_Player_ID">패배한 플레이어 ID</param>
        /// <param name="defeat_Reason">패배 원인</param>
        private void Handle_Player_Defeat(int defeated_Player_ID, Victory_Condition defeat_Reason)
        {
            Player_Data defeated_Player = Get_Player_By_ID(defeated_Player_ID);
            if (defeated_Player == null) return;

            defeated_Player.Update_Player_State(Player_State.Defeated);
            Game_Events.Trigger_Player_Defeat(defeated_Player_ID);
            
            // 상대방 승리 처리
            Player_Data winner = Get_Other_Player(defeated_Player_ID);
            if (winner != null)
            {
                winner.Update_Player_State(Player_State.Victorious);
                Game_Events.Trigger_Player_Victory(winner.Player_ID, defeat_Reason);
            }
            
            Debug.Log($"[{System_Name}] Player {defeated_Player_ID} defeated by {defeat_Reason}");
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// 게임 이벤트 구독
        /// </summary>
        private void Subscribe_To_Events()
        {
            Game_Events.On_Water_Level_Risen += Handle_Water_Level_Risen;
            Game_Events.On_Game_State_Changed += Handle_Game_State_Changed;
            Game_Events.On_Network_Status_Changed += Handle_Network_Status_Changed;
        }

        /// <summary>
        /// 게임 이벤트 구독 해제
        /// </summary>
        private void Unsubscribe_From_Events()
        {
            Game_Events.On_Water_Level_Risen -= Handle_Water_Level_Risen;
            Game_Events.On_Game_State_Changed -= Handle_Game_State_Changed;
            Game_Events.On_Network_Status_Changed -= Handle_Network_Status_Changed;
        }

        /// <summary>
        /// 플레이어별 이벤트 구독
        /// </summary>
        private void Subscribe_To_Player_Events(Player_Data player)
        {
            player.On_Player_State_Changed += Handle_Player_State_Changed;
            player.On_Dike_Height_Changed += Handle_Dike_Height_Changed;
            player.On_Dike_Durability_Changed += Handle_Dike_Durability_Changed;
        }

        /// <summary>
        /// 플레이어별 이벤트 구독 해제
        /// </summary>
        private void Unsubscribe_From_Player_Events(Player_Data player)
        {
            player.On_Player_State_Changed -= Handle_Player_State_Changed;
            player.On_Dike_Height_Changed -= Handle_Dike_Height_Changed;
            player.On_Dike_Durability_Changed -= Handle_Dike_Durability_Changed;
        }

        /// <summary>
        /// 강물 수위 상승 이벤트 처리
        /// </summary>
        private void Handle_Water_Level_Risen(float new_Water_Level)
        {
            current_Water_Level = new_Water_Level;
        }

        /// <summary>
        /// 게임 상태 변경 이벤트 처리
        /// </summary>
        private void Handle_Game_State_Changed(Game_State old_State, Game_State new_State)
        {
            switch (new_State)
            {
                case Game_State.Game_Over:
                    // 게임 종료 시 모든 플레이어 상태 초기화 준비
                    break;
            }
        }

        /// <summary>
        /// 네트워크 상태 변경 이벤트 처리
        /// </summary>
        private void Handle_Network_Status_Changed(Network_Status status)
        {
            current_Network_Status = status;
            
            // 모든 플레이어의 네트워크 상태 업데이트
            foreach (var player in active_Players)
            {
                player.Update_Network_Status(status, 0.0f); // 지연시간은 추후 측정
            }
        }

        /// <summary>
        /// 플레이어 상태 변경 이벤트 처리
        /// </summary>
        private void Handle_Player_State_Changed(Player_Data player)
        {
            total_Player_State_Changes++;
            Game_Events.Trigger_Player_State_Changed(player);
        }

        /// <summary>
        /// 제방 높이 변경 이벤트 처리
        /// </summary>
        private void Handle_Dike_Height_Changed(Player_Data player, float new_Height)
        {
            Update_Player_Alert_Level(player);
        }

        /// <summary>
        /// 제방 내구도 변경 이벤트 처리
        /// </summary>
        private void Handle_Dike_Durability_Changed(Player_Data player, int new_Durability)
        {
            if (new_Durability <= 0)
            {
                Handle_Player_Defeat(player.Player_ID, Victory_Condition.Enemy_Dike_Destroyed);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// ID로 플레이어 검색
        /// </summary>
        private Player_Data Get_Player_By_ID(int player_ID)
        {
            return active_Players.FirstOrDefault(p => p.Player_ID == player_ID);
        }

        /// <summary>
        /// 상대방 플레이어 가져오기
        /// </summary>
        private Player_Data Get_Other_Player(int player_ID)
        {
            return active_Players.FirstOrDefault(p => p.Player_ID != player_ID);
        }

        /// <summary>
        /// 다음 사용 가능한 플레이어 ID 가져오기
        /// </summary>
        private int Get_Next_Available_Player_ID()
        {
            for (int i = 0; i < maximum_Player_Count; i++)
            {
                if (!active_Players.Any(p => p.Player_ID == i))
                    return i;
            }
            return active_Players.Count;
        }

        /// <summary>
        /// 플레이어 수 업데이트
        /// </summary>
        private void Update_Player_Counts()
        {
            connected_Player_Count = active_Players.Count(p => p.Current_Player_State != Player_State.Disconnected);
            ready_Player_Count = active_Players.Count(p => p.Is_Player_Ready);
            all_Players_Connected = connected_Player_Count == maximum_Player_Count;
            all_Players_Ready = ready_Player_Count == maximum_Player_Count && connected_Player_Count == maximum_Player_Count;
        }

        /// <summary>
        /// 플레이어 통계 업데이트
        /// </summary>
        private void Update_Player_Statistics(float delta_Time)
        {
            foreach (var player in active_Players)
            {
                if (player.Current_Player_State == Player_State.Playing_Surface ||
                    player.Current_Player_State == Player_State.Playing_Underground)
                {
                    player.Update_Statistics(delta_Time);
                }
            }
        }

        /// <summary>
        /// 연결 상태 업데이트
        /// </summary>
        private void Update_Connection_Status()
        {
            foreach (var player in active_Players)
            {
                // 네트워크 연결 상태 확인 및 업데이트
                // 실제 네트워크 구현 시 세부 로직 추가 예정
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// 모든 활성 플레이어 반환
        /// </summary>
        public List<Player_Data> Get_All_Players()
        {
            return new List<Player_Data>(active_Players);
        }

        /// <summary>
        /// 로컬 플레이어 반환
        /// </summary>
        public Player_Data Get_Local_Player()
        {
            return local_Player;
        }

        /// <summary>
        /// 원격 플레이어 반환
        /// </summary>
        public Player_Data Get_Remote_Player()
        {
            return remote_Player;
        }

        /// <summary>
        /// 모든 플레이어가 연결되었는지 확인
        /// </summary>
        public bool Are_All_Players_Connected()
        {
            return all_Players_Connected;
        }

        /// <summary>
        /// 모든 플레이어가 준비되었는지 확인
        /// </summary>
        public bool Are_All_Players_Ready()
        {
            return all_Players_Ready;
        }

        /// <summary>
        /// 연결된 플레이어 수 반환
        /// </summary>
        public int Get_Connected_Player_Count()
        {
            return connected_Player_Count;
        }

        /// <summary>
        /// 특정 플레이어의 경고 레벨 반환
        /// </summary>
        public Water_Level_Alert Get_Player_Alert_Level(int player_ID)
        {
            return player_Alert_Levels.TryGetValue(player_ID, out Water_Level_Alert alert) ? alert : Water_Level_Alert.Safe;
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
        /// 플레이어 매니저 저장 데이터 구조
        /// </summary>
        [System.Serializable]
        private struct Player_Manager_Save_Data
        {
            public int player_Count;
            public Player_Data[] players;
            public float water_Level;
            public int total_State_Changes;
        }

        #endregion
    }
}
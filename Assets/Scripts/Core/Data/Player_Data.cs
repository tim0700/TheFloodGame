using System;
using UnityEngine;

/// <summary>
/// 플레이어 상태 및 제방 정보를 관리하는 데이터 구조
/// Input: 플레이어 액션, 제방 상태 변경
/// Output: 플레이어 상태 정보, 제방 정보
/// Type: Serializable Data Structure
/// </summary>

namespace TheFloodGame.Core.Data
{
    [Serializable]
    public class Player_Data
    {
        [Header("Player Identification")]
        [SerializeField] private int player_ID;
        [SerializeField] private string player_Name;
        [SerializeField] private bool is_Local_Player;

        [Header("Player State")]
        [SerializeField] private Player_State current_Player_State;
        [SerializeField] private Player_Location current_Location;
        [SerializeField] private Vector3 player_Position;
        [SerializeField] private bool is_Player_Ready;

        [Header("Dike System")]
        [SerializeField] private float current_Dike_Height;
        [SerializeField] private float maximum_Dike_Height;
        [SerializeField] private int dike_Durability;
        [SerializeField] private int maximum_Dike_Durability;
        [SerializeField] private float dike_Repair_Progress;

        [Header("Game Statistics")]
        [SerializeField] private float total_Play_Time;
        [SerializeField] private int resources_Collected;
        [SerializeField] private int buildings_Built;
        [SerializeField] private int attacks_Launched;
        [SerializeField] private int defenses_Built;

        [Header("Network Information")]
        [SerializeField] private Network_Status network_Status;
        [SerializeField] private float network_Latency;
        [SerializeField] private bool is_Host_Player;

        // Properties for safe access
        public int Player_ID => player_ID;
        public string Player_Name => player_Name;
        public bool Is_Local_Player => is_Local_Player;
        public Player_State Current_Player_State => current_Player_State;
        public Player_Location Current_Location => current_Location;
        public Vector3 Player_Position => player_Position;
        public bool Is_Player_Ready => is_Player_Ready;
        public float Current_Dike_Height => current_Dike_Height;
        public float Maximum_Dike_Height => maximum_Dike_Height;
        public int Dike_Durability => dike_Durability;
        public int Maximum_Dike_Durability => maximum_Dike_Durability;
        public float Dike_Repair_Progress => dike_Repair_Progress;
        public float Total_Play_Time => total_Play_Time;
        public int Resources_Collected => resources_Collected;
        public int Buildings_Built => buildings_Built;
        public int Attacks_Launched => attacks_Launched;
        public int Defenses_Built => defenses_Built;
        public Network_Status Network_Status => network_Status;
        public float Network_Latency => network_Latency;
        public bool Is_Host_Player => is_Host_Player;

        // Events
        public event Action<Player_Data> On_Player_State_Changed;
        public event Action<Player_Data, float> On_Dike_Height_Changed;
        public event Action<Player_Data, int> On_Dike_Durability_Changed;
        public event Action<Player_Data> On_Player_Defeated;
        public event Action<Player_Data> On_Player_Victory;

        /// <summary>
        /// 새로운 플레이어 데이터 생성자
        /// </summary>
        /// <param name="id">플레이어 ID</param>
        /// <param name="name">플레이어 이름</param>
        /// <param name="is_Local">로컬 플레이어 여부</param>
        public Player_Data(int id, string name, bool is_Local = false)
        {
            Initialize_Player_Data(id, name, is_Local);
        }

        /// <summary>
        /// 플레이어 데이터 초기화
        /// </summary>
        private void Initialize_Player_Data(int id, string name, bool is_Local)
        {
            player_ID = id;
            player_Name = name;
            is_Local_Player = is_Local;
            
            current_Player_State = Player_State.Connected_Waiting;
            current_Location = Player_Location.Surface_Area;
            player_Position = Vector3.zero;
            is_Player_Ready = false;

            current_Dike_Height = 1.0f; // 기본 제방 높이
            maximum_Dike_Height = 50.0f; // 최대 제방 높이
            dike_Durability = 100;
            maximum_Dike_Durability = 100;
            dike_Repair_Progress = 0.0f;

            Reset_Statistics();
            
            network_Status = Network_Status.Connected;
            network_Latency = 0.0f;
            is_Host_Player = false;
        }

        /// <summary>
        /// 플레이어 상태 업데이트
        /// </summary>
        /// <param name="new_State">새로운 상태</param>
        public void Update_Player_State(Player_State new_State)
        {
            if (current_Player_State != new_State)
            {
                current_Player_State = new_State;
                On_Player_State_Changed?.Invoke(this);

                // 특별한 상태에 대한 추가 처리
                if (new_State == Player_State.Defeated)
                {
                    On_Player_Defeated?.Invoke(this);
                }
                else if (new_State == Player_State.Victorious)
                {
                    On_Player_Victory?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// 플레이어 위치 업데이트
        /// </summary>
        /// <param name="new_Location">새로운 위치</param>
        /// <param name="world_Position">월드 좌표</param>
        public void Update_Player_Location(Player_Location new_Location, Vector3 world_Position)
        {
            current_Location = new_Location;
            player_Position = world_Position;
        }

        /// <summary>
        /// 제방 높이 증가
        /// </summary>
        /// <param name="height_Increase">증가할 높이</param>
        /// <returns>실제 증가된 높이</returns>
        public float Increase_Dike_Height(float height_Increase)
        {
            float previous_Height = current_Dike_Height;
            current_Dike_Height = Mathf.Clamp(current_Dike_Height + height_Increase, 0, maximum_Dike_Height);
            
            float actual_Increase = current_Dike_Height - previous_Height;
            
            if (actual_Increase > 0)
            {
                On_Dike_Height_Changed?.Invoke(this, current_Dike_Height);
            }
            
            return actual_Increase;
        }

        /// <summary>
        /// 제방에 대미지 적용
        /// </summary>
        /// <param name="damage_Amount">대미지 양</param>
        /// <returns>제방이 파괴되었는지 여부</returns>
        public bool Apply_Dike_Damage(int damage_Amount)
        {
            dike_Durability = Mathf.Max(0, dike_Durability - damage_Amount);
            On_Dike_Durability_Changed?.Invoke(this, dike_Durability);
            
            if (dike_Durability <= 0)
            {
                Update_Player_State(Player_State.Defeated);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 제방 수리
        /// </summary>
        /// <param name="repair_Amount">수리량</param>
        public void Repair_Dike(int repair_Amount)
        {
            dike_Durability = Mathf.Clamp(dike_Durability + repair_Amount, 0, maximum_Dike_Durability);
            On_Dike_Durability_Changed?.Invoke(this, dike_Durability);
        }

        /// <summary>
        /// 플레이어가 패배했는지 확인
        /// </summary>
        /// <param name="current_Water_Level">현재 강물 수위</param>
        /// <returns>패배 시 true</returns>
        public bool Check_Player_Defeat(float current_Water_Level)
        {
            bool is_Defeated = (current_Water_Level >= current_Dike_Height) || (dike_Durability <= 0);
            
            if (is_Defeated && current_Player_State != Player_State.Defeated)
            {
                Update_Player_State(Player_State.Defeated);
            }
            
            return is_Defeated;
        }

        /// <summary>
        /// 통계 정보 업데이트
        /// </summary>
        public void Update_Statistics(float play_Time_Delta, int resources_Gained = 0, bool building_Built = false, bool attack_Launched = false, bool defense_Built = false)
        {
            total_Play_Time += play_Time_Delta;
            
            if (resources_Gained > 0)
                resources_Collected += resources_Gained;
                
            if (building_Built)
                buildings_Built++;
                
            if (attack_Launched)
                attacks_Launched++;
                
            if (defense_Built)
                defenses_Built++;
        }

        /// <summary>
        /// 네트워크 상태 업데이트
        /// </summary>
        /// <param name="status">네트워크 상태</param>
        /// <param name="latency">지연 시간</param>
        public void Update_Network_Status(Network_Status status, float latency)
        {
            network_Status = status;
            network_Latency = latency;
        }

        /// <summary>
        /// 플레이어 준비 상태 설정
        /// </summary>
        /// <param name="ready_Status">준비 상태</param>
        public void Set_Player_Ready(bool ready_Status)
        {
            is_Player_Ready = ready_Status;
        }

        /// <summary>
        /// 플레이어 데이터 리셋 (새 게임 시작 시)
        /// </summary>
        public void Reset_Player_Data()
        {
            current_Player_State = Player_State.Connected_Waiting;
            current_Location = Player_Location.Surface_Area;
            player_Position = Vector3.zero;
            is_Player_Ready = false;

            current_Dike_Height = 1.0f;
            dike_Durability = maximum_Dike_Durability;
            dike_Repair_Progress = 0.0f;

            Reset_Statistics();
        }

        /// <summary>
        /// 통계 데이터 리셋
        /// </summary>
        private void Reset_Statistics()
        {
            total_Play_Time = 0.0f;
            resources_Collected = 0;
            buildings_Built = 0;
            attacks_Launched = 0;
            defenses_Built = 0;
        }

        /// <summary>
        /// 플레이어 데이터를 문자열로 변환 (디버깅용)
        /// </summary>
        public override string ToString()
        {
            return $"Player {player_ID}: {player_Name} | State: {current_Player_State} | Dike: {current_Dike_Height}m | Durability: {dike_Durability}/{maximum_Dike_Durability}";
        }
    }

    /// <summary>
    /// 제방 정보만을 위한 별도 데이터 구조
    /// </summary>
    [Serializable]
    public struct Dike_Info
    {
        public float height;
        public int durability;
        public int max_Durability;
        public Vector3 position;
        public Water_Level_Alert alert_Level;

        public Dike_Info(float initial_Height, int initial_Durability, Vector3 dike_Position)
        {
            height = initial_Height;
            durability = initial_Durability;
            max_Durability = initial_Durability;
            position = dike_Position;
            alert_Level = Water_Level_Alert.Safe;
        }

        public void Update_Alert_Level(float water_Level)
        {
            float height_Ratio = water_Level / height;
            
            if (height_Ratio >= 1.0f)
                alert_Level = Water_Level_Alert.Flooding;
            else if (height_Ratio >= 0.95f)
                alert_Level = Water_Level_Alert.Critical;
            else if (height_Ratio >= 0.85f)
                alert_Level = Water_Level_Alert.Warning;
            else if (height_Ratio >= 0.70f)
                alert_Level = Water_Level_Alert.Caution;
            else
                alert_Level = Water_Level_Alert.Safe;
        }
    }
}
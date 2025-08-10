using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
using System;

/// <summary>
/// 멀티플레이 게임의 전체 상태 관리 및 조율을 담당하는 메인 매니저
/// Input: 네트워크 이벤트, 플레이어 액션, 시간 업데이트
/// Output: 게임 상태 변경, 승패 판정, 네트워크 동기화
/// Type: NetworkBehaviour (Singleton)
/// </summary>
public class Game_Manager : NetworkBehaviour
{
    #region Singleton Pattern
    /// <summary>
    /// 전역 접근 가능한 Game_Manager 인스턴스
    /// 다른 클래스에서 Game_Manager.Instance로 접근 가능
    /// </summary>
    public static Game_Manager Instance { get; private set; }
    
    /// <summary>
    /// Unity Awake 함수 - 객체 생성 시 한 번만 실행
    /// Singleton 패턴 구현: 동시에 여러 개의 Game_Manager가 존재하지 않도록 보장
    /// </summary>
    private void Awake()
    {
        // 이미 다른 Game_Manager 인스턴스가 존재한다면 자신을 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // 자신을 전역 인스턴스로 설정
        Instance = this;
        
        // 씬이 바뀌어도 파괴되지 않도록 설정 (게임 내내 유지)
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Game State Enum
    /// <summary>
    /// 게임의 현재 상태를 나타내는 열거형
    /// 네트워크를 통해 모든 클라이언트에게 동기화됨
    /// </summary>
    public enum Game_State_Type
    {
        Waiting_For_Players,  // 플레이어 2명 접속 대기 중
        Game_Starting,        // 게임 시작 카운트다운 (3초)
        In_Game,             // 실제 게임 진행 중 
        Game_Paused,         // 게임 일시정지 (플레이어 접속 끊김 등)
        Game_Over            // 게임 종료 (승부 결정됨)
    }
    #endregion

    #region Dike System Data Structures
    /// <summary>
    /// 제방의 각 층(Layer)에 대한 데이터를 저장하는 구조체
    /// 네트워크를 통해 전송 가능하며, NetworkList에서 사용하기 위해 IEquatable 구현
    /// 돌(100), 철(200), 강철(400) 체력
    /// </summary>
    [System.Serializable]
    public struct Dike_Layer_Data : INetworkSerializable, System.IEquatable<Dike_Layer_Data>
    {
        public float Current_Health;  // 현재 체력 (공격받으면 감소)
        public float Max_Health;      // 최대 체력 (건설 시 설정됨)
        public int Material_Type;     // 0: 돌, 1: 철, 2: 강철
        
        /// <summary>
        /// 새로운 제방 층 생성자
        /// </summary>
        /// <param name="max_health">최대 체력 (재료에 따라 결정)</param>
        /// <param name="material_type">재료 종류 (0: 돌, 1: 철, 2: 강철)</param>
        public Dike_Layer_Data(float max_health, int material_type)
        {
            Current_Health = max_health;  // 생성 시에는 최대 체력으로 시작
            Max_Health = max_health;
            Material_Type = material_type;
        }

        /// <summary>
        /// Unity Netcode에서 이 구조체를 네트워크로 전송하기 위한 직렬화 함수
        /// 클라이언트 간에 제방 데이터를 동기화할 때 사용됨
        /// </summary>
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Current_Health);
            serializer.SerializeValue(ref Max_Health);
            serializer.SerializeValue(ref Material_Type);
        }

        /// <summary>
        /// IEquatable 인터페이스 구현 - NetworkList에서 요구함
        /// 두 제방 층 데이터가 같은지 비교하는 함수
        /// </summary>
        public bool Equals(Dike_Layer_Data other)
        {
            return Current_Health.Equals(other.Current_Health) &&
                   Max_Health.Equals(other.Max_Health) &&
                   Material_Type == other.Material_Type;
        }

        public override bool Equals(object obj)
        {
            return obj is Dike_Layer_Data other && Equals(other);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Current_Health, Max_Health, Material_Type);
        }

        public static bool operator ==(Dike_Layer_Data left, Dike_Layer_Data right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Dike_Layer_Data left, Dike_Layer_Data right)
        {
            return !(left == right);
        }
    }
    
    /// <summary>
    /// 제방 재료 종류 열거형 (Excel 파일 기반)
    /// 각 재료별로 체력과 필요 자원이 다름
    /// </summary>
    public enum Dike_Material_Type
    {
        Stone = 0,     // 돌 제방 - 체력 100, 재료: 석재 + 나무
        Iron = 1,      // 철 제방 - 체력 200, 재료: 철 + 석재  
        Steel = 2      // 강철 제방 - 체력 400, 재료: 강철 + 석재 + 나무
    }
    #endregion

    #region Network Variables
    /// <summary>
    /// 네트워크를 통해 모든 클라이언트에 동기화되는 게임 데이터들
    /// 서버(Host)에서만 수정 가능하고, 모든 클라이언트가 읽을 수 있음
    /// </summary>
    [Header("=== Network Game State ===")]
    /// <summary>
    /// 현재 게임 상태 - 모든 클라이언트에게 실시간 동기화
    /// 서버에서만 변경 가능, 클라이언트는 읽기 전용
    /// </summary>
    public NetworkVariable<Game_State_Type> Current_Game_State = new NetworkVariable<Game_State_Type>(
        Game_State_Type.Waiting_For_Players,  // 초기값: 플레이어 대기
        NetworkVariableReadPermission.Everyone,   // 모든 클라이언트가 읽기 가능
        NetworkVariableWritePermission.Server     // 서버만 쓰기 가능
    );

    /// <summary>
    /// 게임 경과 시간 (초 단위) - 물 범람 타이밍 및 게임 종료 시간 계산에 사용
    /// </summary>
    public NetworkVariable<float> Network_Game_Timer = new NetworkVariable<float>(
        0f,                                        // 초기값: 0초
        NetworkVariableReadPermission.Everyone,   // 모든 클라이언트 읽기 가능
        NetworkVariableWritePermission.Server     // 서버만 증가시킬 수 있음
    );

    /// <summary>
    /// 현재 물 수위 (미터 단위) - 제방 높이와 비교하여 범람 판정
    /// 게임 시작 1분 후부터 상승 시작, 시간이 지날수록 상승 속도 증가
    /// </summary>
    public NetworkVariable<float> Network_Water_Level = new NetworkVariable<float>(
        0f,                                        // 초기값: 0미터
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>
    /// 현재 접속 중인 플레이어 수 - 2명이 되면 게임 시작
    /// </summary>
    public NetworkVariable<int> Connected_Players_Count = new NetworkVariable<int>(
        0,                                         // 초기값: 0명
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    #endregion

    #region Dike Network Data
    /// <summary>
    /// 각 플레이어의 제방 층 데이터를 저장하는 네트워크 리스트
    /// 각 요소는 제방의 한 층을 나타내며, 아래부터 위로 순서대로 저장됨
    /// </summary>
    [Header("=== Dike Management ===")]
    /// <summary>
    /// 플레이어 1의 제방 층들 - 인덱스 0이 가장 아래층, 마지막이 최상위층
    /// NetworkList는 선언과 동시에 초기화 필수
    /// </summary>
    public NetworkList<Dike_Layer_Data> Player_1_Dike_Layers = new NetworkList<Dike_Layer_Data>();
    
    /// <summary>
    /// 플레이어 2의 제방 층들 - 공격은 항상 최상위층(마지막 인덱스)만 가능
    /// NetworkList는 선언과 동시에 초기화 필수
    /// </summary>
    public NetworkList<Dike_Layer_Data> Player_2_Dike_Layers = new NetworkList<Dike_Layer_Data>();

    #endregion

    #region Game Settings
    /// <summary>
    /// 게임 규칙과 밸런싱을 위한 설정값들 (Inspector에서 조정 가능)
    /// </summary>
    [Header("=== Game Settings ===")]
    [SerializeField] private float Game_Duration_Minutes = 15f;          // 최대 게임 시간 (분)
    [SerializeField] private float Water_Start_Delay = 60f;              // 물 상승 시작 지연 시간 (초)
    [SerializeField] private float Water_Base_Speed = 0.05f;             // 초기 물 상승 속도 (m/초)
    [SerializeField] private float Water_Acceleration = 0.01f;           // 시간당 속도 증가량 (m/초²)
    [SerializeField] private float Initial_Water_Level = 0f;             // 게임 시작 시 물 높이
    [SerializeField] private float Single_Dike_Layer_Height = 1f;        // 제방 1층당 높이 (미터)
    [SerializeField] private float Victory_Condition_Check_Interval = 0.5f;  // 승리 조건 체크 주기 (초)
    
    [Header("=== Dike Material Settings ===")]
    /// <summary>
    /// Excel 파일 기반 제방 재료별 체력 설정
    /// 인덱스 0: 돌(100), 1: 철(200), 2: 강철(400)
    /// </summary>
    [SerializeField] private float[] Dike_Material_Health = { 100f, 200f, 400f };
    #endregion

    #region Events
    /// <summary>
    /// 다른 시스템들이 구독할 수 있는 게임 이벤트들
    /// UI, 사운드, 이펙트 시스템 등에서 이 이벤트들을 구독하여 반응
    /// </summary>
    public static event Action<Game_State_Type> On_Game_State_Changed;   // 게임 상태 변경 시
    public static event Action<int> On_Player_Flooded;                   // 플레이어 범람 시 (승패 결정)
    public static event Action<int> On_Player_Victory;                    // 승리 선언 시
    public static event Action<float> On_Water_Level_Updated;             // 수위 변경 시 (UI 업데이트용)
    public static event Action<int, int> On_Dike_Layer_Damaged;          // 제방층 피해 시 (player_id, layer_index)
    public static event Action<int, int> On_Dike_Layer_Destroyed;        // 제방층 파괴 시 (player_id, layer_index)
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Initialize_Network_Lists();
        
        if (IsServer)
        {
            Initialize_Server_Components();
        }
        
        Initialize_Client_Components();
        Subscribe_To_Network_Events();
    }

    public override void OnNetworkDespawn()
    {
        Unsubscribe_From_Network_Events();
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (IsServer && Current_Game_State.Value == Game_State_Type.In_Game)
        {
            Update_Game_Timer();
            Update_Water_Level();
            Check_Victory_Conditions();
        }
    }
    #endregion

    #region Network Lists Initialization
    /// <summary>
    /// NetworkList 초기화 함수 (이미 선언 시 초기화되어 있으므로 불필요하지만 안전장치로 유지)
    /// Unity Netcode에서는 NetworkList를 필드 선언 시점에 초기화해야 함
    /// </summary>
    private void Initialize_Network_Lists()
    {
        // 이제 선언과 동시에 초기화되므로 이 코드는 실행되지 않을 것임
        if (Player_1_Dike_Layers == null)
            Player_1_Dike_Layers = new NetworkList<Dike_Layer_Data>();
        
        if (Player_2_Dike_Layers == null)
            Player_2_Dike_Layers = new NetworkList<Dike_Layer_Data>();
            
        Debug.Log("[Game_Manager] NetworkList initialization check completed");
    }
    #endregion

    #region Server Initialization
    private void Initialize_Server_Components()
    {
        Connected_Players_Count.Value = NetworkManager.Singleton.ConnectedClients.Count;
        Network_Water_Level.Value = Initial_Water_Level;
        Network_Game_Timer.Value = 0f;
        
        // 초기 제방 설정 (기본 돌 제방 1층)
        Reset_Player_Dikes();
        
        // 플레이어 연결/해제 이벤트 구독
        NetworkManager.Singleton.OnClientConnectedCallback += On_Client_Connected;
        NetworkManager.Singleton.OnClientDisconnectCallback += On_Client_Disconnected;
        
        Debug.Log("[Game_Manager] Server components initialized");
    }
    #endregion

    #region Client Initialization  
    private void Initialize_Client_Components()
    {
        Debug.Log("[Game_Manager] Client components initialized");
    }
    #endregion

    #region Network Events Subscription
    private void Subscribe_To_Network_Events()
    {
        Current_Game_State.OnValueChanged += Handle_Game_State_Changed;
        Network_Water_Level.OnValueChanged += Handle_Water_Level_Changed;
        Connected_Players_Count.OnValueChanged += Handle_Players_Count_Changed;
    }

    private void Unsubscribe_From_Network_Events()
    {
        Current_Game_State.OnValueChanged -= Handle_Game_State_Changed;
        Network_Water_Level.OnValueChanged -= Handle_Water_Level_Changed;
        Connected_Players_Count.OnValueChanged -= Handle_Players_Count_Changed;
    }
    #endregion

    #region Client Connection Management
    private void On_Client_Connected(ulong clientId)
    {
        Connected_Players_Count.Value = (int)NetworkManager.Singleton.ConnectedClients.Count;
        Debug.Log($"[Game_Manager] Client {clientId} connected. Total players: {Connected_Players_Count.Value}");
        
        if (Connected_Players_Count.Value >= 2)
        {
            Start_Game();
        }
    }

    private void On_Client_Disconnected(ulong clientId)
    {
        Connected_Players_Count.Value = (int)NetworkManager.Singleton.ConnectedClients.Count;
        Debug.Log($"[Game_Manager] Client {clientId} disconnected. Remaining players: {Connected_Players_Count.Value}");
        
        if (Current_Game_State.Value == Game_State_Type.In_Game)
        {
            Pause_Game();
        }
    }
    #endregion

    #region Game Flow Control
    public void Start_Game()
    {
        if (!IsServer) return;
        
        if (Connected_Players_Count.Value >= 2)
        {
            Change_Game_State(Game_State_Type.Game_Starting);
            Invoke(nameof(Begin_Game_Play), 3f); // 3초 카운트다운 후 게임 시작
        }
    }

    private void Begin_Game_Play()
    {
        if (!IsServer) return;
        
        Network_Game_Timer.Value = 0f;
        Network_Water_Level.Value = Initial_Water_Level;
        Reset_Player_Dikes();
        
        Change_Game_State(Game_State_Type.In_Game);
    }

    public void Pause_Game()
    {
        if (!IsServer) return;
        
        if (Current_Game_State.Value == Game_State_Type.In_Game)
        {
            Change_Game_State(Game_State_Type.Game_Paused);
        }
    }

    public void Resume_Game()
    {
        if (!IsServer) return;
        
        if (Current_Game_State.Value == Game_State_Type.Game_Paused)
        {
            Change_Game_State(Game_State_Type.In_Game);
        }
    }

    private void End_Game(int winner_player_id)
    {
        if (!IsServer) return;
        
        Change_Game_State(Game_State_Type.Game_Over);
        Announce_Victory_ClientRpc(winner_player_id);
    }
    #endregion

    #region Game State Management
    private void Change_Game_State(Game_State_Type new_state)
    {
        if (!IsServer) return;
        
        Current_Game_State.Value = new_state;
        Debug.Log($"[Game_Manager] Game state changed to: {new_state}");
    }

    private void Handle_Game_State_Changed(Game_State_Type previous_state, Game_State_Type new_state)
    {
        On_Game_State_Changed?.Invoke(new_state);
        Debug.Log($"[Game_Manager] State transition: {previous_state} -> {new_state}");
    }
    #endregion

    #region Timer and Water Management
    /// <summary>
    /// 매 프레임 게임 타이머 업데이트 (서버에서만 실행)
    /// 최대 게임 시간 도달 시 게임 종료 처리
    /// </summary>
    private void Update_Game_Timer()
    {
        Network_Game_Timer.Value += Time.deltaTime;  // 경과 시간 누적
        
        // 최대 게임 시간(15분) 도달 체크
        if (Network_Game_Timer.Value >= Game_Duration_Minutes * 60f)
        {
            Handle_Time_Up();  // 시간 종료로 인한 무승부 처리
        }
    }

    /// <summary>
    /// 물 수위 업데이트 로직 (서버에서만 실행)
    /// 게임 시작 1분 후부터 물이 상승하기 시작하며, 시간이 지날수록 상승 속도가 빨라짐
    /// </summary>
    private void Update_Water_Level()
    {
        // 게임 시작 1분 동안은 물이 상승하지 않음 (준비 시간)
        if (Network_Game_Timer.Value < Water_Start_Delay) return;
        
        // 물 상승 시작 이후 경과 시간 계산
        float elapsed_flood_time = Network_Game_Timer.Value - Water_Start_Delay;
        
        // 현재 상승 속도 = 기본 속도 + (가속도 × 경과 시간)
        float current_speed = Water_Base_Speed + (Water_Acceleration * elapsed_flood_time);
        
        // 수위 증가 적용
        Network_Water_Level.Value += current_speed * Time.deltaTime;
    }

    private void Handle_Water_Level_Changed(float previous_level, float new_level)
    {
        On_Water_Level_Updated?.Invoke(new_level);
    }

    private void Handle_Time_Up()
    {
        // 시간 종료 시 양쪽 다 살아있으면 무승부
        End_Game(0);
    }
    #endregion

    #region Dike Management
    private void Reset_Player_Dikes()
    {
        if (!IsServer) return;
        
        // 기존 제방 제거
        Player_1_Dike_Layers.Clear();
        Player_2_Dike_Layers.Clear();
        
        // 초기 제방 설정 (돌 제방 1층)
        var initial_dike = new Dike_Layer_Data(Dike_Material_Health[0], 0);
        Player_1_Dike_Layers.Add(initial_dike);
        Player_2_Dike_Layers.Add(initial_dike);
        
        Debug.Log("[Game_Manager] Player dikes reset");
    }

    /// <summary>
    /// 플레이어의 총 제방 높이 계산 함수
    /// 제방 층 개수 × 층당 높이(1m) = 총 높이
    /// 예: 3층 제방 = 3 × 1m = 3m 높이
    /// </summary>
    /// <param name="player_id">플레이어 ID (1 또는 2)</param>
    /// <returns>총 제방 높이 (미터 단위)</returns>
    public float Calculate_Total_Dike_Height(int player_id)
    {
        // 해당 플레이어의 제방 층 리스트 가져오기
        var dike_layers = (player_id == 1) ? Player_1_Dike_Layers : Player_2_Dike_Layers;
        
        // 층 개수 × 층당 높이로 총 높이 계산
        return dike_layers.Count * Single_Dike_Layer_Height;
    }

    public int Get_Dike_Layers_Count(int player_id)
    {
        return (player_id == 1) ? Player_1_Dike_Layers.Count : Player_2_Dike_Layers.Count;
    }

    public float Get_Dike_Layer_Health_Percentage(int player_id, int layer_index)
    {
        var dike_layers = (player_id == 1) ? Player_1_Dike_Layers : Player_2_Dike_Layers;
        if (layer_index >= 0 && layer_index < dike_layers.Count)
        {
            var layer = dike_layers[layer_index];
            return layer.Current_Health / layer.Max_Health;
        }
        return 0f;
    }
    #endregion

    #region Victory Conditions
    private float victory_check_timer = 0f;
    
    /// <summary>
    /// 승리 조건 체크 로직 (서버에서만 실행)
    /// 매 프레임마다 실행하지 않고 일정 간격(0.5초)으로 체크하여 성능 최적화
    /// 유일한 패배 조건: 수위가 자신의 제방 높이를 넘는 범람 상황
    /// </summary>
    private void Check_Victory_Conditions()
    {
        victory_check_timer += Time.deltaTime;
        
        // 설정된 간격(0.5초)마다만 승리 조건 체크
        if (victory_check_timer >= Victory_Condition_Check_Interval)
        {
            victory_check_timer = 0f;  // 타이머 리셋
            
            // 각 플레이어의 현재 총 제방 높이 계산
            float player1_total_height = Calculate_Total_Dike_Height(1);
            float player2_total_height = Calculate_Total_Dike_Height(2);
            
            // 플레이어 1 범람 체크: 물 수위가 제방을 넘으면 즉시 패배
            if (Network_Water_Level.Value > player1_total_height)
            {
                On_Player_Flooded?.Invoke(1);           // 범람 이벤트 발생
                End_Game(2);                             // 플레이어 2 승리
                Debug.Log($"[Game_Manager] Player 1 flooded! Water: {Network_Water_Level.Value}m, Dike: {player1_total_height}m");
                return;
            }
            
            // 플레이어 2 범람 체크: 동일한 로직
            if (Network_Water_Level.Value > player2_total_height)
            {
                On_Player_Flooded?.Invoke(2);           // 범람 이벤트 발생
                End_Game(1);                             // 플레이어 1 승리
                Debug.Log($"[Game_Manager] Player 2 flooded! Water: {Network_Water_Level.Value}m, Dike: {player2_total_height}m");
                return;
            }
        }
    }
    #endregion

    #region Player Management

    private void Handle_Players_Count_Changed(int previous_count, int new_count)
    {
        Debug.Log($"[Game_Manager] Players count changed: {previous_count} -> {new_count}");
        
        if (new_count < 2 && Current_Game_State.Value == Game_State_Type.In_Game)
        {
            Pause_Game();
        }
    }
    #endregion

    #region RPC Functions
    [ClientRpc]
    private void Announce_Victory_ClientRpc(int winner_player_id)
    {
        On_Player_Victory?.Invoke(winner_player_id);
        
        string result_message = winner_player_id switch
        {
            0 => "Draw! Time's up!",
            1 => "Player 1 Wins!",
            2 => "Player 2 Wins!",
            _ => "Unknown result"
        };
        
        Debug.Log($"[Game_Manager] {result_message}");
    }

    /// <summary>
    /// 클라이언트에서 서버로 제방 건설 요청을 보내는 RPC 함수
    /// 서버에서 유효성 검증 후 새로운 제방 층을 추가
    /// </summary>
    /// <param name="player_id">건설하는 플레이어 ID</param>
    /// <param name="material_type">제방 재료 종류 (0: 돌, 1: 철, 2: 강철)</param>
    [ServerRpc(RequireOwnership = false)]  // 클라이언트 소유권 없이도 호출 가능
    public void Add_Dike_Layer_ServerRpc(int player_id, int material_type)
    {
        // 유효하지 않은 재료 타입 체크
        if (material_type < 0 || material_type >= Dike_Material_Health.Length) return;
        
        // 해당 플레이어의 제방 리스트 선택
        var dike_layers = (player_id == 1) ? Player_1_Dike_Layers : Player_2_Dike_Layers;
        
        // 새로운 제방 층 생성 (재료에 따른 최대 체력 설정)
        var new_layer = new Dike_Layer_Data(Dike_Material_Health[material_type], material_type);
        
        // 제방 리스트 맨 위에 추가 (가장 높은 층이 됨)
        dike_layers.Add(new_layer);
        
        Debug.Log($"[Game_Manager] Player {player_id} added {(Dike_Material_Type)material_type} dike layer. Health: {Dike_Material_Health[material_type]}");
    }

    /// <summary>
    /// 상대방 제방 공격을 처리하는 RPC 함수
    /// 게임 규칙: 항상 최상위 제방층만 공격 가능
    /// 제방이 파괴되면 총 높이가 1m 감소하여 범람 위험 증가
    /// </summary>
    /// <param name="target_player">공격 대상 플레이어 ID</param>
    /// <param name="damage">가할 피해량</param>
    [ServerRpc(RequireOwnership = false)]
    public void Attack_Enemy_Dike_ServerRpc(int target_player, float damage)
    {
        // 공격 대상 플레이어의 제방 리스트 가져오기
        var target_layers = (target_player == 1) ? Player_1_Dike_Layers : Player_2_Dike_Layers;
        
        // 제방이 존재하는 경우에만 공격 처리
        if (target_layers.Count > 0)
        {
            // 최상위 제방층 인덱스 (마지막 요소)
            int top_layer_index = target_layers.Count - 1;
            var layer = target_layers[top_layer_index];
            
            // 체력에서 피해량 차감 (최소 0까지)
            layer.Current_Health = Mathf.Max(0f, layer.Current_Health - damage);
            
            if (layer.Current_Health <= 0)
            {
                // 체력이 0 이하면 제방층 완전 파괴
                target_layers.RemoveAt(top_layer_index);                        // 리스트에서 제거
                On_Dike_Layer_Destroyed?.Invoke(target_player, top_layer_index); // 파괴 이벤트 발생
                Debug.Log($"[Game_Manager] Player {target_player} dike layer {top_layer_index} destroyed!");
            }
            else
            {
                // 체력이 남아있으면 피해만 적용
                target_layers[top_layer_index] = layer;                         // 수정된 데이터 저장
                On_Dike_Layer_Damaged?.Invoke(target_player, top_layer_index);  // 피해 이벤트 발생
                Debug.Log($"[Game_Manager] Player {target_player} dike layer {top_layer_index} damaged. Health: {layer.Current_Health}/{layer.Max_Health}");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void Request_Pause_Game_ServerRpc()
    {
        Pause_Game();
    }

    [ServerRpc(RequireOwnership = false)]
    public void Request_Resume_Game_ServerRpc()
    {
        Resume_Game();
    }
    #endregion

    #region Public Getters
    public bool Is_Game_In_Progress()
    {
        return Current_Game_State.Value == Game_State_Type.In_Game;
    }

    public bool Is_Game_Over()
    {
        return Current_Game_State.Value == Game_State_Type.Game_Over;
    }

    public float Get_Remaining_Time_Minutes()
    {
        return Mathf.Max(0f, Game_Duration_Minutes - (Network_Game_Timer.Value / 60f));
    }

    public float Get_Current_Water_Level()
    {
        return Network_Water_Level.Value;
    }

    public float Get_Water_Start_Delay()
    {
        return Water_Start_Delay;
    }

    public bool Is_Water_Rising()
    {
        return Network_Game_Timer.Value >= Water_Start_Delay;
    }

    public float Get_Current_Water_Rise_Speed()
    {
        if (!Is_Water_Rising()) return 0f;
        
        float elapsed_flood_time = Network_Game_Timer.Value - Water_Start_Delay;
        return Water_Base_Speed + (Water_Acceleration * elapsed_flood_time);
    }

    public bool Is_Player_Flooded(int player_id)
    {
        float player_dike_height = Calculate_Total_Dike_Height(player_id);
        return Network_Water_Level.Value > player_dike_height;
    }
    #endregion
}
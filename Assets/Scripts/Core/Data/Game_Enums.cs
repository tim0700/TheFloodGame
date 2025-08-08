/// <summary>
/// The Flood Game에서 사용되는 모든 열거형 정의
/// Input: 없음 (정적 열거형 정의)
/// Output: 게임 상태, 플레이어 상태, 게임 모드 등의 열거형
/// Type: Static Enums Collection
/// </summary>

namespace TheFloodGame.Core.Data
{
    /// <summary>
    /// 전체 게임의 현재 상태를 나타내는 열거형
    /// </summary>
    public enum Game_State
    {
        Waiting_For_Players,    // 플레이어 대기 중
        Game_Starting,          // 게임 시작 준비
        In_Progress,            // 게임 진행 중
        Game_Paused,            // 게임 일시정지 (네트워크 문제 등)
        Game_Ending,            // 게임 종료 처리 중
        Game_Over              // 게임 완료
    }

    /// <summary>
    /// 개별 플레이어의 현재 상태를 나타내는 열거형
    /// </summary>
    public enum Player_State
    {
        Disconnected,           // 연결 끊김
        Connected_Waiting,      // 연결됨, 대기 중
        Ready_To_Start,         // 게임 시작 준비 완료
        Playing_Surface,        // 지상에서 플레이 중
        Playing_Underground,    // 지하(갱도)에서 플레이 중
        Defeated,              // 패배 상태 (제방 파괴됨)
        Victorious             // 승리 상태
    }

    /// <summary>
    /// 게임 모드를 정의하는 열거형
    /// </summary>
    public enum Game_Mode
    {
        Local_Multiplayer,      // 로컬 멀티플레이 (Hot Seat)
        Online_Multiplayer,     // 온라인 멀티플레이
        Practice_Mode,          // 연습 모드 (AI 상대)
        Tutorial_Mode          // 튜토리얼 모드
    }

    /// <summary>
    /// 플레이어가 현재 위치한 게임 환경을 나타내는 열거형
    /// </summary>
    public enum Player_Location
    {
        Surface_Area,           // 지상 영역
        Underground_Mine,       // 지하 갱도
        Transition_Area        // 전환 구역 (지상-지하 이동 중)
    }

    /// <summary>
    /// 강물 수위 상승 경고 단계를 나타내는 열거형
    /// </summary>
    public enum Water_Level_Alert
    {
        Safe,                  // 안전 (충분한 여유)
        Caution,              // 주의 (제방 높이의 70% 도달)
        Warning,              // 경고 (제방 높이의 85% 도달)
        Critical,             // 위험 (제방 높이의 95% 도달)
        Flooding              // 범람 중 (제방 높이 초과)
    }

    /// <summary>
    /// 승리 조건 유형을 정의하는 열거형
    /// </summary>
    public enum Victory_Condition
    {
        Enemy_Dike_Destroyed,   // 상대방 제방 파괴
        Enemy_Disconnected,     // 상대방 연결 끊김
        Time_Limit_Reached,     // 시간 제한 도달 (높은 제방 승리)
        Enemy_Surrender        // 상대방 항복
    }

    /// <summary>
    /// 게임 시스템의 우선순위를 정의하는 열거형
    /// </summary>
    public enum System_Priority
    {
        Critical,              // 필수 시스템 (게임 매니저, 네트워크)
        High,                 // 핵심 게임플레이 (플레이어, 타이머)
        Medium,               // 일반 기능 (UI, 사운드)
        Low                   // 보조 기능 (시각 효과, 애니메이션)
    }

    /// <summary>
    /// 네트워크 연결 상태를 나타내는 열거형
    /// </summary>
    public enum Network_Status
    {
        Disconnected,          // 연결 안됨
        Connecting,           // 연결 중
        Connected,            // 연결됨
        Reconnecting,         // 재연결 중
        Connection_Lost,      // 연결 끊김
        Connection_Unstable   // 불안정한 연결
    }
}
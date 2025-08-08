using System;
using TheFloodGame.Core.Data;

/// <summary>
/// 모든 게임 시스템이 구현해야 하는 공통 인터페이스
/// Input: 시스템별 초기화 및 업데이트 요청
/// Output: 시스템 상태 및 생명주기 관리
/// Type: Interface
/// </summary>

namespace TheFloodGame.Core.Interfaces
{
    public interface IGameSystem
    {
        /// <summary>
        /// 시스템의 고유 식별자
        /// </summary>
        string System_Name { get; }

        /// <summary>
        /// 시스템의 우선순위 (초기화 및 업데이트 순서 결정)
        /// </summary>
        System_Priority Priority { get; }

        /// <summary>
        /// 현재 시스템이 활성화되어 있는지 여부
        /// </summary>
        bool Is_System_Active { get; }

        /// <summary>
        /// 시스템이 완전히 초기화되었는지 여부를 확인
        /// </summary>
        /// <returns>초기화 완료 시 true</returns>
        bool Is_System_Initialized();

        /// <summary>
        /// 시스템 초기화 수행
        /// </summary>
        /// <returns>초기화 성공 시 true</returns>
        bool Initialize_System();

        /// <summary>
        /// 시스템 시작 (게임 시작 시 호출)
        /// </summary>
        void Start_System();

        /// <summary>
        /// 시스템 업데이트 (매 프레임 호출)
        /// </summary>
        /// <param name="delta_Time">이전 프레임으로부터의 시간</param>
        void Update_System(float delta_Time);

        /// <summary>
        /// 시스템 일시정지
        /// </summary>
        void Pause_System();

        /// <summary>
        /// 시스템 재개
        /// </summary>
        void Resume_System();

        /// <summary>
        /// 시스템 정지 (게임 종료 시 호출)
        /// </summary>
        void Stop_System();

        /// <summary>
        /// 시스템 정리 및 메모리 해제
        /// </summary>
        void Cleanup_System();

        /// <summary>
        /// 시스템 리셋 (새 게임 시작 시 호출)
        /// </summary>
        void Reset_System();

        /// <summary>
        /// 현재 시스템이 정상 동작 중인지 상태 검사
        /// </summary>
        /// <returns>정상 상태일 때 true</returns>
        bool Check_System_Health();

        /// <summary>
        /// 시스템 오류 발생 시 호출되는 이벤트
        /// </summary>
        event Action<IGameSystem, string> On_System_Error;

        /// <summary>
        /// 시스템 상태 변경 시 호출되는 이벤트
        /// </summary>
        event Action<IGameSystem, bool> On_System_State_Changed;
    }

    /// <summary>
    /// 네트워크 동기화가 필요한 시스템을 위한 확장 인터페이스
    /// </summary>
    public interface INetworkGameSystem : IGameSystem
    {
        /// <summary>
        /// 네트워크 동기화 수행
        /// </summary>
        void Synchronize_Network_Data();

        /// <summary>
        /// 네트워크 데이터 수신 처리
        /// </summary>
        /// <param name="network_Data">수신된 데이터</param>
        void Handle_Network_Data(byte[] network_Data);

        /// <summary>
        /// 네트워크 연결 상태 확인
        /// </summary>
        /// <returns>네트워크 연결 상태</returns>
        Network_Status Get_Network_Status();
    }

    /// <summary>
    /// 저장 가능한 시스템을 위한 확장 인터페이스
    /// </summary>
    public interface ISaveableGameSystem : IGameSystem
    {
        /// <summary>
        /// 시스템 데이터를 저장 가능한 형태로 직렬화
        /// </summary>
        /// <returns>직렬화된 데이터</returns>
        string Serialize_System_Data();

        /// <summary>
        /// 저장된 데이터로부터 시스템 상태 복원
        /// </summary>
        /// <param name="serialized_Data">직렬화된 데이터</param>
        /// <returns>복원 성공 시 true</returns>
        bool Deserialize_System_Data(string serialized_Data);
    }

    /// <summary>
    /// 설정 가능한 시스템을 위한 확장 인터페이스
    /// </summary>
    public interface IConfigurableGameSystem : IGameSystem
    {
        /// <summary>
        /// 시스템 설정 적용
        /// </summary>
        /// <param name="config_Data">설정 데이터</param>
        void Apply_System_Configuration(object config_Data);

        /// <summary>
        /// 현재 시스템 설정 반환
        /// </summary>
        /// <returns>현재 설정 데이터</returns>
        object Get_System_Configuration();
    }
}
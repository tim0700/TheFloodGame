using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

// CharacterController가 반드시 필요한 스크립트임을 명시
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;    // 이동 속도
    public float gravity = -9.81f;     // 중력 값 (음수여야 아래로 떨어짐)
    public float jumpHeight = 1.5f;    // 점프 높이

    private CharacterController controller;    // 캐릭터컨트롤러 컴포넌트를 저장할 변수

    private Vector3 velocity;    // 현재 y축 속도 등을 저장하는 벡터
    private bool isGrounded;    // 땅에 닿아 있는지 여부
    public Transform groundCheck;    // 바닥 체크용 기준 위치 (보통 발밑에 빈 오브젝트 생성 후 연결)

    public float groundDistance = 0.4f;    // 바닥 체크 반지름 (바닥과 닿았다고 판단할 거리)
    public LayerMask Floor;        // 어떤 Layer를 바닥으로 인식할지 지정

    void Start()
    {
        controller = GetComponent<CharacterController>();        // CharacterController 컴포넌트 가져오기
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, Floor);        // 바닥에 닿아 있는지 확인 (원 형태로 충돌 검사)

        if (isGrounded && velocity.y < 0)        // 땅에 닿아있고 y속도가 내려가는 중이면, y속도를 약간만 유지시켜 착지 느낌
        {
            velocity.y = -2f;
        }

        // 입력값 가져오기 (A/D: x축, W/S: z축)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;        // 이동 방향 계산 (로컬 좌표 기준: 오른쪽과 앞으로)
        controller.Move(move * speed * Time.deltaTime);        // 캐릭터 이동 (속도 * 시간 보정)

        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("Space 누름");
        }

        if (Input.GetButtonDown("Jump") && isGrounded)        // 점프 입력 (스페이스바) & 땅에 있을 때만
        {
            Debug.Log("점프 실행됨!");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);            // 중력에 기반한 점프 속도 계산
        }

        velocity.y += gravity * Time.deltaTime;        // y축 중력 지속 적용
        controller.Move(velocity * Time.deltaTime);        // 캐릭터에 y축 속도 적용 (낙하/점프)
    }
}
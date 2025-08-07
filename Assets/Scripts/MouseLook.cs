using UnityEngine;

public class MouseLook : MonoBehaviour
{
    // 마우스 감도 (회전 속도 조절)
    public float mouseSensitivity = 100f;

    // 플레이어 본체 오브젝트 (Y축 회전용)
    public Transform playerBody;

    // 위아래 시야 각도 누적값 (X축 회전용)
    private float xRotation = 0f;

    void Start()
    {
        // 마우스 커서를 화면 중앙에 고정하고 보이지 않게 설정
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 마우스 이동값 받아오기 (Time.deltaTime으로 프레임 보정)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 마우스 Y축은 위아래 시야 회전이므로 xRotation에 누적 (마우스 아래로 움직이면 위를 보기 위해 음수 처리)
        xRotation -= mouseY;

        // 시야가 너무 위/아래로 꺾이지 않도록 제한
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 카메라(본 스크립트가 붙은 오브젝트)의 X축 회전 적용
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 플레이어 본체는 마우스 X축에 따라 Y축 회전
        playerBody.Rotate(Vector3.up * mouseX);
    }
}

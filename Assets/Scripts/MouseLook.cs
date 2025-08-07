using UnityEngine;

public class MouseLook : MonoBehaviour
{
    // ���콺 ���� (ȸ�� �ӵ� ����)
    public float mouseSensitivity = 100f;

    // �÷��̾� ��ü ������Ʈ (Y�� ȸ����)
    public Transform playerBody;

    // ���Ʒ� �þ� ���� ������ (X�� ȸ����)
    private float xRotation = 0f;

    void Start()
    {
        // ���콺 Ŀ���� ȭ�� �߾ӿ� �����ϰ� ������ �ʰ� ����
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ���콺 �̵��� �޾ƿ��� (Time.deltaTime���� ������ ����)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ���콺 Y���� ���Ʒ� �þ� ȸ���̹Ƿ� xRotation�� ���� (���콺 �Ʒ��� �����̸� ���� ���� ���� ���� ó��)
        xRotation -= mouseY;

        // �þ߰� �ʹ� ��/�Ʒ��� ������ �ʵ��� ����
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // ī�޶�(�� ��ũ��Ʈ�� ���� ������Ʈ)�� X�� ȸ�� ����
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // �÷��̾� ��ü�� ���콺 X�࿡ ���� Y�� ȸ��
        playerBody.Rotate(Vector3.up * mouseX);
    }
}

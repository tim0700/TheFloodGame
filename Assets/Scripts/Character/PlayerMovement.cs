using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

// CharacterController�� �ݵ�� �ʿ��� ��ũ��Ʈ���� ���
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;    // �̵� �ӵ�
    public float gravity = -9.81f;     // �߷� �� (�������� �Ʒ��� ������)
    public float jumpHeight = 1.5f;    // ���� ����

    private CharacterController controller;    // ĳ������Ʈ�ѷ� ������Ʈ�� ������ ����

    private Vector3 velocity;    // ���� y�� �ӵ� ���� �����ϴ� ����
    private bool isGrounded;    // ���� ��� �ִ��� ����
    public Transform groundCheck;    // �ٴ� üũ�� ���� ��ġ (���� �߹ؿ� �� ������Ʈ ���� �� ����)

    public float groundDistance = 0.4f;    // �ٴ� üũ ������ (�ٴڰ� ��Ҵٰ� �Ǵ��� �Ÿ�)
    public LayerMask Floor;        // � Layer�� �ٴ����� �ν����� ����

    void Start()
    {
        controller = GetComponent<CharacterController>();        // CharacterController ������Ʈ ��������
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, Floor);        // �ٴڿ� ��� �ִ��� Ȯ�� (�� ���·� �浹 �˻�)

        if (isGrounded && velocity.y < 0)        // ���� ����ְ� y�ӵ��� �������� ���̸�, y�ӵ��� �ణ�� �������� ���� ����
        {
            velocity.y = -2f;
        }

        // �Է°� �������� (A/D: x��, W/S: z��)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;        // �̵� ���� ��� (���� ��ǥ ����: �����ʰ� ������)
        controller.Move(move * speed * Time.deltaTime);        // ĳ���� �̵� (�ӵ� * �ð� ����)

        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("Space ����");
        }

        if (Input.GetButtonDown("Jump") && isGrounded)        // ���� �Է� (�����̽���) & ���� ���� ����
        {
            Debug.Log("���� �����!");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);            // �߷¿� ����� ���� �ӵ� ���
        }

        velocity.y += gravity * Time.deltaTime;        // y�� �߷� ���� ����
        controller.Move(velocity * Time.deltaTime);        // ĳ���Ϳ� y�� �ӵ� ���� (����/����)
    }
}
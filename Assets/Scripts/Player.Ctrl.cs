using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Player_Ctrl : MonoBehaviour
{
    Rigidbody rb;

    [Header("Rotate")]
    public float mouseSpeed;
    float yRotation;
    float xRotation;
    Camera cam;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   //���콺 Ŀ���� ȭ�� �ȿ��� ����
        Cursor.visible = false;                     //���콺 Ŀ���� ������ �ʵ��� ����

        rb = GetComponent<Rigidbody>();             // Rigidbody ������Ʈ ��������
        rb.freezeRotation = true;                   // Rigidbody�� ȸ���� �����Ͽ� ���� ���꿡 ������ ���� �ʵ��� ����

        cam = Camera.main;                          // ���� ī�޶� �Ҵ�
    }

    void Update()
    {

    }

}
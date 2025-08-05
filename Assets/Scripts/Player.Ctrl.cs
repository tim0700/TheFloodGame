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
        Cursor.lockState = CursorLockMode.Locked;   //마우스 커서를 화면 안에서 고정
        Cursor.visible = false;                     //마우스 커서를 보이지 않도록 설정

        rb = GetComponent<Rigidbody>();             // Rigidbody 컴포넌트 가져오기
        rb.freezeRotation = true;                   // Rigidbody의 회전을 고정하여 물리 연산에 영향을 주지 않도록 설정

        cam = Camera.main;                          // 메인 카메라를 할당
    }

    void Update()
    {

    }

}
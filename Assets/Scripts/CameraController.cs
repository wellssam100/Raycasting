using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private Vector3 dirControl = new Vector3 (0, 0, 0);
    private Camera _Camera;
    private float rotation = 0.0f;

    public CharacterController characterController;
    public float speed = 6;
    private float gravity = 9.81f;
    private float verticalSpeed = 2;
    private float horizontalSpeed = 2;

    public Transform cameraHolder;
    public float mouseSensitivity = 2.0f;
    public float upLimit = -50;
    public float downLimit = 50;
    // Start is called before the first frame update
    void Start()
    {
        _Camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //Move();
        Rotate();
        
    }

    public void Move()
    {
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * verticalMove + transform.right * horizontalMove;
        characterController.Move(speed * Time.deltaTime * move );
    }
    public void Rotate() 
    {
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = verticalSpeed * Input.GetAxis("Mouse Y");

        transform.Rotate(-v, h, 0);

        //transform.rotation = Quaternion.AngleAxis(-v, Vector3.up);
        //transform.rotation = Quaternion.AngleAxis(h, Vector3.right);



        //cameraHolder.localRotation = Quaternion.Euler(currentRotation);
    }


}

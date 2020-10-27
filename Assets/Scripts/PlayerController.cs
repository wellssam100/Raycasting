using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    private float xRotation=0f;
    public float speed = 2f;
    public Transform playerBody;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        transform.position = new Vector3(.5f, .1f, -1.0f);
        transform.rotation.Set(180f, 180f, 180f,1);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Rotate();
    }
    public void Move()
    {
        float y = Input.GetAxis("Horizontal") * speed;
        float x = Input.GetAxis("Vertical") * speed;
        float z = Input.GetAxis("Jump") * speed;

     transform.position += (transform.forward * x * Time.deltaTime) + (transform.right * y * Time.deltaTime) + (transform.up * z * Time.deltaTime);
      
    }
    public void Rotate() 
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.right * -mouseY);
        playerBody.Rotate(Vector3.up * mouseX);
     
    }

}

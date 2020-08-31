using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private Vector3 dirControl = new Vector3 (0, 0, 0);
    private Camera _Camera;
    private float rotation = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        _Camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A)) 
        {
            dirControl += -(transform.right);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            dirControl += transform.right;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            dirControl += transform.forward;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            dirControl += -(transform.forward);
        }


        if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A))
        {
            dirControl -= -(transform.right);
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D))
        {
            dirControl -= transform.right;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
        {
            dirControl -= transform.forward;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
        {
            dirControl -= -(transform.forward);
        }

        if (Input.GetKeyDown(KeyCode.E)) 
        {
            rotation += 1;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            rotation += -1;
        }
        if (Input.GetKeyUp(KeyCode.E)) 
        {
            rotation -= 1;
        }
        if (Input.GetKeyUp(KeyCode.Q)) 
        {
            rotation -= -1;
        }



        if (!dirControl.Equals(Vector3.zero))
        transform.position += dirControl;

        transform.Rotate(0.0f, rotation, 0.0f, Space.Self);
        
    }
    
    
    
}

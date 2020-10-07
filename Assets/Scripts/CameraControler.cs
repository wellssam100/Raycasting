using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class CameraControler : MonoBehaviour
{
    private float unitCirclePoint;
    private Quaternion rotation;
    public float speed = 1;
    public Transform target;
    public float radius=2f;
    private float angleIncrement =Mathf.PI / 360f;
    private RayTracing myTracing;
    // Start is called before the first frame update
    void Start()
    {
        
        unitCirclePoint = Mathf.PI/2f;
        transform.position = new Vector3(Mathf.Cos(unitCirclePoint), 0f, Mathf.Sin(unitCirclePoint)) *radius;
        myTracing = GetComponentInChildren<RayTracing>();
        rotation = new Quaternion();
    }

    // Update is called once per frame
    void Update()
    {
        



        Vector3 currentPos = new Vector3(Mathf.Cos(unitCirclePoint),0f, Mathf.Sin(unitCirclePoint)) *radius;
        unitCirclePoint = unitCirclePoint +(angleIncrement*speed);
        if (Mathf.Abs(unitCirclePoint) >= 2*360*angleIncrement) 
        {
            UnityEngine.Debug.Log("Next List...");
            unitCirclePoint = 0;
            RefreshList();
        }
        Vector3 nextPos = new Vector3(Mathf.Cos(unitCirclePoint),0f, Mathf.Sin(unitCirclePoint)) *radius;

        transform.position = Vector3.Lerp(currentPos, nextPos, 1);


       // transform.position = Vector3.Lerp(currentPos, nextPos, speed/Time.deltaTime);
       if(speed>0)
       transform.RotateAround(transform.position, -Vector3.up, .5f*speed);

       
    }
    void RefreshList() 
    {
        myTracing.ListSize++;
        myTracing.SetUpScene();
    }
}

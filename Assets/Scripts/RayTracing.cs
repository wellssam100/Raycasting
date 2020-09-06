﻿using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;

public class RayTracing : MonoBehaviour
{
    //SETUP DISPLAY
    public ComputeShader RayTracingShader;
    private RenderTexture _target;
    //SETUP CAMERA
    private Camera _camera;
    //SETUP SKYBOX
    public Texture SkyboxTexture;
    //Anti Aliasing Samples
    private uint _currentSample = 0;
    private Material _addMaterial;
    //Directional Lighting
    public Light DirectionalLight;

    //Public Options
    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public int SpheresMax = 100;
    public float SpherePlacementRadius = 50.0f;
    private ComputeBuffer _fordSphereBuffer, _randomSphereBuffer;

    public Color myColor;

    private int SpheresTotal;
    //Sphere Bob variable
    private float step;
    //Ford's Sphere tree depth
    public int depth;

    public struct Pair 
    {
        public int a;
        public int b;
    }
    public struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
    }

    //SETUP DISPLAY
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }

    private void OnEnable() 
    {
        _currentSample = 0;
        SetUpScene();
    }

    private void OnDisable() 
    {
        if (_fordSphereBuffer != null)
            _fordSphereBuffer.Release();
    }

    private void SetUpScene()
    {
        List<Sphere> fordSpheres = new List<Sphere>();
        List<Sphere> randomSpheres = new List<Sphere>();
        List<Pair> fractionMap =  new List<Pair>();
        

        drawRandomSpheres(randomSpheres);
        setupFordSpheres(fordSpheres, fractionMap);
               
        //Assign Ford Spheres to compute buffer
        _fordSphereBuffer = new ComputeBuffer(fordSpheres.Count, 40);
        _fordSphereBuffer.SetData(fordSpheres);
        SpheresTotal = fordSpheres.Count;



       
        
    }

    private void drawRandomSpheres(List<Sphere> spheres) 
    {
        //Add a number of spheres to the list
        for (int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();

            sphere.radius = SphereRadius.x + UnityEngine.Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = UnityEngine.Random.insideUnitCircle * SpherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            //reject spheres that are intersecting others
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                {
                    goto SkipSphere;
                }
            }
            
            Color color = UnityEngine.Random.ColorHSV();
            bool metal = UnityEngine.Random.value < 0.5f;
            sphere.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;

            spheres.Add(sphere);

        SkipSphere:
            continue;

        }
    }
    private void setupFordSpheres(List<Sphere> spheres, List<Pair> fractionMap) 
    {
        Sphere s1, s2 = new Sphere();
        s1.position = new Vector3(0, 0.5f, 0);
        s2.position = new Vector3(1, 0.5f, 0);
        s1.radius = 0.5f;
        s2.radius = 0.5f;

        Color colorBase_1 = new Color(s1.position.x, s1.position.x, s1.position.x);
        Color colorBase_2 = new Color(s2.position.x, s2.position.x, s2.position.x);

        Color color_1 = colorBase_1 * myColor;
        Color color_2 = colorBase_2 * myColor;


        bool metal = UnityEngine.Random.value < 0.5f;
        s1.albedo = Vector3.zero;
        s1.specular = new Vector3(color_1.r, color_1.g, color_1.b);
        s2.albedo = Vector3.zero;
        s2.specular = new Vector3(color_2.r, color_2.g, color_2.b);


        spheres.Add(s1);
        spheres.Add(s2);

        drawFordSpheres(1, 2, depth, spheres, fractionMap);
    }
    private void drawFordSpheres(int a, int b, int step , List<Sphere> spheres, List<Pair> fractionMap) 
    {
        if (step == 0) 
        {
            return;
        }
        //create hashmap with fractions in it (or maybe key-values of a,b)
        Pair p = new Pair();
        p.a = a;
        p.b = b;

        if (!fractionMap.Contains(p)) 
        {
            //Set values
            Sphere sphere = new Sphere();
            sphere.position = new Vector3 ((float)a / (float)b, 1.0f / (2.0f * (float)(Math.Pow(b, 2))), 0);
            sphere.radius = 1.0f / (2.0f * (float)(Math.Pow(b, 2)));
            //Add Gradient Color
            Color colorBase = new Color(sphere.position.x, sphere.position.x, sphere.position.x);
            Color color = colorBase * myColor;
            sphere.albedo = new Vector3(color.r, color.g, color.b);
            sphere.specular = Vector3.one * 0.04f;
            //Add Sphere to Lst
            spheres.Add(sphere);
        }
        step--;
        fractionMap.Add(p);
       // UnityEngine.Debug.Log("x: " + p.a + "/" + p.b + "\n");
        drawFordSpheres(a, b + 1, step, spheres, fractionMap);
        drawFordSpheres(a + 1, b + 1, step, spheres, fractionMap);
    
    }
    //returns true if the elements are the same, false if they are different
    private bool comPair(Pair p1, Pair p2) 
    {
        if (p1.a == p2.a && p1.b == p2.b) 
        {
            return true;
        }
        return false;
    }

    private void Awake() 
    {
        _camera = GetComponent<Camera>();

    }

    private void Update() 
    {
        //To restart the anti-aliasing sampling
        //I might be able to take these two out, since the spheres will always be moving
        if (transform.hasChanged) 
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }
        if (DirectionalLight.transform.hasChanged) 
        {
            _currentSample = 0;
            DirectionalLight.transform.hasChanged = false;
        }
        //SphereBob();



    }

    private void SphereBob() 
    {
       
        List<Sphere> spheres = new List<Sphere>();

        Sphere[] tempArray = new Sphere[SpheresTotal];
        _randomSphereBuffer.GetData(tempArray, 0, 0, SpheresTotal);
        for (int i = 0; i < tempArray.Length; i++) 
        {
            spheres.Add(tempArray[i]);
            
        }


        List<Sphere> temp = new List<Sphere>();

        foreach(Sphere sphere in spheres)
        {
            Sphere tempSphere = sphere;
            float up = (float)(Math.Sin(step))/ sphere.radius;
            step = (step + (float)Math.PI / 2048)%((float)(Math.PI)*2);
            tempSphere.position.y = sphere.position.y + up;
            temp.Add(tempSphere);
        }
        _randomSphereBuffer.SetData(temp);
        Sphere[] check = new Sphere[1];
        _randomSphereBuffer.GetData(check, 0, 0, 1); 
        _currentSample = 0;

        RayTracingShader.SetBuffer(0, "_Spheres", _randomSphereBuffer);

    }

    //to each rational number a/b (in lowest terms), assign a circle above but tangent to the x-axis at a/b with radius1/2b2
    /*private void fordLine(List<Sphere> spheres) 
    {
        for(int i=0;i<SpheresMax;i++)
        {
            spheres[i].radius = ((float)(1)) / (2 * Math.Pow(SpheresMax, 2));
            spheres[i].position = (i / SpheresMax,sphere.radius,0);
        }
    }*/
   

    //SETUP DISPLAY
    private void Render (RenderTexture destination)
    {
        InitRenderTexture();

        //set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);


        if (_addMaterial == null) 
        {
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        }
        _addMaterial.SetFloat("_Sample", _currentSample);

        //Blit the Results
        //Blit(source,dest):Copies source texture into destination render texture with a shader.
        //Blit(source, dest, mat ) mat == Material to use. Material's shader could do some post-processing effect, for example.
        Graphics.Blit(_target, destination,_addMaterial);
        _currentSample++;
    }
   
    //SETUP DISPLAY
    private void InitRenderTexture() 
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            //Release render texture if we already have one
            if (_target != null)
            {
                _target.Release();
            }

            _target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }


    }

    //SETUP CAMERA
    private void SetShaderParameters() 
    {
        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);

        //Set the texture on the shader
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        //Anti-Aliasing pixel offset
        RayTracingShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
        //Directional Lighting direction
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));
        //TODO: figure out if I can set then draw from two different buffers
        RayTracingShader.SetBuffer(0, "_Spheres", _fordSphereBuffer);
    }

    
}

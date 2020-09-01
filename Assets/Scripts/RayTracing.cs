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
    private ComputeBuffer _sphereBuffer;

    private int SpheresTotal;
    private float step;


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
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
    }

    private void SetUpScene()
    {
        List<Sphere> spheres = new List<Sphere>();

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

        //Assign to compute buffer
        _sphereBuffer = new ComputeBuffer(spheres.Count, 40);
        _sphereBuffer.SetData(spheres);
        SpheresTotal = spheres.Count;

       
        
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
       // SphereBob();



    }

    private void SphereBob() 
    {
       
        List<Sphere> spheres = new List<Sphere>();

        Sphere[] tempArray = new Sphere[SpheresTotal];
        _sphereBuffer.GetData(tempArray, 0, 0, SpheresTotal);
        for (int i = 0; i < tempArray.Length; i++) 
        {
            spheres.Add(tempArray[i]);
            
        }


        List<Sphere> temp = new List<Sphere>();

        foreach(Sphere sphere in spheres)
        {
            Sphere tempSphere = sphere;
            float up = (float)(Math.Sin(step));
            step = step + (float)Math.PI / 72;
            tempSphere.position.y = sphere.position.y + up;
            temp.Add(tempSphere);
        }
        _sphereBuffer.SetData(temp);
        Sphere[] check = new Sphere[1];
        _sphereBuffer.GetData(check, 0, 0, 1); 
        _currentSample = 0;

        RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);

    }

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

        RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
    }

    
}

using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
//using System.IO.Ports;
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
    //can be 0, 1, 2 to decide between large, medium or small sphere list
    public int ListSize = 1;
    public Color myColor1, myColor2;

    private int SpheresTotal;
    
    //Ford's Sphere tree depth
    public int depth;

    public struct iPair
    {
        public iPair(ComplexNum a, ComplexNum b)
        {
            this.a = a;
            this.b = b;
        }
        public ComplexNum a;
        public ComplexNum b;
    }
    public struct Pair
    {
        public float a;
        public float b;
    }
    public struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
    }
    private List<iPair> largeList = new List<iPair>()
    {
              new iPair( new ComplexNum(-1,0), new ComplexNum(0,1)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(0,1)),
          new iPair( new ComplexNum(0,0), new ComplexNum(0,1)),
          new iPair( new ComplexNum(0,1), new ComplexNum(0,1)),
          new iPair( new ComplexNum(1,0), new ComplexNum(0,1)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(0,2)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(0,2)),
          new iPair( new ComplexNum(0,1), new ComplexNum(0,2)),
          new iPair( new ComplexNum(1,0), new ComplexNum(0,2)),
          new iPair( new ComplexNum(-2,-2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-2,2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(0,1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(0,2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(1,0), new ComplexNum(0,3)),
          new iPair( new ComplexNum(1,1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(1,2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(2,-2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(2,0), new ComplexNum(0,3)),
          new iPair( new ComplexNum(2,1), new ComplexNum(0,3)),
          new iPair( new ComplexNum(2,2), new ComplexNum(0,3)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(0,4)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(0,4)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(0,4)),
          new iPair( new ComplexNum(0,1), new ComplexNum(0,4)),
          new iPair( new ComplexNum(0,3), new ComplexNum(0,4)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(0,4)),
          new iPair( new ComplexNum(1,0), new ComplexNum(0,4)),
          new iPair( new ComplexNum(1,2), new ComplexNum(0,4)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(0,4)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(0,4)),
          new iPair( new ComplexNum(2,1), new ComplexNum(0,4)),
          new iPair( new ComplexNum(2,3), new ComplexNum(0,4)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(0,4)),
          new iPair( new ComplexNum(3,0), new ComplexNum(0,4)),
          new iPair( new ComplexNum(3,2), new ComplexNum(0,4)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(1,0)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(1,0)),
          new iPair( new ComplexNum(0,0), new ComplexNum(1,0)),
          new iPair( new ComplexNum(0,1), new ComplexNum(1,0)),
          new iPair( new ComplexNum(1,0), new ComplexNum(1,0)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(1,1)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(1,1)),
          new iPair( new ComplexNum(0,1), new ComplexNum(1,1)),
          new iPair( new ComplexNum(1,0), new ComplexNum(1,1)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(1,2)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(1,2)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(1,2)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(1,2)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(1,2)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(1,2)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(1,2)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(1,2)),
          new iPair( new ComplexNum(0,1), new ComplexNum(1,2)),
          new iPair( new ComplexNum(0,2), new ComplexNum(1,2)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(1,2)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(1,2)),
          new iPair( new ComplexNum(1,0), new ComplexNum(1,2)),
          new iPair( new ComplexNum(1,1), new ComplexNum(1,2)),
          new iPair( new ComplexNum(2,0), new ComplexNum(1,2)),
          new iPair( new ComplexNum(2,1), new ComplexNum(1,2)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(1,3)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(1,3)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(1,3)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(1,3)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(1,3)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(1,3)),
          new iPair( new ComplexNum(0,1), new ComplexNum(1,3)),
          new iPair( new ComplexNum(0,3), new ComplexNum(1,3)),
          new iPair( new ComplexNum(1,0), new ComplexNum(1,3)),
          new iPair( new ComplexNum(1,2), new ComplexNum(1,3)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(1,3)),
          new iPair( new ComplexNum(3,0), new ComplexNum(1,3)),
          new iPair( new ComplexNum(-4,-1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-4,0), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-3,-1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-3,1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-2,-2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-2,2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-1,-3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-1,3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-1,4), new ComplexNum(1,4)),
          new iPair( new ComplexNum(0,-4), new ComplexNum(1,4)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(0,1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(0,2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(0,3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(0,4), new ComplexNum(1,4)),
          new iPair( new ComplexNum(1,-4), new ComplexNum(1,4)),
          new iPair( new ComplexNum(1,-3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(1,0), new ComplexNum(1,4)),
          new iPair( new ComplexNum(1,1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(1,2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(1,3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(2,-2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(2,0), new ComplexNum(1,4)),
          new iPair( new ComplexNum(2,1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(2,2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(2,3), new ComplexNum(1,4)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(3,-1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(3,0), new ComplexNum(1,4)),
          new iPair( new ComplexNum(3,1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(3,2), new ComplexNum(1,4)),
          new iPair( new ComplexNum(4,0), new ComplexNum(1,4)),
          new iPair( new ComplexNum(4,1), new ComplexNum(1,4)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(2,0)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(2,0)),
          new iPair( new ComplexNum(0,1), new ComplexNum(2,0)),
          new iPair( new ComplexNum(1,0), new ComplexNum(2,0)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(2,1)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(2,1)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(2,1)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(2,1)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(2,1)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(2,1)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(2,1)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(2,1)),
          new iPair( new ComplexNum(0,1), new ComplexNum(2,1)),
          new iPair( new ComplexNum(0,2), new ComplexNum(2,1)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(2,1)),
          new iPair( new ComplexNum(1,0), new ComplexNum(2,1)),
          new iPair( new ComplexNum(1,1), new ComplexNum(2,1)),
          new iPair( new ComplexNum(1,2), new ComplexNum(2,1)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(2,1)),
          new iPair( new ComplexNum(2,0), new ComplexNum(2,1)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(2,2)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(2,2)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(2,2)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(2,2)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(2,2)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(2,2)),
          new iPair( new ComplexNum(0,1), new ComplexNum(2,2)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(2,2)),
          new iPair( new ComplexNum(1,0), new ComplexNum(2,2)),
          new iPair( new ComplexNum(1,2), new ComplexNum(2,2)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(2,2)),
          new iPair( new ComplexNum(2,1), new ComplexNum(2,2)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-3,-1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-3,1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-2,-2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-2,2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-1,-3), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-1,3), new ComplexNum(2,3)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(2,3)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(0,1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(0,2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(0,3), new ComplexNum(2,3)),
          new iPair( new ComplexNum(1,-3), new ComplexNum(2,3)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(1,0), new ComplexNum(2,3)),
          new iPair( new ComplexNum(1,1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(1,2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(1,3), new ComplexNum(2,3)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(2,3)),
          new iPair( new ComplexNum(2,-2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(2,0), new ComplexNum(2,3)),
          new iPair( new ComplexNum(2,1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(2,2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(3,-1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(3,0), new ComplexNum(2,3)),
          new iPair( new ComplexNum(3,1), new ComplexNum(2,3)),
          new iPair( new ComplexNum(3,2), new ComplexNum(2,3)),
          new iPair( new ComplexNum(-4,-1), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-4,1), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-1,-4), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-1,4), new ComplexNum(2,4)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(2,4)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(2,4)),
          new iPair( new ComplexNum(0,1), new ComplexNum(2,4)),
          new iPair( new ComplexNum(0,3), new ComplexNum(2,4)),
          new iPair( new ComplexNum(1,-4), new ComplexNum(2,4)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(2,4)),
          new iPair( new ComplexNum(1,0), new ComplexNum(2,4)),
          new iPair( new ComplexNum(1,4), new ComplexNum(2,4)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(2,4)),
          new iPair( new ComplexNum(2,1), new ComplexNum(2,4)),
          new iPair( new ComplexNum(2,3), new ComplexNum(2,4)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(2,4)),
          new iPair( new ComplexNum(3,0), new ComplexNum(2,4)),
          new iPair( new ComplexNum(3,2), new ComplexNum(2,4)),
          new iPair( new ComplexNum(4,-1), new ComplexNum(2,4)),
          new iPair( new ComplexNum(4,1), new ComplexNum(2,4)),
          new iPair( new ComplexNum(-2,-2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-2,2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(0,1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(0,2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(1,0), new ComplexNum(3,0)),
          new iPair( new ComplexNum(1,1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(1,2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(2,-2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(2,0), new ComplexNum(3,0)),
          new iPair( new ComplexNum(2,1), new ComplexNum(3,0)),
          new iPair( new ComplexNum(2,2), new ComplexNum(3,0)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(3,1)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(3,1)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(3,1)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(3,1)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(3,1)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(3,1)),
          new iPair( new ComplexNum(0,1), new ComplexNum(3,1)),
          new iPair( new ComplexNum(0,3), new ComplexNum(3,1)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(3,1)),
          new iPair( new ComplexNum(1,0), new ComplexNum(3,1)),
          new iPair( new ComplexNum(2,1), new ComplexNum(3,1)),
          new iPair( new ComplexNum(3,0), new ComplexNum(3,1)),
          new iPair( new ComplexNum(-3,-1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-3,1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-2,-2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-2,2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-1,-3), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-1,3), new ComplexNum(3,2)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(3,2)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(0,1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(0,2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(0,3), new ComplexNum(3,2)),
          new iPair( new ComplexNum(1,-3), new ComplexNum(3,2)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(1,0), new ComplexNum(3,2)),
          new iPair( new ComplexNum(1,1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(1,2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(1,3), new ComplexNum(3,2)),
          new iPair( new ComplexNum(2,-2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(2,0), new ComplexNum(3,2)),
          new iPair( new ComplexNum(2,1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(2,2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(2,3), new ComplexNum(3,2)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(3,2)),
          new iPair( new ComplexNum(3,-1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(3,0), new ComplexNum(3,2)),
          new iPair( new ComplexNum(3,1), new ComplexNum(3,2)),
          new iPair( new ComplexNum(-4,-1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-4,1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-1,-4), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-1,4), new ComplexNum(3,3)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(0,1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(1,-4), new ComplexNum(3,3)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(3,3)),
          new iPair( new ComplexNum(1,0), new ComplexNum(3,3)),
          new iPair( new ComplexNum(1,2), new ComplexNum(3,3)),
          new iPair( new ComplexNum(1,4), new ComplexNum(3,3)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(3,3)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(2,1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(2,3), new ComplexNum(3,3)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(3,3)),
          new iPair( new ComplexNum(3,2), new ComplexNum(3,3)),
          new iPair( new ComplexNum(4,-1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(4,1), new ComplexNum(3,3)),
          new iPair( new ComplexNum(-4,-3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-4,-1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-4,0), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-4,1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-4,2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-3,-3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-3,-1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-3,3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-3,4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-2,-4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-2,-2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-2,2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-1,-4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-1,3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-1,4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(0,-4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(0,1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(0,2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(0,3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(0,4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(1,-4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(1,-3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(1,0), new ComplexNum(3,4)),
          new iPair( new ComplexNum(1,1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(1,2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(1,4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(2,-2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(2,0), new ComplexNum(3,4)),
          new iPair( new ComplexNum(2,2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(2,3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(2,4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(3,-4), new ComplexNum(3,4)),
          new iPair( new ComplexNum(3,-3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(3,0), new ComplexNum(3,4)),
          new iPair( new ComplexNum(3,1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(3,2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(3,3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(4,-2), new ComplexNum(3,4)),
          new iPair( new ComplexNum(4,-1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(4,0), new ComplexNum(3,4)),
          new iPair( new ComplexNum(4,1), new ComplexNum(3,4)),
          new iPair( new ComplexNum(4,3), new ComplexNum(3,4)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(4,0)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(4,0)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(4,0)),
          new iPair( new ComplexNum(0,1), new ComplexNum(4,0)),
          new iPair( new ComplexNum(0,3), new ComplexNum(4,0)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(4,0)),
          new iPair( new ComplexNum(1,0), new ComplexNum(4,0)),
          new iPair( new ComplexNum(1,2), new ComplexNum(4,0)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(4,0)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(4,0)),
          new iPair( new ComplexNum(2,1), new ComplexNum(4,0)),
          new iPair( new ComplexNum(2,3), new ComplexNum(4,0)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(4,0)),
          new iPair( new ComplexNum(3,0), new ComplexNum(4,0)),
          new iPair( new ComplexNum(3,2), new ComplexNum(4,0)),
          new iPair( new ComplexNum(-4,0), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-4,1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-3,-1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-3,1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-2,-2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-2,2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-1,-4), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-1,-3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-1,3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(0,-4), new ComplexNum(4,1)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(0,1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(0,2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(0,3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(0,4), new ComplexNum(4,1)),
          new iPair( new ComplexNum(1,-3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(1,0), new ComplexNum(4,1)),
          new iPair( new ComplexNum(1,1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(1,2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(1,3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(1,4), new ComplexNum(4,1)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(2,-2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(2,0), new ComplexNum(4,1)),
          new iPair( new ComplexNum(2,1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(2,2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(2,3), new ComplexNum(4,1)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(3,-1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(3,0), new ComplexNum(4,1)),
          new iPair( new ComplexNum(3,1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(3,2), new ComplexNum(4,1)),
          new iPair( new ComplexNum(4,-1), new ComplexNum(4,1)),
          new iPair( new ComplexNum(4,0), new ComplexNum(4,1)),
          new iPair( new ComplexNum(-4,-1), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-4,1), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-1,-4), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-1,4), new ComplexNum(4,2)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(4,2)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(4,2)),
          new iPair( new ComplexNum(0,1), new ComplexNum(4,2)),
          new iPair( new ComplexNum(0,3), new ComplexNum(4,2)),
          new iPair( new ComplexNum(1,-4), new ComplexNum(4,2)),
          new iPair( new ComplexNum(1,0), new ComplexNum(4,2)),
          new iPair( new ComplexNum(1,2), new ComplexNum(4,2)),
          new iPair( new ComplexNum(1,4), new ComplexNum(4,2)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(4,2)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(4,2)),
          new iPair( new ComplexNum(2,3), new ComplexNum(4,2)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(4,2)),
          new iPair( new ComplexNum(3,0), new ComplexNum(4,2)),
          new iPair( new ComplexNum(3,2), new ComplexNum(4,2)),
          new iPair( new ComplexNum(4,-1), new ComplexNum(4,2)),
          new iPair( new ComplexNum(4,1), new ComplexNum(4,2)),
          new iPair( new ComplexNum(-4,-2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-4,-1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-4,0), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-4,1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-4,3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-3,-4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-3,-3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-3,1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-3,3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-2,-2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-2,0), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-2,2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-2,4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-1,-4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-1,-3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-1,-1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-1,1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-1,4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(0,-4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(0,-2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(0,1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(0,2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(0,3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(0,4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(1,-4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(1,-1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(1,0), new ComplexNum(4,3)),
          new iPair( new ComplexNum(1,1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(1,3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(1,4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(2,-4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(2,-2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(2,0), new ComplexNum(4,3)),
          new iPair( new ComplexNum(2,1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(2,2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(2,3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(3,-3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(3,-1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(3,0), new ComplexNum(4,3)),
          new iPair( new ComplexNum(3,2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(3,3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(3,4), new ComplexNum(4,3)),
          new iPair( new ComplexNum(4,-3), new ComplexNum(4,3)),
          new iPair( new ComplexNum(4,-1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(4,0), new ComplexNum(4,3)),
          new iPair( new ComplexNum(4,1), new ComplexNum(4,3)),
          new iPair( new ComplexNum(4,2), new ComplexNum(4,3)),
          new iPair( new ComplexNum(-5,-2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-5,0), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-5,2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-4,-3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-4,-1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-4,1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-4,3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-3,-4), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-3,-2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-3,0), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-3,2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-3,4), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-2,-5), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-2,-3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-2,-1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-2,1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-2,3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-2,5), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-1,-4), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-1,-2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-1,0), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-1,2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(-1,4), new ComplexNum(4,4)),
          new iPair( new ComplexNum(0,-5), new ComplexNum(4,4)),
          new iPair( new ComplexNum(0,-3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(0,-1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(0,1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(0,3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(0,5), new ComplexNum(4,4)),
          new iPair( new ComplexNum(1,-4), new ComplexNum(4,4)),
          new iPair( new ComplexNum(1,-2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(1,0), new ComplexNum(4,4)),
          new iPair( new ComplexNum(1,2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(1,4), new ComplexNum(4,4)),
          new iPair( new ComplexNum(2,-5), new ComplexNum(4,4)),
          new iPair( new ComplexNum(2,-3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(2,-1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(2,1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(2,3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(2,5), new ComplexNum(4,4)),
          new iPair( new ComplexNum(3,-4), new ComplexNum(4,4)),
          new iPair( new ComplexNum(3,-2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(3,0), new ComplexNum(4,4)),
          new iPair( new ComplexNum(3,2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(3,4), new ComplexNum(4,4)),
          new iPair( new ComplexNum(4,-3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(4,-1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(4,1), new ComplexNum(4,4)),
          new iPair( new ComplexNum(4,3), new ComplexNum(4,4)),
          new iPair( new ComplexNum(5,-2), new ComplexNum(4,4)),
          new iPair( new ComplexNum(5,0), new ComplexNum(4,4)),
          new iPair( new ComplexNum(5,2), new ComplexNum(4,4))
    

    };
    private List<iPair> mediumList = new List<iPair>() { 
        new iPair( new ComplexNum(-1, 0), new ComplexNum(0, 1)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(0, 1)),
    new iPair( new ComplexNum(0, 0), new ComplexNum(0, 1)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(0, 1)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(0, 1)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(0, 2)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(0, 2)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(0, 2)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(0, 2)),
    new iPair( new ComplexNum(-2, -2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-2, -1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-2, 0), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-2, 1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-2, 2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-1, -2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-1, -1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-1, 1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-1, 2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(0, -2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(0, 2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(1, -2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(1, -1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(1, 1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(1, 2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(2, -2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(2, -1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(2, 0), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(2, 1), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(2, 2), new ComplexNum(0, 3)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(1, 0)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(1, 0)),
    new iPair( new ComplexNum(0, 0), new ComplexNum(1, 0)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(1, 0)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(1, 0)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(1, 1)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(1, 1)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(1, 1)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(1, 1)),
    new iPair( new ComplexNum(-2, -1), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(-2, 0), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(-1, -1), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(-1, 1), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(-1, 2), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(0, -2), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(0, 2), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(1, -2), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(1, -1), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(1, 1), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(2, 0), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(2, 1), new ComplexNum(1, 2)),
    new iPair( new ComplexNum(-3, 0), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(-2, 1), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(-1, -2), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(0, -3), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(0, 3), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(1, 2), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(2, -1), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(3, 0), new ComplexNum(1, 3)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(2, 0)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(2, 0)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(2, 0)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(2, 0)),
    new iPair( new ComplexNum(-2, 0), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(-2, 1), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(-1, -2), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(-1, -1), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(-1, 1), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(0, -2), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(0, 2), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(1, -1), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(1, 1), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(1, 2), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(2, -1), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(2, 0), new ComplexNum(2, 1)),
    new iPair( new ComplexNum(-2, -1), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(-2, 1), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(-1, -2), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(-1, 2), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(1, -2), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(1, 2), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(2, -1), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(2, 1), new ComplexNum(2, 2)),
    new iPair( new ComplexNum(-3, -2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-3, -1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-3, 0), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-3, 1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-2, -2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-2, -1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-2, 0), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-2, 1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-2, 2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-2, 3), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-1, -3), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-1, -2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-1, -1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-1, 1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-1, 2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-1,  3), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(0, -3), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(0, -2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(0, 2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(0, 3), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(1, -3), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(1, -2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(1, -1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(1, 1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(1, 2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(1, 3), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(2, -3), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(2, -2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(2, -1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(2, 0), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(2, 1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(2, 2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(3, -1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(3, 0), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(3, 1), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(3, 2), new ComplexNum(2, 3)),
    new iPair( new ComplexNum(-2, -2), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-2, -1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-2, 0), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-2, 1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-2, 2), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-1, -2), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-1, -1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-1, 1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-1, 2), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(0, -2), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(0, 2), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(1, -2), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(1, -1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(1, 1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(1, 2), new ComplexNum(3,   0)),
    new iPair( new ComplexNum(2, -2), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(2, -1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(2, 0), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(2, 1), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(2, 2), new ComplexNum(3, 0)),
    new iPair( new ComplexNum(-3, 0), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(-2, -1), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(-1, 2), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(0, -3), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(0, 3), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(1, -2), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(2, 1), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(3, 0), new ComplexNum(3, 1)),
    new iPair( new ComplexNum(-3, -1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-3, 0), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-3, 1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-3, 2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-2, -3), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-2, -2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-2, -1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-2, 0), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-2, 1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-2, 2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-1, -3), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-1, -2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-1, -1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-1, 1), new ComplexNum(3,  2)),
    new iPair( new ComplexNum(-1, 2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-1, 3), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(0, -3), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(0, -2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(0, 2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(0, 3), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(1, -3), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(1, -2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(1, -1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(1, 1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(1, 2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(1, 3), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(2, -2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(2, -1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(2, 0), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(2, 1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(2, 2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(2, 3), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(3, -2), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(3, -1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(3, 0), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(3, 1), new ComplexNum(3, 2)),
    new iPair( new ComplexNum(-4, -1), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-4, 1), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-3, -2), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-3, 2), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-2, -3), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-2, -1), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-2, 1), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-2, 3), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-1, -4), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-1, -2), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-1, 0), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-1, 2), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(-1, 4), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(0, -1), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(0, 1), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(1, -4), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(1, -2), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(1, 0), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(1, 2), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(1, 4), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(2, -3), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(2, -1), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(2, 1), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(2, 3), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(3, -2), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(3, 2), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(4, -1), new ComplexNum(3, 3)),
    new iPair( new ComplexNum(4, 1), new ComplexNum(3, 3))
};
    private List<iPair> smallList = new List<iPair>()
    {
       new iPair(new ComplexNum(-1,0), new ComplexNum(0,1)),
       new iPair(new ComplexNum(0,-1), new ComplexNum(0,1)),
       new iPair(new ComplexNum(0,0), new ComplexNum(0,1)),
       new iPair(new ComplexNum(0,1), new ComplexNum(0,1)),
       new iPair(new ComplexNum(1,0), new ComplexNum(0,1)),
       new iPair(new ComplexNum(-1,0), new ComplexNum(0,2)),
       new iPair(new ComplexNum(0,-1), new ComplexNum(0,2)),
       new iPair(new ComplexNum(0,1), new ComplexNum(0,2)),
       new iPair(new ComplexNum(1,0), new ComplexNum(0,2)),
       new iPair(new ComplexNum(-1,0), new ComplexNum(1,0)),
       new iPair(new ComplexNum(0,-1), new ComplexNum(1,0)),
       new iPair(new ComplexNum(0,0), new ComplexNum(1,0)),
       new iPair(new ComplexNum(0,1), new ComplexNum(1,0)),
       new iPair(new ComplexNum(1,0), new ComplexNum(1,0)),
       new iPair(new ComplexNum(-1,0), new ComplexNum(1,1)),
       new iPair(new ComplexNum(0,-1), new ComplexNum(1,1)),
       new iPair(new ComplexNum(0,1), new ComplexNum(1,1)),
       new iPair(new ComplexNum(1,0), new ComplexNum(1,1)),
       new iPair(new ComplexNum(-2,-1), new ComplexNum(1,2)),
       new iPair(new ComplexNum(-2,0), new ComplexNum(1,2)),
       new iPair(new ComplexNum(-1,-1), new ComplexNum(1,2)),
       new iPair(new ComplexNum(-1,0), new ComplexNum(1,2)),
       new iPair(new ComplexNum(-1,1), new ComplexNum(1,2)),
       new iPair(new ComplexNum(-1,2), new ComplexNum(1,2)),
       new iPair(new ComplexNum(0,-2), new ComplexNum(1,2)),
       new iPair(new ComplexNum(0,-1), new ComplexNum(1,2)),
       new iPair(new ComplexNum(0,1), new ComplexNum(1,2)),
       new iPair(new ComplexNum(0,2), new ComplexNum(1,2)),
       new iPair(new ComplexNum(1,-2), new ComplexNum(1,2)),
       new iPair(new ComplexNum(1,-1), new ComplexNum(1,2)),
       new iPair(new ComplexNum(1,0), new ComplexNum(1,2)),
       new iPair(new ComplexNum(1,1), new ComplexNum(1,2)),
       new iPair(new ComplexNum(2,0), new ComplexNum(1,2)),
       new iPair(new ComplexNum(2,1), new ComplexNum(1,2)),
       new iPair(new ComplexNum(-1,0), new ComplexNum(2,0)),
       new iPair(new ComplexNum(0,-1), new ComplexNum(2,0)),
       new iPair(new ComplexNum(0,1), new ComplexNum(2,0)),
       new iPair(new ComplexNum(1,0), new ComplexNum(2,0)),
       new iPair(new ComplexNum(-2,0), new ComplexNum(2,1)),
       new iPair(new ComplexNum(-2,1), new ComplexNum(2,1)),
       new iPair(new ComplexNum(-1,-2), new ComplexNum(2,1)),
       new iPair(new ComplexNum(-1,-1), new ComplexNum(2,1)),
       new iPair(new ComplexNum(-1,0), new ComplexNum(2,1)),
       new iPair(new ComplexNum(-1,1), new ComplexNum(2,1)),
       new iPair(new ComplexNum(0,-2), new ComplexNum(2,1)),
       new iPair(new ComplexNum(0,-1), new ComplexNum(2,1)),
       new iPair(new ComplexNum(0,1), new ComplexNum(2,1)),
       new iPair(new ComplexNum(0,2), new ComplexNum(2,1)),
       new iPair(new ComplexNum(1,-1), new ComplexNum(2,1)),
       new iPair(new ComplexNum(1,0), new ComplexNum(2,1)),
       new iPair(new ComplexNum(1,1), new ComplexNum(2,1)),
       new iPair(new ComplexNum(1,2), new ComplexNum(2,1)),
       new iPair(new ComplexNum(2,-1), new ComplexNum(2,1)),
       new iPair(new ComplexNum(2,0), new ComplexNum(2,1)),
       new iPair(new ComplexNum(-2,-1), new ComplexNum(2,2)),
       new iPair(new ComplexNum(-2,1), new ComplexNum(2,2)),
       new iPair(new ComplexNum(-1,-2), new ComplexNum(2,2)),
       new iPair(new ComplexNum(-1,0), new ComplexNum(2,2)),
       new iPair(new ComplexNum(-1,2), new ComplexNum(2,2)),
       new iPair(new ComplexNum(0,-1), new ComplexNum(2,2)),
       new iPair(new ComplexNum(0,1), new ComplexNum(2,2)),
       new iPair(new ComplexNum(1,-2), new ComplexNum(2,2)),
       new iPair(new ComplexNum(1,0), new ComplexNum(2,2)),
       new iPair(new ComplexNum(1,2), new ComplexNum(2,2)),
       new iPair(new ComplexNum(2,-1), new ComplexNum(2,2)),
       new iPair(new ComplexNum(2,1), new ComplexNum(2,2))
    };
    private List<List<iPair>> control;

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
        List<Sphere> fordCircles = new List<Sphere>();
        List<Sphere> randomSpheres = new List<Sphere>();
        List<Sphere> fordSpheres = new List<Sphere>();
        control = new List<List<iPair>>() { largeList, mediumList, smallList };

        //drawRandomSpheres(randomSpheres);
        //setupFordCircles(fordSpheres, fractionMap);

        setupFordSpheres(fordSpheres);

        //Assign Ford Spheres to compute buffer
        // _fordSphereBuffer = new ComputeBuffer(fordCircles.Count, 40);
        // _fordSphereBuffer.SetData(fordCircles);
        //SpheresTotal = fordCircles.Count;

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
   
    private void setupFordSpheres(List<Sphere> spheres )
    {

        /*Sphere test = new Sphere();
        test.radius = 1;
        test.position = new Vector3(1f, 2f, 1f);
        test.albedo = new Vector3(1f, 1f, 1f);
        test.specular = new Vector3(0.5f, 0.5f, 0.5f);
        spheres.Add(test);*/
        /*ComplexNum unit_1 = new ComplexNum();
        unit_1.r = 1;
        unit_1.i = 0;
        ComplexNum unit_2 = new ComplexNum();
        unit_2.r = -1;
        unit_2.i = 0; 
        ComplexNum unit_3 = new ComplexNum();
        unit_3.r = 0;
        unit_3.i = 1; 
        ComplexNum unit_4 = new ComplexNum();
        unit_4.r = 0;
        unit_4.i = -1;
        ComplexNum b = new ComplexNum();
        b.r = 1;
        b.i = 1;
        //|alpha|^2 <= |beta|^2
        ComplexNum a = new ComplexNum();
        a.r = -1;
        a.i = -1;

        for (int i = 0; i < depth; i++) 
        {
            for (int br = 0; br <2/*TODO:ADD REAL BETA ITERATIONS*//*; br++) 
            {
                for (int bi = 0; bi < 2; bi++) 
                {
                    b.r = (float)br;
                    b.i = (float)bi;
                    if (b.r == 0 && b.i == 0) 
                    {
                        continue;
                    }
                    //alpha's real part
                    for (int ar = (int)-Math.Floor(b.mag()); ar < (int)Math.Ceiling(b.mag()); ar++)
                    {
                        // alpha's imaginary part
                        for (int ai = (int)-Math.Floor(b.mag()); ai < (int)Math.Ceiling(b.mag()); ai++)
                        {
                            UnityEngine.Debug.Log(ai + " | " + ar);
                            if (a.r == 0 && a.i == 0)
                            {
                                continue;
                            }
                            a.r = (float)ar;
                            a.i = (float)ai;
                            //UnityEngine.Debug.Log(a+" |"+b+" |"+a.mag() / b.mag());
                            if (a.mag() / b.mag() <= 1 && (a.mag() / b.mag()) != 0)
                            {
                                drawFordSpheres(a, b, 1, spheres);
                            }
                        }

                    }
                }
            }
        }*/
        List<iPair> temp = control[ListSize];
        foreach (iPair coordiante in temp) 
        {
            //UnityEngine.Debug.Log("ComplexNum: \tAlpha " + coordiante.a + " | Beta "+coordiante.b+"\n\t\tCoordinate "+coordiante.a/coordiante.b+" | Mag "+ (coordiante.a/coordiante.b).mag());
            drawFordSpheres(coordiante.a, coordiante.b, spheres);
        }
       

    }
    
    private void drawFordSpheres(ComplexNum a, ComplexNum b, List<Sphere> spheres ) 
    {
        /*Sphere sphere = new Sphere();
        //coordinates might not actually be a complex number, not sure
        ComplexNum coordinates = a / b;
        float[] check = { (float)Math.Pow(a.r, 2) + (float)Math.Pow(a.i, 2), (float)Math.Pow(b.r, 2) + (float)Math.Pow(b.i, 2), a.r * b.r + a.i * b.i, b.i * a.r - a.i * b.r };
       // UnityEngine.Debug.Log(GCD(check) == 1);
        if (GCD(check) == 1)
        {
            sphere.radius = (float)1 / ((float)2 * b.mag());
            sphere.position = new Vector3(coordinates.r, sphere.radius, coordinates.i);


            sphere.albedo = Vector3.zero;
            sphere.specular = new Vector3(1, .5f, 1);

            spheres.Add(sphere);
        }
        else {
            UnityEngine.Debug.Log("Could not draw sphere " + coordinates);
        }*/
        Sphere sphere = new Sphere();
        ComplexNum coordinates = a / b; //Check / Funciton
        sphere.radius = 1.0f / (2.0f * b.mag()*b.mag());// Check the mag function
        sphere.position = new Vector3(coordinates.r, sphere.radius, coordinates.i);


        Color color = Color.Lerp(myColor1, myColor2, sphere.radius);
        sphere.albedo = new Vector3(color.r, color.g, color.b);
        sphere.specular = new Vector3(0f,0f,0f);

        spheres.Add(sphere);


    }
  
    private void setupFordCircles(List<Sphere> spheres, List<Pair> fractionMap) 
    {
        Sphere s1, s2 = new Sphere();
        s1.position = new Vector3(0, 0.5f, 0);
        s2.position = new Vector3(1, 0.5f, 0);
        s1.radius = 0.5f;
        s2.radius = 0.5f;

        Color colorBase_1 = new Color(s1.position.x, s1.position.x, s1.position.x);
        Color colorBase_2 = new Color(s2.position.x, s2.position.x, s2.position.x);

        Color colorBase2_1 = new Color(s1.position.x - 1, s1.position.x - 1, s1.position.x - 1);
        Color colorBase2_2 = new Color(s2.position.x - 1, s2.position.x - 1, s2.position.x - 1);


        Color color_1 = Color.Lerp(myColor1, myColor2, s1.position.x);

        Color color_2 = Color.Lerp(myColor1, myColor2, s2.position.x);

      

        bool metal = UnityEngine.Random.value < 0.5f;
        s1.albedo = Vector3.zero;
        s1.specular = new Vector3(color_1.r, color_1.g, color_1.b);
        s2.albedo = Vector3.zero;
        s2.specular = new Vector3(color_2.r, color_2.g, color_2.b);


        spheres.Add(s1);
        spheres.Add(s2);

        drawFordCircles(1, 2, depth, spheres, fractionMap);
    }
    
    private void drawFordCircles(float a, float b, int step , List<Sphere> spheres, List<Pair> fractionMap) 
    {
        if (step == 0) 
        {
            return;
        }
        //create hashmap with fractions in it (or maybe key-values of a,b)
        Pair p = new Pair();
        p.a = a;
        p.b = b;

        bool isUnique = true;
        foreach (Pair pair in fractionMap) 
        {
            if ((float)pair.a / (float)pair.b == (float)p.a / (float)p.b)
                isUnique = false;

        }
        if (isUnique) 
        {
            //Set values
            Sphere sphere = new Sphere();
            float gcd = GCD(a, b);
            p.a = a / gcd;
            p.b = b / gcd;
            sphere.position = new Vector3 ((float)p.a / (float)p.b, 1.0f / (2.0f * (float)(Math.Pow(p.b, 2))), 0);
            sphere.radius = 1.0f / (2.0f * (float)(Math.Pow(p.b, 2)));
            //Add Gradient Color
            Color colorBase1 = new Color(sphere.position.x, sphere.position.x, sphere.position.x);
            Color colorBase2 = new Color(sphere.position.x - 1, sphere.position.x - 1, sphere.position.x - 1);

            // Color color = (colorBase1 * myColor1)*(colorBase2*myColor2);
            Color color = Color.Lerp(myColor1, myColor2, sphere.position.x);
            sphere.albedo = new Vector3(color.r, color.g, color.b);
            sphere.specular = Vector3.one * 0.04f;
            //Add Sphere to Lst
            spheres.Add(sphere);
        }
        step--;
        fractionMap.Add(p);
       // UnityEngine.Debug.Log("x: " + p.a + "/" + p.b + "\n");
        drawFordCircles(p.a, p.b + 1, step, spheres, fractionMap);
        drawFordCircles(p.a + 1, p.b + 1, step, spheres, fractionMap);
    
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
    
    private static float GCD(float a, float b)
    {
        float remainder;
        while (b != 0)
        {
            remainder = a % b;
            a = b;
            b = remainder;
        }
        return a;
    }

    //if the numbers are a.mag()^2, b.mag()^2, a1*b1+a2b2, a1b1-b1a2)
    //
    private static float GCD(float[] arr) {
       
        float result = arr[0];
        for (int i = 1; i < arr.Length; i++)
        {
            result = GCD(arr[i], result);

            if (result == 1)
            {
                return 1;
            }
        }
        return result;
        /*bool isCoPrime = false;

       for (int i = 0; i < numbers.Length; i++) {
           for (int j = 0; j < numbers.Length; j++) {
               if (numbers[j] == 1) 
               {
                   isCoPrime = true;
                   break;
               }
               if (1 == GCD(numbers[i], numbers[j]))
               {
                   isCoPrime = true;
                   break;
               }
           }
       }

       return isCoPrime;*/
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
        //temp step imma get rid of it
        float step=0;
        List<Sphere> spheres = new List<Sphere>();

        Sphere[] tempArray = new Sphere[SpheresTotal];
        _randomSphereBuffer.GetData(tempArray, 0, 0, SpheresTotal);
        for (int i = 0; i < tempArray.Length; i++)
        {
            spheres.Add(tempArray[i]);

        }


        List<Sphere> temp = new List<Sphere>();

        foreach (Sphere sphere in spheres)
        {
            Sphere tempSphere = sphere;
            float up = (float)(Math.Sin(step)) / sphere.radius;
            step = (step + (float)Math.PI / 2048) % ((float)(Math.PI) * 2);
            tempSphere.position.y = sphere.position.y + up;
            temp.Add(tempSphere);
        }
        _randomSphereBuffer.SetData(temp);
        Sphere[] check = new Sphere[1];
        _randomSphereBuffer.GetData(check, 0, 0, 1);
        _currentSample = 0;

        RayTracingShader.SetBuffer(0, "_Spheres", _randomSphereBuffer);

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
        //TODO: figure out if I can set then draw from two different buffers
        RayTracingShader.SetBuffer(0, "_Spheres", _fordSphereBuffer);
        //Something like
        // RayTracingShader.SetBuffer(0, "_Spheres", _randomSpheresBuffer);
    }


}

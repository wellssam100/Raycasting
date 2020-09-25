using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FordSpheres : MonoBehaviour
{

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
    private List<iPair> complexNums = new List<iPair>()
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
    // Start is called before the first frame update
    void Start()
    {
        foreach (iPair coordiante in complexNums)
        {
            UnityEngine.Debug.Log("ComplexNum: \tAlpha " + coordiante.a + " | Beta " + coordiante.b + "\n\t\tCoordinate " + coordiante.a / coordiante.b + " | Mag " + (coordiante.a / coordiante.b).mag());
            drawFordSpheres(coordiante.a, coordiante.b);
        }
    }
    private void drawFordSpheres(ComplexNum a, ComplexNum b)
    {
        ComplexNum coordinates = a / b; //Check / Funciton
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        float radius = 1.0f / (2.0f * b.mag() * b.mag());
        sphere.transform.position = new Vector3(coordinates.r, radius, coordinates.i);
       // sphere.transform.lossyScale = new Vector3();
        //add collider
       // sphere.AddComponent()
       //      SphereCollider sc = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider
       // sphere.collider.radius = 1.0f / (2.0f * b.mag() * b.mag());// Check the mag function
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ComplexNum 
{
    public float r;
    public float i;
    public ComplexNum() {
        r = 0;
        i = 0;
    }
    public ComplexNum(float real, float imag) {
        this.r = real;
        this.i= imag;
    }
    public float mag()
    {
        return (float)Math.Abs(Math.Pow(this.r, 2) + 0.Math.Pow(this.i, 2));
    }
   
    private ComplexNum GCDComplex(ComplexNum a, ComplexNum b)
    {
        ComplexNum c = new ComplexNum();
        c.r = a.r + b.r;
        c.i = a.i + b.i;
        float [] check = { a.mag(), b.mag(), c.mag() };
        return null;
        //float gcd = GCD(check);
        //TODO we have the norm GCD, we need the actual complex number
        //I think I need to change this to the a/b -> b=r until r =0

    }

    public static ComplexNum operator /(ComplexNum a, ComplexNum b) {
        float real = ((a.r * b.r) - (a.i * b.i)) / ((float)Math.Pow(b.r, 2) + (float)Math.Pow(b.i, 2));
        float imag = ((a.i * b.r) - (a.r * b.i)) / ((float)Math.Pow(b.r, 2) + (float)Math.Pow(b.i, 2));
        return new ComplexNum(real, imag);
    }
    public static ComplexNum operator +(ComplexNum a, ComplexNum b) {
        float real = a.r + b.r;
        float imag = a.i + b.i;
        return new ComplexNum(real, imag);
    }
    public static ComplexNum operator -(ComplexNum a, ComplexNum b) {
        float real = a.r - b.r;
        float imag = a.i - b.i;
        return new ComplexNum(real, imag);
    }
    public static ComplexNum operator *(ComplexNum a, ComplexNum b) {

        return new ComplexNum((a.r * b.r) - (a.i * b.i), ((a.r * b.i) + (a.i * b.r)));
    }
    public static ComplexNum operator -(ComplexNum a) {
        return (a - a) - a;
    }
    public override string ToString() {

        string s = "CN:";
        UnityEngine.Debug.Log(this.r);
        if (this.r > 0)
        {
            s = s + " " + this.r;
            if (this.i > 0) { s = s + "+ " + this.i + "i"; }
            else if (this.i == 0) { s = s + "+ " + this.i + "i"; }
            else if (this.i < 0) { s = s + "- " + (-this.i) + "i"; }
        }
        else if (this.r == 0)
        {
            s = s + " 0";

            if (this.i > 0) { s = s + "+ " + this.i + "i"; }
            else if (this.i == 0) { s = s + "+ " + this.i + "i"; }
            else if (this.i < 0) { s = s + "- " + (-this.i) + "i"; }
        }
        else if (this.r < 0) 
        {
            s = s + " " + this.r;
            if (this.i > 0) { s = s + "+ " + this.i + "i"; }
            else if (this.i == 0) { s = s + "+ " + this.i + "i"; }
            else if (this.i < 0) { s = s + "- " + (-this.i) + "i"; }
        }        
        return s;
    }
}

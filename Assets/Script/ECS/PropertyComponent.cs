using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;
using Unity.Collections;

public struct PropertyComponent : IComponentData 
{
    public int ID;
    public NativeString64 Suburb;
    public NativeString64 PropertyType;
    public int Price;
    public NativeString64 Result;
    public NativeString64 Seller;
    //public DateTime Date;
    public int Bedroom;
    public int Bathroom;
    public int Car;
    public int Landsize;
    public int YearBuilt;
    public NativeString64 CouncilArea;
    public float Latitude;
    public float Longtitude;
    public NativeString64 RegionName;

    public float x;
    public float y;
    public float z;

    //public float speed;
}

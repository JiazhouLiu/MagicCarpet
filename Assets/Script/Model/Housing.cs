using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Housing : MonoBehaviour
{
    public int ShowID;
    public string ShowSuburb;
    public string ShowType;
    public int ShowPrice;
    public string ShowResult;
    public string ShowSeller;
    public string ShowDate;
    public int ShowBedroom;
    public int ShowBathroom;
    public int ShowCar;
    public int ShowLandsize;
    public int ShowYearBuilt;
    public string ShowCouncilArea;
    public float ShowLatitude;
    public float ShowLongtitude;
    public string ShowRegionName;
    public Vector3 ShowGroundPosition;
    public Vector3 ShowAirPosition;
    public bool Show = true;
    public bool ShowAir = false;
    public bool ShowGround = false;

    public int ID { get; set; }
    public string Suburb { get; set; }
    public string Type { get; set; }
    public int Price { get; set; }
    public string Result { get; set; }
    public string Seller { get; set; }
    public DateTime Date { get; set; }
    public int Bedroom { get; set; }
    public int Bathroom { get; set; }
    public int Car { get; set; }
    public int Landsize { get; set; }
    public int YearBuilt { get; set; }
    public string CouncilArea { get; set; }
    public float Latitude { get; set; }
    public float Longtitude { get; set; }
    public string RegionName { get; set; }

    public Color MarkColor { get; set; }
    public float InAirXPosition { get; set; }
    public float InAirYPosition { get; set; }
    public float InAirZPosition { get; set; }

    public float GroundXPosition { get; set; }
    public float GroundYPosition { get; set; }
    public float GroundZPosition { get; set; }

    public bool Air { get; set; }
    public bool Ground { get; set; }

    public Housing() { }

    private void Update()
    {

        ShowID = ID;
        ShowSuburb = Suburb;
        ShowType = Type;
        ShowPrice = Price;
        ShowResult = Result;
        ShowSeller = Seller;
        ShowDate = Date.ToString();
        ShowBedroom = Bedroom;
        ShowBathroom = Bathroom;
        ShowCar = Car;
        ShowLandsize = Landsize;
        ShowYearBuilt = YearBuilt;
        ShowCouncilArea = CouncilArea;
        ShowLatitude = Latitude;
        ShowLongtitude = Longtitude;
        ShowRegionName = RegionName;
        ShowGroundPosition = new Vector3(GroundXPosition, GroundYPosition, GroundZPosition);
        ShowAirPosition = new Vector3(InAirXPosition, InAirYPosition, InAirZPosition);

        ShowAir = Air;
        ShowGround = Ground;
    }

    public Housing(int id, string suburb, string type, int price, string result, string seller, DateTime date,
        int bed, int bath, int car, int landsize, int yearbuilt, string councilArea, float lat, float longt, string region)
    {
        ID = id;
        Suburb = suburb;
        Type = type;
        Price = price;
        Result = result;
        Seller = seller;
        Date = date;
        Bedroom = bed;
        Bathroom = bath;
        Car = car;
        Landsize = landsize;
        YearBuilt = yearbuilt;
        CouncilArea = councilArea;
        Latitude = lat;
        Longtitude = longt;
        RegionName = region;

        MarkColor = Color.white;
        InAirXPosition = 0;
        InAirYPosition = 0;
        InAirZPosition = 0;
        GroundXPosition = 0;
        GroundYPosition = 0;
        GroundZPosition = 0;

        Air = false;
        Ground = false;
    }

    public void CopyEntity(Housing h)
    {
        ID = h.ID;
        Suburb = h.Suburb;
        Type = h.Type;
        Price = h.Price;
        Result = h.Result;
        Seller = h.Seller;
        Date = h.Date;
        Bedroom = h.Bedroom;
        Bathroom = h.Bathroom;
        Car = h.Car;
        Landsize = h.Landsize;
        YearBuilt = h.YearBuilt;
        CouncilArea = h.CouncilArea;
        Latitude = h.Latitude;
        Longtitude = h.Longtitude;
        RegionName = h.RegionName;
        MarkColor = h.MarkColor;
        InAirXPosition = h.InAirXPosition;
        InAirYPosition = h.InAirYPosition;
        InAirZPosition = h.InAirZPosition;
        GroundXPosition = h.GroundXPosition;
        GroundYPosition = h.GroundYPosition;
        GroundZPosition = h.GroundZPosition;
        Air = h.Air;
        Ground = h.Ground;
    }
}

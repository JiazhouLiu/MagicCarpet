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
    public bool Show = true;

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
    public float XPosition { get; set; }
    public float YPosition { get; set; }
    public float ZPosition { get; set; }

    public Housing() { }

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
        XPosition = 0;
        YPosition = 0;
        ZPosition = 0;
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
        XPosition = h.XPosition;
        YPosition = h.YPosition;
        ZPosition = h.ZPosition;

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
    }
}

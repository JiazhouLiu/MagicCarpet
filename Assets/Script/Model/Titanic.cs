using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titanic : MonoBehaviour
{
    public int ShowID;
    public string ShowName;
    public string ShowClass;
    public string ShowJoined;
    public string ShowJob;
    public float ShowTicketCost;
    public float ShowAge;
    public string ShowGender;
    public string ShowSurvived;
    public string ShowDepartment;

    public int ID { get; set; }
    public string Name { get; set; }
    public string Class { get; set; }
    public string Joined { get; set; }
    public string Job { get; set; }
    public float TicketCost { get; set; }
    public float Age { get; set; }
    public string Gender { get; set; }
    public string Survived { get; set; }
    public string Department { get; set; }

    public Color MarkColor { get; set; }
    public float XPosition { get; set; }
    public float YPosition { get; set; }

    public Titanic() { }

    public Titanic(int id, string name, string Tclass, string joined, string job, float ticketCost,
        float age, string gender, string survived, string department){
        ID = id;
        Name = name;
        Class = Tclass;
        Joined = joined;
        Job = job;
        TicketCost = ticketCost;
        Age = age;
        Gender = gender;
        Survived = survived;
        Department = department;
        MarkColor = Color.white;
        XPosition = 0;
        YPosition = 0;
    }

    public void CopyEntity(Titanic t) {
        ID = t.ID;
        Name = t.Name;
        Class = t.Class;
        Joined = t.Joined;
        Job = t.Job;
        TicketCost = t.TicketCost;
        Age = t.Age;
        Gender = t.Gender;
        Survived = t.Survived;
        Department = t.Department;
        MarkColor = t.MarkColor;
        XPosition = t.XPosition;
        YPosition = t.YPosition;


        ShowID = ID;
        ShowName = Name;
        ShowClass = Class;
        ShowJoined = Joined;
        ShowJob = Job;
        ShowTicketCost = TicketCost;
        ShowAge = Age;
        ShowGender = Gender;
        ShowSurvived = Survived;
        ShowDepartment = Department;
    }
}

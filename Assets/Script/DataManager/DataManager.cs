using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public TextAsset DataSource;
    public GameObject markPrefab;
    public Transform visParent;
    public float markSize = 0.1f;
    public float speed = 1;
    public ObjectGeneratorNoColumn og;
    public MagicCarpetManager mcm;

    private List<GameObject> MarkCollection;
    private List<GameObject> CurrentSM;

    private bool canMove = false;

    private readonly char lineSeperater = '\n'; // It defines line seperate character
    private readonly char fieldSeperator = ','; // It defines field seperate chracter

    // Start is called before the first frame update
    void Start()
    {
        MarkCollection = new List<GameObject>();
        CurrentSM = new List<GameObject>();

        ReadData(DataSource);

        GetAxesValuesFacetByClass();
    }

    private void ReadData(TextAsset ta) {

        string[] lines = ta.text.Split(lineSeperater);

        for (int i = 1; i < lines.Length; i++) {

            if (lines[i].Length > 10) {
                GameObject mark = Instantiate(markPrefab, new Vector3(0, 0, 0),
            Quaternion.identity, visParent);
                mark.transform.localScale = Vector3.one * markSize;

                Titanic person = new Titanic(i, (lines[i].Split(fieldSeperator)[0] + ", " + lines[i].Split(fieldSeperator)[1]),
                lines[i].Split(fieldSeperator)[2], lines[i].Split(fieldSeperator)[3],
                lines[i].Split(fieldSeperator)[4], float.Parse(lines[i].Split(fieldSeperator)[5]),
                float.Parse(lines[i].Split(fieldSeperator)[6]), lines[i].Split(fieldSeperator)[7],
                lines[i].Split(fieldSeperator)[8], lines[i].Split(fieldSeperator)[9]);

                mark.GetComponent<Titanic>().CopyEntity(person);
                MarkCollection.Add(mark);
            }
            
        }
    }

    private void Update()
    {
        if (canMove) {
            bool allMoved = true;
            foreach (GameObject mark in MarkCollection)
            {
                Titanic t = mark.GetComponent<Titanic>();
                mark.transform.localPosition = Vector3.Lerp(mark.transform.localPosition,
                        new Vector3(t.XPosition, t.YPosition, 0), Time.deltaTime * speed);

                if (Vector3.Distance(mark.transform.localPosition,
                        new Vector3(t.XPosition, t.YPosition, 0)) > 0.01f)
                {
                    allMoved = false;
                }
            }

            if (allMoved) {
                canMove = false;
            }
        }

        if (Input.GetKeyDown("x"))
        {
            GetAxesValuesFacetByAge();
        }
        if (Input.GetKeyDown("c"))
        {
            GetAxesValuesFacetByClass();
        }
    }

    private void GetAxesValuesFacetByClass() {

        CurrentSM = og.UpdateSM(CurrentSM, 4, 1);
        mcm.UpdateCurrentSM(CurrentSM);

        float minAge = 100;
        float maxAge = 0;

        float minTicketCost = 10000;
        float maxTicketCost = 0;
        foreach (GameObject mark in MarkCollection) {
            Titanic t = mark.GetComponent<Titanic>();
            if (t.Age > maxAge)
                maxAge = t.Age;
            if (t.Age < minAge)
                minAge = t.Age;
            if (t.TicketCost > maxTicketCost)
                maxTicketCost = t.TicketCost;
            if (t.TicketCost < minTicketCost)
                minTicketCost = t.TicketCost;
            if (t.Survived == "TRUE")
                t.MarkColor = Color.blue;
            else
                t.MarkColor = Color.green;

            mark.GetComponent<SpriteRenderer>().color = t.MarkColor;
        }

        foreach (GameObject mark in MarkCollection)
        {
            Titanic t = mark.GetComponent<Titanic>();
            t.XPosition = (t.TicketCost - minTicketCost) / (maxTicketCost - minTicketCost);
            t.YPosition = (t.Age - minAge) / (maxAge - minAge);

            if (CurrentSM.Count == 4)
            {
                switch (t.Class)
                {
                    case "1st Class Passenger":
                        mark.transform.SetParent(CurrentSM[0].transform);
                        break;
                    case "2nd Class Passenger":
                        mark.transform.SetParent(CurrentSM[1].transform);
                        break;
                    case "3rd Class Passenger":
                        mark.transform.SetParent(CurrentSM[2].transform);
                        break;
                    case "Crew":
                        mark.transform.SetParent(CurrentSM[3].transform);
                        break;
                    default:
                        break;
                }
            }

            canMove = true;
        }
    }

    private void GetAxesValuesFacetByAge()
    {
        CurrentSM = og.UpdateSM(CurrentSM, 4, 1);
        mcm.UpdateCurrentSM(CurrentSM);

        float minAge = 100;
        float maxAge = 0;

        float minTicketCost = 10000;
        float maxTicketCost = 0;
        foreach (GameObject mark in MarkCollection)
        {
            Titanic t = mark.GetComponent<Titanic>();
            if (t.Age > maxAge)
                maxAge = t.Age;
            if (t.Age < minAge)
                minAge = t.Age;
            if (t.TicketCost > maxTicketCost)
                maxTicketCost = t.TicketCost;
            if (t.TicketCost < minTicketCost)
                minTicketCost = t.TicketCost;
            if (t.Survived == "TRUE")
                t.MarkColor = Color.blue;
            else
                t.MarkColor = Color.green;

            mark.GetComponent<SpriteRenderer>().color = t.MarkColor;
        }

        foreach (GameObject mark in MarkCollection)
        {
            Titanic t = mark.GetComponent<Titanic>();
            t.XPosition = (t.TicketCost - minTicketCost) / (maxTicketCost - minTicketCost);

            if (t.Gender == "Male")
            {
                t.YPosition = 0.66f;
            }
            else {
                t.YPosition = 0.33f;
            }

            if (CurrentSM.Count == 4) {
                if (t.Age < 20 && t.Age >= 0)
                {
                    mark.transform.SetParent(CurrentSM[0].transform);
                }
                else if (t.Age < 40 && t.Age >= 20)
                {
                    mark.transform.SetParent(CurrentSM[1].transform);
                }
                else if (t.Age < 60 && t.Age >= 40)
                {
                    mark.transform.SetParent(CurrentSM[2].transform);
                }
                else if (t.Age < 80 && t.Age >= 60)
                {
                    mark.transform.SetParent(CurrentSM[3].transform);
                }
            }

            canMove = true;
        }
    }
}

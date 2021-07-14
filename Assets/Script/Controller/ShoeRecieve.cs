using System;
using System.Collections;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UnityEngine;


//#include "ESP826WiFi.h"

//void setup() {
//    Serial.begin(115200);
//    Serial.println("Setup done");
//}

//void loop() {
//    Serial.print(a);

//    Delayed(100);
//}


public class ShoeRecieve : MonoBehaviour
{

    [Header("Serial port name")] public string portName = "COM9";
    [HideInInspector] public int baudRate = 115200;
    [HideInInspector] public Parity parity = Parity.None;
    [HideInInspector] public int dataBits = 8;
    [HideInInspector] public StopBits stopBits = StopBits.One;

    public string value;


    /// ////////////////////////////////////////////


    private SerialPort sp = null;
    private Thread ReadThread;
    //Thread CheckPortThread;

    void Start()
    {
        OpenPortControl();
    }

    void ReadSerial()
    {
        while (ReadThread.IsAlive)
        {
            try
            {
                Debug.Log(sp.BytesToRead);
                if (sp.BytesToRead > 1)
                {
                    string indata = sp.ReadLine();
                    value = indata;
                }
            }
            catch (SystemException f)
            {
                print(f);
                continue;
                //ReadThread.Abort();
            }
            ////Thread.Sleep(100);
        }
    }

    void Update()
    {

    }

    public void OpenPortControl()
    {
        sp = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        // Serial port initialization
        if (!sp.IsOpen)
        {
            try
            {
                sp.Open();
            }
            catch (SystemException f)
            {
                print(name + ": FAILED TO OPEN PORT");

            }
        }

        if (sp.IsOpen) {
            ReadThread = new Thread(ReceiveData); // This thread is used to receive serial data 
            ReadThread.Start();
            print("SerialOpen!");
        }
        else
            print(name + ": FAILED TO OPEN PORT 2");
    }

    public void ClosePortControl()
    {
        if (sp != null && sp.IsOpen)
        {
            sp.Close(); // Close the serial port
            sp.Dispose(); // Release the serial port from the memory
        }

        if (ReadThread != null)
            ReadThread.Abort();
    }

    private void ReceiveData()
    {
        while (ReadThread.IsAlive)
        {
            try
            {
                //Debug.Log(sp.BytesToRead);
                if (sp.BytesToRead > 1)
                {
                    string indata = sp.ReadLine();
                    value = indata;
                }
                else
                    continue;
            }
            catch (SystemException f)
            {
                print(f);
                continue;
            }
            //Thread.Sleep(100);
        }
    }

    void OnApplicationQuit()
    {
        ClosePortControl();

        //if (ReadThread != null)
        //    ReadThread.Abort();
    }

    private void OnDestroy()
    {
        ClosePortControl();
        //if (ReadThread != null)
        //    ReadThread.Abort();
    }



    //public void AbortThread()
    //{
    //    sp.Close();
    //    ReadThread.Abort();

    //}

    //void TryPort()
    //{
    //    print("CAlled");
    //    try
    //    {
    //        sp.Open();
    //    }
    //    catch (SystemException f)
    //    {
    //        print("FAILED TO OPEN PORT");

    //    }
    //    if (sp.IsOpen)
    //    {
    //        print("SerialOpen!");

    //        ReadThread = new Thread(new ThreadStart(ReadSerial));
    //        ReadThread.Start();

    //        //  SetJoystickMode(6);

    //    }
    //    else
    //    {

    //        StartCoroutine(CheckPort());
    //    }
    //}
    //IEnumerator CheckPort()  // Ignore
    //{
    //    yield return new WaitForSeconds(1f);
    //    CheckPortThread = new Thread(new ThreadStart(TryPort));
    //    CheckPortThread.Start();
    //}


}
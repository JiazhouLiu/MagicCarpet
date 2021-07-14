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
    [Header("baud rate")] public int baudRate = 115200;
    [Header("Check bit")] public Parity parity = Parity.None;
    [Header("Data Bit")] public int dataBits = 8;
    [Header("Stop Bit")] public StopBits stopBits = StopBits.One;

    public string value;


    /// ////////////////////////////////////////////


    private SerialPort sp = null;
    private Thread ReadThread;
    private byte[] datasBytes;
    private int i = 0;
    //Thread CheckPortThread;

    void Start()
    {
        //sp = new SerialPort(COM, 115200);
        //sp.ReadTimeout = 3000;
        //sp.WriteTimeout = 3000;
        //sp.WriteTimeout = 3000;
        //sp.Parity = Parity.None;
        //sp.DataBits = 8;
        //sp.StopBits = StopBits.One;
        //sp.RtsEnable = true;
        //sp.Handshake = Handshake.None;
        //sp.NewLine = "\n";  // Need this or ReadLine() fails
        //sp.Open();

        //try
        //{
        //    sp.Open();
        //}
        //catch (SystemException f)
        //{
        //    print(name + ": FAILED TO OPEN PORT");

        //}

        OpenPortControl();

        if (sp.IsOpen)
            print("SerialOpen!");
        else
            print(name + ": FAILED TO OPEN PORT");
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
            sp.Open();
        }
        ReadThread = new Thread(ReceiveData); // This thread is used to receive serial data 
        ReadThread.Start();
    }

    public void ClosePortControl()
    {
        if (sp != null && sp.IsOpen)
        {
            sp.Close(); // Close the serial port
            sp.Dispose(); // Release the serial port from the memory
        }

    }

    private void ReceiveData()
    {
        int bytesToRead = 0;
        while (true)
        {
            if (sp != null && sp.IsOpen)
            {
                try
                {
                    datasBytes = new byte[1024];
                    bytesToRead = sp.Read(datasBytes, 0, datasBytes.Length);
                    if (bytesToRead == 0)
                    {
                        continue;
                    }
                    else
                    {
                        string strbytes = Encoding.Default.GetString(datasBytes);
                        i++;
                        if (i > 0)
                        {
                            value = strbytes[0].ToString();
                        }
                        //Debug.Log(strbytes);
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            Thread.Sleep(100);
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
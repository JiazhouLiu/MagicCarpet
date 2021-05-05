using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class ShoeRecieve : MonoBehaviour
{

    public string COM = "COM9";

    public string value;


    /// ////////////////////////////////////////////


    SerialPort sp;
    public Thread ReadThread;
    Thread CheckPortThread;

    void Start()
    {
        //ReadThread = new Thread(new ThreadStart(ReadSerial));
        //ReadThread.Start();
        sp = new SerialPort(COM, 115200);
        sp.ReadTimeout = 3000;
        sp.WriteTimeout = 3000;
        sp.WriteTimeout = 3000;
        sp.Parity = Parity.None;
        sp.DataBits = 8;
        sp.StopBits = StopBits.One;
        sp.RtsEnable = true;
        sp.Handshake = Handshake.None;
        sp.NewLine = "\n";  // Need this or ReadLine() fails

        try
        {
            sp.Open();
        }
        catch (SystemException f)
        {
            print("FAILED TO OPEN PORT");

        }
        if (sp.IsOpen)
        {
            print("SerialOpen!");

            ReadThread = new Thread(new ThreadStart(ReadSerial));
            ReadThread.Start();
            
        }

        // sendSlider(2, 0);
    }
    void TryPort()
    {
        print("CAlled");
        try
        {
            sp.Open();
        }
        catch (SystemException f)
        {
            print("FAILED TO OPEN PORT");

        }
        if (sp.IsOpen)
        {
            print("SerialOpen!");

            ReadThread = new Thread(new ThreadStart(ReadSerial));
            ReadThread.Start();
          
            //  SetJoystickMode(6);
            
        }
        else
        {

            StartCoroutine(CheckPort());
        }
    }
    IEnumerator CheckPort()  // Ignore
    {
        yield return new WaitForSeconds(1f);
        CheckPortThread = new Thread(new ThreadStart(TryPort));
        CheckPortThread.Start();


    }

    void ReadSerial()
    {
        while (ReadThread.IsAlive)
        {
            try
            {
                //Debug.Log(sp.BytesToRead);
                if (sp.BytesToRead > 1)
                {

                   string indata = sp.ReadLine();

                   //print(indata);
                   value = indata;
                   
                }
            }
            catch (SystemException f)
            {
                print(f);
                continue;
                //ReadThread.Abort();
            }
            //Thread.Sleep(100);
        }
    }

    void Update()  
    {

    }

    void OnApplicationQuit()
    {
        if(ReadThread != null)
            ReadThread.Abort();
    }

    public void AbortThread()
    {
        ReadThread.Abort();
    }

    private void OnDestroy()
    {
        if (ReadThread != null)
            ReadThread.Abort();
    }
}
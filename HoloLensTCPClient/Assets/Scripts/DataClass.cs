using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DataClass
{
    private static DataClass _instance = new DataClass();

    // public string ServerURL = "http://192.168.43.175:42123";
    private string DefaultServerURL = "http://172.26.30.48:42123";
    //public string ServerURL = "https://e236fd17.ap.ngrok.io";

    private float DefaultPhotoCaptureInterval = 3f;

    public static DataClass Instance()
    {
        return _instance;
    }

    private DataClass()
    {

    }

    public void SetPhotoCaptureInterval(float time)
    {
        PlayerPrefs.SetFloat("PhotoCaptureInterval",time);
        PlayerPrefs.Save();
    }

    public float GetPhotoCaptureInterval()
    {
        if (PlayerPrefs.HasKey("PhotoCaptureInterval"))
        {
            return PlayerPrefs.GetFloat("PhotoCaptureInterval");
        }
        return DefaultPhotoCaptureInterval;
    }

    public void SetServerURL1(String url)
    {
        PlayerPrefs.SetString("ServerURL1", url);
        PlayerPrefs.Save();
    }

    public void SetServerURL2(String url)
    {
        PlayerPrefs.SetString("ServerURL2", url);
        PlayerPrefs.Save();
    }

    public String GetServerURL1()
    {
        if(PlayerPrefs.HasKey("ServerURL1"))
        {
            return PlayerPrefs.GetString("ServerURL1");
        }
        return DefaultServerURL;
    }

    public String GetServerURL2()
    {
        if (PlayerPrefs.HasKey("ServerURL2"))
        {
            return PlayerPrefs.GetString("ServerURL2");
        }
        return DefaultServerURL;
    }

}
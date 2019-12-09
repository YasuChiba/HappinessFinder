using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixedCanvasManager : MonoBehaviour {

    public Text LogText;
    public Text LogTextRight;

	// Use this for initialization
	void Start () {

		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddLog(string text)
    {
        string tmp = LogText.text;
        LogText.text = text + "\n" + tmp;
    }

    /*
    public void AddLogToRight(string text)
    {
        string tmp = LogText.text;
        LogTextRight.text = text + "\n" + tmp;
    }
    */
}

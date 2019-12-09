using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.UI.Keyboard;
using System;

public class TestCompon : MonoBehaviour {


    public Button button;
    public GameObject spatialMapping;


	// Use this for initialization
	void Start () {
        button.onClick.AddListener(() => buttonClicked());

        this.gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2;
        this.gameObject.transform.LookAt(Camera.main.transform);
        this.gameObject.transform.Rotate(0, 180, 0);
    }

    // Update is called once per frame
    void Update () {
		
	}

    void buttonClicked()
    {
        spatialMapping.SetActive(false);

        Debug.Log(" buttonClicked");
        if (!Keyboard.Instance.gameObject.activeSelf)
        {
            Keyboard.Instance.PresentKeyboard();
            Keyboard.Instance.RepositionKeyboard(this.gameObject.transform, null, 0);
            Keyboard.Instance.OnTextSubmitted += KeyboardOnTextSubmitted;
            Keyboard.Instance.OnClosed += KeyboardOnClosed;


        }



    }


    private void KeyboardOnTextSubmitted(object sender, EventArgs eventArgs)
    {
        string text = ((Keyboard)sender).InputField.text;
        if (!string.IsNullOrEmpty(text))
        {
            Debug.Log("INPUTTED STRING = " + text);
        }
    }

    private void KeyboardOnClosed(object sender, EventArgs eventArgs)
    {
        Keyboard.Instance.OnTextSubmitted -= KeyboardOnTextSubmitted;
        Keyboard.Instance.OnClosed -= KeyboardOnClosed;
        spatialMapping.SetActive(true);
    }

}

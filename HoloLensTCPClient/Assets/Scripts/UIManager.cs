using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.UI.Keyboard;
using System;

public enum UIFieldType
{
    URL1,
    URL2,
    PhotoInterval
}


public class UIManager : MonoBehaviour {

    public MyInputField URL1InputFIeld;
    public MyInputField URL2InputFIeld;

    public MyInputField PhotoIntervalField;

    public Button CloseButton;
    public Button SetURL1Button;
    public Button SetURL2Button;

    public Button SetPhotoIntervalButton;

    public Toggle ShowLogToggle;

    public event Action CloseUI;

    public GameObject FixedCanvas;

    public Button FindServerButton;

    private FindServerManager findServerManager;




    private UIFieldType currentSelectedInputField;


    private void Awake()
    {
        URL1InputFIeld.InputFieldSelected += () =>
         {
             InputFieldSelected(UIFieldType.URL1);
         };

        URL2InputFIeld.InputFieldSelected += () =>
        {
            InputFieldSelected(UIFieldType.URL2);
        };

        PhotoIntervalField.InputFieldSelected += () =>
        {
            InputFieldSelected(UIFieldType.PhotoInterval);
        };

        ;

     

        CloseButton.onClick.AddListener(() =>
        {
            CloseUI();
            DismissUI();
        });

        SetURL1Button.onClick.AddListener(() =>
        {
            DataClass.Instance().SetServerURL1(URL1InputFIeld.text);
        });

        SetURL2Button.onClick.AddListener(() =>
        {
            DataClass.Instance().SetServerURL2(URL2InputFIeld.text);
        });

        SetPhotoIntervalButton.onClick.AddListener(() =>
        {
            DataClass.Instance().SetPhotoCaptureInterval(float.Parse(PhotoIntervalField.text));
        });

        FindServerButton.onClick.AddListener(() =>
        {
            if(findServerManager == null)
            {
                findServerManager = new FindServerManager();
                findServerManager.StartFindingServer();
                FindServerButton.GetComponentInChildren<Text>().text = "StopFindServer";
                CloseButton.interactable = false;
            } else
            {
                CloseButton.interactable = true;
                FindServerButton.GetComponentInChildren<Text>().text = "Find Server";



                var result = findServerManager.StopFindingServer();

                findServerManager = null;


                if(result.Length == 0)
                {
                    //URL1InputFIeld.text = "nothing found and nothing change";

                }
                else if(result.Length == 1)
                {
                    URL1InputFIeld.text = result[0];
                    URL2InputFIeld.text = "";
                    DataClass.Instance().SetServerURL1(result[0]);
                    DataClass.Instance().SetServerURL2("");

                }
                else
                {
                    URL1InputFIeld.text = result[0];
                    URL2InputFIeld.text = result[1];
                    DataClass.Instance().SetServerURL1(result[0]);
                    DataClass.Instance().SetServerURL2(result[1]);

                }
            }
            
           
        });

        URL1InputFIeld.text = DataClass.Instance().GetServerURL1();
        URL2InputFIeld.text = DataClass.Instance().GetServerURL2();

        PhotoIntervalField.text = DataClass.Instance().GetPhotoCaptureInterval().ToString();
    }
    // Use this for initialization
    void Start () {
        this.DismissUI();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void InputFieldSelected(UIFieldType fieldType)
    {
        Debug.Log("Inputfiled Selected");
        if(!Keyboard.Instance.gameObject.activeSelf)
        {
            Keyboard.Instance.PresentKeyboard();
            Keyboard.Instance.RepositionKeyboard(this.gameObject.transform, null, 0);
            Keyboard.Instance.OnTextSubmitted += KeyboardOnTextSubmitted;
            Keyboard.Instance.OnClosed += KeyboardOnClosed;

            if (fieldType == UIFieldType.URL1)
            {
                Keyboard.Instance.InputField.text = URL1InputFIeld.text;
            }
            else if (fieldType == UIFieldType.URL2) {
                Keyboard.Instance.InputField.text = URL2InputFIeld.text;
            }
            else if (fieldType == UIFieldType.PhotoInterval)
            {
                Keyboard.Instance.InputField.text = PhotoIntervalField.text;
            }
            currentSelectedInputField = fieldType;
        }
        
    }

    private void KeyboardOnTextSubmitted(object sender, EventArgs eventArgs) {
        string text = ((Keyboard)sender).InputField.text;
        Debug.Log("inputted text = " + text);
        if(!string.IsNullOrEmpty(text))
        {
            if(currentSelectedInputField == UIFieldType.URL1)
            {
                URL1InputFIeld.text = text;
            }else if (currentSelectedInputField == UIFieldType.URL2){
                URL2InputFIeld.text = text;

            }
            else if (currentSelectedInputField == UIFieldType.PhotoInterval)
            {
                PhotoIntervalField.text = text;
            }
        }
    }

    private void KeyboardOnClosed(object sender, EventArgs eventArgs)
    {
        Keyboard.Instance.OnTextSubmitted -= KeyboardOnTextSubmitted;
        Keyboard.Instance.OnClosed -= KeyboardOnClosed;
    }



    public void InputFieldDeselected()
    {
        Debug.Log("Inputfiled DeSelected");

    }

    public void ShowUI(Transform cameraTransform)
    {

        Debug.Log("Show UI");
        this.gameObject.SetActive(true);
        this.gameObject.transform.position = cameraTransform.position + cameraTransform.forward*2;
        this.gameObject.transform.LookAt(cameraTransform);
        this.gameObject.transform.Rotate(0, 180, 0);
    }

    private  void DismissUI()
    {
        Debug.Log("Dismiss UI");
        this.gameObject.SetActive(false);
    }

    public void ShowLogToggleChanged()
    {
        FixedCanvas.SetActive(ShowLogToggle.isOn);
    }

}

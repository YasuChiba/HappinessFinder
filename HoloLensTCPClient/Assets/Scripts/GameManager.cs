using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using HoloToolkit.Unity.InputModule;
using UnityEngine.UI;

public class GameManager : MonoBehaviour, IInputClickHandler
{

    public GameObject canvas;
    public GameObject spatialMapping;

    private UIManager uiManager;
    private PhotoManager client;

    public GameObject cursor;



    //max time between first tap and second tap(for double tap)
    private float MultTapTime = 0.5f;
    private int p_MultTapCount = 0;
    private float p_MultTapStart;

    private bool IsShowingUI = false;

    private void Awake()
    {
        this.uiManager = canvas.GetComponent<UIManager>();
        this.client = this.GetComponent<PhotoManager>();

        Assert.IsNotNull(this.uiManager);
        Assert.IsNotNull(this.client);

        // InputManager.Instance.PushFallbackInputHandler(gameObject);
        InputManager.Instance.AddGlobalListener(gameObject);

        uiManager.CloseUI += CloseUI;
        cursor.SetActive(false);


    }

    // Use this for initialization
    void Start () {

    }

    void ShowUI()
    {
        cursor.SetActive(true);
        uiManager.ShowUI(Camera.main.transform);
        IsShowingUI = true;
        client.StopPhotoCapture();
        spatialMapping.SetActive(false);//こうしないとキーボード使えない

    }

    void CloseUI()
    {
        cursor.SetActive(false);

        IsShowingUI = false;
        spatialMapping.SetActive(true);
    }

    void SingleTap()
    {
        Debug.Log("Single Tap");
        if(IsShowingUI)
        {
            return;
        }

        client.ChangePhotoCaptureState();

    }

    void DoubleTap()
    {
        Debug.Log("Double Tap");
        if (IsShowingUI)
        {
            return;
        }

        ShowUI();

    }


    // Update is called once per frame
    void Update () {
		if(p_MultTapCount >= 1)
        {
            if((Time.time - p_MultTapStart) > MultTapTime)
            {
                //Single Tap
                if(p_MultTapCount == 1)
                {
                    this.SingleTap();
                }

                //Double Tap
                if(p_MultTapCount == 2)
                {
                    this.DoubleTap();
                }

                p_MultTapCount = 0;
            }
        }
	}


    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("OnInputClicked");

        float nowTime = Time.time;
        float tapTime = nowTime - p_MultTapStart;
        if(tapTime > MultTapTime)
        {
            p_MultTapCount = 1;
        } else
        {
            p_MultTapCount++;
        }
        p_MultTapStart = nowTime;
    }
}

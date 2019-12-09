using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using HoloToolkit.Unity.InputModule;
using UnityEngine.XR.WSA.WebCam;
using System.Linq;

public class PhotoCaptureScript : MonoBehaviour, IInputClickHandler
{

    PhotoCapture photoCaptureObject = null;

    void Start()
    {


        Debug.Log("startttt");
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);

    }

    // エアタップの取得
    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("start captureing");
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;
        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);

    }


    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {

        if (result.success)
        {
            Debug.Log("capture success");
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.Log("capture fail");
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        Debug.Log("OnCapturedPhotoToMemorys");

        if (result.success)
        {
            Debug.Log("TTTTTTTTTTTTTTTTTT");

            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);

        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);

    }

}

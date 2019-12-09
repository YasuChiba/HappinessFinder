using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.WSA.WebCam;

public class Client : MonoBehaviour, IInputClickHandler
{

    public string IP;
    public int port;
    public Text connectButtonText;
    public TextMesh debugText;

    public GameObject UIImagePrefab;
    public GameObject Canvas;


    private PhotoCapture photoCaptureObject = null;
    private Resolution cameraResolution;

    private List<GameObject> AddedObjects = new List<GameObject>();

    private bool IsPhotoStarted = true;

    private bool IsPhotoEnabled = false;
     

    //private TcpNetworkClientManager client = null;
    
    // Use this for initialization
    void Start () {
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        //debugText.text = cameraResolution.width.ToString() + " " + cameraResolution.height.ToString();
       // targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        // targetTexture = new Texture2D(480, 270);
        InputManager.Instance.PushFallbackInputHandler(gameObject);
        //InputManager.Instance.AddGlobalListener(gameObject);

        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        StartCoroutine(PeriodicExecution());


//        client = new TcpNetworkClientManager(IP, port);
    }

    void OnDestroy()
    {
        //InputManager.Instance.RemoveGlobalListener(gameObject);
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);


    }



    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        Debug.Log("OnCapturedPhotoToMemory");

        if(result.success)
        {
            
            
            List<byte> imageBufferList = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            Matrix4x4 cameraToWorldMatrix = new Matrix4x4();
            photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
            Quaternion cameraRotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));

            Matrix4x4 projectionMatrix;
            photoCaptureFrame.TryGetProjectionMatrix(Camera.main.nearClipPlane, Camera.main.farClipPlane, out projectionMatrix);
            Matrix4x4 pixelToCameraMatrix = projectionMatrix.inverse;
            

            Debug.Log("Buffer   SIZE = " + imageBufferList.ToArray().Length.ToString());

            StartCoroutine(PostRequest(imageBufferList.ToArray(), cameraRotation, cameraToWorldMatrix, pixelToCameraMatrix));

        }
    }

    void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            IsPhotoEnabled = true;
            Debug.Log("Capture create Succes");
        }
        else
        {
            Debug.Log("Caputer create fail");
        }
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;
        CameraParameters c = new CameraParameters();
        //c.hologramOpacity = 0.9f;
        c.hologramOpacity = 0.0f;

        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        Debug.Log("Resolution  " + cameraResolution.width + "      " + cameraResolution.height);
        c.pixelFormat = CapturePixelFormat.JPEG;

        photoCaptureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);


    }

    IEnumerator PostRequest(byte[] imageData,Quaternion cameraRotation, Matrix4x4 cameraToWorldMatrix, Matrix4x4 pixelToCameraMatrix)
    {
        Debug.Log("start request");
        Debug.Log("imagedata Length " + imageData.Length);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageData, fileName: "image/jpeg");

        // UnityWebRequest request = UnityWebRequest.Post("http://172.26.30.45:44124/picture", form);
        UnityWebRequest request = UnityWebRequest.Post("http://172.26.30.41:42123/picture", form);


        Debug.Log("start requesting");
        yield return request.SendWebRequest();
        Debug.Log(request.isNetworkError);
        Debug.Log(request.downloadHandler.text);

        if (!request.isNetworkError)
        {

            PictureRecognitionResult results = JsonUtility.FromJson<PictureRecognitionResult>(request.downloadHandler.text);
            Debug.Log(results);

            if(results.results.Length != 0)
            {

                foreach (var tmpObject in AddedObjects)
                {
                    Destroy(tmpObject);

                }
                AddedObjects.Clear();
            }

         
            foreach(var result in results.results)
            {

                /*

                float top = -(result.top / cameraResolution.height);
                float left = result.left / cameraResolution.width;
                float width = result.width / cameraResolution.width;
                float height = result.height / cameraResolution.height;
                */
                float top = -(result.top - .5f);
                float left = result.left - .5f;
                float width = result.width;
                float height = result.height;

                Vector3 objPosition = cameraToWorldMatrix.MultiplyPoint3x4(pixelToCameraMatrix.MultiplyPoint3x4(new Vector3(left ,  top, 0)));

                Debug.Log("ObjPosition:   " + objPosition.x + "   " + objPosition.y + "    " + objPosition.z);

                Debug.Log("width  " + width + "   height" + height);
                Debug.Log("cameraReso width  " + cameraResolution.width + "   height  " + cameraResolution.height);


                GameObject obj = Instantiate(UIImagePrefab);
                GameObject rectImageObj = obj.transform.Find("Image").gameObject;

                obj.transform.position = objPosition;
                obj.transform.rotation = cameraRotation;
                obj.transform.Rotate(new Vector3(0, 1, 0), 180);
                obj.transform.parent = Canvas.transform;

                RectTransform detectedRectTransform = rectImageObj.GetComponent<RectTransform>();
                detectedRectTransform.sizeDelta = new Vector2(width, height);
                // obj.transform.localScale = new Vector3(width, height, 0.05f);

                /*
                top = -result.top;
                left = result.left;
                GameObject objCuvbe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                objCuvbe.transform.position = cameraToWorldMatrix.MultiplyPoint3x4(pixelToCameraMatrix.MultiplyPoint3x4(new Vector3(left, top, 0)));
                objCuvbe.transform.localScale = new Vector3(height, 0.01f, width);
                */
                AddedObjects.Add(obj);
                //AddedObjects.Add(objCuvbe);



            }

        }


    }

    public static Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
    {
        Vector3 from = new Vector3(0, 0, 0);
        var axsX = proj.GetRow(0);
        var axsY = proj.GetRow(1);
        var axsZ = proj.GetRow(2);
        from.z = to.z / axsZ.z;
        from.y = (to.y - (from.z * axsY.z)) / axsY.y;
        from.x = (to.x - (from.z * axsX.z)) / axsX.x;
        return from;
    }

    IEnumerator PeriodicExecution()
    {
        while(true)
        {
            if(IsPhotoStarted && IsPhotoEnabled)
            {
                //PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

            }
            yield return new WaitForSeconds(3);

        }
    }




    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("OnInputClicked");
        // PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        IsPhotoStarted = !IsPhotoStarted;
    }

    public void ConnectButtonClicked()
    {
        /*
        if(client != null)
        {
            Debug.Log("Disconnected");
            connectButtonText.text = "Connect";
            client = null;
        }
        else
        {
            Debug.Log("Connected");
            client = new TcpNetworkClientManager(IP, port);
            connectButtonText.text = "Disconnect";
        }
        */
    }
}

[Serializable]
public class PictureRecognitionResult
{
    public PictureRecognitionResultItem[] results;
}


[Serializable]
public class PictureRecognitionResultItem
{
    public float top;
    public float left;
    public float width;
    public float height;
}
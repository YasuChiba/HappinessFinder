using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.WSA.WebCam;
using HoloToolkit.Unity.SpatialMapping;

public class Client2 : MonoBehaviour
{

    public FixedCanvasManager fixedCanvasManager;


    private PhotoCapture photoCaptureObject = null;
    private Resolution cameraResolution;

    private List<GameObject> AddedObjects = new List<GameObject>();

    //写真撮影をしているかどうか
    private bool IsPhotoStarted = true;

    //撮影が可能かどうか
    private bool IsPhotoEnabled = false;

    private AudioManager audioManager;

    private bool isServer1 = true;

    //private TcpNetworkClientManager client = null;

    // Use this for initialization
    void Start()
    {
        audioManager = GetComponent<AudioManager>();

        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        StartCoroutine(PeriodicExecution());
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
   

    void CreateUI(PictureRecognitionResultItem2 result, PhotoCaptureFrame photoCaptureFrame)
    {

        Matrix4x4 cameraToWorldMatrix;
        photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
        Matrix4x4 projectionMatrix;
        photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix);

        Vector3 headPosition = cameraToWorldMatrix.MultiplyPoint(Vector3.zero);

      
        /*
        var imagePosZeroToOneTopLeft = new Vector2(result.xmin, 1 - result.ymin);
        var imagePosProjectedTopLeft = (imagePosZeroToOneTopLeft * 2) - new Vector2(1, 1);    // -1 to 1 space
        var cameraSpacePosTopLeft = UnProjectVector(projectionMatrix, new Vector3(imagePosProjectedTopLeft.x, imagePosProjectedTopLeft.y, 1));
        var worldSpaceBoxPosTopLeft = cameraToWorldMatrix.MultiplyPoint(cameraSpacePosTopLeft);
        */

        var imagePosZeroToOneCenter = new Vector2((result.xmin + result.xmax) / 2, 1 - (result.ymin + result.ymax) / 2);
        var imagePosProjectedCenter = (imagePosZeroToOneCenter * 2) - new Vector2(1, 1);
        var cameraSpacePosCenter = UnProjectVector(projectionMatrix, new Vector3(imagePosProjectedCenter.x, imagePosProjectedCenter.y, 1));
        var worldSpaceBoxPosCenter = cameraToWorldMatrix.MultiplyPoint(cameraSpacePosCenter);


        RaycastHit hit;
        if (Physics.Raycast(headPosition, (worldSpaceBoxPosCenter - headPosition).normalized, out hit, 20f, SpatialMappingManager.Instance.LayerMask))
        {
            /*
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cube.transform.position = hit.point;
            cube.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            cube.GetComponent<Renderer>().material.color = Color.yellow;
            */
            GameObject sphere =(GameObject)Resources.Load("SizeChangableSphere");
            AddedObjects.Add(Instantiate(sphere, hit.point, Quaternion.identity));

        }
        else
        {
            /*
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = worldSpaceBoxPosCenter;
            cube.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            cube.GetComponent<Renderer>().material.color = Color.yellow;
            */
            GameObject sphere = (GameObject)Resources.Load("SizeChangableSphere");
            AddedObjects.Add(Instantiate(sphere, worldSpaceBoxPosCenter, Quaternion.identity));
        }

    }


    IEnumerator PostRequest(byte[] imageData, PhotoCaptureFrame photoCaptureFrame)
    {

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageData, fileName: "image/jpeg");

        // UnityWebRequest request = UnityWebRequest.Post("http://172.26.30.45:44124/picture", form);
        //UnityWebRequest request = UnityWebRequest.Post("http://172.26.30.41:42123/picture2", form);
        UnityWebRequest request;

        if(isServer1)
        {
            request = UnityWebRequest.Post(DataClass.Instance().GetServerURL1() + "/picture2", form);
        } else
        {
            request = UnityWebRequest.Post(DataClass.Instance().GetServerURL2() + "/picture2", form);
        }
        isServer1 = !isServer1;

        Debug.Log("start requesting");
        yield return request.SendWebRequest();
        Debug.Log(request.isNetworkError);
        if (request.isNetworkError)
        {
            fixedCanvasManager.AddLog("network error");

        }

        Debug.Log(request.downloadHandler.text);

        //もし写真停止中だったらなにも表示しない
        if (!request.isNetworkError && IsPhotoStarted)
        {

            PictureRecognitionResult2 results = JsonUtility.FromJson<PictureRecognitionResult2>(request.downloadHandler.text);
            Debug.Log(results);

            if (results.results.Length != 0)
            {
                //見つけたらいったん止める
                StopPhotoCapture();
                fixedCanvasManager.AddLog("Found "+ results.results.Length.ToString() + " Clover(s)");

                audioManager.cloverFound();

                foreach (var tmpObject in AddedObjects)
                {
                    Destroy(tmpObject);

                }
                AddedObjects.Clear();

                foreach (var result in results.results)
                {
                    CreateUI(result, photoCaptureFrame);
                }

            } else
            {
                fixedCanvasManager.AddLog("Could not Find Clover(s)");

            }
        }


    }


    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {       

            List<byte> imageBufferList = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            Debug.Log("Buffer   SIZE = " + imageBufferList.ToArray().Length.ToString());

            StartCoroutine(PostRequest(imageBufferList.ToArray(), photoCaptureFrame));

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
   

    IEnumerator PeriodicExecution()
    {
        while (true)
        {
            //fixedCanvasManager.AddLogToRight("IsPhotoStarted = " + IsPhotoStarted);
            if (IsPhotoStarted && IsPhotoEnabled)
            {
                fixedCanvasManager.AddLog("Start Taking Photo");
                audioManager.shutter();

                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

            }
            yield return new WaitForSeconds(DataClass.Instance().GetPhotoCaptureInterval());

        }
    }


    //一時停止中なら再開。動いているなら停止
    public void ChangePhotoCaptureState()
    {
        Debug.Log("Change photcapture state");
        IsPhotoStarted = !IsPhotoStarted;
    }

    public void StopPhotoCapture()
    {
        IsPhotoStarted = false;
    }


}




[Serializable]
public class PictureRecognitionResult2
{
    public PictureRecognitionResultItem2[] results;
}


[Serializable]
public class PictureRecognitionResultItem2
{
    public float ymin;
    public float ymax;
    public float xmin;
    public float xmax;
}
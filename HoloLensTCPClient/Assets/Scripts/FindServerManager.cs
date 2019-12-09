using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;


class FindServerManager
{

    private int UDPReceivePort = 5000;
    private byte[] p_UDPReceivedData;

    public List<String> receivedAddresses = new List<String>() { };


    public void StartFindingServer()
    {
        receivedAddresses = new List<String>() { };

#if WINDOWS_UWP
        Debug.Log("Start init udp client");
        // 初期化処理
        UDPClientReceiver_Init();
#endif
    }

    public String[] StopFindingServer()
    {
#if WINDOWS_UWP
        if(p_Socket != null) {
            p_Socket.Dispose();
        }

#endif

        return receivedAddresses.ToArray();
    }


#if WINDOWS_UWP
    Windows.Networking.Sockets.DatagramSocket p_Socket;

    object p_LockObject = new object();

    const int MAX_BUFFER_SIZE = 1024;



    private async void UDPClientReceiver_Init()
    {
        try
        {
            // UDP通信インスタンスの初期化
            p_Socket = new Windows.Networking.Sockets.DatagramSocket();
            // 受信時のコールバック関数を登録する
            p_Socket.MessageReceived += OnMessage;
            // 指定のポートで受信を開始する
            p_Socket.BindServiceNameAsync(UDPReceivePort.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    async void OnMessage
       (Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        using (System.IO.Stream stream = args.GetDataStream().AsStreamForRead())
        {
            // 受信データを取得
            byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
            await stream.ReadAsync(receiveBytes, 0, MAX_BUFFER_SIZE);
            lock (p_LockObject)
            {
                // 受信データを処理に引き渡す
                UDPReceiveEvent(receiveBytes);
            }
        }
    }
#endif
    private void UDPReceiveEvent(byte[] receiveData)
    {
        

        // 受信データを記録する
        p_UDPReceivedData = receiveData;
        string text = System.Text.Encoding.UTF8.GetString(p_UDPReceivedData);

        Debug.Log("UDP reveived   " + text);

        if(!receivedAddresses.Contains(text))
        {
            receivedAddresses.Add(text);
        }
    }

   
}
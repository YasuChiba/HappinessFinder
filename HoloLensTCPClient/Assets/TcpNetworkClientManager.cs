using System;
#if WINDOWS_UWP
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
#endif
using UnityEngine;
using UnityEngine.UI;

public class TcpNetworkClientManager
{
#if WINDOWS_UWP
    private Stream stream = null;
    private StreamWriter writer = null;
#endif

    public TcpNetworkClientManager(string IP, int port)
    {

        Debug.Log("TcpNetworkClientManager1  "+IP);

#if WINDOWS_UWP
        Debug.Log("TcpNetworkClientManager2");

        Task.Run(async () => {
           Debug.Log("TcpNetworkClientManager3");

            StreamSocket socket = new StreamSocket();
                   Debug.Log("TcpNetworkClientManager4");

            await socket.ConnectAsync(new HostName(IP),port.ToString());
                   Debug.Log("TcpNetworkClientManager5");

            stream = socket.OutputStream.AsStreamForWrite();
            writer = new StreamWriter(stream);
            StreamReader reader = new StreamReader(socket.InputStream.AsStreamForRead());
            try
            {
                           Debug.Log("TcpNetworkClientManage6");

                string data = await reader.ReadToEndAsync();
                Debug.Log("TcpNetworkClientManager7");

            }
            catch (Exception) {
                Debug.Log("TcpNetworkClientManager8");

            }
            writer = null;
        });
#endif
    }

    public void SendTCPMessage(string data)
    {
        Debug.Log("sendTCPMessage1");
#if WINDOWS_UWP
        Debug.Log("sendTCPMessage2");

        if (writer != null) Task.Run(async () =>
        {
            await writer.WriteAsync(data);
            await writer.FlushAsync();
        });
#endif
    }

    public void SendImage(byte[] image)
    {
        Debug.Log("SendImage1");
#if WINDOWS_UWP
        Debug.Log("SendImage2");

        if (stream != null) Task.Run(async () =>
        {
            await stream.WriteAsync(image, 0, image.Length);
            await stream.FlushAsync();
        });
#endif
    }
}
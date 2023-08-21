using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;

public class UdpTool : MonoBehaviour
{
    Socket socket;
    EndPoint clientEndPoint;
    byte[] ReceiveData = new byte[1024];
    Queue<byte[]> dataQueue = new Queue<byte[]>();

    // Start is called before the first frame update
    void Start()
    {
        StartServer(16650);
    }

    // Update is called once per frame
    void Update()
    {
        if (DequeueDataCount > 0)
        {
            byte[] bytes = DequeueData();
            string recvStr = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Debug.Log($"收到客户端发过来的数据:{recvStr}");
            //发回去做测试用
            SocketSend("from server");
        }
    }

    public void StartServer(int localPort = 16650)
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, localPort));
            clientEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

            //开始异步接收数据
            socket.BeginReceiveFrom(ReceiveData, 0, ReceiveData.Length,
                SocketFlags.None, ref clientEndPoint, new AsyncCallback(ReceiveMessageToQueue), clientEndPoint);

            Debug.Log("server启动 port1:" + localPort);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    /// <summary>
    /// 接收消息并存放 
    /// </summary>
    /// <param name="iar"></param>
    private void ReceiveMessageToQueue(IAsyncResult iar)
    {
        Debug.Log("监听");
        try
        {
            if (socket != null)
            {
                int length = socket.EndReceiveFrom(iar, ref clientEndPoint);
                Debug.Log("继续监听");
                socket.BeginReceiveFrom(ReceiveData, 0, ReceiveData.Length,
                    SocketFlags.None, ref clientEndPoint, new AsyncCallback(ReceiveMessageToQueue), clientEndPoint);

                if (length > 0)
                {
                    byte[] data = new byte[length];
                    Buffer.BlockCopy(ReceiveData, 0, data, 0, length);
                    dataQueue.Enqueue(data);
                }
            }
            else
            {
                Debug.Log("serverSocket == null");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// 从队列中取出数据
    /// </summary>
    /// <returns></returns>
    protected byte[] DequeueData()
    {
        return dataQueue.Dequeue();
    }

    protected int DequeueDataCount
    {
        get { return dataQueue.Count; }
    }

    public void SocketSend(string sendStr)
    {
        //发送给指定客户端
        byte[] sendData = Encoding.UTF8.GetBytes(sendStr);
        socket.SendTo(sendData, clientEndPoint);
    }

    //连接关闭
    public void CloseServer()
    {
        // 关闭socket
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
        Debug.Log("关闭server");
    }

    private void OnApplicationQuit()
    {
        CloseServer();
    }
}
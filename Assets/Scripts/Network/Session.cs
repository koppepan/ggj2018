using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Network
{
    [System.Serializable]
    public struct Message
    {
        public string key;
        public string body;
    }

    public class Session
    {
        TcpListener listener;
        TcpClient tcpClient;

        NetworkStream stream;

        byte[] ReceiveBuffer = new byte[1024];

        public Action OnAcceptConnect = delegate { };
        public Action OnCloseSession = delegate { };

        public Action<Message> OnRecvMessage = delegate { };

        // サーバーに接続に行く
        public Session(string host, int port)
        {
            tcpClient = new TcpClient(AddressFamily.InterNetwork);
            IPAddress[] remoteHost = Dns.GetHostAddresses(host);

            Debug.Log(string.Format("Connection Request to {0}:{1}", remoteHost[0], port));

            tcpClient.BeginConnect(remoteHost[0], port, new AsyncCallback(ConnectCallback), tcpClient);
        }

        void ConnectCallback(IAsyncResult result)
        {
            tcpClient = (TcpClient)result.AsyncState;
            tcpClient.EndConnect(result);
            Debug.Log(string.Format("Connection to {0}:{1}",
                ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address,
                ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port));

            stream = tcpClient.GetStream();
            OnAcceptConnect();
            BeginReceive();
        }


        // クライアントからの接続待ち
        public Session(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Debug.Log(string.Format("Listen Start to {0}:{1}", IPAddress.Parse(UnityEngine.Network.player.ipAddress), port));

            listener.BeginAcceptTcpClient(
                new AsyncCallback(ListenerCallback),
                listener);
        }

        void ListenerCallback(IAsyncResult result)
        {
            listener = (TcpListener)result.AsyncState;
            tcpClient = listener.EndAcceptTcpClient(result);

            Debug.Log(string.Format("Connection to {0}:{1}",
                ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address,
                ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port));

            listener.Stop();

            stream = tcpClient.GetStream();
            OnAcceptConnect();
            BeginReceive();
        }

        // Sessionを切断
        public void Close()
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            if (tcpClient != null)
            {
                tcpClient.Close();
            }
			Debug.Log ("close session.");
            OnCloseSession();
        }

        // 受信
        void BeginReceive()
        {
            try
            {
                if (!stream.CanRead) return;
                ReceiveBuffer = new byte[1024];

                stream.BeginRead(
                    ReceiveBuffer,
                    0,
                    ReceiveBuffer.Length,
                    new AsyncCallback(ReceiveDataCallback),
                    stream
                );
            }
            catch (SocketException e)
            {
                Close();
                Debug.LogError(e);
            }
        }

        void ReceiveDataCallback(IAsyncResult result)
        {
            try
            {
                // 読み込んだバイト数を取得
                int bytes = stream.EndRead(result);

                //切断されたか調べる
                if (bytes <= 0)
                {
                    Close();
                    return;
                }

                // 読み込んだデータを復元
                var json = Encoding.GetEncoding("utf-8").GetString(ReceiveBuffer);
                var msg = JsonUtility.FromJson<Message>(json);

                OnRecvMessage(msg);

                BeginReceive();
            }
            catch (SocketException e)
            {
                Close();
                Debug.LogError(e);
            }
        }

        // 送信
        public void Send(Message data)
        {
            if (stream == null || !stream.CanWrite)
            {
                Close();
                return;
            }

            try
            {
                var json = JsonUtility.ToJson(data);
                var msg = Encoding.UTF8.GetBytes(json);

                tcpClient.Client.Send(msg);
            }
            catch (SocketException e)
            {
                Debug.LogError(e);
                Close();
            }
        }
    }
}

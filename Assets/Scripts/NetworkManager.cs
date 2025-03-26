using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class Packet
{
    public string code;
    public string id;
}
[Serializable]
public class LoginPacket : Packet
{
    public string password;
}
[Serializable]
public class SignInPacket : Packet
{
    public string password;
    public string name;
    public string email;
}
[Serializable]
public class RecvPacket 
{
    public string code;
    public string message;
    public string name;
    public string email;
}
public class NetworkManager : Singleton<NetworkManager>
{
    public GameObject LogInPanel;
    public GameObject SignInPanel;
    public PlayerDataScriptableObject playerData;

    private Socket serverSocket;
    private IPEndPoint serverEndPoint;
    private Thread recvThread;

    public InputField idUI;
    public InputField passwordUI;

    public InputField NewidUI;
    public InputField NewpasswordUI; 
    public InputField NewNameUI;
    public InputField NewEmailUI;

    LoginPacket loginPacket;

    public ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ConnectedToServer();
    }

    private void RecvPacket()
    {
        while(true)
        {
            byte[] lengthBuffer = new byte[2];

            int RecvLength = serverSocket.Receive(lengthBuffer, 2, SocketFlags.None);
            ushort length = BitConverter.ToUInt16(lengthBuffer, 0);
            length = (ushort)IPAddress.NetworkToHostOrder((short)length);
            byte[] recvBuffer = new byte[4096];
            RecvLength = serverSocket.Receive(recvBuffer, length, SocketFlags.None);

            string jsonString = Encoding.UTF8.GetString(recvBuffer);
            Debug.Log(jsonString);
            Thread.Sleep(10);
            RecvPacket recvPacket = JsonUtility.FromJson<RecvPacket>(jsonString);
            if(recvPacket.message.Equals("success")) //로그인 성공하면
            {
                if(recvPacket.code.Equals("loginresult"))
                {
                    playerData.user_id = idUI.text;
                    playerData.user_name = recvPacket.name;
                    playerData.user_email = recvPacket.email;
                    messageQueue.Enqueue(recvPacket.message);
                }
                else if(recvPacket.code.Equals("signinresult"))
                {
                    playerData.user_id = NewidUI.text;
                    playerData.user_name = NewNameUI.text;
                    playerData.user_email = NewEmailUI.text;
                }                
            }
            
        }
    }

    void ConnectedToServer()
    {
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverEndPoint = new IPEndPoint(IPAddress.Loopback, 4000);
        serverSocket.Connect(serverEndPoint);
        recvThread = new Thread(new ThreadStart(RecvPacket));
        recvThread.IsBackground = true; // 추가
        recvThread.Start();
    }

    void SendPacket(string message)
    {
        byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
        ushort length = (ushort)IPAddress.HostToNetworkOrder((short)messageBuffer.Length);

        byte[] headerBuffer = BitConverter.GetBytes(length);

        byte[] packetBuffer = new byte[headerBuffer.Length + messageBuffer.Length];
        Buffer.BlockCopy(headerBuffer, 0, packetBuffer, 0, headerBuffer.Length);
        Buffer.BlockCopy(messageBuffer, 0, packetBuffer, headerBuffer.Length, messageBuffer.Length);

        int SendLength = serverSocket.Send(packetBuffer, packetBuffer.Length, SocketFlags.None);
    }

    public void OnLogin()
    {
        loginPacket = new LoginPacket();
        loginPacket.code = "login";
        loginPacket.id = idUI.text;
        loginPacket.password = passwordUI.text;

        SendPacket(JsonUtility.ToJson(loginPacket));
    }

    public void OnSignInBtnClicked()
    {
        LogInPanel.SetActive(false);
        SignInPanel.SetActive(true);
    }
    public void OnSignIn()
    {
        SignInPacket packet = new SignInPacket();
        packet.code = "signin";
        packet.id = NewidUI.text;
        packet.password = NewpasswordUI.text;
        packet.name = NewNameUI.text;
        packet.email = NewEmailUI.text;

        SendPacket(JsonUtility.ToJson(packet));

        LogInPanel.SetActive(true);
        SignInPanel.SetActive(false);
    }

    public void OnApplicationQuit()
    {
        if(recvThread != null)
        {
            recvThread.Abort();
        }
        if (serverSocket != null)
        {
            serverSocket.Shutdown(SocketShutdown.Both); //저나 끊을게요 하고 던져주고
            serverSocket.Close(); //끊기
        }
    }
}

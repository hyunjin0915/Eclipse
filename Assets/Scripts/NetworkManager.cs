using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 서버와 주고받을 패킷 기본 구조
/// </summary>
[Serializable]
public class Packet
{
    public string code;  // 요청/응답 타입 구분 (예: "login", "signin", "loginresult" 등)
    public string id;    // 사용자 ID
}

[Serializable]
public class LoginPacket : Packet
{
    public string password;  // 로그인용 비밀번호
}

[Serializable]
public class SignInPacket : Packet
{
    public string password;  // 회원가입용 비밀번호
    public string name;      // 회원가입용 이름
    public string email;     // 회원가입용 이메일
}

[Serializable]
public class RecvPacket
{
    public string code;     // 응답 타입 (예: "loginresult", "signinresult")
    public string message;  // 성공/실패 메시지
    public string name;     // 서버가 보내주는 사용자 이름
    public string email;    // 서버가 보내주는 이메일
}

/// <summary>
/// Unity에서 싱글톤으로 동작하는 네트워크 매니저
/// TCP 소켓을 열어 로그인/회원가입 요청을 보내고, 
/// 서버 응답을 별도 스레드에서 받아 Unity 메인 스레드로 전달한다.
/// </summary>
public class NetworkManager : Singleton<NetworkManager>
{
    // 로그인/회원가입 UI 패널
    public GameObject LogInPanel;
    public GameObject SignInPanel;
    // 로그인 성공 시 저장할 플레이어 데이터
    public PlayerDataScriptableObject playerData;

    private Socket serverSocket;          // TCP 소켓 객체
    private IPEndPoint serverEndPoint;    // 서버 IP+포트 정보
    private Thread recvThread;            // 서버 응답 수신 전용 스레드

    // UI에서 입력한 ID/패스워드 필드
    public InputField idUI;
    public InputField passwordUI;
    // 회원가입 UI 필드
    public InputField NewidUI;
    public InputField NewpasswordUI;
    public InputField NewNameUI;
    public InputField NewEmailUI;

    private LoginPacket loginPacket;      // 로그인용 패킷 인스턴스

    // 스레드에서 수신한 JSON 문자열을 메인 스레드로 넘기는 큐
    public ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

    // Unity가 시작될 때 한 번 호출됨
    void Start()
    {
        ConnectedToServer();  // 서버에 연결 및 응답 스레드 시작
    }

    /// <summary>
    /// 별도 스레드에서 서버로부터 응답을 지속해서 받는 루프
    /// </summary>
    private void RecvPacket()
    {
        while (true)
        {
            // 1) 앞서 보낸 메시지 길이(2바이트) 먼저 읽음
            byte[] lengthBuffer = new byte[2];
            int RecvLength = serverSocket.Receive(lengthBuffer, 2, SocketFlags.None);
            // 네트워크 바이트 오더(Big-Endian) -> 호스트 바이트 오더로 변환
            ushort length = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(lengthBuffer, 0));

            // 2) 실제 JSON 메시지 읽기
            byte[] recvBuffer = new byte[4096];
            RecvLength = serverSocket.Receive(recvBuffer, length, SocketFlags.None);

            // 3) 바이트 -> UTF8 문자열 변환
            string jsonString = Encoding.UTF8.GetString(recvBuffer, 0, RecvLength);
            Debug.Log($"[RecvPacket] JSON: {jsonString}");

            // 4) JSON -> RecvPacket 객체로 디시리얼라이즈
            RecvPacket recvPacket = JsonUtility.FromJson<RecvPacket>(jsonString);

            // 5) 서버 응답 처리: 로그인/회원가입 성공 시 저장
            if (recvPacket.message.Equals("success"))
            {
                if (recvPacket.code.Equals("loginresult"))
                {
                    // 로그인 성공 시 PlayerDataScriptableObject에 ID/이름/이메일 저장
                    playerData.user_id = idUI.text;
                    playerData.user_name = recvPacket.name;
                    playerData.user_email = recvPacket.email;
                    // 메인 스레드에서 처리할 수 있게 큐에 메시지 푸시
                    messageQueue.Enqueue(recvPacket.message);
                }
                else if (recvPacket.code.Equals("signinresult"))
                {
                    // 회원가입 성공 시에도 PlayerData 업데이트
                    playerData.user_id = NewidUI.text;
                    playerData.user_name = NewNameUI.text;
                    playerData.user_email = NewEmailUI.text;
                }
            }

            Thread.Sleep(10);  // CPU 과도 점유 방지
        }
    }

    /// <summary>
    /// 서버에 TCP 소켓 연결 설정하고 수신 스레드 실행
    /// </summary>
    void ConnectedToServer()
    {
        // 1) 소켓 생성 (IPv4, TCP 스트림)
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // 2) 서버 주소(IP랑 포트) 지정 (로컬호스트:4000)
        serverEndPoint = new IPEndPoint(IPAddress.Loopback, 4000);
        // 3) Connect 호출 -> 블로킹으로 연결 성립 대기
        serverSocket.Connect(serverEndPoint);

        // 4) 서버 응답 받을 전용 백그라운드 스레드 생성 및 시작
        recvThread = new Thread(new ThreadStart(RecvPacket));
        recvThread.IsBackground = true;
        recvThread.Start();
    }

    /// <summary>
    /// JSON 메시지를 TCP 소켓으로 전송
    /// (2바이트 길이 헤더 + UTF8 바이트 메시지)
    /// </summary>
    void SendPacket(string message)
    {
        // 1) 문자열 -> UTF8 바이트
        byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
        // 2) 길이(ushort)를 네트워크 바이트 오더로 변환
        ushort length = (ushort)IPAddress.HostToNetworkOrder((short)messageBuffer.Length);
        byte[] headerBuffer = BitConverter.GetBytes(length);
        // 3) 헤더+메시지 합쳐서 전송
        byte[] packetBuffer = new byte[headerBuffer.Length + messageBuffer.Length];
        Buffer.BlockCopy(headerBuffer, 0, packetBuffer, 0, headerBuffer.Length);
        Buffer.BlockCopy(messageBuffer, 0, packetBuffer, headerBuffer.Length, messageBuffer.Length);

        // 4) 실제 소켓 전송
        int SendLength = serverSocket.Send(packetBuffer, packetBuffer.Length, SocketFlags.None);
        Debug.Log($"[SendPacket] Sent {SendLength} bytes: {message}");
    }

    /// <summary>
    /// 로그인 버튼 눌렀을 때 호출
    /// UI 입력값으로 LoginPacket 구성 후 전송
    /// </summary>
    public void OnLogin()
    {
        loginPacket = new LoginPacket
        {
            code = "login",
            id = idUI.text,
            password = passwordUI.text
        };
        SendPacket(JsonUtility.ToJson(loginPacket));
    }

    /// <summary>
    /// 회원가입 패널 열기
    /// </summary>
    public void OnSignInBtnClicked()
    {
        LogInPanel.SetActive(false);
        SignInPanel.SetActive(true);
    }

    /// <summary>
    /// 회원가입 진행 후 서버에 SignInPacket 전송
    /// </summary>
    public void OnSignIn()
    {
        SignInPacket packet = new SignInPacket
        {
            code = "signin",
            id = NewidUI.text,
            password = NewpasswordUI.text,
            name = NewNameUI.text,
            email = NewEmailUI.text
        };
        SendPacket(JsonUtility.ToJson(packet));

        // UI 다시 로그인 화면으로 전환
        LogInPanel.SetActive(true);
        SignInPanel.SetActive(false);
    }

    /// <summary>
    /// 애플리케이션 종료 시 소켓과 스레드 깔끔히 정리
    /// </summary>
    public void OnApplicationQuit()
    {
        if (recvThread != null)
            recvThread.Abort();  // 수신 스레드 중단

        if (serverSocket != null)
        {
            // 양방향 소켓 닫기
            serverSocket.Shutdown(SocketShutdown.Both);
            serverSocket.Close();
        }
    }
}

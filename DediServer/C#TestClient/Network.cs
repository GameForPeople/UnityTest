using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Runtime.InteropServices;

using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


enum PROTOCOL:int
{
    DEMAND_LOGIN = 100
      , FAIL_LOGIN = 101
      , PERMIT_LOGIN = 102
      , DEMAND_GAMESTATE = 400
};

public class Network : MonoBehaviour {

    // State object for receiving data from remote device.  
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    //[StructLayout(LayoutKind.Sequential)]
    //public class socket_data
    //{
    //    public int msg;
    //
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)] public char[] data;
    //}

    [StructLayout(LayoutKind.Sequential)]
    public class DemandLoginStruct
    {
        public int msg;
        public int type;
        public int pw;
        public int IDSize;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)] public char[] data;
    }

    private const string iP_ADDRESS = "14.53.250.117";
    private const int SERVER_PORT = 9000;

    public Thread thread;
    public Socket socket;

    public bool isRecvOn = false;
    public int recvType = 0;
    public int sendType = 0;

    public int winCount = 0;
    public int loseCount = 0;
    public int money = 0;

    public byte[] byteToRecvTypeBuffer = new byte[100];
    public byte[] recvTypeBuffer = new byte[100];

    public object _obj = new object();

    public GameObject m_scenenManager;

    //private int dataLength;                     // Send Data Length. (byte)
    //private byte[] sendBuffer;                        // Data encoding to send. ( to Change bytes)
    //private byte[] Receivebyte = new byte[2000];    // Receive data by this array to save.
    //private string ReceiveString;                     // Receive bytes to Change string.

    ~Network()
    {
        socket.Close();
        socket = null;
    }

    // Use this for initialization
    void Start() {
        // Socket Create
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 10000);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);

        // Socket connect.
        try
        {
            IPAddress ipAddr = System.Net.IPAddress.Parse(iP_ADDRESS);
            IPEndPoint ipEndPoint = new System.Net.IPEndPoint(ipAddr, SERVER_PORT);
            socket.Connect(ipEndPoint); // 야 솔직히 커넥트는 비동기로 안한다. 이거는 c++서버도 안했다

        }
        catch (SocketException SCE)
        {
            Debug.Log("Socket connect error! : " + SCE.ToString());
            return;
        }

        //SendData(100);
        m_scenenManager = GameObject.Find("SceneManager");
    }
    // Update is called once per frame
    void Update() {
        if (sendType > 0)
        {
            SendData(sendType);
        }
        else
        {
            // 나중에 여기에 else말고 else if로 바꾸고, 현제 씬이 인게임씬인지를 확인하는 코드가 추가되어야함
            SendData((int)PROTOCOL.DEMAND_GAMESTATE);
        }

        if (isRecvOn)
        {
            RecvType(socket);
            ProcessRecvData();
            //if (recvType > 0)
            //{
            //    RecvData(socket);
            //}
            //else
            //{
            //    RecvType(socket);
            //}

            isRecvOn = false;
        }
    }

    //[ComVisibleAttribute(true)]
    public void StructToBytes(object obj, ref byte[] packet)
    {
        int size = Marshal.SizeOf(obj);
        packet = new byte[size];
        IntPtr buffer = Marshal.AllocHGlobal(size + 1);
        Marshal.StructureToPtr(obj, buffer, false);
        Marshal.Copy(buffer, packet, 0, size);
        Marshal.FreeHGlobal(buffer);
    }

    public void BytesToStructure(byte[] bValue, ref object obj, Type t)
    {
        int size = Marshal.SizeOf(t);
        IntPtr buffer = Marshal.AllocHGlobal(size);
        Marshal.Copy(bValue, 0, buffer, size);
        obj = Marshal.PtrToStructure(buffer, t);
        Marshal.FreeHGlobal(buffer);
    }

    public void SendData(int InMsg)
    {
        if (InMsg == (int)PROTOCOL.DEMAND_GAMESTATE)
        {
           byte[] sendDamandGameState = BitConverter.GetBytes((int)400);
            socket.Send(sendDamandGameState);

            isRecvOn = true;
        }
        else if (InMsg == (int)PROTOCOL.DEMAND_LOGIN)
        {
            DemandLoginStruct demandLogin = new DemandLoginStruct();
            demandLogin.msg = InMsg;
            demandLogin.type = 1;
            demandLogin.pw = 1234;

            string IDBuffer = "abcd";
            int lengthBuffer = IDBuffer.Length;

            demandLogin.data = new char[30];

            Debug.Log("lengthBuffer = " + lengthBuffer);

            for (int i = 0; i < lengthBuffer; i++)
                demandLogin.data[i] = IDBuffer[i];

            demandLogin.IDSize = lengthBuffer;

            byte[] packet = new byte[1];
            StructToBytes(demandLogin, ref packet);
            socket.Send(packet);

            isRecvOn = true;
        }
    }

    private void RecvType(Socket client)
    {
        client.Receive(byteToRecvTypeBuffer);

        recvType = BitConverter.ToInt32(byteToRecvTypeBuffer, 0);
        Debug.Log("RecvType is : " + recvType);
    }

    public void ProcessRecvData()
    {
        if(recvType > (int)PROTOCOL.DEMAND_GAMESTATE)
        {
            //if(recvType == 401)
            //{
                m_scenenManager.SendMessage("ProcessRecvData", byteToRecvTypeBuffer);
            //}
        }
        else if(recvType == (int)PROTOCOL.FAIL_LOGIN)
        {
        }
        else if (recvType == (int)PROTOCOL.PERMIT_LOGIN)
        {
            winCount = BitConverter.ToInt32(byteToRecvTypeBuffer, 4);
            Debug.Log("win is --> " + recvType);

            loseCount = BitConverter.ToInt32(byteToRecvTypeBuffer, 8);
            Debug.Log("lose is --> " + recvType);

            money = BitConverter.ToInt32(byteToRecvTypeBuffer, 12);
            Debug.Log("money is --> " + recvType);
        }
        

        recvType = 0;
    }
}



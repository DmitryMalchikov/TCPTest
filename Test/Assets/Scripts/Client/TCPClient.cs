using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UniRx;
using UnityEngine;

public class TCPClient : Singleton<TCPClient>
{
    event Action<string> OnServerResponse;
    public int Port = 13000;
    public string IP = "127.0.0.1";

    TcpClient client;
    NetworkStream stream;
    StreamReader reader;
    StreamWriter writer;

    Queue<string> serverMessages;

    private void Start()
    {
        serverMessages = new Queue<string>();
        ConnectToServer();
        Observable.EveryUpdate().Subscribe(_ => GetMessageFromServer());
    }

    private void ConnectToServer()
    {
        client = new TcpClient(IP, Port);
        stream = client.GetStream();
        writer = new StreamWriter(stream);
        reader = new StreamReader(stream, true);
    }

    private void SendMessageToServer(string message)
    {
        try
        {
            writer.WriteLine(message);
            writer.Flush();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    private void GetMessageFromServer()
    {
        if (stream.DataAvailable)
        {
            var message = reader.ReadLine();
            serverMessages.Enqueue(message);
            //OnIncomingMessage(message);
            Debug.Log(message);
        }
    }

    private void OnIncomingMessage()
    {
        if (serverMessages.Count > 0)
        {
            string message = serverMessages.Dequeue();
            Reflector.InvokeMethod(GameManagerClient.Instance, message);
        }
    }

    public void SendHello()
    {
        Observable.Start(() => SendMessageToServer("Hello"))
        .ObserveOnMainThread()
        .Subscribe(x => Debug.Log("Sent"));
    }

    public void SendSas()
    {
        CallServerMethod("SwitchLight", 0);
    }

    public void CallServerMethod(string methodName, params object[] parameters)
    {
        MethodCall mc = new MethodCall()
        {
            MethodName = methodName,
            Parameters = parameters
        };

        string message = JsonConvert.SerializeObject(mc);

        Observable.Start(() => SendMessageToServer(message))
        .ObserveOnMainThread()
        .Subscribe(x => Debug.Log(message));
    }

    private void Update()
    {
        OnIncomingMessage();
    }

    private void OnApplicationQuit()
    {
        client.Close();
    }
}

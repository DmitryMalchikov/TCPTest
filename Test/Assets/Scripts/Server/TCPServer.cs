using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UniRx;
using UnityEngine;

public class TCPServer : Singleton<TCPServer>
{
    public Int32 Port = 13000;    

    TcpListener server;
    List<TcpClient> clients;
    List<ServerClient> serverClients;
    List<NetworkObject> networkObjects;
    Queue<int> newClients;
    Queue<int> disconnectedClients;
    Queue<ClientMessage> currentMessages;
    bool serverOnline;

    private void Start()
    {
        serverOnline = false;
        clients = new List<TcpClient>();
        serverClients = new List<ServerClient>();
        currentMessages = new Queue<ClientMessage>();
        networkObjects = new List<NetworkObject>();
        newClients = new Queue<int>();
        disconnectedClients = new Queue<int>();

        Observable.Start(() => StartServer())
                .ObserveOnMainThread()
                .Subscribe(x => Debug.Log("Ser"));

        Observable.EveryUpdate().Subscribe(_ => GetClientMessage());
    }

    void StartServer()
    {
        Debug.Log("Start");
        try
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            server = new TcpListener(localAddr, Port);

            serverOnline = true;
            server.Start();
            
            while (serverOnline)
            {
                Debug.Log("Waiting for a connection... ");
                
                TcpClient client = server.AcceptTcpClient();

                OnClientConnected(client);
            }
        }
        catch (SocketException e)
        {
            Debug.LogFormat("SocketException: {0}", e);
        }
        finally
        {
            Debug.Log("Stop");
            server.Stop();
        }
    }

    private void GetClientMessage()
    {
        for (int i = 0; i < serverClients.Count; i++)
        {
            if (!IsClientConnected(serverClients[i].Client))
            {
                OnClientDisconnected(serverClients[i]);
                continue;
            }
            else
            {
                NetworkStream stream = serverClients[i].Client.GetStream();

                if (stream.DataAvailable)
                {
                    StreamReader reader = new StreamReader(stream, true);
                    string message = reader.ReadLine();

                    OnIncomingMessage(serverClients[i].Id, message);
                }
            }
        }
    }

    private void SendMessageToClient(int clientId, string message)
    {
        ServerClient cl = serverClients.Find(c => c.Id == clientId);

        if (cl == null)
        {
            return;
        }

        NetworkStream stream = cl.Client.GetStream();

        StreamWriter writer = new StreamWriter(stream);
        writer.WriteLine(message);
        writer.Flush();

        Debug.Log("Message sent");
    }

    private void SendMessageToAll(string message)
    {
        for (int i = 0; i < serverClients.Count; i++)
        {
            SendMessageToClient(serverClients[i].Id, message);
        }
    }

    private bool IsClientConnected(TcpClient client)
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void OnIncomingMessage(int clientId, string message)
    {
        currentMessages.Enqueue(new ClientMessage(clientId, message));
    }

    private void OnClientConnected(TcpClient client)
    {
        Debug.Log("Client conected!");

        ServerClient newClient = new ServerClient(client);

        serverClients.Add(newClient);
        newClients.Enqueue(newClient.Id);
    }

    public void ClientConnected(int id)
    {
        GameObject newClient = new GameObject("Client" + id);
        NetworkObject no = newClient.AddComponent<NetworkObject>();
        no.NetworkId = id;
        networkObjects.Add(no);
        no.OnClientConnected();
    }

    public void ClientDisconnected(int id)
    {
        NetworkObject no = networkObjects.Find(n => n.NetworkId == id);
        if (no != null)
        {
            Destroy(no.gameObject);
            networkObjects.Remove(no);
        }
    }

    private void OnClientDisconnected(ServerClient client)
    {
        disconnectedClients.Enqueue(client.Id);
        client.Client.Close();
        serverClients.Remove(client);
        Debug.Log("Client disconnected!");
    }

    private void CallMessages()
    {
        if (currentMessages.Count > 0)
        {
            var message = currentMessages.Dequeue();
            NetworkObject no = networkObjects.Find(n => n.NetworkId == message.Id);
            if (no != null)
            {
                Reflector.InvokeMethod(no, message.Message);
            }
        }
    }

    public void CallAllClientsMethod(string methodName, params object[] parameters)
    {
        MethodCall mc = new MethodCall()
        {
            MethodName = methodName,
            Parameters = parameters
        };

        string message = JsonConvert.SerializeObject(mc);

        Observable.Start(() => SendMessageToAll(message))
        .ObserveOnMainThread()
        .Subscribe(x => Debug.Log("Sent"));
    }

    public void CallClientMethod(string methodName, int clientId, params object[] parameters)
    {
        MethodCall mc = new MethodCall()
        {
            MethodName = methodName,
            Parameters = parameters
        };

        string message = JsonConvert.SerializeObject(mc);

        Observable.Start(() => SendMessageToClient(clientId, message))
        .ObserveOnMainThread()
        .Subscribe(x => Debug.Log(message));
    }

    private void CheckClients()
    {
        if (newClients.Count > 0)
        {
            ClientConnected(newClients.Dequeue());
            Debug.Log(newClients.Count);
        }

        if (disconnectedClients.Count > 0)
        {
            ClientDisconnected(disconnectedClients.Dequeue());
            Debug.Log(disconnectedClients.Count);
        }
    }

    private void Update()
    {
        CallMessages();

        CheckClients();
    }
}

public class ServerClient
{
    static int curId;

    public int Id;
    public TcpClient Client;

    public ServerClient(TcpClient client)
    {
        Id = curId++;
        Client = client;
    }
}

public class ClientMessage
{
    public int Id;
    public string Message;

    public ClientMessage(int id, string message)
    {
        Id = id;
        Message = message;
    }
}

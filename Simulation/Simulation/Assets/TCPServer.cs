using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    public int port = 6321;
    private TcpListener server;
    private bool serverStarted;

    private List<ServerClient> clients;
    private List<ServerClient> disconnected;

    private void Start()
    {
        clients = new List<ServerClient>();
        disconnected = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
            Debug.Log("Sever started");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            throw;
        }
    }

    private void Update()
    {
        if (!serverStarted)
        {
            return;
        }

        foreach (ServerClient c in clients)
        {
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnected.Add(c);
                continue;
            }
            else
            {
                NetworkStream networkStream = c.tcp.GetStream();
                if (networkStream.DataAvailable)
                {
                    StreamReader reader = new StreamReader(networkStream, true);
                    String data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient , server);
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
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

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();
        
        Broadcast(clients[clients.Count - 1].name + " welcome" , clients );
        Debug.Log(clients[clients.Count - 1].name + " welcome");
    }

    private void OnIncomingData(ServerClient c, String data)
    {
        Debug.Log(c.name +  ": " + data);
    }

    private void Broadcast(String data, List<ServerClient> c1)
    {
        foreach (ServerClient c in c1)
        {
            try
            {
                StreamWriter  writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message + "to client " + c.name);
                throw;
            } 
        }
    }
}

public class ServerClient
{
    public TcpClient tcp;
    public String name;

    public ServerClient(TcpClient clientSocket)
    {
        name = "Guest";
        tcp = clientSocket;
    }
}

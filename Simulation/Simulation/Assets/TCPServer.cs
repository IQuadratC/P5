using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Utility;

public class TCPServer : MonoBehaviour
{
    public int port = 6321;
    private TcpListener server;
    private bool serverStarted;

    private ServerClient client;

    private void Start()
    {

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

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient , server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;

        client = new ServerClient(listener.EndAcceptTcpClient(ar));
        

        Broadcast(client.name + " welcome");
        Debug.Log(client.name + " welcome");
        
        // Start reading the socket and receive any incoming messages
        client.tcp.GetStream().BeginRead(client.Bytes,
            0,
            client.Bytes.Length,
            MessageReceived,
            null);
    }
    
    private void MessageReceived(IAsyncResult ar)
    {
        if (!ar.IsCompleted) return;
        // End the stream read
        int bytesIn = client.tcp.GetStream().EndRead(ar);
        string str = "";
        if (bytesIn > 0)
        {
            // Create a string from the received data. For this server 
            // our data is in the form of a simple string, but it could be
            // binary data or a JSON object. Payload is your choice.
            byte[] tmp = new byte[bytesIn];
            Array.Copy(client.Bytes, 0, tmp, 0, bytesIn);
            str = Encoding.ASCII.GetString(tmp);
        }
        // Clear the buffer and start listening again
        Array.Clear(client.Bytes, 0, client.Bytes.Length);
        client.tcp.GetStream().BeginRead(client.Bytes,
            0,
            client.Bytes.Length,
            MessageReceived,
            null);
            
        void Action()
        {
            if (str == "")
            {
                client.tcp.Close();
                return;
            }
            Debug.Log(str);
        }
        Threader.RunOnMainThread(Action);
    }

    private void Broadcast(String data)
    {
        try
        {
            StreamWriter  writer = new StreamWriter(client.tcp.GetStream());
            writer.WriteLine(data);
            writer.Flush();
        }
        catch (Exception e)
        {
            Debug.Log("Write error: " + e.Message + "to client " + client.name);
            throw;
        }
    }
}

public class ServerClient
{
    public TcpClient tcp;
    public String name;

    public byte[] Bytes = new byte[4096];

    public ServerClient(TcpClient clientSocket)
    {
        name = "Guest";
        tcp = clientSocket;
    }
}

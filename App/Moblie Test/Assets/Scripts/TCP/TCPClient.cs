using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Utility.Events;

namespace UI
{
    public class TCPClient : MonoBehaviour
    {
        public GameEvent sendEvent;
        public string sendMessage;
        
        public GameEvent reciveEvent;
        public string reciveMessage;
        
        TcpClient client;
        byte[] bytes = new byte[4096];
        private void Awake()
        {
            client = new TcpClient();
        }

        public void Start()
        {
            // Connect to the remote server. The IP address and port # could be
            // picked up from a settings file.
            client.Connect("95.89.112.92", 54000);

            // Start reading the socket and receive any incoming messages
            client.GetStream().BeginRead(bytes,
                0,
                bytes.Length,
                MessageReceived,
                null);

            Send("Hallo");
        }

        private void MessageReceived(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                // End the stream read
                var bytesIn = client.GetStream().EndRead(ar);
                if (bytesIn > 0)
                {
                    // Create a string from the received data. For this server 
                    // our data is in the form of a simple string, but it could be
                    // binary data or a JSON object. Payload is your choice.
                    var tmp = new byte[bytesIn];
                    Array.Copy(bytes, 0, tmp, 0, bytesIn);
                    var str = Encoding.ASCII.GetString(tmp);
                    Debug.Log(str);
                }
                // Clear the buffer and start listening again
                Array.Clear(bytes, 0, bytes.Length);
                client.GetStream().BeginRead(bytes,
                    0,
                    bytes.Length,
                    MessageReceived,
                    null);
            }
        }

        private void Send(string text)
        {
            // Encode the message and send it out to the server.
            var msg = Encoding.ASCII.GetBytes(text);
            client.GetStream().Write(msg, 0, msg.Length);
        }
    }
}
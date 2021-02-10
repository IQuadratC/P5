using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Utility;
using Utility.Events;
using Utility.Variables;

namespace TCP
{
    public class TCPClient : MonoBehaviour
    {
        public StringVariable sendMessage;
        
        public GameEvent reciveEvent;
        public StringVariable reciveMessage;
        
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
            try
            {
                client.Connect("95.89.112.92", 54000);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to Connect to Server!" + e);
                throw;
            }
            
            // Start reading the socket and receive any incoming messages
            client.GetStream().BeginRead(bytes,
                0,
                bytes.Length,
                MessageReceived,
                null);
        }

        private void MessageReceived(IAsyncResult ar)
        {
            if (!ar.IsCompleted) return;
            // End the stream read
            int bytesIn = client.GetStream().EndRead(ar);
            if (bytesIn > 0)
            {
                // Create a string from the received data. For this server 
                // our data is in the form of a simple string, but it could be
                // binary data or a JSON object. Payload is your choice.
                byte[] tmp = new byte[bytesIn];
                Array.Copy(bytes, 0, tmp, 0, bytesIn);
                string str = Encoding.ASCII.GetString(tmp);
                    
                reciveMessage.Value = str;

                void Action()
                {
                    reciveEvent.Raise();
                    Debug.Log(str);
                }

                Threader.RunOnMainThread(Action);
                
                
            }
            // Clear the buffer and start listening again
            Array.Clear(bytes, 0, bytes.Length);
            client.GetStream().BeginRead(bytes,
                0,
                bytes.Length,
                MessageReceived,
                null);
        }

        public void Send()
        {
            // Encode the message and send it out to the server.
            byte[] msg = Encoding.UTF8.GetBytes(sendMessage.Value);
            client.GetStream().Write(msg, 0, msg.Length);
        }
    }
}
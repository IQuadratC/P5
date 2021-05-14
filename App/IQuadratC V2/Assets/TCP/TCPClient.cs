using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        [SerializeField] private StringVariable ip;
        
        TcpClient client;
        byte[] bytes = new byte[4096];
        
        public void OnEnable()
        {
            void process()
            {
                client = new TcpClient();
                // Connect to the remote server. The IP address and port # could be
                // picked up from a settings file.
                try
                {
                    client.Connect(ip.Value, 54000);
                }
                catch (Exception e)
                {
                    void run()
                    {
                        Debug.Log("Failed to Connect to Server! Error: " + e);
                    }
                    Threader.RunOnMainThread(run);
                    throw;
                }
            
                // Start reading the socket and receive any incoming messages
                client.GetStream().BeginRead(bytes,
                    0,
                    bytes.Length,
                    MessageReceived,
                    null);
            }
            
            Threader.RunAsync(process);
            
        }

        private void OnDisable()
        {
            client.Close();
        }
        
        private void MessageReceived(IAsyncResult ar)
        {
            if (!ar.IsCompleted) return;
            // End the stream read
            int bytesIn = client.GetStream().EndRead(ar);
            string str = "";
            if (bytesIn > 0)
            {
                // Create a string from the received data. For this server 
                // our data is in the form of a simple string, but it could be
                // binary data or a JSON object. Payload is your choice.
                byte[] tmp = new byte[bytesIn];
                Array.Copy(bytes, 0, tmp, 0, bytesIn);
                str = Encoding.ASCII.GetString(tmp);
            }
            // Clear the buffer and start listening again
            Array.Clear(bytes, 0, bytes.Length);
            client.GetStream().BeginRead(bytes,
                0,
                bytes.Length,
                MessageReceived,
                null);
            
            void Action()
            {
                Debug.Log(str);
                reciveMessage.Value = str;
                reciveEvent.Raise();
            }
            Threader.RunOnMainThread(Action);
        }

        public void Send()
        {
            Debug.Log(sendMessage.Value);
            // Encode the message and send it out to the server.
            byte[] msg = Encoding.UTF8.GetBytes(sendMessage.Value);
            client.GetStream().Write(msg, 0, msg.Length);
        }
    }
}
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient {
	private TcpClient socketConnection; 	
	private Thread clientReceiveThread;
	
	private string ip;
	private int port;
	
	public TCPClient(string ip, int port)
	{
		this.ip = ip;
		this.port = port;
		CreateClientThread();
	}

	private void CreateClientThread () {
		clientReceiveThread = new Thread (Connect); 			
		clientReceiveThread.IsBackground = true; 			
		clientReceiveThread.Start();  		
	}

	private void Connect() { 	
		
		if (socketConnection != null)
		{
			socketConnection.GetStream().Close();
			socketConnection.Close();
			socketConnection = null;
		}
		
		try { 			
			socketConnection = new TcpClient(ip, 5000);   
			ListenForData();
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	} 
	
	private void ListenForData() { 		
		try {
			Byte[] bytes = new Byte[1024];             
			while (true) {
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData); 						
						Debug.Log("server message received as: " + serverMessage); 					
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}  	
	
	public void SendMessageTCP(string message) {
		if (socketConnection == null) {             
			return;         
		}  		
		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {                 
				string clientMessage = message; 				
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Client sent his message - should be received by server");             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}
}
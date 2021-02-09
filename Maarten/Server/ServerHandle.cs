
using SharedFiles.Utility;
using UnityEngine;

public static class ServerHandle
{
    public static void GameEnterReqest(int fromClient, Packet packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();
        string password = packet.ReadString();

        if (GameManager.instance.password != "" && GameManager.instance.password != password)
        {
            Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} has entered a wrong password!");
            ServerSend.GameEnterRejected(fromClient, "Wrong Password!");
            return;
        }
        
        if (GameManager.instance.players.Count >= Server.MaxPlayers)
        {
            Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} has entered a wrong password!");
            ServerSend.GameEnterRejected(fromClient, "Server full");
            return;
        }
        
        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            ServerSend.GameEnterRejected(fromClient, "Your Id does not match the server Id!");
            return;
        }
        Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
        
        ServerSend.GameStateChange(fromClient);
        Server.clients[fromClient].EnterPlayer(username);
    }

    public static void TrooperTransformUpdate(int fromClient, Packet packet)
    {
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();
        Vector3 velocity = packet.ReadVector3();
        bool grounded = packet.ReadBool();

        Server.clients[fromClient].playerManager.trooper.UpdateTransform(position, rotation, velocity, grounded);
    }
    
    public static void TrooperGrappleUpdate(int fromClient, Packet packet)
    {
        bool isGrappling = packet.ReadBool();
        string objectId = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        float distanceFromGrapple = packet.ReadFloat();
        
        Server.clients[fromClient].playerManager.trooper.GrappleUpdate(objectId, isGrappling, position, distanceFromGrapple);
    }
}

using System.Collections;
using System.Collections.Generic;
using Player;
using SharedFiles.Utility;
using UnityEngine;

public static class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="toClient">The client to send the packet the packet to.</param>
    /// <param name="packet">The packet to send to the client.</param>
    private static void SendTcpData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="toClient">The client to send the packet the packet to.</param>
    /// <param name="packet">The packet to send to the client.</param>
    private static void SendUdpData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="packet">The packet to send.</param>
    private static void SendTcpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="exceptClient">The client to NOT send the data to.</param>
    /// <param name="packet">The packet to send.</param>
    private static void SendTcpDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="packet">The packet to send.</param>
    private static void SendUdpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="exceptClient">The client to NOT send the data to.</param>
    /// <param name="packet">The packet to send.</param>
    private static void SendUdpDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }

    #region Packets

    /// <summary>Sends a welcome message to the given client.</summary>
    public static void ServerConnection(int toClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.serverConnection))
        {
            packet.Write(toClient);

            SendTcpData(toClient, packet);
        }
    }
    public static void GameEnterRejected(int toClient, string message)
    {
        using (Packet packet = new Packet((int)ServerPackets.gameEnterRejected))
        {
            packet.Write(message);
            
            SendTcpData(toClient, packet);
        }
    }
    public static void GameStateChange(int toClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.gameState))
        {
            packet.Write((int) GameManager.instance.currentGameMode.gameModeType);
            packet.Write(GameManager.instance.currentLobbyPreFabName);
            
            SendTcpData(toClient, packet);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="toClient">The client that should spawn the player.</param>
    /// <param name="playerManager">The player to spawn.</param>
    public static void PlayerEnter(Player.PlayerManager playerManager)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerEnter))
        {
            packet.Write(playerManager.client.id);
            packet.Write(playerManager.username);

            SendTcpData(playerManager.client.id, packet);
        }
    }

    public static void PlayerLeave(Player.PlayerManager playerManager)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerLeave))
        {
            packet.Write(playerManager.client.id);

            SendTcpDataToAll(playerManager.client.id, packet);
        }
    }
    
    public static void PlayerState(Player.PlayerManager playerManager)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerState))
        {
            packet.Write(playerManager.client.id);
            packet.Write((int) playerManager.state);

            SendTcpDataToAll(packet);
        }
    }

    public static void TrooperTransformUpdate(Trooper trooper)
    {
        using (Packet packet = new Packet((int)ServerPackets.trooperTransformUpdate))
        {
            packet.Write(trooper.player.client.id);
            packet.Write(trooper.transform.position);
            packet.Write(trooper.transform.rotation);
            packet.Write(trooper.velocity);
            packet.Write(trooper.grounded);

            SendUdpDataToAll(trooper.player.client.id, packet);
        }
    }
    
    public static void TrooperGrappleUpdate(Trooper trooper)
    {
        using (Packet packet = new Packet((int)ServerPackets.trooperGrappleUpdate))
        {
            packet.Write(trooper.player.client.id);
            packet.Write(trooper.isGrappling);
            packet.Write(trooper.grappleObjectId);
            packet.Write(trooper.grapplePoint);
            packet.Write(trooper.distanceFromGrapple);
            
            SendUdpDataToAll(trooper.player.client.id, packet);
        }
    }
    #endregion
}

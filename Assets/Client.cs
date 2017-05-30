using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
    private Network network;
    private int identity;
    private int connection;

    private List<MessageEvent> messageEvents;

    void Start()
    {
        network = FindObjectOfType<Network>() as Network;
        identity = 0;
    }

    void Update()
    {
        if (!network.IsClient())
        {
            return;
        }

        int connection;
        byte[] buffer;
        int size;

        NetworkEventType messageType = network.ReceiveMessage(out connection, out buffer, out size);

        switch (messageType)
        {
            case NetworkEventType.ConnectEvent:
                this.connection = connection;
                identity = network.Connection;
                Debug.Log("Connection to host was confirmed.");
                break;
            case NetworkEventType.DataEvent:
                if (size >= 5)
                {
                    for (int index = 0; index < messageEvents.Count; index++)
                    {
                        if (messageEvents[index].messageType == buffer[0])
                        {
                            messageEvents[index].Event(connection, buffer, size);
                            break;
                        }
                    }
                }
                break;
        }
    }

    //Returns zero if instance isn't a connected client.
    public int Identity
    {
        get
        {
            return identity;
        }
    }

    public void SendTimestampedMessage(byte MessageType, byte[] Data, bool bUseReliableChannel)
    {
        NetworkWriter writer = new NetworkWriter();

        writer.Write(MessageType);
        writer.Write(NetworkTransport.GetNetworkTimestamp());

        for (int index = 0; index < Data.Length; index++)
        {
            writer.Write(Data[index]);
        }

        byte[] bytes = writer.ToArray();

        byte sendError;
        NetworkTransport.Send
        (
            network.Socket, connection, (bUseReliableChannel) ? network.ReliableChannel : network.UnreliableChannel,
            bytes, bytes.Length, out sendError
        );
    }
}

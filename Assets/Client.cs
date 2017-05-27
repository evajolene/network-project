using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    private Network network;
    private int identity;
    private int connection;

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

        int host;
        int connection;
        int channel;
        byte[] buffer = new byte[32];
        int size;
        byte error;

        NetworkEventType messageType = NetworkTransport.Receive
        (
            out host, out connection, out channel, buffer, buffer.Length, out size, out error
        );

        switch (messageType)
        {
            case NetworkEventType.ConnectEvent:
                this.connection = connection;
                identity = network.Connection;
                Debug.Log("Connection to host was confirmed.");
                break;
            case NetworkEventType.DataEvent:
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

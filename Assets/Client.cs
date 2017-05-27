using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    private Network network;
    private bool bIsConnectionConfirmed;
    private byte identity;
    private int connection;

    void Start()
    {
        Tester();
        network = FindObjectOfType<Network>() as Network;
        bIsConnectionConfirmed = false;
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
                bIsConnectionConfirmed = true;
                this.connection = connection;
                Debug.Log("Connection to host was confirmed.");
                break;
            case NetworkEventType.DataEvent:
                if (size == 1)
                {
                    Debug.Log("Got identity from host: " + buffer[0]);
                    identity = buffer[0];
                }
                break;
        }
    }

    //Returns zero if instance isn't a connected client.
    public byte Identity
    {
        get
        {
            return (bIsConnectionConfirmed) ? identity : (byte)0;
        }
    }

    public void Tester()
    {
        NetworkWriter writer = new NetworkWriter();
        writer.Write((short)1000);
        byte[] bytes = writer.ToArray();
        Debug.Log(bytes.Length);
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

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Host : MonoBehaviour
{
    private Network network;
    private List<byte> connections;
    private float accumulatedTime;

    private List<MessageEvent> messageEvents;

    void Start()
    {
        network = FindObjectOfType<Network>() as Network;
        connections = new List<byte>();
        messageEvents = new List<MessageEvent>();
    }

    public MessageEvent GetMessageListener(byte MessageType)
    {
        for (int index = 0; index < messageEvents.Count; index++)
        {
            if (messageEvents[index].messageType == MessageType)
            {
                return messageEvents[index];
            }
        }

        MessageEvent newListener = new MessageEvent(MessageType);
        messageEvents.Add(newListener);
        return newListener;
    }

    public bool IsHosting
    {
        get
        {
            return network.IsHost();
        }
    }

    void Update()
    {
        if (!network.IsHost())
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        accumulatedTime += deltaTime;

        //Listen ~75 ticks a second.
        if (accumulatedTime < 0.01334f)
        {
            return;
        }

        accumulatedTime -= 0.01334f;

        int pollCount = 3;

        while (pollCount > 0)
        {
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
                case NetworkEventType.Nothing:
                    pollCount = 0;
                    break;
                case NetworkEventType.ConnectEvent:
                    byte connectionIndex = (byte)connection;
                    connections.Add(connectionIndex);
                    byte sendError;
                    NetworkTransport.Send(host, connection, network.ReliableChannel, new byte[]{ connectionIndex }, 1, out sendError);
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

            pollCount--;
        }
    }

    public void SendTimestampedMessage(int ClientIdentity, byte MessageType, int Timestamp, byte[] Data, bool bUseReliableChannel)
    {
        NetworkWriter writer = new NetworkWriter();

        writer.Write(MessageType);
        writer.Write(Timestamp);

        for (int index = 0; index < Data.Length; index++)
        {
            writer.Write(Data[index]);
        }

        byte[] bytes = writer.ToArray();

        byte sendError;
        NetworkTransport.Send
        (
            network.Socket, ClientIdentity, (bUseReliableChannel) ? network.ReliableChannel : network.UnreliableChannel,
            bytes, bytes.Length, out sendError
        );
    }
}

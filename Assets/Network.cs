using UnityEngine;
using UnityEngine.Networking;

public class Network : MonoBehaviour
{
    private int socket;
    private bool bIsClient;
    private int connection;
    private byte reliableChannel, unreliableChannel;

    void Start()
    {
        bIsClient = false;
        socket = -1;
        connection = 0;

        NetworkTransport.Init();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Host();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            Join();
        }
    }

    public void Host()
    {
        if (socket >= 0)
        {
            return;
        }

        ConnectionConfig settings = new ConnectionConfig();
        reliableChannel = settings.AddChannel(QosType.Reliable);
        unreliableChannel = settings.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(settings, 16);
        socket = NetworkTransport.AddHostWithSimulator(topology, 10, 150, 12345);

        bIsClient = false;
    }

    public void Join()
    {
        if (socket >= 0)
        {
            return;
        }

        ConnectionConfig settings = new ConnectionConfig();
        reliableChannel = settings.AddChannel(QosType.Reliable);
        unreliableChannel = settings.AddChannel(QosType.Unreliable);

        HostTopology topology = new HostTopology(settings, 2);
        socket = NetworkTransport.AddHostWithSimulator(topology, 10, 150);

        byte connectionError;
        ConnectionSimulatorConfig simulator = new ConnectionSimulatorConfig(25, 33, 42, 55, 0.5f);
        connection = NetworkTransport.ConnectWithSimulator(socket, "127.0.0.1", 12345, 0, out connectionError, simulator);
        if (connection > 0 && connectionError == 0)
        {
            Debug.Log("Connected as: " + connection);
            bIsClient = true;
        }
        else
        {
            Debug.Log("Error joining: " + (NetworkError)connectionError);
            NetworkTransport.RemoveHost(socket);
            socket = -1;
        }
    }

    public int Socket
    {
        get
        {
            return socket;
        }
    }

    //Client's identifier.
    public int Connection
    {
        get
        {
            return connection;
        }
    }

    public bool IsHost()
    {
        return socket >= 0 && !bIsClient;
    }

    public bool IsClient()
    {
        return socket >= 0 && bIsClient;
    }

    public byte ReliableChannel
    {
        get
        {
            return reliableChannel;
        }
    }

    public byte UnreliableChannel
    {
        get
        {
            return unreliableChannel;
        }
    }
}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerPhysics))]
public class Player : MonoBehaviour
{
    private Host host;
    private Client client;
    private float accumulatedTime;
    private List<byte> packedDirections;

    private List<PackedMovement> packedInputs;
    private PlayerPhysics playerPhysics;

    private class PackedMovement
    {
        public Vector3 position;
        public byte[] directions;
        public int time;
    };

    private bool bHasHostStarted = false;
    void Start()
    {
        host = FindObjectOfType<Host>() as Host;
        client = FindObjectOfType<Client>() as Client;
        packedDirections = new List<byte>();
        packedInputs = new List<PackedMovement>();
        playerPhysics = GetComponent<PlayerPhysics>();
    }

    void FixedUpdate()
    {
        if (client.Identity != 1)
        {
            return;
        }
    }

    private void ClientMoved(object Sender, Host.MessageEventArgs Arguments)
    {
        int size = Arguments.size;

        NetworkReader reader = new NetworkReader(Arguments.buffer);
        reader.ReadByte();

        PackedMovement movement = new PackedMovement();
        movement.time = reader.ReadInt32();

        int directionIndex = 5;
        movement.position = new Vector3(-99999, -99999, -99999);
        if (size > 8)
        {
            movement.position.x = reader.ReadSingle();
            movement.position.y = reader.ReadSingle();
            movement.position.z = reader.ReadSingle();

            directionIndex += 12;
        }

        if (size == directionIndex + 1)
        {
            movement.directions = new byte[]
            {
                Arguments.buffer[directionIndex], Arguments.buffer[directionIndex], Arguments.buffer[directionIndex]
            };
        }
        else if (size == directionIndex + 3)
        {
            movement.directions = new byte[]
            {
                Arguments.buffer[directionIndex],
                Arguments.buffer[directionIndex + 1],
                Arguments.buffer[directionIndex + 2]
            };
        }

        packedInputs.Add(movement);
    }
    int count = 0;
    void Update()
    {
        if (client.Identity != 1)
        {
            if (host.IsHosting)
            {
                if (!bHasHostStarted)
                {
                    Host.MessageEvent messageEvent = host.GetMessageListener(0);
                    messageEvent.eventHandler += ClientMoved;

                    bHasHostStarted = true;
                    return;
                }

                if (packedInputs.Count > 0)
                {
                    Vector3 position = transform.position;

                    packedInputs.Sort((a, b) => a.time.CompareTo(b.time));

                    for (int input = 0; input < packedInputs.Count; input++)
                    {
                        for (int directionIndex = 0; directionIndex < packedInputs[input].directions.Length; directionIndex++)
                        {
                            byte packedDirection = packedInputs[input].directions[directionIndex];
                            position += playerPhysics.SimulateStep(position, packedDirection);
                        }

                        if (packedInputs[input].position.x > -9999)
                        {
                            if ((position - packedInputs[input].position).magnitude > 0.0015f)
                            {
                                //Debug.Log((position - packedInputs[input].position).magnitude);
                                /*
                                NetworkWriter writer = new NetworkWriter();
                                writer.Write((byte)0);
                                writer.Write(packedInputs[input].position);
                                writer.Write(position.x);
                                writer.Write(position.y);
                                writer.Write(position.z);*/
                            }
                        }
                    }

                    transform.position = position;

                    packedInputs.Clear();
                }
            }
            return;
        }

        float deltaTime = Time.deltaTime;
        accumulatedTime += deltaTime;

        //Player locally updates ~80 times per second.
        if (accumulatedTime >= 0.0125f)
        {
            Vector3 direction = Vector3.zero;
            byte packedDirection = 0;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                packedDirection |= 1;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                packedDirection |= 4;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                packedDirection |= 2;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                packedDirection |= 8;
            }

            transform.position += playerPhysics.SimulateStep(transform.position, packedDirection);
            accumulatedTime -= 0.0125f;

            packedDirections.Add(packedDirection);

            //Sends message ~26.66 times (80/3) per second.
            if (packedDirections.Count % 3 == 0)
            {
                NetworkWriter writer = new NetworkWriter();

                if (packedDirections.Count % 6 == 0)
                {
                    writer.Write(transform.position.x);
                    writer.Write(transform.position.y);
                    writer.Write(transform.position.z);
                }

                int index = packedDirections.Count - 3;
                if (packedDirections[index] == packedDirections[index + 1] && packedDirections[index] == packedDirections[index + 2])
                {
                    writer.Write(packedDirections[index]);
                }
                else
                {
                    writer.Write(packedDirections[index]);
                    writer.Write(packedDirections[index + 1]);
                    writer.Write(packedDirections[index + 2]);
                }

                client.SendTimestampedMessage(0, writer.ToArray(), false);
            }
        }
    }
}
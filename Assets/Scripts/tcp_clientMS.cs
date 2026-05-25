using System;
using System.Net.Sockets;
using UnityEngine;

public class tcp_clientMS : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;

    // Public so your other scripts (e.g. your agent) can read the latest action/reward
    public float[] action;
    public float reward;

    // Reusable 4-byte buffer for reading lengths
    private readonly byte[] _lenBuffer = new byte[4];

    void Start()
    {
        // Connect and grab the stream
        client = new TcpClient("127.0.0.1", 12345);
        stream = client.GetStream();

        // Initialize your action vector & reward
        action = new float[3];  // e.g. [forward/back, left/right, rotation]
        reward = 0f;
    }

    void Update()
    {
        // Only proceed if the socket is alive
        if (stream != null && stream.CanWrite && stream.CanRead)
        {
            // 1) Send the last reward to Python
            SendMessage(BitConverter.GetBytes(reward));

            // 2) Block until Python sends you back your next action
            var payload = ReceiveMessage();
            if (payload != null && payload.Length >= action.Length * 4)
            {
                for (int i = 0; i < action.Length; i++)
                {
                    action[i] = BitConverter.ToSingle(payload, i * 4);
                }
            }
            else
            {
                Debug.LogError($"Expected {action.Length*4} bytes, got {payload?.Length ?? 0}");
            }

            // Now your other scripts (e.g. your agent’s FixedUpdate) can
            // read `action[]` and apply it immediately.
        }
    }

    private void SendMessage(byte[] payload)
    {
        // Write a 4-byte length header (little-endian)
        var lenBytes = BitConverter.GetBytes(payload.Length);
        stream.Write(lenBytes, 0, 4);
        // Then the payload itself
        stream.Write(payload, 0, payload.Length);
    }

    private byte[] ReceiveMessage()
    {
        // Read exactly 4 bytes for the length
        int read = 0;
        while (read < 4)
        {
            int n = stream.Read(_lenBuffer, read, 4 - read);
            if (n == 0) return null; // connection closed
            read += n;
        }

        int length = BitConverter.ToInt32(_lenBuffer, 0);
        var payload = new byte[length];

        // Read exactly `length` bytes
        int offset = 0;
        while (offset < length)
        {
            int n = stream.Read(payload, offset, length - offset);
            if (n == 0) return null; // connection closed
            offset += n;
        }

        return payload;
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}

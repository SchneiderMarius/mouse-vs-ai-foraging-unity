using UnityEngine;

public class pythonControllerMS : MonoBehaviour
{
    [Tooltip("Drag in your GameObject that has the tcp_client script")]
    public tcp_client tcpClient;

    [Tooltip("Movement speed multiplier")]
    public float gain = 1.5f;

    [Tooltip("Rotation speed multiplier")]
    public float rotationGain = 10f;

    private float[] action;

    void Awake()
    {
        if (tcpClient == null)
        {
            Debug.LogError("pythonController: please assign the tcp_client reference in the Inspector!");
            enabled = false;
            return;
        }
    }

    void FixedUpdate()
    {
        // Grab the latest action from tcp_client
        action = tcpClient.action;
        if (action == null || action.Length < 3) return;

        // unpack
        float moveX = action[0];
        float moveZ = action[1];
        float rotationY = action[2];

        // apply gains & deltaTime
        Vector3 move = new Vector3(moveX, 0f, moveZ) * gain * Time.fixedDeltaTime;
        float yaw = rotationY * rotationGain * Time.fixedDeltaTime;

        // apply transform
        transform.Translate(move, Space.Self);
        transform.Rotate(0f, yaw, 0f, Space.Self);
    }
}

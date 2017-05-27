using UnityEngine;

public class PlayerPhysics : MonoBehaviour
{
    private Vector3 velocity;

    void Start()
    {
        velocity = Vector3.zero;
    }

    public Vector3 SimulateStep(Vector3 Position, byte PackedDirection)
    {
        Vector3 direction = Vector3.zero;

        if ((PackedDirection & 1) > 0)
        {
            direction.y = 1.0f;
        }
        else if ((PackedDirection & 4) > 0)
        {
            direction.y = -1.0f;
        }

        if ((PackedDirection & 2) > 0)
        {
            direction.x = 1.0f;
        }
        else if ((PackedDirection & 8) > 0)
        {
            direction.x = -1.0f;
        }

        if (direction != Vector3.zero)
        {
            velocity += direction.normalized * 0.0125f * 12.0f;

            if (velocity.magnitude >= 3.2f)
            {
                velocity = velocity.normalized * 3.2f;
            }
        }
        else
        {
            velocity *= Mathf.Pow(0.00005f, 0.0125f);

            if (velocity.magnitude <= 0.0125f)
            {
                velocity = Vector3.zero;
            }
        }

        return velocity * 0.0125f;
    }
}


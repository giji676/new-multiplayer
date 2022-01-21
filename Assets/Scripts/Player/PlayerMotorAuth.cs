using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ClientNetworkTransform))]

public class PlayerMotorAuth : NetworkBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Vector3 velocity;

    [SerializeField]
    private Vector3 rotation;

    [SerializeField]
    private Vector3 cameraRotation;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update local variables from PlayerControl
    public void UpdateClientVelocityRotation(Vector3 _velocity, Vector3 _rotation, Vector3 _cameraRotation)
    {
        velocity = _velocity;
        rotation = _rotation;
        cameraRotation = _cameraRotation;
    }

    // Run every physics iteration
    private void FixedUpdate()
    {
        PerformMovement();
        perfromRotation();
    }

    // Perform movement based on velocity variable
    private void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.deltaTime);
        }
    }

    // Perform rotation
    private void perfromRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        if (cam != null)
        {
            cam.transform.Rotate(-cameraRotation);
        }
    }
}

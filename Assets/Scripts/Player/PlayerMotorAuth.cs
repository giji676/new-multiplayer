using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMotorAuth : NetworkBehaviour
{
    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private AudioListener audioListener;

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

        if (IsClient && !IsOwner)
        {
            playerCamera.enabled = false;
            audioListener.enabled = false;
        }
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
        if (playerCamera != null)
        {
            playerCamera.transform.Rotate(-cameraRotation);
        }
    }
}

using UnityEngine;
using Unity.Netcode;


[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : NetworkBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private NetworkVariable<Vector3> networkVelocity = new NetworkVariable<Vector3>();

    [SerializeField]
    private NetworkVariable<Vector3> networkRotation = new NetworkVariable<Vector3>();

    [SerializeField]
    private NetworkVariable<Vector3> networkCameraRotation = new NetworkVariable<Vector3>();

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update local variables from PlayerControl
    public void UpdateClientVelocityRotation(Vector3 _velocity, Vector3 _rotation, Vector3 _cameraRotation)
    {
        if (networkVelocity.Value != _velocity || networkRotation.Value != _rotation || networkCameraRotation.Value != _cameraRotation)
        {
            UpdateClientPosRotServerRpc(_velocity, _rotation, _cameraRotation);
        }
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
        if (networkVelocity.Value != Vector3.zero)
        {
            rb.MovePosition(rb.position + networkVelocity.Value * Time.deltaTime);
        }
    }

    // Perform rotation
    private void perfromRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(networkRotation.Value));
        if (cam != null)
        {
            cam.transform.Rotate(-networkCameraRotation.Value);
        }
    }

    [ServerRpc]
    public void UpdateClientPosRotServerRpc(Vector3 newVelocity, Vector3 newRotation, Vector3 newCameraRotation)
    {
        networkVelocity.Value = newVelocity;
        networkRotation.Value = newRotation;
        networkCameraRotation.Value = newCameraRotation;
    }
}

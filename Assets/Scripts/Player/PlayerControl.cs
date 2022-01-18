using Unity.Netcode;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float sensitivity = 1.5f;

    [SerializeField]
    private Vector2 defaultInitialPalnePosition = new Vector2(-4, 4);

    private float _xMov;
    private float _zMov;

    private PlayerMotor motor;

    private void Start()
    {
        motor = GetComponent<PlayerMotor>();
    }

    private void Update()
    {
        // Player rotation and position input
        // Calculate movement velocity as a 3D vector
        _xMov = Input.GetAxisRaw("Horizontal");
        _zMov = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _xMov;
        Vector3 _moveVertical = transform.forward * _zMov;

        // Final movement vector
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * speed;

        // Calcualte rotation as a 3D vector (turning around)
        float _yRot = Input.GetAxisRaw("Mouse X");
        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * sensitivity;

        // Calcualte camera rotation as a 3D vector
        float _xRot = Input.GetAxisRaw("Mouse Y");
        Vector3 _cameraRotation = new Vector3(_xRot, 0f, 0f) * sensitivity;

        // Apply _velocity, _rotation, _cameraRotation in PlayerMotor
        motor.UpdateClientVelocityRotation(_velocity, _rotation, _cameraRotation);
    }
}

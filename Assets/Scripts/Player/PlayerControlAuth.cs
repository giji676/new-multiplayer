using Unity.Netcode;
using UnityEngine;

public class PlayerControlAuth : NetworkBehaviour
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Run,
        ReverseWalk,
        ReverseRun
    }

    [SerializeField]
    private float walkSpeed = 5f;

    [SerializeField]
    private float runSpeed = 10f;

    [SerializeField]
    private float currentSpeed = 0f;

    [SerializeField]
    private float sensitivity = 3f;

    [SerializeField]
    private Vector2 defaultInitialPlanePosition = new Vector2(-4, 4);

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();

    private float _xMov;
    private float _zMov;
    private bool _run;

    private PlayerMotorAuth motor;
    private Animator animator;

    private void Start()
    {
        if (IsClient && IsOwner)
        {            
            // Give the player random position on start
            transform.position = new Vector3(Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y), 0, Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y));
        }

        motor = GetComponent<PlayerMotorAuth>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            ClientInput();
            PlayerStateUpdate();
        }

        ClientVisuals();
    }

    private void ClientInput()
    {
        // Player rotation and position input

        // Calculate movement velocity as a 3D vector
        _xMov = Input.GetAxisRaw("Horizontal");
        _zMov = Input.GetAxisRaw("Vertical");

        _run = Input.GetKey(KeyCode.LeftShift);

        Vector3 _moveHorizontal = transform.right * _xMov;
        Vector3 _moveVertical = transform.forward * _zMov;

        if (_run)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // Final movement vector
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * currentSpeed;

        // Calcualte rotation as a 3D vector (turning around)
        float _yRot = Input.GetAxisRaw("Mouse X");
        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * sensitivity;

        // Calcualte camera rotation as a 3D vector
        float _xRot = Input.GetAxisRaw("Mouse Y");
        Vector3 _cameraRotation = new Vector3(_xRot, 0f, 0f) * sensitivity;

        // Apply _velocity, _rotation, _cameraRotation in PlayerMotor
        motor.UpdateClientVelocityRotation(_velocity, _rotation, _cameraRotation);
    }

    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState newState)
    {
        // Update networkPlayerState with a new state
        networkPlayerState.Value = newState;
    }

    private void PlayerStateUpdate()
    {
        // Player state changes
        if (_zMov > 0 && !_run)
        {
            UpdatePlayerStateServerRpc(PlayerState.Walk);
        }
        else if (_zMov > 0 && _run)
        {
            UpdatePlayerStateServerRpc(PlayerState.Run);
        }
        else if (_zMov < 0 && !_run)
        {
            UpdatePlayerStateServerRpc(PlayerState.ReverseWalk);
        }
        else if (_zMov < 0 && _run)
        {
            UpdatePlayerStateServerRpc(PlayerState.ReverseRun);
        }
        else
        {
            UpdatePlayerStateServerRpc(PlayerState.Idle);
        }
    }

    private void ClientVisuals()
    {
        // Set animator values beased on PlayerState
        if (networkPlayerState.Value == PlayerState.Walk)
        {
            animator.SetFloat("Walk", 1);
        }
        else if (networkPlayerState.Value == PlayerState.Run)
        {
            animator.SetFloat("Walk", 2);
        }
        else if (networkPlayerState.Value == PlayerState.ReverseWalk)
        {
            animator.SetFloat("Walk", -1);
        }
        else if (networkPlayerState.Value == PlayerState.ReverseRun)
        {
            animator.SetFloat("Walk", -2);
        }
        else
        {
            animator.SetFloat("Walk", 0);
        }
    }
}

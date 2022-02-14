using Assets.Scripts.Utilities;
using System.Collections.Generic;
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
    private Vector2 defaultInitialPlanePosition = new Vector2(-4, 4);

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();

    private float _xMov;
    private float _zMov;
    private float _yRot;
    private float _xRot;
    private bool _run;

    private PlayerMotor motor;
    private Animator animator;

    private Usercmd nextUsercmd;
    public List<Usercmd> UsercmdToSendToServer;
    public List<Usercmd> UsercmdToPlay;
    public uint UsercmdNumber;

    [Tooltip("FREQUENCY OF SENDING INPUTS TO SERVER, IN TERMS OF FIXED UPDATE LOOPS")]
    public int FrameSyncRate = 5;

    private void Start()
    {
        UsercmdToSendToServer = new List<Usercmd>();
        UsercmdToPlay = new List<Usercmd>();

        if (IsClient && IsOwner)
        {
            // Give the player random position on start
            transform.position = new Vector3(Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y), 0, Random.Range(defaultInitialPlanePosition.x, defaultInitialPlanePosition.y));
        }

        motor = GetComponent<PlayerMotor>();
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

    private void FixedUpdate()
    {
        UsercmdNumber++;
        nextUsercmd.frame = UsercmdNumber;
        
        if (IsOwner)
        {
            UsercmdToSendToServer.Add(nextUsercmd);
            UsercmdToPlay.Add(nextUsercmd);

        }

        if (UsercmdNumber % FrameSyncRate == 0)
        {
            if (IsOwner)
            {
                UpdateUsercmdServerRpc(ByteArrayUtils.ObjectToByteArray(UsercmdToSendToServer));
                UsercmdToSendToServer.Clear();
            }
        }

    }

    [ServerRpc]
    public void UpdateUsercmdServerRpc(byte[] args)
    {
        var _usercmd = args;
        List<Usercmd> networkUsercmd = (List<Usercmd>)ByteArrayUtils.ByteArrayToObject(_usercmd);

        if (IsServer)
        {
            UsercmdToPlay.AddRange(networkUsercmd);
        }
    }

    private void ClientInput()
    {
        // Player rotation and position input

        // Calculate movement velocity as a 3D vector
        _xMov = Input.GetAxisRaw("Horizontal");
        _zMov = Input.GetAxisRaw("Vertical");
        _run = Input.GetKey(KeyCode.LeftShift);
        _yRot = Input.GetAxisRaw("Mouse X");
        _xRot = Input.GetAxisRaw("Mouse Y");

        // Apply _velocity, _rotation, _cameraRotation in PlayerMotor
        nextUsercmd = new Usercmd() {
            horizontalInput = _xMov,
            verticalInput = _zMov,
            runInput = _run,
            mouseXInput = _yRot,
            mouseYInput = _xRot
        };
        // motor.UpdateClientVelocityRotation(_velocity, _rotation, _cameraRotation);
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

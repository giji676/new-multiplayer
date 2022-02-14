using UnityEngine;
using Unity.Netcode;
using Assets.Scripts.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using Assets.Scripts.Utilities;

[RequireComponent(typeof(Rigidbody))]

public class PlayerMotor : NetworkBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private AudioListener audioListener;

    [SerializeField]
    private float walkSpeed = 5f;

    [SerializeField]
    private float runSpeed = 10f;

    [SerializeField]
    private float currentSpeed = 0f;

    [SerializeField]
    private float sensitivity = 3f;

    [SerializeField]
    public List<MovementHistoryItem> LocalMovementHistory;

    [SerializeField]
    public List<MovementHistoryItem> AuthMovementHistory;


    private PlayerControlAuth playerControl;

    private bool IsLocalOwner;
    public uint currentUsercmd = 0;

    private float _xMov;
    private float _zMov;
    private bool _run;
    private float _yRot;
    private float _xRot;

    [Tooltip("FREQUENCY OF SEND HISTORY FROM THE SERVER, IN TERMS OF FIXED UPDATE LOOPS")]
    public int FrameSyncRate = 5;

    void Start()
    {
        if (IsClient && !IsOwner)
        {
            cam.enabled = false;
            audioListener.enabled = false;
        }
        LocalMovementHistory = new List<MovementHistoryItem>();
    }

    // Run every physics iteration
    private void FixedUpdate()
    {
        currentUsercmd++;

        IsLocalOwner = NetworkObject.IsLocalPlayer;
        if (playerControl == null)
        {
            playerControl = FindObjectOfType<PlayerControlAuth>();
        }

        if (!IsServer || !IsLocalPlayer)
        {
            transform.position = NetworkObject.transform.position;
        }

        if (IsServer || IsLocalOwner)
        {
            if (playerControl != null && playerControl.UsercmdToPlay.Count() > 0)
            {
                Usercmd frame = playerControl.UsercmdToPlay[0];
                PerformMovement_(frame);
                LocalMovementHistory.Add(GetMovementHistoryItem(frame));
                playerControl.UsercmdToPlay.Clear();
            }
        }

        if (IsLocalOwner)
        {
            PerformMovementReconciliation();
        }

        if (IsOwner)
        {
            NetworkObject.transform.position = transform.position;
            if (currentUsercmd % FrameSyncRate == 0)
            {
                UpdateMovementHistoryServerRpc(ByteArrayUtils.ObjectToByteArray(LocalMovementHistory));
                LocalMovementHistory.Clear();
            }
        }

        if (IsLocalOwner)
        {
            if (Input.GetKeyDown("x"))
            {
                int amount = UnityEngine.Random.Range(-10, -1);
                transform.Translate(new Vector3(UnityEngine.Random.Range(-5, -1), 0, UnityEngine.Random.Range(1, 5)));
            }
        }
    }

    [ServerRpc]
    public void UpdateMovementHistoryServerRpc(byte[] args)
    {
        byte[] itemList = args;
        List<MovementHistoryItem> historyFrames = (List<MovementHistoryItem>)ByteArrayUtils.ByteArrayToObject(itemList);

        if (IsLocalOwner)
        {
            AuthMovementHistory.AddRange(historyFrames);
        }
    }

    private void PerformMovement_(Usercmd _frame)
    {
        _xMov = _frame.horizontalInput;
        _zMov = _frame.verticalInput;
        _run = _frame.runInput;
        _yRot = _frame.mouseXInput;
        _xRot = _frame.mouseYInput;

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
        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * sensitivity;
        Vector3 _camRotation = new Vector3(_xRot, 0f, 0f) * sensitivity;

        transform.Translate(_velocity * Time.fixedDeltaTime);
        //transform.Rotate(rb.rotation * Quaternion.Euler(_rotation));
        if (cam != null)
        {
            //cam.transform.Rotate(-_camRotation);
        }
    }

    private MovementHistoryItem GetMovementHistoryItem(Usercmd usercmd)
    {
        MovementHistoryItem movementHistoryItem = new MovementHistoryItem()
        {
            xPosition = transform.position.x,
            yPosition = transform.position.y,
            zPosition = transform.position.z,
            frame = usercmd.frame,
            usercmd = usercmd
        };

        return movementHistoryItem;
    }

    private void PerformMovementReconciliation()
    {
        while (AuthMovementHistory.Count() > 0)
        {
            MovementHistoryItem serverItem = AuthMovementHistory[0];
            try
            {
                MovementHistoryItem localItem = LocalMovementHistory.FirstOrDefault(x => x.frame == serverItem.frame);
                float distance = GetHistoryDistance(serverItem, localItem);

                if (distance > 0.6f)
                {
                    transform.position = new Vector3(serverItem.xPosition, serverItem.yPosition, serverItem.zPosition);
                    var itemsToReplay = LocalMovementHistory.Where(x => x.frame >= serverItem.frame);
                    foreach (var historyItemToReconcole in itemsToReplay)
                    {
                        PerformMovement_(historyItemToReconcole.usercmd);
                    }
                }

                LocalMovementHistory.Remove(localItem);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                AuthMovementHistory.Remove(serverItem);
            }
            AuthMovementHistory.Remove(serverItem);
        }
    }

    private float GetHistoryDistance(MovementHistoryItem serverItem, MovementHistoryItem localItem)
    {
        Vector3 serverPosition = new Vector3(serverItem.xPosition, serverItem.yPosition, serverItem.zPosition);
        Vector3 localPosition = new Vector3(localItem.xPosition, localItem.yPosition, localItem.zPosition);
        return Vector3.Distance(localPosition, serverPosition);
    }
}

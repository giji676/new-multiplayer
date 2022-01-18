using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    private float speed = 5f;

    private PlayerMotor motor;

    private void Start()
    {
        motor = GetComponent<PlayerMotor>();
    }

    private void Update()
    {
        //Calculate movement velocity as a 3D vector
        float _xMov = Input.GetAxisRaw("Horizontal");
        float _zMov = Input.GetAxisRaw("Vertical");
    }
}

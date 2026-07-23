using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;
    public InputManager inputManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputManager = FindObjectOfType<InputManager>();
    }

    private void FixedUpdate()
    {
        moveInput = inputManager.moveInput;
        rb.linearVelocity = moveInput * moveSpeed;
    }
        
}

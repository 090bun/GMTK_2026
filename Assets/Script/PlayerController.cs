using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 5f;
    [Header("加速度")]
    public float acceleration = 20f;
    [Header("阻力係數 (每秒保留速度比例，越小停得越快)")]
    [Range(0f, 1f)]
    public float drag = 0.95f;
    [Header("引力影響程度")]
    public float gravityInfluence = 1f;  // 引力對玩家的影響程度
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 velocity;   // 持續累積的速度，沒輸入時慢慢衰減
    private GravitySource[] gravitySources;
    public InputManager inputManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputManager = FindObjectOfType<InputManager>();
        gravitySources = FindObjectsOfType<GravitySource>();
    }

    private void Update()
    {
        // 1. 玩家移動輸入：有輸入時朝方向加速，沒輸入時逐漸衰減
        moveInput = inputManager.moveInput;
        if (moveInput.sqrMagnitude > 0.0001f)
        {
            velocity += moveInput.normalized * acceleration * Time.deltaTime;
            velocity = Vector2.ClampMagnitude(velocity, moveSpeed);
        }
        else
        {
            velocity *= Mathf.Pow(drag, Time.deltaTime);
        }

        // 2. 計算總引力並持續作用在速度上
        Vector2 totalGravity = CalculateTotalGravity();
        velocity += totalGravity * gravityInfluence * Time.deltaTime;

        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    private Vector2 CalculateTotalGravity()
    {
        Vector2 gravity = Vector2.zero;
        foreach (GravitySource source in gravitySources)
        {
            Vector2 direction = source.GetGravityDirection(transform.position);
            float magnitude = source.GetGravityMagnitude(transform.position);
            gravity += direction * magnitude;
        }
        return gravity;
    }
}

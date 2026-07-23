using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("阻力係數 (每秒保留速度比例，越小停得越快)")]
    [Range(0f, 1f)]
    [SerializeField] private float drag = 0.95f;
    [Header("引力影響程度")]
    [SerializeField] private float gravityInfluence = 1f;  // 引力對玩家的影響程度

    // ═══ 能量系統 ═══
    [SerializeField] private float maxEnergy = 100f;
    public float currentEnergy = 100f;

    // ═══ 噴射推進（按住方向鍵蓄力，放開時噴射） ═══
    [Header("蓄力時間 (秒，按滿此時間力道達最大)")]
    [SerializeField] private float maxChargeTime = 2f;
    [Header("蓄力時的時間縮放 (製造慢動作感)")]
    [Range(0.05f, 1f)]
    [SerializeField] private float slowMotionScale = 0.3f;

    [Header("消耗範圍 [最低消耗, 最高消耗]")]
    [SerializeField] private float minConsumption = 5f;
    [SerializeField] private float maxConsumption = 20f;

    [Header("推進速度範圍 [最低速度, 最高速度]")]
    [SerializeField] private float minBoostSpeed = 2f;
    [SerializeField] private float maxBoostSpeed = 8f;

    private float currentConsumption = 0f;  // 若現在放開，會消耗的能量
    private float currentBoostSpeed = 0f;   // 若現在放開，會獲得的推進速度
    private float chargeTime = 0f;          // 已蓄力時間
    private bool isCharging = false;
    private Vector2 chargeDirection;        // 蓄力當下的方向（噴射的角度）

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

    private void OnDisable()
    {
        // 避免物件在蓄力中被停用/死亡，導致時間縮放卡在慢動作
        if (isCharging) Time.timeScale = 1f;
    }

    private void Update()
    {
        moveInput = inputManager.moveInput;
        bool hasInput = moveInput.sqrMagnitude > 0.0001f;

        if (hasInput && currentEnergy > 0f)
        {
            // 按住方向鍵：進入慢動作蓄力，角度可隨方向鍵即時調整
            if (!isCharging)
            {
                isCharging = true;
                chargeTime = 0f;
                Time.timeScale = slowMotionScale;
            }
            chargeDirection = moveInput.normalized;
            chargeTime = Mathf.Min(chargeTime + Time.unscaledDeltaTime, maxChargeTime);

            float t = chargeTime / maxChargeTime;
            currentConsumption = Mathf.Lerp(minConsumption, maxConsumption, t);
            currentBoostSpeed = Mathf.Lerp(minBoostSpeed, maxBoostSpeed, t);
        }
        else if (isCharging)
        {
            // 放開方向鍵（或能量耗盡）→ 依蓄力時間噴射，至少消耗最低值
            ReleaseBoost();
        }
        else
        {
            // 沒有輸入也沒在蓄力：速度慢慢衰減
            velocity *= Mathf.Pow(drag, Time.deltaTime);
        }

        // 引力持續作用在速度上
        Vector2 totalGravity = CalculateTotalGravity();
        velocity += totalGravity * gravityInfluence * Time.deltaTime;

        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    private void ReleaseBoost()
    {
        isCharging = false;
        Time.timeScale = 1f;

        velocity += chargeDirection * currentBoostSpeed;
        currentEnergy = Mathf.Max(0f, currentEnergy - currentConsumption);

        chargeTime = 0f;
        currentConsumption = 0f;
        currentBoostSpeed = 0f;
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

    public void AddEnergy(float energy)
    {
        currentEnergy = Mathf.Min(currentEnergy + energy, maxEnergy);
    }
}

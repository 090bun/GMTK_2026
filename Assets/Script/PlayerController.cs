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

    [Header("面向噴射方向的轉向速度 (度/秒)")]
    [SerializeField] private float rotationSpeed = 720f;

    [Header("噴射特效 (放開方向鍵時播放，粒子數量依蓄力力道決定)")]
    [SerializeField] private ParticleSystem boostParticles;
    [Header("噴射特效粒子數量範圍 [最低, 最高]")]
    [SerializeField] private int minBoostParticles = 5;
    [SerializeField] private int maxBoostParticles = 30;
    

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
    private MusicCtl musicCtl;

    // ═══ 碎片尾巴（碎片被撿到後依序串接在後面） ═══
    private Transform tailEnd;

    // ═══ 碎片收集計數 ═══
    [Header("需要收集的碎片總數")]
    [SerializeField] private int totalFragments = 3;
    private int collectedFragments = 0;

    [Header("黑洞（碎片收集齊時生成）")]
    [SerializeField] private GameObject blackHolePrefab;
    [SerializeField] private Transform blackHoleSpawnPoint;
    private bool blackHoleSpawned = false;

    // 是否已撞上致命星球（避免重複觸發失敗）
    private bool hasFailed = false;

    // 目前受到的總引力強度，供 UI 顯示警示用
    private float currentGravityMagnitude = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputManager = FindObjectOfType<InputManager>();
        gravitySources = FindObjectsOfType<GravitySource>();
        musicCtl = FindObjectOfType<MusicCtl>();
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

            // 蓄力時讓玩家面向即將噴射的方向
            float targetAngle = Mathf.Atan2(chargeDirection.y, chargeDirection.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.unscaledDeltaTime);

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
        currentGravityMagnitude = totalGravity.magnitude;
        velocity += totalGravity * gravityInfluence * Time.deltaTime;

        transform.position += (Vector3)velocity * Time.deltaTime;

        // 擋住玩家，避免直接穿過星球中心造成彈弓效應甩出去
        HandleSurfaceCollision();
    }

    private void HandleSurfaceCollision()
    {
        foreach (GravitySource source in gravitySources)
        {
            Vector2 sourcePos = source.transform.position;
            Vector2 playerPos = transform.position;
            Vector2 offset = playerPos - sourcePos;
            float distance = offset.magnitude;

            if (distance >= source.surfaceRadius) continue;

            if (source.isDeadly)
            {
                Fail();
            }

            Vector2 outward = distance > 0.0001f ? offset / distance : Vector2.up;

            // 貼回表面，不讓玩家穿過去（保留原本的 z 座標）
            Vector2 surfacePoint = sourcePos + outward * source.surfaceRadius;
            transform.position = new Vector3(surfacePoint.x, surfacePoint.y, transform.position.z);

            // 移除朝向星球中心的速度分量，保留切線/朝外分量（可用噴射脫離）
            float radialSpeed = Vector2.Dot(velocity, outward);
            if (radialSpeed < 0f)
            {
                velocity -= outward * radialSpeed;
            }
        }
    }

    private void ReleaseBoost()
    {
        isCharging = false;
        Time.timeScale = 1f;

        velocity += chargeDirection * currentBoostSpeed;
        currentEnergy = Mathf.Max(0f, currentEnergy - currentConsumption);
        EmitBoostParticles(ChargeProgress);
        musicCtl?.PlayBooutSound();

        chargeTime = 0f;
        currentConsumption = 0f;
        currentBoostSpeed = 0f;
    }

    private void EmitBoostParticles(float chargeProgress)
    {
        if (boostParticles == null) return;
        int particleCount = Mathf.RoundToInt(Mathf.Lerp(minBoostParticles, maxBoostParticles, chargeProgress));
        // boostParticles.Emit(particleCount);
        
        var emission = boostParticles.emission;
        emission.rateOverTime = particleCount;
        boostParticles.Play();
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

    // 蓄力進度 (0~1)，供 UI 顯示方向鍵累積的噴射力道
    public float ChargeProgress => maxChargeTime > 0f ? chargeTime / maxChargeTime : 0f;
    public bool IsCharging => isCharging;

    public void AddEnergy(float energy)
    {
        currentEnergy = Mathf.Min(currentEnergy + energy, maxEnergy);
    }

    // 讓玩家進入失敗狀態（撞到致命星球、超出遊戲範圍等皆可呼叫）
    public void Fail()
    {
        if (hasFailed) return;
        hasFailed = true;
        musicCtl?.PlayFailSound();
    }

    public bool HasFailed => hasFailed;

    // 碎片撿到後要跟隨的對象：目前隊伍尾端（沒有碎片時就是玩家自己）
    public Transform GetTailEnd()
    {
        return tailEnd != null ? tailEnd : transform;
    }

    public void SetTailEnd(Transform newTailEnd)
    {
        tailEnd = newTailEnd;
    }

    // 目前受到的總引力強度，供 UI 顯示警示用
    public float CurrentGravityMagnitude => currentGravityMagnitude;

    public int CollectedFragments => collectedFragments;
    public int TotalFragments => totalFragments;

    public void CollectFragment()
    {
        collectedFragments = Mathf.Min(collectedFragments + 1, totalFragments);

        if (collectedFragments >= totalFragments && !blackHoleSpawned)
        {
            SpawnBlackHole();
        }
    }

    private void SpawnBlackHole()
    {
        if (blackHolePrefab == null || blackHoleSpawnPoint == null) return;

        blackHoleSpawned = true;
        GameObject blackHoleObj = Instantiate(blackHolePrefab, blackHoleSpawnPoint.position, Quaternion.identity);
        BlackHole blackHole = blackHoleObj.GetComponent<BlackHole>();
        if (blackHole != null)
        {
            blackHole.SetTarget(transform);
        }
    }
}

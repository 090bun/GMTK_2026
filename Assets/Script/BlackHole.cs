using UnityEngine;

[RequireComponent(typeof(GravitySource))]
public class BlackHole : MonoBehaviour
{
    [Header("目標（啟動後追蹤並持續放大）")]
    [SerializeField] private Transform target;

    [Header("放大設定")]
    [SerializeField] private float growthRate = 0.1f;   // 每秒放大的比例
    [SerializeField] private float maxScale = 10f;

    [Header("向目標移動設定")]
    [SerializeField] private float moveSpeed = 0.5f;    // 緩慢地向目標移動的速度
    [SerializeField] private float rotateSpeed = 1f;

    private GravitySource gravitySource;
    private Renderer[] renderers;
    private float baseGravitationalForce;
    private float baseEffectRadius;
    private float baseSurfaceRadius;
    private bool isActivated = false;

    private void Awake()
    {
        gravitySource = GetComponent<GravitySource>();
        gravitySource.isDeadly = true;
        baseGravitationalForce = gravitySource.gravitationalForce;
        baseEffectRadius = gravitySource.effectRadius;
        baseSurfaceRadius = gravitySource.surfaceRadius;

        renderers = GetComponentsInChildren<Renderer>();
        SetVisible(false);

        // 啟動前不參與引力計算，只保留 isDeadly（此時 surfaceRadius 為 0 不會觸發撞擊）
        gravitySource.gravitationalForce = 0f;
        gravitySource.effectRadius = 0f;
        gravitySource.surfaceRadius = 0f;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
        {
            r.enabled = visible;
        }
    }

    // 收集齊碎片時呼叫：顯示黑洞並開始追蹤、放大
    public void Activate(Transform newTarget)
    {
        if (isActivated) return;
        isActivated = true;
        target = newTarget;
        SetVisible(true);

        // 還原成 base 值，之後在 Update 裡隨大小成長
        gravitySource.gravitationalForce = baseGravitationalForce;
        gravitySource.effectRadius = baseEffectRadius;
        gravitySource.surfaceRadius = baseSurfaceRadius;
    }

    private void Update()
    {
        if (!isActivated) return;

        Vector3 scale = transform.localScale;
        if (scale.x < maxScale)
        {
            scale *= 1f + growthRate * Time.deltaTime;
            transform.localScale = scale;

            // 引力大小、有效範圍、表面半徑隨黑洞成長同步放大，行為與其他星球一致
            gravitySource.gravitationalForce = baseGravitationalForce * scale.x;
            gravitySource.effectRadius = baseEffectRadius * scale.x;
            gravitySource.surfaceRadius = baseSurfaceRadius * scale.x;
        }

        if (target != null)
        {
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }

    // 啟動後才需要顯示方向指示器警示
    public bool IsActivated => isActivated;
}

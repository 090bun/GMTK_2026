using UnityEngine;

public class BlackHole : MonoBehaviour
{
    [Header("目標（追蹤並持續放大）")]
    [SerializeField] private Transform target;

    [Header("放大設定")]
    [SerializeField] private float growthRate = 0.1f;   // 每秒放大的比例
    [SerializeField] private float maxScale = 10f;

    [Header("向目標移動設定")]
    [SerializeField] private float moveSpeed = 0.5f;    // 緩慢地向目標移動的速度
    [SerializeField] private float rotateSpeed = 1f;   
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        Vector3 scale = transform.localScale;
        if (scale.x < maxScale)
        {
            scale *= 1f + growthRate * Time.deltaTime;
            transform.localScale = scale;
        }

        if (target != null)
        {
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }
}

using UnityEngine;

public class GravitySource : MonoBehaviour
{
    public float gravitationalForce = 10f;  // 引力強度
    public float effectRadius = 20f;        // 有效範圍
    public float surfaceRadius = 2f;        // 表面半徑，玩家靠近到此距離會被擋住並吸附
    public bool isDeadly = false;           // 是否為致命星球（撞到會讓玩家失敗），例如 star_1~5

    public Vector2 GetGravityDirection(Vector3 targetPosition)
    {
        Vector2 direction = (transform.position - targetPosition).normalized;
        return direction;
    }
    
    public float GetGravityMagnitude(Vector3 targetPosition)
    {
        float centerDistance = Vector2.Distance(transform.position, targetPosition);
        // 以星球表面（邊緣）為基準計算距離，而非星球中心
        float distance = Mathf.Max(centerDistance - surfaceRadius, 0f);

        // 檢查是否在範圍內
        if (distance > effectRadius) return 0f;

        // 距離越近，引力越強（平方反比）
        float force = gravitationalForce / (distance * distance + 1f);
        return force;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, surfaceRadius);
    }
}

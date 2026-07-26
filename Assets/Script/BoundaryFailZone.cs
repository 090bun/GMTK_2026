using UnityEngine;

// 掛在前景邊界物件上，玩家一旦超出遊戲範圍碰到就判定失敗
public class BoundaryFailZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerController>()?.Fail();
    }
}

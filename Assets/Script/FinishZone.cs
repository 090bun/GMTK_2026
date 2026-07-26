using UnityEngine;

// 掛在終點物件(finishGap)上，玩家碰到時判定過關
public class FinishZone : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerController>()?.Win();
    }
}

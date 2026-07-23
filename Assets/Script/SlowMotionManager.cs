using UnityEngine;

public class SlowMotionManager : MonoBehaviour
{
    [SerializeField] private  InputManager inputManager;
    [SerializeField] private  float slowMotionScale = 0.3f; // 0.3 = 剩餘30%時間流逝速度
    [SerializeField] private PlayerController playerController;
    private bool isSlowMotion = false;
    void Update()
    {
        if (inputManager.moveInput.sqrMagnitude > 0.0001f && playerController.currentEnergy > 0) {
            EnterSlowMotion();
        } else {
            ExitSlowMotion();
        }
    }
    private void EnterSlowMotion()
    {
        if (isSlowMotion) return;
        isSlowMotion = true;
        Time.timeScale = slowMotionScale;  // 全遊戲變慢
    }
    
    private void ExitSlowMotion()
    {
        if (!isSlowMotion) return;
        isSlowMotion = false;
        Time.timeScale = 1f;  // 恢復正常速度
    }
}

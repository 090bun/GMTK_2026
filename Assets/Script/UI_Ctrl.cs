using UnityEngine;
using UnityEngine.UI;
public class UI_Ctrl : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Image energySlider;
    [SerializeField] private Image chargeSlider;
    [SerializeField] private float chargeSliderStart = 0.7f; // 測試用：蓄力條從此值開始往 1 增加
    void Update()
    {
        energySlider.fillAmount = playerController.currentEnergy / 100f;

        chargeSlider.gameObject.SetActive(playerController.IsCharging);
        if (playerController.IsCharging)
        {
            chargeSlider.fillAmount = Mathf.Lerp(chargeSliderStart, 1f, playerController.ChargeProgress);
        }
    }
}

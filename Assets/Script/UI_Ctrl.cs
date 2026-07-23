using UnityEngine;
using UnityEngine.UI;
public class UI_Ctrl : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Image energySlider;
    void Update()
    {
        energySlider.fillAmount = playerController.currentEnergy / 100f;
    }
}

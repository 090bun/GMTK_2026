using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UI_Ctrl : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RectTransform energySlider;
    [SerializeField] private float energySliderMaxWidth = 1375f;
    [SerializeField] private Image chargeSlider;
    [SerializeField] private float chargeSliderStart = 0.7f; // 測試用：蓄力條從此值開始往 1 增加

    [Header("碎片收集 UI")]
    [SerializeField] private TextMeshProUGUI fragmentText;

    [Header("引力警示 UI")]
    [SerializeField] private Image gravityWarningImage;
    [SerializeField] private float gravityWarningMax = 5f; // 達到此引力強度時警示顏色最紅
    [SerializeField] private Color gravitySafeColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color gravityDangerColor = new Color(1f, 0f, 0f, 0.6f);
    [Header("選單 UI")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameObject menuUI;
    private bool isMenuOpen = false;
    private bool wasMenuPressed = false;

    [Header("失敗畫面故障效果")]
    [SerializeField] private Material glitchMaterial;
    [SerializeField] private float glitchRampDuration = 0.7f;
    [SerializeField] private GameObject playAgainUI;
    private bool failSequenceStarted = false;

    void Update()
    {
        if (failSequenceStarted) return;

        if (playerController.HasFailed)
        {
            failSequenceStarted = true;
            StartCoroutine(FailSequence());
            return;
        }

        Vector2 energySize = energySlider.sizeDelta;
        energySize.x = playerController.currentEnergy / 100f * energySliderMaxWidth;
        energySlider.sizeDelta = energySize;

        chargeSlider.gameObject.SetActive(playerController.IsCharging);
        if (playerController.IsCharging)
        {
            chargeSlider.fillAmount = Mathf.Lerp(chargeSliderStart, 1f, playerController.ChargeProgress);
        }

        fragmentText.text = $"fragment {playerController.CollectedFragments}/{playerController.TotalFragments}";

        float gravityT = gravityWarningMax > 0f
            ? Mathf.Clamp01(playerController.CurrentGravityMagnitude / gravityWarningMax)
            : 0f;
        gravityWarningImage.color = Color.Lerp(gravitySafeColor, gravityDangerColor, gravityT);

        if(inputManager.isMenuPressed && !wasMenuPressed){
            SetMenuOpen(!isMenuOpen);
        }
        wasMenuPressed = inputManager.isMenuPressed;
    }

    // 玩家失敗後：畫面故障閃爍，結束後停在選單畫面等待按下重新開始
    private IEnumerator FailSequence()
    {
        if (glitchMaterial != null)
        {
            float t = 0f;
            while (t < glitchRampDuration)
            {
                t += Time.unscaledDeltaTime;
                glitchMaterial.SetFloat("_GlitchIntensity", Mathf.Clamp01(t / glitchRampDuration));
                yield return null;
            }
        }

        playAgainUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        while (glitchMaterial != null)
        {
            float flicker = 0.5f + Mathf.PerlinNoise(Time.unscaledTime * 3f, 0f) * 0.3f;
            glitchMaterial.SetFloat("_GlitchIntensity", flicker);
            yield return null;
        }
    }

    private void SetMenuOpen(bool open)
    {
        isMenuOpen = open;
        menuUI.SetActive(isMenuOpen);
        if(isMenuOpen){
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else{
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // 給選單按鈕呼叫：關閉選單
    public void OnClickCloseMenu()
    {
        SetMenuOpen(false);
    }

    // 給選單按鈕呼叫：重新遊玩
    public void OnClickRestart()
    {
        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat("_GlitchIntensity", 0f);
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}

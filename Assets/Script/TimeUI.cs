using UnityEngine;
using TMPro;
public class TimeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    // [SerializeField] private float timeLimit = 60f;
    public bool stopTimer = false;
    private float currentTime;

    void Start(){
        currentTime = 0;
    }
    void Update(){
        if(stopTimer) return;
        currentTime += Time.deltaTime;
        float minute = Mathf.Floor(currentTime/60f);
        float second = currentTime%60f;
        if(second < 10) timeText.text = $"Time: {minute:F0}:0{second:F0}";
        else timeText.text = $"Time: {minute:F0}:{second:F0}";
    }

    public void ResetTime(){
        currentTime = 0;
    }

    // 供結束畫面顯示已使用的遊玩時間
    public string GetFormattedTime(){
        float minute = Mathf.Floor(currentTime/60f);
        float second = currentTime%60f;
        return second < 10 ? $"{minute:F0}:0{second:F0}" : $"{minute:F0}:{second:F0}";
    }
}

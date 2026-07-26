using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 單一方向指示器圖示：箭頭指向目標、可選顯示圖示與距離文字
public class DirectionIndicatorItem : MonoBehaviour
{
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI distanceText;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    public void SetScreenPosition(Vector2 screenPosition)
    {
        rectTransform.position = screenPosition;
    }

    public void SetDirection(Vector2 direction)
    {
        if (arrow == null) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        arrow.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetIcon(Sprite sprite)
    {
        if (icon == null || sprite == null) return;
        icon.sprite = sprite;
    }

    public void SetDistance(float distance)
    {
        if (distanceText == null) return;
        distanceText.text = $"{distance:0}";
    }
}

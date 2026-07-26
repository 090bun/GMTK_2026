using System.Collections.Generic;
using UnityEngine;

// 在玩家一定範圍內的星球 (GravitySource) 與道具 (Trackable) 會在畫面邊界顯示方向指示器，
// 方便玩家在宇宙中辨別要前進的方向。
//
// 做法：
// 1. 星球與道具不一定有 collider(場上大多數星球只有 MeshRenderer，沒掛 Collider)，
//    所以偵測改成直接抓 GravitySource / Trackable 的座標比對距離，不依賴物理碰撞查詢
// 2. 從畫面中心朝目標的螢幕座標方向做 Physics2D.Raycast，打在畫面安全區的邊界 collider 上，
//    命中點就是指示器要顯示的螢幕位置
// 3. 指示器依命中位置與方向更新，並顯示與玩家的距離
public class DirectionIndicatorUI : MonoBehaviour
{
    [Header("偵測設定")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectRangeMin = 10f;
    [SerializeField] private float detectRangeMax = 50f;
    [SerializeField] private float scanInterval = 0.25f;

    [Header("畫面邊界 (指示器可以貼到多靠近螢幕邊緣，單位:像素)")]
    [SerializeField] private float edgeMargin = 80f;

    [Header("指示器外觀")]
    [SerializeField] private RectTransform indicatorRoot;
    [SerializeField] private DirectionIndicatorItem indicatorPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Sprite defaultIcon; // 目標沒有自訂圖示時(例如星球)使用的預設圖案

    private struct TargetInfo
    {
        public Sprite icon;
        public float radius; // 物件邊緣半徑，計算距離時會從中心距離扣掉，沒有半徑(例如道具)則為 0
    }

    private readonly Dictionary<Transform, DirectionIndicatorItem> activeIndicators = new Dictionary<Transform, DirectionIndicatorItem>();
    private readonly Dictionary<Transform, TargetInfo> currentTargets = new Dictionary<Transform, TargetInfo>();
    private readonly List<Transform> toRemove = new List<Transform>();

    private GravitySource[] gravitySources = new GravitySource[0];
    private BoxCollider2D screenBoundary;
    private float scanTimer;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        if (player == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.transform;
        }
        if (mainCamera == null) mainCamera = Camera.main;

        gravitySources = FindObjectsOfType<GravitySource>();

        CreateScreenBoundary();
    }

    private void CreateScreenBoundary()
    {
        GameObject boundaryObj = new GameObject("IndicatorScreenBoundary");
        boundaryObj.transform.SetParent(transform, false);
        boundaryObj.hideFlags = HideFlags.HideInHierarchy;
        screenBoundary = boundaryObj.AddComponent<BoxCollider2D>();
        screenBoundary.isTrigger = true;
        UpdateScreenBoundary();
    }

    private void UpdateScreenBoundary()
    {
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        screenBoundary.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        screenBoundary.size = new Vector2(
            Mathf.Max(1f, Screen.width - edgeMargin * 2f),
            Mathf.Max(1f, Screen.height - edgeMargin * 2f));
    }

    private void Update()
    {
        if (player == null || indicatorRoot == null || indicatorPrefab == null) return;

        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            UpdateScreenBoundary();
        }

        scanTimer -= Time.unscaledDeltaTime;
        if (scanTimer <= 0f)
        {
            scanTimer = scanInterval;
            ScanForTargets();
        }

        RefreshIndicators();
    }

    private void ScanForTargets()
    {
        currentTargets.Clear();

        foreach (GravitySource source in gravitySources)
        {
            if (source == null) continue;
            Transform t = source.transform;
            float dist = Mathf.Max(Vector2.Distance(player.position, t.position) - source.surfaceRadius, 0f);
            if (dist >= detectRangeMin && dist <= detectRangeMax)
            {
                currentTargets[t] = new TargetInfo { icon = null, radius = source.surfaceRadius };
            }
        }

        foreach (Trackable trackable in Trackable.All)
        {
            if (trackable == null) continue;
            Transform t = trackable.transform;
            float dist = Vector2.Distance(player.position, t.position);
            if (dist >= detectRangeMin && dist <= detectRangeMax)
            {
                currentTargets[t] = new TargetInfo { icon = trackable.Icon, radius = 0f };
            }
        }
    }

    private void RefreshIndicators()
    {
        // 目標可能在兩次掃描之間被摧毀(例如道具被撿走)，要先把已經不存在的目標清掉，
        // 否則底下存取 target.position 會噴 MissingReferenceException
        toRemove.Clear();
        foreach (var kv in currentTargets)
        {
            if (kv.Key == null) toRemove.Add(kv.Key);
        }
        foreach (Transform t in toRemove) currentTargets.Remove(t);

        toRemove.Clear();
        foreach (var kv in activeIndicators)
        {
            if (kv.Key == null || !currentTargets.ContainsKey(kv.Key))
            {
                if (kv.Value != null) Destroy(kv.Value.gameObject);
                toRemove.Add(kv.Key);
            }
        }
        foreach (Transform t in toRemove) activeIndicators.Remove(t);

        foreach (var kv in currentTargets)
        {
            Transform target = kv.Key;
            Sprite icon = kv.Value.icon != null ? kv.Value.icon : defaultIcon;

            if (!activeIndicators.TryGetValue(target, out DirectionIndicatorItem item))
            {
                item = Instantiate(indicatorPrefab, indicatorRoot);
                item.gameObject.SetActive(true);
                activeIndicators[target] = item;
            }

            PlaceIndicator(target, item, icon, kv.Value.radius);
        }
    }

    private void PlaceIndicator(Transform target, DirectionIndicatorItem item, Sprite icon, float targetRadius)
    {
        Vector2 screenCenter = new Vector2(Screen.width, Screen.height) * 0.5f;
        Vector2 targetScreenPos = mainCamera.WorldToScreenPoint(target.position);

        Vector2 dir = targetScreenPos - screenCenter;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;
        dir.Normalize();

        // 從畫面外一個很遠的點，沿著方向反過來射回中心，確保射線起點在邊界 collider 外面
        // (若直接從畫面中心往外射，起點會在 collider 內部，Physics2D.Raycast 偵測不到)
        const float rayDistance = 10000f;
        Vector2 farPoint = screenCenter + dir * rayDistance;
        Vector2 edgePoint = screenCenter;
        RaycastHit2D hit = Physics2D.Raycast(farPoint, -dir, rayDistance);
        if (hit.collider == screenBoundary)
        {
            edgePoint = hit.point;
        }

        item.SetScreenPosition(edgePoint);
        item.SetDirection(dir);
        item.SetIcon(icon);

        float centerDistance = Vector2.Distance(player.position, target.position);
        item.SetDistance(Mathf.Max(centerDistance - targetRadius, 0f));
    }
}

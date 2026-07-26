using System.Collections.Generic;
using UnityEngine;

// 掛在想被方向指示器 (DirectionIndicatorUI) 追蹤的物件上，例如道具、據點
// 星球本身不需要另外加，DirectionIndicatorUI 會自動抓場景中所有 GravitySource
public class Trackable : MonoBehaviour
{
    [SerializeField] private Sprite icon;

    public Sprite Icon => icon;

    public static readonly List<Trackable> All = new List<Trackable>();

    private void OnEnable()
    {
        All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }
}

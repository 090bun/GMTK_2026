using UnityEngine;

public class DepthManager : MonoBehaviour
{
    [System.Serializable]
    public struct LayerDepth
    {
        public string layerName;
        public float zDepth;
    }

    // 攝影機在 z = -10 朝 +z 看，z 值越小離攝影機越近（越前面）
    // 由近到遠：Star → Player → Prop → BK_FT → BK_MD → BK_LS → BK_BK
    [SerializeField] private LayerDepth[] layerDepths = new LayerDepth[]
    {
        new LayerDepth { layerName = "Star", zDepth = 0f },
        new LayerDepth { layerName = "Player", zDepth = 2f },
        new LayerDepth { layerName = "Prop", zDepth = 4f },
        new LayerDepth { layerName = "BK_FT", zDepth = 6f },
        new LayerDepth { layerName = "BK_MD", zDepth = 9f },
        new LayerDepth { layerName = "BK_LS", zDepth = 12f },
        new LayerDepth { layerName = "BK_BK", zDepth = 15f }
    };

    private void Awake()
    {
        ApplyAllDepths();
    }

    // 遊戲一開始就依照每個物件的 Layer，統一套用對應的前後深度
    private void ApplyAllDepths()
    {
        Transform[] allTransforms = FindObjectsOfType<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            SetGrpDepth(t);
        }
    }

    // 給 Grp 設定 Z
    public void SetGrpDepth(Transform grp)
    {
        string grpLayer = LayerMask.LayerToName(grp.gameObject.layer);
        
        foreach (var layerDepth in layerDepths)
        {
            if (layerDepth.layerName == grpLayer)
            {
                Vector3 pos = grp.position;
                grp.position = new Vector3(pos.x, pos.y, layerDepth.zDepth);
                return;
            }
        }
    }
}
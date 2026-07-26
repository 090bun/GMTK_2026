using UnityEngine;

public class addAni : MonoBehaviour
{
    [SerializeField] private Vector3 tans = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 rota = new Vector3(0, 0, 360);
    [SerializeField] private float speed = 1f;

    [Header("飄動感 (各軸用不同相位/頻率疊加 Perlin Noise，避免路徑呈一直線來回)")]
    [SerializeField] private float noiseSpeed = 0.3f;
    [SerializeField, Range(0f, 1f)] private float noiseInfluence = 0.5f;

    private Vector3 basePos;
    private Vector3 baseRot;
    private Vector3 noiseSeed;

    void Start()
    {
        // 用 local 座標而非世界座標，避免父物件被其他腳本(例如 DirectionIndicatorUI)移動時互相衝突
        basePos = transform.localPosition;
        baseRot = transform.localEulerAngles;

        // 每個物件用不同的噪聲取樣起點，避免場上所有物件飄動同步
        noiseSeed = new Vector3(Random.value, Random.value, Random.value) * 100f;
    }

    void Update()
    {
        float t = Time.time;

        // 各軸給不同相位與頻率倍率，讓路徑不再是單一直線來回
        Vector3 wave = new Vector3(
            Mathf.Sin(t * speed),
            Mathf.Sin(t * speed * 1.13f + 1.7f),
            Mathf.Sin(t * speed * 0.87f + 3.1f));

        Vector3 noise = new Vector3(
            Mathf.PerlinNoise(noiseSeed.x, t * noiseSpeed) * 2f - 1f,
            Mathf.PerlinNoise(noiseSeed.y, t * noiseSpeed) * 2f - 1f,
            Mathf.PerlinNoise(noiseSeed.z, t * noiseSpeed) * 2f - 1f);

        Vector3 offset = Vector3.Lerp(wave, noise, noiseInfluence);

        transform.localPosition = basePos + Vector3.Scale(tans, offset);
        transform.localEulerAngles = baseRot + Vector3.Scale(rota, offset);
    }
}

using UnityEngine;

public class Fragment : MonoBehaviour
{
    [Header("跟隨設定（撿到後像被綁著跟在後面）")]
    [SerializeField] private float followDistance = 1.5f;
    [SerializeField] private float followSpring = 8f;
    [Range(0f, 1f)]
    [SerializeField] private float drag = 0.9f;
    [Header("引力影響程度（撿到後仍會受引力影響）")]
    [SerializeField] private float gravityInfluence = 1f;

    private Transform followTarget;
    private Vector2 velocity;
    private bool attached = false;
    private GravitySource[] gravitySources;
    private Collider col;

    private void Awake()
    {
        gravitySources = FindObjectsOfType<GravitySource>();
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (attached) return;
        if (!other.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        followTarget = player.GetTailEnd();
        player.SetTailEnd(transform);

        attached = true;
        col.enabled = false;
    }

    private void Update()
    {
        if (!attached) return;

        Vector2 totalGravity = CalculateTotalGravity();
        velocity += totalGravity * gravityInfluence * Time.deltaTime;

        Vector2 toSelf = (Vector2)transform.position - (Vector2)followTarget.position;
        float distance = toSelf.magnitude;
        if (distance > followDistance)
        {
            Vector2 pullDirection = distance > 0.0001f ? -toSelf / distance : Vector2.zero;
            velocity += pullDirection * (distance - followDistance) * followSpring * Time.deltaTime;
        }

        velocity *= Mathf.Pow(drag, Time.deltaTime);

        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    private Vector2 CalculateTotalGravity()
    {
        Vector2 gravity = Vector2.zero;
        foreach (GravitySource source in gravitySources)
        {
            Vector2 direction = source.GetGravityDirection(transform.position);
            float magnitude = source.GetGravityMagnitude(transform.position);
            gravity += direction * magnitude;
        }
        return gravity;
    }
}

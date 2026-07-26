using UnityEngine;

public class Porp_energy : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float Energy = 10f;

    private MusicCtl musicCtl;

    private void Awake() {
        musicCtl = FindObjectOfType<MusicCtl>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            playerController.AddEnergy(Energy);
            musicCtl?.PlayPropSound();
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

public class GapSetting : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject startGap;
    [SerializeField] private GameObject finishGap;

    private bool switched = false;

    private void Start()
    {
        startGap.SetActive(true);
        finishGap.SetActive(false);
    }

    private void Update()
    {
        if (switched) return;
        if (playerController.CollectedFragments < playerController.TotalFragments) return;

        switched = true;
        startGap.SetActive(false);
        finishGap.SetActive(true);
    }
}

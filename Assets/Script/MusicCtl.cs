using UnityEngine;

public class MusicCtl : MonoBehaviour
{
    [SerializeField] AudioClip BGM;
    [SerializeField] AudioClip[] BooutSound;
    [SerializeField] AudioClip[] PropSound;
    [SerializeField] AudioClip[] FailSound;
    [SerializeField] AudioSource seSource;

    private AudioSource audioSource;

    void Awake(){
        audioSource = seSource.GetComponent<AudioSource>();
        audioSource.clip = BGM;
        audioSource.Play();
    }

    public void PlayBooutSound(){
        PlayRandomSE(BooutSound);
    }

    public void PlayPropSound(){
        PlayRandomSE(PropSound);
    }

    public void PlayFailSound(){
        PlayRandomSE(FailSound);
    }

    private void PlayRandomSE(AudioClip[] clips){
        if(seSource != null && clips != null && clips.Length > 0){
            seSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}

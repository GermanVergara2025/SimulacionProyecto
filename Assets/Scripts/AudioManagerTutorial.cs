using UnityEngine;

public class AudioManagerTutorial : MonoBehaviour
{
    public static AudioManagerTutorial instance;

    [Header("Clips de sonido")]
    public AudioClip ballLaunchClip;
    public AudioClip pinHitClip;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton simple para que cualquier script pueda llamarlo
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayBallLaunch()
    {
        if (ballLaunchClip != null)
            audioSource.PlayOneShot(ballLaunchClip);
    }

    public void PlayPinHit()
    {
        if (pinHitClip != null)
            audioSource.PlayOneShot(pinHitClip);
    }
}

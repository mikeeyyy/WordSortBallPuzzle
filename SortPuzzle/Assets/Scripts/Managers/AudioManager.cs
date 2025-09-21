using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    public void PlaySfxOnShot(AudioClip Clip)
    {
        audioSource.PlayOneShot(Clip,1.0f);
    }

}

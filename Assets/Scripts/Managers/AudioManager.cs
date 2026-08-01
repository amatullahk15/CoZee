using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] AudioSource uiSource;
    [SerializeField] AudioClip clickClip;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (uiSource == null)
            uiSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayClick()
    {
        if (clickClip != null && uiSource != null)
            uiSource.PlayOneShot(clickClip);
    }
}

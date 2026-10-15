using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    void Awake()
    {
        I = this;
    }

    public void PlaySound(AudioClip sound)
    {
        sfxSource.PlayOneShot(sound);
    }

    public void PlayMusic(AudioClip music)
    {
        if(musicSource.clip == music) return;
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.Play();
    }
}
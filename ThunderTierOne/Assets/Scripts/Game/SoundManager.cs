using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance;

    [SerializeField] AudioClip[] sfxPlayer;
    [SerializeField] AudioSource audioSrc;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }

    public void Fire()
    {
        audioSrc.PlayOneShot(sfxPlayer[0]);
        audioSrc.pitch = 1.0f;
        audioSrc.volume = 1.0f;
    }
    public void Walk()
    {
        if (!audioSrc.isPlaying)
        {
            audioSrc.PlayOneShot(sfxPlayer[1]);
            audioSrc.pitch = 1.0f;
            audioSrc.volume = 0.8f;
        }
    }
    public void Crouching()
    {
        if (!audioSrc.isPlaying)
        {
            audioSrc.PlayOneShot(sfxPlayer[1]);
            audioSrc.pitch = 0.5f;
            audioSrc.volume = 0.3f;
        }
    }
 
    public void Reload()
    {
        audioSrc.PlayOneShot(sfxPlayer[2]);
        audioSrc.pitch = 1.0f;
        audioSrc.volume = 1.0f;
    }

}

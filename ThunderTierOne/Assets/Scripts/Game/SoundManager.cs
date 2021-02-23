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
    }
    public void Walk()
    {
        if (!audioSrc.isPlaying)
            audioSrc.PlayOneShot(sfxPlayer[1]);
    }
 
    public void Reload()
    {
        audioSrc.PlayOneShot(sfxPlayer[2]);
    }

}

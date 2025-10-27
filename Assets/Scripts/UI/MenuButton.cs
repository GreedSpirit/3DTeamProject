using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void HoverSoundPlay()
    {
        audioSource.clip = hoverSound;
        audioSource.Play();
    }

    public void ClickSoundPlay()
    {
        audioSource.clip = clickSound;
        audioSource.Play();
    }
}

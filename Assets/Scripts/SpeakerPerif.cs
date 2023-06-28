using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerPerif : LogicComponent
{
    public Color onColor;
    public Color offColor;
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        switch (inputNodes[0].signal)
        {
            case 0:
                if(audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                break;
            case 1:
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
                break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SlidersControl : MonoBehaviour
{
    [Header("Mixers")]
    public AudioMixer master_mixer;
    public AudioMixerGroup music_mixer;


    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ChangeMasterVolume(float vol)
    {
        master_mixer.SetFloat("Master_vol", Mathf.Log10(vol) * 20);
    }

    public void ChangeMusicVolume(float vol)
    {
        master_mixer.SetFloat("Music_vol", Mathf.Log10(vol) * 20);
    }
}

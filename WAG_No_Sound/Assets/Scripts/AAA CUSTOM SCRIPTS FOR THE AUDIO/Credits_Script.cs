using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits_Script : MonoBehaviour
{
    public AudioSource mySong;
    // Start is called before the first frame update
    void Start()
    {

        mySong.loop = true;
        mySong.Play(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator FadeOutAudioSource(AudioSource music)
    {
        for (float i = 1; i > 0; i -= Time.deltaTime / 3f)
        {
            music.volume = 100 * Mathf.Lerp(100,0,i);
            yield return null;
        }
    }

    public IEnumerator FadeOutGOAudioSource(float time)
    {
        for (float i = time; i > 0.0f; i -= Time.deltaTime)
        {
            mySong.volume = Mathf.Lerp(0.0f, 1.0f, i/time);
            yield return null;
        }
    }
}

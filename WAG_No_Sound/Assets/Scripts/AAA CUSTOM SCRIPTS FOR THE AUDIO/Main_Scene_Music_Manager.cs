using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main_Scene_Music_Manager : MonoBehaviour
{

    public AudioSource day_loop;
    // Start is called before the first frame update
    void Start()
    {
        //Get our day music and loop & play
        day_loop.loop = true;
        day_loop.Play(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

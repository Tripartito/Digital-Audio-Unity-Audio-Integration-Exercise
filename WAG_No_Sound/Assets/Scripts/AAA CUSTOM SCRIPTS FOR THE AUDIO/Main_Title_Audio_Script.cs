using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main_Title_Audio_Script : MonoBehaviour
{

    public AudioSource pre_enter;
    public AudioSource main_loop;

    // Start is called before the first frame update
    void Start()
    {
        pre_enter.Play(0);

        main_loop.loop = true;
        main_loop.PlayDelayed(6.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

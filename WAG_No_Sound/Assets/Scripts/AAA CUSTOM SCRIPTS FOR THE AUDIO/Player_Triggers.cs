using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Triggers : MonoBehaviour
{
    public Main_Scene_Music_Manager m_manager;

    public Collider Village;
    public Collider Cave;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Here we will call the music manager script to do stuff accordingly
        if (other == Village || other == Cave)
        {
            m_manager.DetectMusicChange(other);
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == Village || other == Cave)
        {
            m_manager.ReturningToForest();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

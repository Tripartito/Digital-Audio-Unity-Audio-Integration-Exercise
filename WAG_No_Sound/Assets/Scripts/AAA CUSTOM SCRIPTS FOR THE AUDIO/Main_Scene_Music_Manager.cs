using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main_Scene_Music_Manager : MonoBehaviour
{

    [Header("Areas Music")]
    public AudioSource Desert;
    public AudioSource Dungeon;
    public AudioSource Village;
    public AudioSource Woodlands_day;
    public AudioSource Woodlands_night;

    public Collider village_col;
    public Collider dungeon_col;

    public DayNightCycle day_night_cycle;


    private GameObject desert;
    private GameObject dungeon;
    private GameObject village;
    private AudioSource Current_AS;

    private bool day = true;
    private bool day_was = true;

    enum SongPlaying {
        DESERT = 0,
        DUNGEON,
        VILLAGE,
        WOODLANDS_DAY,
        WOODLANDS_NIGHT
    }

    SongPlaying song_playing;

    // Start is called before the first frame update
    void Start()
    {
        //Get our day music and loop & play
        Woodlands_day.loop = true;
        Woodlands_day.Play(0);

        Current_AS = Woodlands_day;

        village = GameObject.Find("Village");
        dungeon = GameObject.Find("Cave");

        song_playing = SongPlaying.WOODLANDS_DAY;
    }

    // Update is called once per frame
    void Update()
    {
        if(Current_AS == Woodlands_day || Current_AS == Woodlands_night)
        if (day_night_cycle.timeOfDay > 6 && day_night_cycle.timeOfDay < 18)
        {
                day = true;
                if (day_was == false)
                {
                    Current_AS.Stop();

                    Current_AS = Woodlands_day;
                    Current_AS.Play();
                    Current_AS.loop = true;
                }
        }
        else
        {
                day = false;

                if (day_was == true)
                {
                    Current_AS.Stop();

                    Current_AS = Woodlands_night;
                    Current_AS.Play();
                    Current_AS.loop = true;
                }
            }




        day_was = day;
    }

    void StopCurrentSong()
    {
        Current_AS.Stop();
    }

    public void DetectMusicChange(Collider collider)
    {
        if (collider == village_col)
        {
            song_playing = SongPlaying.VILLAGE;
            Current_AS.Stop();

            Village.Play(0);
            Village.loop = true;
            Current_AS = Village;
        }
       else if(collider == dungeon_col)
        {
            song_playing = SongPlaying.DUNGEON;
            Current_AS.Stop();
            Dungeon.Play(0);
            Dungeon.loop = true;
            Current_AS = Dungeon;
        }
    }

    public void ReturningToForest()
    {
        if (day == true)
        {
            song_playing = SongPlaying.WOODLANDS_DAY;
            Current_AS.Stop();
            Woodlands_day.Play(0);
            Woodlands_day.loop = true;
            Current_AS = Woodlands_day;
        }
        else
        {
            song_playing = SongPlaying.WOODLANDS_NIGHT;
            Current_AS.Stop();
            Woodlands_night.Play(0);
            Woodlands_night.loop = true;
            Current_AS = Woodlands_night;
        }
    }
}

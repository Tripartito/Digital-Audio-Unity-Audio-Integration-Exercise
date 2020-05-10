////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class PlayerFoot : MonoBehaviour
{
    public MaterialChecker materialChecker;
    public AK.Wwise.Event FootstepSound;

    private AdventuressAnimationEventHandler playerEventHandler;
    private AudioSource audioSource;

    #region private variables
    private bool inWater;
    #endregion
    private void Awake()
    {
        playerEventHandler = GameObject.Find("Player").GetComponent<AdventuressAnimationEventHandler>();
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayFootstepSound(uint speedType, float volume)
    {
        if (!inWater)
        {
            int materialID;

            switch (materialChecker.CheckMaterial(gameObject))  //This also sets the material if a SoundMaterial is found!
            {
                case "Surface_Type / Dirt":
                    materialID = 0;
                    break;
                case "Surface_Type / Grass":
                    materialID = 1;
                    break;
                case "Surface_Type / Rubble":
                    materialID = 2;
                    break;
                case "Surface_Type / Sand":
                    materialID = 3;
                    break;
                case "Surface_Type / Stone":
                    materialID = 4;
                    break;
                case "Surface_Type / Wood":
                    materialID = 6;
                    break;
                default:
                    materialID = 0;
                    break;
            }

            audioSource.PlayOneShot(playerEventHandler.stepSounds[materialID, speedType, Random.Range(0, 6)], volume);
        }
        else
        {
            audioSource.PlayOneShot(playerEventHandler.stepSounds[5, speedType, Random.Range(0, 6)], volume);
        }

        FootstepSound.Post(gameObject);
    }

    public void EnterWaterZone()
    {
        inWater = true;
    }

    public void ExitWaterZone()
    {
        inWater = false;
    }

}

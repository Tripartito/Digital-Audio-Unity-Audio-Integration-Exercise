////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinPickup : MonoBehaviour {

	private AudioSource coinAudioSource;
	private AudioClip[] coinPickupAudioClip;
	private AudioClip coinSpawnAudioClip;

    public bool playSpawnSoundAtSpawn = true;
    public AK.Wwise.Event spawnSound;

	void Awake()
	{
		coinAudioSource = GetComponent<AudioSource>();
		coinSpawnAudioClip = Resources.Load("Objects/Coin_Spawn") as AudioClip;
		coinPickupAudioClip = new AudioClip[4];

		for (int i = 0; i < 4; ++i)
			coinPickupAudioClip[i] = Resources.Load("Objects/Pickups/BAS_Pickup_Coin_0" + (i + 1).ToString()) as AudioClip;
	}

	void Start(){
        if (playSpawnSoundAtSpawn){
            spawnSound.Post(gameObject);
			coinAudioSource.PlayOneShot(coinSpawnAudioClip);
		}
	}

	public void AddCoinToCoinHandler(){
		InteractionManager.SetCanInteract(this.gameObject, false);
		GameManager.Instance.coinHandler.AddCoin ();
		//Destroy (gameObject, 0.1f); //TODO: Pool instead?

		coinAudioSource.PlayOneShot(coinPickupAudioClip[Random.Range(0, 4)]);
	}
}

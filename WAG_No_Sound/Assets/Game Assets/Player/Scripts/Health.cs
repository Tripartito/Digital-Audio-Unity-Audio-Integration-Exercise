////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour, IDamageable {

	private AudioSource playerAudioSource;
	private readonly AudioClip[] damageAudioClips = new AudioClip[13];
	private readonly AudioClip[] deathAudioClips = new AudioClip[3];
	public void Awake()
	{
		playerAudioSource = GameObject.Find("Player").GetComponent<AudioSource>();

		for (int i = 0; i < 11; ++i)
		{
			if (i < 7)
				damageAudioClips[i] = Resources.Load("Character/Adventuress_damaged_0" + (i + 1).ToString()) as AudioClip;
			else
				damageAudioClips[i] = Resources.Load("Character/Adventuress_damaged_" + (i + 3).ToString()) as AudioClip;
		}

		for (int i = 0; i < 3; ++i)
		{
			deathAudioClips[i] = Resources.Load("Character/Adventuress_Death_0" + (i + 1).ToString()) as AudioClip;
		}
	}

	public void OnDamage(Attack a)
	{
		PlayerManager.Instance.TakeDamage(a);

		if (PlayerManager.Instance.HealthOfPlayer < 0.0f)
			playerAudioSource.PlayOneShot(deathAudioClips[Random.Range(0, 3)]);
		else
			playerAudioSource.PlayOneShot(damageAudioClips[Random.Range(0, 11)]);
	}
}

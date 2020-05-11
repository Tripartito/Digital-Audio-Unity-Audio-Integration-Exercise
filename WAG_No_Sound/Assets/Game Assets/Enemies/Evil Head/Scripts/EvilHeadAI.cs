////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EvilHeadAI : Creature
{
    [Header("Audio Mixer Group for SFX")]
    public AudioMixerGroup mixer_group;


    private AudioSource headAudioSource;

    private AudioClip hoverAudioClip;
    private AudioClip telegraphAudioClip;

    private AudioClip[] chargeAudioClips;
    private AudioClip[] biteAudioClips;
    private AudioClip[] hurtAudioClips;
    private AudioClip[,] deathAudioClips;

    [Header("Evil Head Specifics")]
    public GameObject SmokeFX;
    public GameObject deathFX;
    public GameObject keepOnDeath;

    [Header("Wwise")]
    public AK.Wwise.RTPC MovementRTPC;
    public AK.Wwise.Event HoverSoundStart;
    public AK.Wwise.Event HoverSoundEnd;
    public AK.Wwise.Event BiteSound;
    public AK.Wwise.Event ChargeSound;
    public AK.Wwise.Event TelegraphSound;

    #region private variables
    private Vector3 targetLocation = Vector3.zero;
    private IEnumerator chargeRoutine;

    //Cached Animator hashes
    private readonly int spawnHash = Animator.StringToHash("Spawn");
    private readonly int deathHash = Animator.StringToHash("Death");
    #endregion

    private void SetMovementSpeed(float speed) {
        MovementRTPC.SetValue(gameObject, speed);
    }

    private void Awake()
    {
        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }

        headAudioSource = GetComponent<AudioSource>();
        hoverAudioClip = Resources.Load("Creatures/BAS_Evil_Head_Hover_LP") as AudioClip;
        telegraphAudioClip = Resources.Load("Creatures/BAS_Evil_Head_Charge_Bite") as AudioClip;

        biteAudioClips = new AudioClip[3];
        chargeAudioClips = new AudioClip[3];
        hurtAudioClips = new AudioClip[3];
        deathAudioClips = new AudioClip[3, 2];
        for (int i = 0; i < 3; ++i)
        {
            biteAudioClips[i] = Resources.Load("Creatures/BAS_Evil_Head_Bite_0" + (i + 1).ToString()) as AudioClip;
            chargeAudioClips[i] = Resources.Load("Creatures/BAS_Evil_Head_Charge_0" + (i + 1).ToString()) as AudioClip;
            hurtAudioClips[i] = Resources.Load("Creatures/BAS_Evil_Head_Hurt_0" + (i + 1).ToString()) as AudioClip;
            deathAudioClips[i, 0] = Resources.Load("Creatures/BAS_Evil_Head_Death_0" + (i + 1).ToString()) as AudioClip;
            deathAudioClips[i, 1] = Resources.Load("Creatures/BAS_Evil_Head_Death_Vox_0" + (i + 1).ToString()) as AudioClip;
        }
    }

    public override void Start(){
		base.Start();
        HoverSoundStart.Post(this.gameObject);

        headAudioSource.loop = true;
        headAudioSource.clip = hoverAudioClip;
        headAudioSource.Play();
    }

    public override void OnSpotting()
    {
        base.OnSpotting();
        anim.SetTrigger(spawnHash);
		SmokeFX.SetActive(true);
    }

    public override void Move(Vector3 yourPosition, Vector3 targetPosition)
    {
        base.Move(yourPosition, targetPosition);
        SetMovementSpeed(20f);
    }

    public override void Anim_MeleeAttack()
    {
        
        base.Anim_MeleeAttack();
        SetMovementSpeed(100f);
    }

    /// <summary>
    /// Called from Animation Event. This happens when the Evil Head telegraphs its attack!
    /// </summary>
    public void DisableMovement()
    {
        thisNavMeshAgent.destination = transform.position;
        targetLocation = targetOfNPC.transform.position + Vector3.up;
        StartCoroutine(RotateTowardsTarget(targetLocation, 1f));
        TelegraphSound.Post(gameObject);

        headAudioSource.PlayOneShot(telegraphAudioClip, 1.0f);
    }


    public void ReenableMovement()
    {
        SetMovementSpeed(0f);
        thisNavMeshAgent.SetDestination(transform.position);
        //thisNavMeshAgent.SetDestination(targetOfNPC == null ? transform.position : targetOfNPC.transform.position);
    }

    /// <summary>
    /// Called from Animation Event. Initiates the charging towards the player!
    /// </summary>
    public void Charge()
    {
        if (chargeRoutine != null)
        {
            StopCoroutine(chargeRoutine);
        }
        chargeRoutine = ChargeTowardsPlayer(0.5f);
        StartCoroutine(chargeRoutine);
    }

    IEnumerator RotateTowardsTarget(Vector3 targetLocation, float seconds)
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetLocation - transform.position);

        for (float t = 0; t < 1; t += Time.deltaTime / seconds)
        {
            float s = Curves.Instance.Overshoot.Evaluate(t);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, s);
            yield return null;
        }
    }

    IEnumerator ChargeTowardsPlayer(float seconds)
    {
        //print(Time.realtimeSinceStartup + ": ChargeTowardsPlayer");
        TelegraphSound.Stop(gameObject,0, AkCurveInterpolation.AkCurveInterpolation_Linear);
        ChargeSound.Post(gameObject);

        headAudioSource.PlayOneShot(chargeAudioClips[Random.Range(0, 3)]);

        Vector3 currentPosition = transform.position;
        Vector3 destination = targetLocation + ((targetLocation) - currentPosition).normalized * 2f;

        for (float t = 0; t < 1; t += Time.deltaTime / seconds)
        {
            float s = Curves.Instance.SmoothOut.Evaluate(t);
            Vector3 nextPosition = Vector3.Lerp(currentPosition, destination, s);
            transform.position = nextPosition;
            yield return null;
        }
        ReenableMovement();
    }

    public override void OnDeathAnimation()
    {
        base.OnDeathAnimation();
        isAlive = false;

        if (chargeRoutine != null)
        {
            StopCoroutine(chargeRoutine);
        }

        thisNavMeshAgent.nextPosition = transform.position;

        anim.SetTrigger(deathHash);
    }

    /// <summary>
    /// Called from the EvilHeadMethodRerouter class when an object is hit during charge.
    /// </summary>
    public void StopCharge()
    {
        if (chargeRoutine != null)
        {
            StopCoroutine(chargeRoutine);
        }
        ReenableMovement();
    }

    override public void OnDamageReset()
    {
        headAudioSource.PlayOneShot(hurtAudioClips[Random.Range(0, 3)]);
    }

    public void Explode()
    {
        SetMovementSpeed(0f);
        //print(Time.realtimeSinceStartup + ": Explode");
        HoverSoundEnd.Post(this.gameObject);

        headAudioSource.Stop();

        GameObject fx = (GameObject)Instantiate(deathFX, transform.position, Quaternion.identity);

        AudioSource audSrc = fx.AddComponent<AudioSource>();
        audSrc.outputAudioMixerGroup = mixer_group;
        audSrc.spatialBlend = 1.0f;

        int rng = Random.Range(0, 3);
        audSrc.PlayOneShot(deathAudioClips[rng, 0]);
        audSrc.PlayOneShot(deathAudioClips[rng, 1]);

        Destroy(fx, 5f);

        //This following section makes sure that the particles of the Evil Head doesn't just suddenly disappear in a flash. Rather, they stop emitting and are removed after some time 
        if (keepOnDeath != null)
        {
            keepOnDeath.transform.parent = null;
            foreach (ParticleSystem p in keepOnDeath.GetComponentsInChildren<ParticleSystem>())
            {
                p.Stop();
            }
            Destroy(keepOnDeath, 5f);
        }

        PlayCreatureDeathSound();
        Destroy(gameObject);
    }

    /// <summary>
    /// Called from Animation Event. Initiates the charging towards the player!
    /// </summary>
    public void PlayBiteSound()
    {
        BiteSound.Post(this.gameObject);

        headAudioSource.PlayOneShot(biteAudioClips[Random.Range(0, 3)]);
    }
}

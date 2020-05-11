////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class EvilSpitPlantAI : Creature
{
    private AudioSource plantAudioSource;

    private AudioClip hoverAudioClip;
    private AudioClip telegraphAudioClip;

    private AudioClip[] chargeAudioClips;
    private AudioClip[] shotAudioClips;
    private AudioClip[] hurtAudioClips;
    private AudioClip[] deathAudioClips;
    private AudioClip[] headAudioClips;

    [Header("Custom Creature Options:")]
    public GameObject bulletPrefab;
    public GameObject chargeParticles;
    public GameObject shootParticles;
    public GameObject spitBulletSpawnPoint;

    #region private variables
    private bool hasSpawned = false;
    private bool lockRotation = false;
    private readonly int spawnHash = Animator.StringToHash("Spawn");
    private readonly int despawnHash = Animator.StringToHash("Despawn");
    private readonly int isAliveHash = Animator.StringToHash("IsAlive");
    #endregion

    [Header("WWISE")]
    public AK.Wwise.Event AttackSound = new AK.Wwise.Event();
    public AK.Wwise.Event ChargeSound = new AK.Wwise.Event();
    public AK.Wwise.Event Death_Headfall = new AK.Wwise.Event();
    public AK.Wwise.Event asdasdasfasda;

    public void Awake()
    {
        plantAudioSource = GetComponent<AudioSource>();

        hoverAudioClip = Resources.Load("Creatures/BAS_Evil_Head_Hover_LP") as AudioClip;
        telegraphAudioClip = Resources.Load("Creatures/BAS_Evil_Head_Charge_Bite") as AudioClip;

        shotAudioClips = new AudioClip[3];
        chargeAudioClips = new AudioClip[3];
        hurtAudioClips = new AudioClip[6];
        deathAudioClips = new AudioClip[3];
        headAudioClips = new AudioClip[3];
        for (int i = 0; i < 3; ++i)
        {
            shotAudioClips[i] = Resources.Load("Creatures/BAS_Evil_SpitPlant_Shoot_0" + (i + 1).ToString()) as AudioClip;
            chargeAudioClips[i] = Resources.Load("Creatures/BAS_Evil_SpitPlant_Charge_0" + (i + 1).ToString()) as AudioClip;
            deathAudioClips[i] = Resources.Load("Creatures/BAS_Evil_SpitPlant_Death_0" + (i + 4).ToString()) as AudioClip;
            headAudioClips[i] = Resources.Load("Creatures/BAS_Evil_SpitPlant_Death_Headfall_0" + (i + 1).ToString()) as AudioClip;
        }

        for (int i = 0; i < 6; ++i)
        {
            hurtAudioClips[i] = Resources.Load("Creatures/BAS_Evil_SpitPlant_Hurt_0" + (i + 1).ToString()) as AudioClip;
        }
    }

    public override void OnSpotting()
    {
        base.OnSpotting();

        if (!hasSpawned)
        {
            anim.SetTrigger(spawnHash);
            hasSpawned = true;
        }
    }

    public override void OffSpotting()
    {
        base.OffSpotting();

        if (hasSpawned)
        {
            anim.SetTrigger(despawnHash);
            hasSpawned = false;
        }
    }

    /// <summary>
    /// Called from Animation Event. This shoots the projectile!
    /// </summary>
    public void Shoot()
    {
        if (targetOfNPC != null && !GameManager.Instance.AIPaused)
        {
            AttackSound.Post(this.gameObject);

            plantAudioSource.PlayOneShot(shotAudioClips[Random.Range(0, 3)]);

            GameObject bullet = Instantiate(bulletPrefab, spitBulletSpawnPoint.transform.position, Quaternion.LookRotation(transform.forward)) as GameObject; //TODO: Pool spitbullets
            bullet.GetComponent<EvilSpitPlantProjectile>().parent = gameObject;
            bullet.GetComponent<EvilSpitPlantProjectile>().damage = this.AttackDamage;

            GameObject bulletSpawnFX = Instantiate(shootParticles, spitBulletSpawnPoint.transform.position, Quaternion.identity, spitBulletSpawnPoint.transform) as GameObject; //TODO: Pool spitbullet spawn particles
            Destroy(bulletSpawnFX, 5f);
        }
    }

    public void PlayChargeSound()
    {
        ChargeSound.Post(gameObject);
        plantAudioSource.PlayOneShot(chargeAudioClips[Random.Range(0, 3)], 0.5f);
    }

    /// <summary>
    /// Called from Animation Event. This happens when the Evil Spit Plant telegraphs its attack!
    /// </summary>
    public void ChargeUp()
    {
        if (chargeParticles != null)
        {
            GameObject chargeFX = Instantiate(chargeParticles, spitBulletSpawnPoint.transform.position, Quaternion.identity, spitBulletSpawnPoint.transform) as GameObject; //TODO: Pool charge particles.
            Destroy(chargeFX, 5f);
        }
    }

    public override void Move(Vector3 yourPosition, Vector3 targetPosition)
    {
        if (!lockRotation)
        {
            Quaternion newRotation = Quaternion.LookRotation(targetOfNPC.transform.position - transform.position);
            RotationObject.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * NoNavRotationSpeed);
        }
    }

    public override void OnDamageReset()
    {
        base.OnDamageReset();
        lockRotation = false;

        int rng = Random.Range(0, 3);
        plantAudioSource.PlayOneShot(hurtAudioClips[rng]);
        plantAudioSource.PlayOneShot(hurtAudioClips[rng + 3]);
    }

    /// <summary>
    /// Called from Animation Event.
    /// </summary>
    public void LockRotation()
    {
        lockRotation = true;
    }

    /// <summary>
    /// Called from Animation Event. This happens when the Evil Head telegraphs its attack!
    /// </summary>
    public void UnlockRotation()
    {
        lockRotation = false;
    }

    public override void OnDeathAnimation()
    {
        base.OnDeathAnimation();

        anim.SetBool(isAliveHash, false);

        float angle = Vector3.Angle(RotationObject.transform.forward, LastAttack.attackDir);
        if (Mathf.Abs(angle) > 90)
        {
            anim.SetTrigger(DeathAnimations.FrontTrigger);
        }
        else
        {
            anim.SetTrigger(DeathAnimations.BehindTrigger);
        }
        LockRotation();

        plantAudioSource.PlayOneShot(deathAudioClips[Random.Range(0, 3)]);
    }

    public void OnDeathHeadFall()
    {
        Death_Headfall.Post(this.gameObject);
        plantAudioSource.PlayOneShot(headAudioClips[Random.Range(0, 3)]);
    }
}

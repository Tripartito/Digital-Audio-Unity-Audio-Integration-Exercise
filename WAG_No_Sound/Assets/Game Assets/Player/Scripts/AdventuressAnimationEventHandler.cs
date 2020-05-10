////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using UnityEngine;
using System.Collections;

public class AdventuressAnimationEventHandler : MonoBehaviour
{
    [Header("Wwise")]
    public AK.Wwise.Event Swing = new AK.Wwise.Event();
    public AK.Wwise.Event GetItem = new AK.Wwise.Event();
    public AK.Wwise.Trigger GetItemStinger = new AK.Wwise.Trigger();

    //--------------------------------
    // Walk, Run
    // Dirt, Grass, Rubble, Sand, Stone, Water, Wood
    // 1-6
    public AudioClip[,,] stepSounds = new AudioClip[7, 2, 6]; //Movement/Material/Variation
    private readonly string[] clipNamesID = { "_dirt", "_grass", "_rubble", "_sand", "_stone", "_water", "_wood" };
    private readonly string[] movTypes = { "_run", "_walk" };

    private readonly AudioClip[] lightSwingSounds = new AudioClip[9];
    private readonly AudioClip[] heavySwingSounds = new AudioClip[4];

    private AudioSource characterAudioSource;
    //--------------------------------

    [Header("Object Links")]
    [SerializeField]
    private Animator playerAnimator;

    [SerializeField]
    private GameObject runParticles;

    private PlayerFoot foot_L;
    private PlayerFoot foot_R;

    #region private variables
    private bool hasPausedMovement;
    private readonly int canShootMagicHash = Animator.StringToHash("CanShootMagic");
    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");
    #endregion

    private void Awake()
    {
        characterAudioSource = GetComponent<AudioSource>();
        
        //Footsteps
        for (int i = 0; i < 7; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                for (int k = 0; k < 6; ++k)
                {
                    stepSounds[i, j, k] = Resources.Load("Character/Footsteps/BAS_Player_Footstep" + clipNamesID[i] + movTypes[j] + "_0" + (k + 1).ToString()) as AudioClip;
                }
            }
        }

        //Swing (Extertion)
        for (int i = 0; i < 9; ++i)
        {
            lightSwingSounds[i] = Resources.Load("Character/Adventuress_exertion_0" + (i + 1).ToString()) as AudioClip;
        }

        for (int i = 0; i < 4; ++i)
        {
            heavySwingSounds[i] = Resources.Load("Character/Adventuress_exertion_" + (i + 10).ToString()) as AudioClip;
        }

        //-----------------------------------

        GameObject L = GameObject.Find("toe_left");
        GameObject R = GameObject.Find("toe_right");
        if (L != null)
        {
            foot_L = L.GetComponent<PlayerFoot>();
        }
        else {
            print("Left foot missing");
        }
        if (R != null)
        {
            foot_R = R.GetComponent<PlayerFoot>();
        }
        else
        {
            print("Right foot missing");
        }
    }


    void enableWeaponCollider()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.equippedWeaponInfo != null)
        {
            PlayerManager.Instance.equippedWeaponInfo.EnableHitbox();
        }
    }

    void disableWeaponCollider()
    {
        if (PlayerManager.Instance != null && PlayerManager.Instance.equippedWeaponInfo != null)
        {
            PlayerManager.Instance.equippedWeaponInfo.DisableHitbox();
        }

    }

    void ScreenShake()
    {
        PlayerManager.Instance.cameraScript.CamShake(new PlayerCamera.CameraShake(0.4f, 0.7f));
    }

    bool onCooldown = false;
    public enum FootSide { left, right };
    public void TakeFootstep(FootSide side)
    {
        if (foot_L != null && foot_R != null) {
            if (!PlayerManager.Instance.inAir && !onCooldown)
            {
                Vector3 particlePosition;

                uint speedType = 1;
                float volume = 0.6f;
                if (PlayerManager.Instance.isSprinting)
                {
                    speedType = 0;
                    volume = 1.0f;
                }

                if (side == FootSide.left )
                {
                    if (foot_L.FootstepSound.Validate())
                    { 
                        foot_L.PlayFootstepSound(speedType, volume);
                        particlePosition = foot_L.transform.position;
                        FootstepParticles(particlePosition);
                    }
                }
                else
                {
                    if (foot_R.FootstepSound.Validate())
                    {
                        foot_R.PlayFootstepSound(speedType, volume);
                        particlePosition = foot_R.transform.position;
                        FootstepParticles(particlePosition);
                    }
                }
            }
        }
    }

    void FootstepParticles(Vector3 particlePosition) {
        GameObject p = Instantiate(runParticles, particlePosition + Vector3.up * 0.1f, Quaternion.identity) as GameObject;
        p.transform.parent = SceneStructure.Instance.TemporaryInstantiations.transform;
        Destroy(p, 5f);
        StartCoroutine(FootstepCooldown());
    }

    IEnumerator FootstepCooldown()
    {
        onCooldown = true;
        yield return new WaitForSecondsRealtime(0.2f);
        onCooldown = false;
    }

    void ReadyToShootMagic()
    {
        PlayerManager.Instance.playerAnimator.SetBool(canShootMagicHash, true);
    }

    public enum AttackState { NotAttacking, Attacking };
    void SetIsAttacking(AttackState state)
    {
        if (state == AttackState.NotAttacking)
        {
            playerAnimator.SetBool(isAttackingHash, false);
        }
        else
        {
            playerAnimator.SetBool(isAttackingHash, true);
        }
    }

    public void Weapon_SwingEvent()
    {
        // PLAY SOUND
        Weapon W = PlayerManager.Instance.equippedWeaponInfo;
        W.WeaponTypeSwitch.SetValue(PlayerManager.Instance.weaponSlot);
        Swing.Post(PlayerManager.Instance.weaponSlot);

        if (W.WeaponTypeSwitch.Name == "Player_Weapon_Type / Hammer" || W.WeaponTypeSwitch.Name == "Player_Weapon_Type / PickAxe")
            characterAudioSource.PlayOneShot(heavySwingSounds[Random.Range(0, 4)]);
        else
            characterAudioSource.PlayOneShot(lightSwingSounds[Random.Range(0, 9)]);
    }

    public void PauseMovement()
    {
        if (!hasPausedMovement)
        {
            hasPausedMovement = true;
            PlayerManager.Instance.motor.SlowMovement();
        }
    }

    public void ResumeMovement()
    {
        if (hasPausedMovement)
        {
            hasPausedMovement = false;
            PlayerManager.Instance.motor.UnslowMovement();
        }
    }

    public void FreezeMotor()
    {
        StartCoroutine(PickupEvent());
    }

    private IEnumerator PickupEvent()
    {
        PlayerManager.Instance.PauseMovement(gameObject);
        yield return new WaitForSeconds(2f);
        PlayerManager.Instance.ResumeMovement(gameObject);
    }

    public void PickUpItem()
    {
        PlayerManager.Instance.PickUpEvent();
        GetItem.Post(this.gameObject);
        GetItemStinger.Post(GameManager.Instance.MusicGameObject);
    }

    public void WeaponSound()
    {
        Weapon EquippedWeapon = PlayerManager.Instance.equippedWeaponInfo;
        EquippedWeapon.WeaponImpact.Post(EquippedWeapon.transform.parent.gameObject);
    }
}

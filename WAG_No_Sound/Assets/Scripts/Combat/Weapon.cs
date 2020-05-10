////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2018 Audiokinetic Inc. / All Rights Reserved
//
////////////////////////////////////////////////////////////////////////

﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Weapon : MonoBehaviour, IInteractable
{
    private string lastMaterialHit;
    private AudioSource weaponAudioSource;

    //Dagger, Sword, Axe, PickAxe, Hammer
    //Dirt, Grass, Stone, Sand, Wood, Crate, Barrel, Agent
    private readonly Dictionary<string, uint> surfaceTypeMap = new Dictionary<string, uint>();
    private readonly Dictionary<string, uint> weaponTypeMap = new Dictionary<string, uint>();
    private readonly AudioClip[,] weaponAudioClips = new AudioClip[5, 8];
    private AudioClip weaponPickupAudioClip;
    private AudioClip[] weaponEquipAudioClips;

    [Header("WWISE")]
    public AK.Wwise.Event WeaponImpact = new AK.Wwise.Event();
    public AK.Wwise.Switch WeaponTypeSwitch = new AK.Wwise.Switch();

    [Header("Combo Actions")]
    //public AK.Wwise.Event ComboEvent = new AK.Wwise.Event();
    public AK.Wwise.State WeaponCombo1 = new AK.Wwise.State();
    public AK.Wwise.State WeaponCombo2 = new AK.Wwise.State();
    public AK.Wwise.State WeaponCombo3 = new AK.Wwise.State();
    [Space(20f)]

    public WeaponTypes weaponType = WeaponTypes.Dagger;
    public WeaponAnimationTypes weaponAnimationType = WeaponAnimationTypes.OneHanded;

    public bool equipped = false;
    public bool playerWeapon = false;

    [Header("Weapon Objects")]
    public GameObject prefab;
    public Collider hitbox;
    public GameObject hitEffect;

    [Header("Weapon Stats")]
    public float BaseDamage;
    public float attackCooldown;
    public float attackFrame;
    public float animationSpeedMultiplier = 1;
    public float knockbackStrength = 1f;
    public bool PickupEventOnPickup = true;

    [Header("Combo Info")]
    public float comboCompletionBonusDamage = 0;
    public int maxComboHits;
    public float postComboCooldown;
    public bool swingDash;
    public float dashAmount;
    public bool TriggerDamageAllowed = false;

    public UnityEvent OnEquip;
    public UnityEvent OnUnequip;

    private List<GameObject> alreadyHitObjects = new List<GameObject>();

    #region hidden public variables
    [HideInInspector]
    public bool applyBonusDamage = false;
    #endregion

    public void Awake()
    {
        weaponAudioSource = GameObject.Find("WeaponHolder").GetComponent<AudioSource>();

        weaponPickupAudioClip = Resources.Load("Character/Weapons/BAS_pickUpWeapon_001") as AudioClip;
        weaponEquipAudioClips = new AudioClip[5]
        {
            Resources.Load("Objects/Pickups/BAS_Pickup_WeaponType_Dagger") as AudioClip,
            Resources.Load("Objects/Pickups/BAS_Pickup_WeaponType_Sword_01") as AudioClip,
            Resources.Load("Objects/Pickups/BAS_Pickup_WeaponType_Axe_01") as AudioClip,
            Resources.Load("Objects/Pickups/BAS_Pickup_WeaponType_Pickaxe_01") as AudioClip,
            Resources.Load("Objects/Pickups/BAS_Pickup_WeaponType_Hammer_01") as AudioClip
        };

        // Weapon Hit Sounds BEGIN ----------------------------------------------------------------
        weaponTypeMap.Add("Player_Weapon_Type / Dagger", 0);
        weaponTypeMap.Add("Player_Weapon_Type / Sword", 1);
        weaponTypeMap.Add("Player_Weapon_Type / Axe", 2);
        weaponTypeMap.Add("Player_Weapon_Type / PickAxe", 3);
        weaponTypeMap.Add("Player_Weapon_Type / Hammer", 4);

        surfaceTypeMap.Add("Surface_Type / Dirt", 0);
        surfaceTypeMap.Add("Surface_Type / Grass", 1);
        surfaceTypeMap.Add("Surface_Type / Leaves", 2);
        surfaceTypeMap.Add("Surface_Type / Stone", 3);
        surfaceTypeMap.Add("Surface_Type / Wood", 4);
        surfaceTypeMap.Add("Surface_Type / Barrel", 5);
        surfaceTypeMap.Add("Surface_Type / Crate", 6);
        surfaceTypeMap.Add("Surface_Type / Agent", 7);

        //Dagger, Sword, Axe, PickAxe, Hammer
        for (int i = 0; i < 5; ++i)
        {
            //Dirt, Grass, Leaves, Stone, Wood, Crate, Barrel, Agent
            for (int j = 0; j < 8; ++j)
            {
                string loadString = "Character/";

                if (j == 7) //"Impact_Bump" //Agent (default)
                {
                    loadString += "Weapons/Impact_Bump";
                    weaponAudioClips[i, j] = Resources.Load(loadString) as AudioClip;
                }
                //"BAS_Impact_Hammer_SurfaceType_Barrel"  // Barrles and Crates
                //"BAS_Impact_Axe_SurfaceType_Crate"
                else if (j > 4)
                {
                    loadString += "Impact/BAS_Impact";

                    switch (i)
                    {
                        case 0:
                            loadString += "_Dagger";
                            break;
                        case 1:
                            loadString += "_Sword";
                            break;
                        case 2:
                            loadString += "_Axe";
                            break;
                        case 3:
                            loadString += "_PickAxe";
                            break;
                        case 4:
                            loadString += "_Hammer";
                            break;
                    }

                    switch (j)
                    {
                        case 5:
                            loadString += "_SurfaceType_Barrel";
                            break;
                        case 6:
                            loadString += "_SurfaceType_Crate";
                            break;
                    }

                    weaponAudioClips[i, j] = Resources.Load(loadString) as AudioClip;
                }
                //"BAS_imp_axe_grass"         //Materials
                //"BAS_imp_dagger_stone_2"
                else
                {
                    loadString += "Weapons/BAS_imp";

                    if (i == 2) { loadString += "_axe"; }
                    else if (i > 2 || i == 1) { loadString += "_sword"; }
                    else if (i == 0) { loadString += "_dagger"; }

                    switch (j)
                    {
                        case 0:
                            loadString += "_dirt";
                            break;
                        case 1:
                            loadString += "_grass";
                            break;
                        case 2:
                            loadString += "_leaves";
                            break;
                        case 3:
                            loadString += "_stone";
                            break;
                        case 4:
                            if (i == 2)    //Workaround for lack of axe/wood sound
                                loadString = "Character/Weapons/BAS_imp_sword_wood";
                            else
                                loadString += "_wood";
                            break;
                    }

                    weaponAudioClips[i, j] = Resources.Load(loadString) as AudioClip;

                    if (weaponAudioClips[i, j] == null)
                    {
                        weaponAudioClips[i, j] = Resources.Load(loadString + "_2") as AudioClip;
                    }
                }
            }
            // Weapon Hit Sounds END ----------------------------------------------------------------
        }
    }

    public void EnableHitbox()
    {
        //Reset list of already hit GameObjects (since this is a new swing)
        alreadyHitObjects.Clear();
        hitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }

    //This method is called by the <Pickup> script
    public void OnInteract()
    {
        if (playerWeapon && !equipped)
        {
            if (!InteractionManager.inConversation)
            {
                // Add to picked up weapons
                if (!PlayerManager.Instance.pickedUpWeapons.Contains(weaponType))
                {
                    if (PickupEventOnPickup)
                    {
                        PlayerManager.Instance.StartPickupEvent();
                        weaponAudioSource.PlayOneShot(weaponPickupAudioClip);
                    }
                    PlayerManager.Instance.pickedUpWeapons.Add(weaponType);
                    PlayerManager.Instance.pickedUpWeaponObjects.Add(gameObject);
                }

                EquipWeapon();
            }
        }
    }

    void Start()
    {
        prefab = gameObject;
        if (!playerWeapon)
        {
            BaseDamage = GetComponentInParent<Creature>().AttackDamage;
        }
        else
        {
            Physics.IgnoreCollision(hitbox, PlayerManager.Instance.playerCollider);
        }
    }

    public void EquipWeapon()
    {
        // destroy the pickup script 
        SetPickupEnabled(false);

        prefab.transform.position = PlayerManager.Instance.weaponSlot.transform.position;
        prefab.transform.rotation = PlayerManager.Instance.weaponSlot.transform.rotation;
        PlayerManager.Instance.Inventory_EquipWeapon(gameObject);

        PlayerManager.Instance.equippedWeaponInfo = this;
        PlayerManager.Instance.equippedWeapon = prefab;
        prefab.transform.parent = PlayerManager.Instance.weaponSlot.transform;
        Utility.StripGameObjectFromComponents(gameObject, typeof(Pickup));
        equipped = true;

        uint weaponEquipped = 0;
        weaponTypeMap.TryGetValue(WeaponTypeSwitch.Name, out weaponEquipped);
        weaponAudioSource.PlayOneShot(weaponEquipAudioClips[weaponEquipped]);
    }

    public void UnequipWeapon()
    {
        PlayerManager.Instance.Inventory_UnequipWeapon(gameObject);
    }

    void SetPickupEnabled(bool enabled)
    {
        var pickupScript = gameObject.GetComponent<Pickup>();
        if (pickupScript != null)
        {
            pickupScript.enabled = enabled;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (TriggerDamageAllowed && !playerWeapon)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                Attack attack = new Attack(BaseDamage, transform.position - PlayerManager.Instance.player.transform.position, knockbackStrength);

                if (!alreadyHitObjects.Contains(col.gameObject))
                {
                    SetAndPlayWeaponImpact(col.gameObject);
                    GameManager.DamageObject(col.gameObject, attack);
                }
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (playerWeapon)
        {
            if (equipped && col.gameObject.tag != "Player")
            {
                Vector3 playerToHitPoint = col.contacts[0].point - PlayerManager.Instance.player.transform.position;
                Attack attack = new Attack(BaseDamage, playerToHitPoint, knockbackStrength, SwingTypes.None, weaponType, col.contacts[0].point);

                AnimatorStateInfo currentAnimation = PlayerManager.Instance.playerAnimator.GetCurrentAnimatorStateInfo(0);
                if (currentAnimation.IsName("Player_RightSwing"))
                {
                    attack.swingType = SwingTypes.Right;
                    WeaponCombo1.SetValue();
                }
                else if (currentAnimation.IsName("Player_LeftSwing"))
                {
                    attack.swingType = SwingTypes.Left;
                    WeaponCombo2.SetValue();
                }
                else if (currentAnimation.IsName("Player_TopSwing"))
                {
                    attack.swingType = SwingTypes.Top;
                    WeaponCombo3.SetValue();
                }

                if (!alreadyHitObjects.Contains(col.gameObject))
                {
                    //get material of the contact point
                    SoundMaterial sm = col.gameObject.GetComponent<SoundMaterial>();
                    if (sm != null) {

                        uint thisSwitch = 0;
                        AkSoundEngine.GetSwitch((uint)sm.material.GroupId, transform.parent.gameObject, out thisSwitch);
                        //print("Current Switch: "+ thisSwitch +", New: "+ sm.material.ID);

                        if (thisSwitch != (uint)sm.material.Id)
                        {
                            sm.material.SetValue(transform.parent.gameObject); // Set Impact Material
                                                                               //print("New Impact Material: "+ sm.gameObject.name);
                            lastMaterialHit = sm.material.Name;
                            Debug.Log(lastMaterialHit);
                        }
                    }

                    SetAndPlayWeaponImpact(col.gameObject);
                    GameManager.DamageObject(col.gameObject, attack);

                    GameObject hit = Instantiate(hitEffect, transform.position, Quaternion.identity) as GameObject; //TODO: Pool hit effects
                    Destroy(hit, 5f);

                    if (col.gameObject.layer == LayerMask.NameToLayer("Agent"))
                    {
                        //ComboEvent.Post(transform.parent.gameObject);
                        attack.damage += applyBonusDamage ? comboCompletionBonusDamage : 0;

                        float newTimeScale = applyBonusDamage ? 0.2f : 0.5f;
                        float transitionTime = 0.1f;
                        float holdTime = applyBonusDamage ? 0.2f : 0.1f;
                        float shakeDuration = applyBonusDamage ? 0.3f : 0.15f;
                        float shakeScale = applyBonusDamage ? 0.2f : 0.1f;
                        
                        GameManager.Instance.gameSpeedHandler.SetGameSpeed(gameObject.GetInstanceID(), newTimeScale, transitionTime, transitionTime, holdTime);
                        PlayerManager.Instance.CamShake(new PlayerCamera.CameraShake(shakeScale, shakeDuration));
                    }
                }
            }
        }
        else
        {
            if (col.gameObject.CompareTag("Player"))
            {
                Attack attack = new Attack(BaseDamage, col.contacts[0].point - PlayerManager.Instance.player.transform.position, BaseDamage);
                GameManager.DamageObject(col.gameObject, attack);
                WeaponTypeSwitch.SetValue(transform.parent.gameObject); // Weapon Type
                WeaponImpact.Post(transform.parent.gameObject);
            }
        }
    }
    void SetAndPlayWeaponImpact(GameObject HitObj){
        //print("Impact");
        //WeaponTypeSwitch.SetValue(transform.parent.gameObject); // Weapon Type
        alreadyHitObjects.Add(HitObj);
        WeaponImpact.Post(transform.parent.gameObject);

        uint weaponTypeMapVal = 0;
        weaponTypeMap.TryGetValue(WeaponTypeSwitch.Name, out weaponTypeMapVal);

        uint materialSurfaceMapVal = 0;
        if (!surfaceTypeMap.TryGetValue(lastMaterialHit, out materialSurfaceMapVal))
            materialSurfaceMapVal = 7;

        weaponAudioSource.PlayOneShot(weaponAudioClips[weaponTypeMapVal, materialSurfaceMapVal]);
    }

}

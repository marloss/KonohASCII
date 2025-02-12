﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionAttribute : MonoBehaviour
{
    [Header("Resources")]
    public PlayerMovement playermovement;
    public PlayerAnimation playeranimation;
    [Space(20f)] 
    [Header("Health")] 
    public float maximum_health;
    [SerializeField] private float current_health;
    [Space(20f)] 
    [Header("Chakra")] 
    public float chakra_maximum;
    [SerializeField] private float chakra_current;
    [Space(20f)] 
    [Header("Brakes")] 
    public bool isBusy;
    public bool isStaggered;
    [Space(20f)] 
    [Header("Layermasks and Button mapping")]
    public LayerMask enemylayer;
    [Space]
    public KeyCode attack_keycode;
    public KeyCode rangeattack_keycode; 

    void Update()
    {
        CheckBusyBooleanStatement();
        CQCAttack();
        RangeAttack();
    }

    private void CQCAttack()
    {
        if (Input.GetKey(attack_keycode) && !isBusy)
            playeranimation.SetAnimationState("attack",playeranimation.default_animator);
    }

    private void RangeAttack()
    {
        if (Input.GetKey(rangeattack_keycode) && !isBusy)
        {
            playeranimation.SetAnimationState("range_attack",playeranimation.default_animator);
        }
    }

    private void CheckBusyBooleanStatement()
    {
        isBusy = playeranimation.default_animator.GetCurrentAnimatorStateInfo(0).IsTag("Action");

        switch (isBusy || isStaggered)
        {
            case false:
                playermovement.ChangeRigidbodyState(false);
                break;
            case true:
                playermovement.ChangeRigidbodyState(true,false);
                break;
        }
    }
}
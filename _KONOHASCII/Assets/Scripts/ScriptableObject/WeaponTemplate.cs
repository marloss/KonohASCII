﻿using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapon/new Weapon Template", order = 1)]
[ExecuteInEditMode]
public class WeaponTemplate : ScriptableObject
{
    /// <summary>
    /// Manual creation for new weapons.
    /// </summary>
    [Tooltip("Weapon name starts with capital letter, like 'Kunai'.")]
    public string weaponName;

    [Tooltip("Moves towards zero with every successful hit, then breaks. Only primary weapons can break.")]
    public int weaponDurability;

    public Sprite weaponSprite;

    [Tooltip("Animation override controller for diverse animations")]
    public AnimatorOverrideController weaponAnimatorController;

    [Space] public int damage;

    [Space] [Header("Long range weapon attributes")]
    public bool isRange;

    public bool isPrimaryWeapon;
    public float weaponSpeed;
    [Space] public int ammunition;

    [Space]
    [Header("Short range weapon attributes")]
    [Tooltip("Delays animation to balance. Animation speed multiplier / animationDelay")]
    public float animationDelay = 1;

    [Space] [Header("Weapon effects")] 
    public bool bleed;
    public bool poison;
    public bool explosion;

    private void Start()
    {
        isPrimaryWeapon = isRange;
    }
}
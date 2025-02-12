﻿using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerAction : MonoBehaviour
{
    [Header("Resources")] public Gamemanager gamemanager;
    public PlayableCharacter playableCharacter;
    public PlayerMovement playerMovement;
    public PlayerAnimation playerAnimation;
    public Targeter playerAutoTargeter;
    [Space(20f)] public bool isBlocking;
    public bool isPrimaryAttack;
    public bool isSecondaryAttack;
    public bool isDashAttack;
    [Space] public bool hasDashed = false;
    public bool isDashingInProgress;
    public float dashForce;
    private float maximumDashDistance; //lack of pointers, must declare for later
    private bool hasDamaged; // same deal
    public float maximumDashAngle;
    public float minimuimDashAngle;

    [Header("Weapon_Attribute")]
    [SerializeField]
    [Tooltip("Primary weapon is always close range weapon. Like Katana. If null, will attribute as fist attack")]
    public WeaponTemplate activePrimaryWeapon;

    [Space] public int fistDamage;
    [Space] public int kickDamage;
    [Space] public int attackRadius;
    [Space] public bool isCombo;
    [SerializeField] private bool canProceedWithCombo;
    public float punchAnimationTimeLeft;
    [Space, SerializeField] private bool isTouchingFlag;
    private Collider2D flagObjectCollider;

    [Space] public float maximumSecondsBetweenWeaponSwap;

    private float secondsBetweenWeaponSwap;

    [Tooltip("Secondary weapon is always throwable. Like Shuriken")]
    public WeaponTemplate activeSecondaryWeapon;

    [SerializeField] private GameObject weaponContainer;
    [Space] public int secondaryWeaponAmmunition;
    [Space] public float maximumWeaponPickUpCooldown;
    private float currentWeaponPickUp;

    [Space] public Transform[] weaponPosition;
    [Space(20f)] [Header("Health")] public float maximumHealthPoints;
    [SerializeField] private float healthPoints;
    [Space(20f)] [Header("Chakra")] public int maximumChakra;
    [SerializeField] private int chakra;
    [Space(20f)] [Header("Brakes")] public bool isBusy;
    [Header("Target Dash")] public Sprite targetIndicator;
    [Space] public bool canTargetDash;

    [FormerlySerializedAs("targetDashPosition")]
    public GameObject autoTargeterObject;

    [Space] public bool isStaggered;
    public bool isFacingRight;
    [Space] public float maximumCastWeaponAngle; //Two values -x and x
    [SerializeField] private float castWeaponAngle;
    [Header("Visible Crosshair")] public GameObject crosshairGameObject;
    public Vector3 crosshairOffsetPosition;
    public float crosshairRadius;
    public float crosshairVisualSpeed;
    [SerializeField] private Sprite crosshairSprite;

    [Header("ChakraAttribute")] public float maximumTimeBtwChakraRegeneratingProcedure;
    [SerializeField] private float timeBtwChakraRegeneratingProcedure;
    [SerializeField] private bool hasChakraRegenerationCooldownPassed;
    [Space] public float maximumIntervalBtwChakraRegeneration;
    [SerializeField] private float intervalBtwChakraRegeneration;
    public int chakraRegenerationRate;

    [Space] [Tooltip("Shadow represents the player's y position relative to ground.")]
    public GameObject shadow;

    public Vector3 shadowPosition;
    [SerializeField] private float shadowPositionYOffset;
    [SerializeField] private float shadowPositionXOffset;

    [Space(20f)] [Header("Layermasks and Button mapping")]
    public LayerMask enemylayer;

    void Update()
    {
        if (isCombo)
        {
            isCombo = CheckComboState(); //Cancels Combo state, when returning to "Idle" animation state
            canProceedWithCombo = CheckComboFollowUpState();
            //Checks if the current punch animation is on it's last animation ("ThirdPunch")
        }

        UpdateHealthDisplay();
        shadow.transform.position = shadowPosition;
        isFacingRight = CheckObjectOrientation();
        CheckBusyBooleanStatement();
        PrimaryShortRangeAttack();
        RangeAttack();
        ChakraBlock();
        ManageTargetDash();
        ManageSecondaryWeapon();
    }

    private void LateUpdate()
    {
        shadowPosition = CalculateShadowPosition(transform);
        EngageWithItemFlag();
    }

    private void FixedUpdate()
    {
        if (isCombo)
            punchAnimationTimeLeft -= Time.deltaTime;
        CrosshairDisplay();
    }

    public void FetchRudimentaryValues()
    {
        healthPoints = maximumHealthPoints;
        gamemanager = GameObject.FindGameObjectWithTag("Gamemanager").GetComponent<Gamemanager>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimation = GetComponentInChildren<PlayerAnimation>();
        GetComponent<PlayableCharacter>()
            .AssignNewPlayableCharacter(GetComponent<PlayableCharacter>().playableCharacter);
        chakra = maximumChakra;
        timeBtwChakraRegeneratingProcedure = maximumTimeBtwChakraRegeneratingProcedure;
        intervalBtwChakraRegeneration = maximumIntervalBtwChakraRegeneration;
        crosshairGameObject.transform.position = CalculateCrosshairPosition();
        crosshairGameObject.GetComponent<SpriteRenderer>().sprite = crosshairSprite;
        weaponContainer = gamemanager.GetComponent<Gamemanager>().weaponEntity;
        secondsBetweenWeaponSwap = maximumSecondsBetweenWeaponSwap;
        gamemanager.pauseManager.LoadPlayableCharacterStatistics(playableCharacter.playableCharacter, this);
    }

    private void PrimaryShortRangeAttack()
    {
        switch (CheckMidAirState())
        {
            case true:
                if (isPrimaryAttack && !isBusy && !canTargetDash)
                    playerAnimation.SetAnimationState("airAttack", playerAnimation.defaultAnimator);

                break;
            case false:
                switch (isCombo)
                {
                    case true:

                        switch (FetchIsPrimaryWeaponActive())
                        {
                            case true:
                                if (isPrimaryAttack && canProceedWithCombo)
                                {
                                    playerAnimation.SetAnimationState("attack", playerAnimation.defaultAnimator);
                                    punchAnimationTimeLeft =
                                        playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).length / 2;
                                }

                                break;
                            case false:
                                if (isPrimaryAttack && canProceedWithCombo)
                                {
                                    playerAnimation.SetAnimationState("weapon_attack", playerAnimation.defaultAnimator);
                                    punchAnimationTimeLeft =
                                        playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).length / 2;
                                }

                                break;
                        }

                        break;
                    case false:
                        switch (FetchIsPrimaryWeaponActive())
                        {
                            case true:
                                if (isPrimaryAttack && !isBusy)
                                {
                                    playerAnimation.SetAnimationState("attack", playerAnimation.defaultAnimator);
                                    isCombo = true;
                                    punchAnimationTimeLeft =
                                        playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).length / 2;
                                }

                                break;
                            case false:
                                if (isPrimaryAttack && !isBusy)
                                {
                                    playerAnimation.SetAnimationState("weapon_attack", playerAnimation.defaultAnimator);
                                    isCombo = true;
                                    punchAnimationTimeLeft =
                                        playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).length / 2;
                                }

                                break;
                        }

                        break;
                }

                break;
        }
    }

    private void RangeAttack()
    {
        bool IsAttackInitiated()
        {
            return SignalSecondaryWeaponUsage() && !playerMovement.isStandingOnWall && secondaryWeaponAmmunition > 0;
        }

        //Creates instance of weapon prefab.
        //Modifies said instance from selected asset.
        if (!IsAttackInitiated()) return;

        secondaryWeaponAmmunition--;
        playerAnimation.SetAnimationState("range_attack", playerAnimation.defaultAnimator);
        GameObject _temporaryWeapon;
        switch (isFacingRight)
        {
            case true:
                _temporaryWeapon = Instantiate(weaponContainer,
                    weaponPosition[0].position, weaponPosition[0].rotation);
                _temporaryWeapon.GetComponent<SecondaryWeaponContainer>()
                    .AssignNewWeapon(activeSecondaryWeapon, CalculateWeaponCastingAngle(), 1);
                break;
            case false:
                _temporaryWeapon = Instantiate(weaponContainer,
                    weaponPosition[1].position, weaponPosition[1].rotation);
                _temporaryWeapon.GetComponent<SecondaryWeaponContainer>()
                    .AssignNewWeapon(activeSecondaryWeapon, CalculateWeaponCastingAngle(), -1);
                break;
        }
    }

    private void ManageSecondaryWeapon()
    {
        //When the ammo capacity reaches 0, disable the weapon
        bool IsSecondaryWeaponEmpty()
        {
            return activeSecondaryWeapon && secondaryWeaponAmmunition < 1;
        }

        if (!IsSecondaryWeaponEmpty()) return;
        activeSecondaryWeapon = null;
    }

    private void ChakraBlock()
    {
        //Indicates if chakra regeneration can begin
        //That can only happen, if the cooldown is completed.
        //This is so, that the player has to consider using chakra block
        hasChakraRegenerationCooldownPassed = CheckCanChakraRegenerate();

        playerAnimation.SetAnimationState("chakraValue", chakra, playerAnimation.defaultAnimator);
        playerAnimation.SetAnimationState("isBlocking", isBlocking, playerAnimation.defaultAnimator);
        switch (hasChakraRegenerationCooldownPassed)
        {
            case true:
                if (intervalBtwChakraRegeneration <= 0)
                {
                    chakra += chakraRegenerationRate;
                    intervalBtwChakraRegeneration = maximumIntervalBtwChakraRegeneration;
                }
                else
                    intervalBtwChakraRegeneration -= Time.deltaTime;

                break;
            case false:
                if (timeBtwChakraRegeneratingProcedure > 0)
                {
                    timeBtwChakraRegeneratingProcedure -= Time.deltaTime;
                }

                break;
        }
    }

    private void CrosshairDisplay()
    {
        switch (playerMovement.isStandingOnWall)
        {
            case true:
                crosshairGameObject.SetActive(false);
                break;
            case false:
                crosshairGameObject.SetActive(true);
                castWeaponAngle = CalculateCrosshairYAngle(castWeaponAngle, false);
                crosshairGameObject.transform.position = CalculateCrosshairPosition();
                break;
        }
    }

    private void EngageWithItemFlag()
    {
        switch (secondsBetweenWeaponSwap <= 0)
        {
            case true:
                if (playerMovement.pickUpButton && isTouchingFlag)
                {
                    var flagNumber = flagObjectCollider.GetComponent<ItemFlag>().FetchFlagType();
                    var hasPrimaryWeapon = activePrimaryWeapon != null;
                    var hasSecondaryWeapon = activeSecondaryWeapon != null;
                    var isWeaponPrimary = flagObjectCollider.GetComponent<ItemFlag>().weaponFlag.isPrimaryWeapon;
                    var currentPrimaryWeapon = activePrimaryWeapon;
                    var currentSecondaryWeapon = activeSecondaryWeapon;
                    switch (hasSecondaryWeapon && !isWeaponPrimary)
                    {
                        case true:
                            var temporaryAmmunition = secondaryWeaponAmmunition;

                            if (flagObjectCollider.gameObject.GetComponent<ItemFlag>().ammoCapacity < 1)
                                print("WARNING: WEAPON DISAPPEAR DUE TO LESS THEN 1 AMMO");

                            secondaryWeaponAmmunition =
                                flagObjectCollider.gameObject.GetComponent<ItemFlag>().ammoCapacity;

                            flagObjectCollider.gameObject.GetComponent<ItemFlag>().ammoCapacity =
                                temporaryAmmunition;
                            break;
                        case false:
                            if (!isWeaponPrimary)
                                secondaryWeaponAmmunition =
                                    flagObjectCollider.gameObject.GetComponent<ItemFlag>().ammoCapacity;
                            break;
                    }

                    switch (flagNumber)
                    {
                        case 1: //Weapon case
                            AssingNewWeapon(flagObjectCollider.GetComponent<ItemFlag>().weaponFlag);
                            switch (isWeaponPrimary)
                            {
                                case true:
                                    if (hasPrimaryWeapon)
                                        flagObjectCollider.gameObject.GetComponent<ItemFlag>().weaponFlag =
                                            currentPrimaryWeapon;
                                    else
                                        Destroy(flagObjectCollider.gameObject);
                                    break;
                                case false:
                                    if (hasSecondaryWeapon)

                                        flagObjectCollider.gameObject.GetComponent<ItemFlag>().weaponFlag =
                                            currentSecondaryWeapon;

                                    else
                                        Destroy(flagObjectCollider.gameObject);

                                    break;
                            }

                            break;
                    }

                    secondsBetweenWeaponSwap = maximumSecondsBetweenWeaponSwap;
                    gamemanager.pauseManager.LoadPlayableCharacterStatistics(playableCharacter.playableCharacter,
                        this); //Refresh
                    flagObjectCollider.GetComponent<ItemFlag>().RefreshFlag();
                }

                break;
            case false:
                secondsBetweenWeaponSwap -= Time.deltaTime;
                break;
        }
    }

    private void UpdateHealthDisplay()
    {
        gamemanager.uiManager.playerHeatlhBar.fillAmount = FetchHealthBarProgress();
    }

    private Vector3 CalculateCrosshairPosition()
    {
        castWeaponAngle = CalculateCrosshairYAngle(castWeaponAngle, false);
        float _verticalMouseInput = castWeaponAngle;
        bool _isFacingRight = isFacingRight;
        Vector3 _crosshairPosition = _isFacingRight
            ? new Vector3(transform.position.x + crosshairRadius + crosshairOffsetPosition.x,
                transform.position.y + _verticalMouseInput)
            : new Vector3(transform.position.x + crosshairRadius * -1 + crosshairOffsetPosition.x * -1,
                transform.position.y + _verticalMouseInput);

        return _crosshairPosition;
    }

    private Vector3 CalculateShadowPosition(Transform _shadowPosition)
    {
        Vector3 _shadowVerdictPosition = new Vector3(_shadowPosition.transform.position.x,
            CalculatePreShadowPositionWithRayCast(_shadowPosition).position.y + shadowPositionYOffset);

        return _shadowVerdictPosition;
    }

    private Transform CalculatePreShadowPositionWithRayCast(Transform _shadowArgumentPosition)
    {
        Transform _raycastStartPosition = _shadowArgumentPosition;
        Transform _shadowPosition = _raycastStartPosition;
        bool _isFacingRight = isFacingRight;
        RaycastHit2D _raycastHit2D = _isFacingRight
            ? Physics2D.Raycast(
                new Vector3(transform.position.x - shadowPositionXOffset, transform.position.y - shadowPositionYOffset),
                new Vector2(_raycastStartPosition.position.x - shadowPositionXOffset,
                    _raycastStartPosition.position.y - 150))
            : Physics2D.Raycast(
                new Vector3(transform.position.x + shadowPositionXOffset, transform.position.y - shadowPositionYOffset),
                new Vector2(_raycastStartPosition.position.x + shadowPositionXOffset,
                    _raycastStartPosition.position.y - 150));
        //hardcoded raycast lenght
        if (_raycastHit2D.collider)
            if (_raycastHit2D.collider.CompareTag("Untagged"))
                _shadowPosition = _raycastHit2D.collider.transform;
        return _shadowPosition;
    }

    private bool SignalSecondaryWeaponUsage()
    {
        //Can the player attack?
        bool _canThePlayerUseSecondaryAttack = isSecondaryAttack && !isBusy &&
                                               !playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0)
                                                   .IsName("wallgrip") && activeSecondaryWeapon != null;
        return _canThePlayerUseSecondaryAttack;
    }

    private bool FetchIsPrimaryWeaponActive()
    {
        bool _doesPlayerHavePrimaryWeapon = activePrimaryWeapon;
        return _doesPlayerHavePrimaryWeapon;
    }

    private float FetchHealthBarProgress()
    {
        float _currentHealthBarBlanketValue = 1 - (healthPoints / maximumHealthPoints);
        return _currentHealthBarBlanketValue;
    }

    private float CalculateWeaponCastingAngle()
    {
        Transform weaponStartPositionTransform = isFacingRight ? weaponPosition[1] : weaponPosition[0];
        Vector3 weaponStartPosition = isFacingRight
            ? -weaponStartPositionTransform.right
            : weaponStartPositionTransform.right;

        Vector3 WeaponStartPositionCrosshairCurrentPositionVector =
            crosshairGameObject.transform.position - weaponStartPositionTransform.position;

        float hasReachedOtherSide =
            crosshairGameObject.transform.position.y < weaponStartPositionTransform.position.y ? -1 : 1;
        float _viewAngle = Vector2.Angle(weaponStartPosition, WeaponStartPositionCrosshairCurrentPositionVector) *
                           hasReachedOtherSide;

        return _viewAngle;
    }

    private float CalculateCrosshairYAngle(float _crosshairYAngle, bool _isInvert)
    {
        _crosshairYAngle += _isInvert
            ? -playerMovement.mouseYAxisInput * crosshairVisualSpeed * Time.deltaTime
            : playerMovement.mouseYAxisInput * crosshairVisualSpeed * Time.deltaTime;
        _crosshairYAngle = Mathf.Clamp(_crosshairYAngle, -maximumCastWeaponAngle, maximumCastWeaponAngle);

        return _crosshairYAngle;
    }

    private bool CheckMidAirState()
    {
        bool _isInMidair = playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).IsName("fall") ||
                           playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).IsName("jump");
        return _isInMidair;
    }

    private void ManageTargetDash()
    {
        //Deals with Target dash.

        // Redundants
        bool IsTargetBelow()
        {
            return transform.position.y > playerAutoTargeter.target.transform.position.y;
        }

        bool isTargetRightSide()
        {
            return transform.position.x < playerAutoTargeter.target.transform.position.x;
        }

        bool TargetExist()
        {
            return playerAutoTargeter.target != null;
        }

        ///////////////////////////////////////////////////////////////////
        // Parents 
        bool CheckTargetDashState()
        {
            //Signals, when the player is setup to target dash
            float TargetDegree()
            {
                return FetchTargetDegree();
            }

            return TargetExist() && TargetDegree() > minimuimDashAngle && TargetDegree() < maximumDashAngle &&
                   !isDashingInProgress &&
                   !playerMovement.isStandingOnWall && !playerMovement.isStandingOnGround && !hasDashed &&
                   !playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Action");
        }

        bool IsDashAttackButtonPressed()
        {
            //No need for new input
            return isPrimaryAttack && !playerMovement.isStandingOnGround && !hasDashed;
        }

        float FetchTargetDegree()
        {
            //Calculate the degree between player's forward direction and the target
            bool IsPlayerOnTargetRightSide()
            {
                return transform.position.x < playerAutoTargeter.target.transform.position.x;
            }

            Vector3 EnemyForward()
            {
                return IsPlayerOnTargetRightSide()
                    ? -playerAutoTargeter.target.transform.right
                    : playerAutoTargeter.target.transform.right;
            }

            Vector3 PlayerEnemyVector()
            {
                return (playerAutoTargeter.target.transform.position - transform.position).normalized;
            }

            if (TargetExist())
                Debug.DrawRay(playerAutoTargeter.target.transform.position, EnemyForward());

            return Vector2.Angle(EnemyForward(), PlayerEnemyVector());
        }

        void InitiateTargetDash()
        {
            bool FetchDashDamageStatus()
            {
                return isDashingInProgress && Targets().Length > 0 && !hasDamaged;
            }

            bool TargetDashInitiated()
            {
                return IsDashAttackButtonPressed() && canTargetDash && !hasDashed && TargetExist();
            }

            bool IsDashInProgress()
            {
                return hasDashed;
            }

            float FetchDashDirection()
            {
                Vector3 playerForward = isFacingRight ? -Vector3.left : Vector3.left;
                Vector3 PlayerToEnemy = playerAutoTargeter.target.transform.position - transform.position;
                return IsTargetBelow()
                    ? Vector2.Angle(playerForward, PlayerToEnemy.normalized) * -1
                    : Vector2.Angle(playerForward, PlayerToEnemy.normalized);
            }

            bool IsDashingFinished()
            {
                bool HasMetTarget()
                {
                    return Vector2.Distance(playerMovement.transform.position,
                               playerAutoTargeter.target.transform.position) < 1.4 //Hard coded
                           && hasDashed;
                }

                bool IsFurtherFromTarget()
                {
                    return Vector2.Distance(playerMovement.transform.position,
                        playerAutoTargeter.target.transform.position) > maximumDashDistance;
                }

                if (!TargetExist()) return true;
                if (IsFurtherFromTarget()) return true;
                return HasMetTarget();
            }

            Vector3 TargetDirection()
            {
                return TargetExist()
                    ? (playerAutoTargeter.target.transform.position - transform.position).normalized
                    : (isFacingRight
                        ? (transform.position + Vector3.right - transform.position).normalized
                        : (transform.position + Vector3.left - transform.position).normalized);
            }

            Collider2D[] Targets()
            {
                return Physics2D.OverlapCircleAll(weaponPosition[0].position, attackRadius);
            }

            isDashAttack = TargetDashInitiated();
            isDashingInProgress = IsDashInProgress();

            if (TargetDashInitiated())
            {
                hasDamaged = false;
                playerMovement.rigidbody2D.velocity = TargetDirection() * dashForce;
                transform.localEulerAngles = new Vector3(0, 0, FetchDashDirection());
                maximumDashDistance =
                    Vector2.Distance(transform.position, playerAutoTargeter.target.transform.position);
                hasDashed = true;
            }

            if (FetchDashDamageStatus())
            {
                foreach (var VARIABLE in Targets())
                {
                    print("BAM");
                }

                hasDamaged = true;
            }


            if (isDashingInProgress && IsDashingFinished())
            {
                playerMovement.rigidbody2D.velocity = TargetDirection() * 0;
                transform.localEulerAngles = new Vector3(0, 0, 0);
                playerMovement.rigidbody2D.velocity = Vector2.zero;
                maximumDashDistance = 0;
                hasDashed = false;
            }
        }

        void SetupSpriteRendererAttributes(SpriteRenderer _spriteRenderer)
        {
            if (!TargetExist()) return;

            _spriteRenderer.flipX = isTargetRightSide();
            _spriteRenderer.flipY = IsTargetBelow();
        }

        Sprite SetTargetDashIndicatorSprite(Sprite _sprite)
        {
            //Places mark on target, once the player can dash.
            return canTargetDash ? _sprite : null;
        }

        Vector3 SetTargetDashIndicatorPosition(Sprite _indicatorSprite, SpriteRenderer _spriteRenderer)
        {
            _spriteRenderer.sprite = _indicatorSprite;
            return canTargetDash ? playerAutoTargeter.target.transform.position : Vector3.zero;
        }

        ///////////////////////////////////////////////////////////////////
        canTargetDash = CheckTargetDashState();
        playerAnimation.SetAnimationState("isDashingMotion", hasDashed, playerAnimation.defaultAnimator);

        autoTargeterObject.transform.position = SetTargetDashIndicatorPosition(
            SetTargetDashIndicatorSprite(targetIndicator),
            autoTargeterObject.GetComponent<SpriteRenderer>());

        SetupSpriteRendererAttributes(autoTargeterObject.GetComponent<SpriteRenderer>());
        InitiateTargetDash();
    }

    private bool CheckCanChakraRegenerate()
    {
        bool hasTimePassedToRegenerateChakra = timeBtwChakraRegeneratingProcedure <= 0;
        bool doesChakraNeedRegeneration = chakra < maximumChakra;
        return hasTimePassedToRegenerateChakra && doesChakraNeedRegeneration;
    }

    private bool CheckObjectOrientation()
    {
        return playerAnimation.gameObject.transform.localScale.x > 0;
    }

    private bool CheckComboState()
    {
        return !playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
    }

    private bool CheckComboFollowUpState()
    {
        bool _isIdleAnimationState = playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle");
        bool _hasPunchTimeLeft = punchAnimationTimeLeft < 0;
        bool _hasComboFollowUpAllowed = playerAnimation.canFollowUpCombo;
        bool _comboFollowUp = !_isIdleAnimationState && _hasPunchTimeLeft &&
                              _hasComboFollowUpAllowed;

        return _comboFollowUp;
    }

    private void CheckBusyBooleanStatement()
    {
        isBusy = playerAnimation.defaultAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Action") ||
                 gamemanager.isGamePaused;

        if (isBusy || isStaggered) //freeze if staggered or action is taking place
            switch (!playerMovement.isGripped)
            {
                case true:
                    playerMovement.ChangeRigidbodyState(true, false, playerMovement.rigidbody2D);
                    break;
                case false:
                    playerMovement.ChangeRigidbodyState(false, true, playerMovement.rigidbody2D);
                    break;
            }

        else
        {
            switch (!playerMovement.isGripped)
            {
                case true:
                    playerMovement.ChangeRigidbodyState(false, false, playerMovement.rigidbody2D);
                    break;
                case false:
                    playerMovement.ChangeRigidbodyState(true, false, playerMovement.rigidbody2D);
                    break;
            }
        }
    }

    public void TakeInjury(int _damage)
    {
        switch (isBlocking)
        {
            case true:
                chakra--;
                break;
            case false:
                healthPoints -= _damage;
                break;
        }
    }

    public void TakeInjury(int _damage, bool _isTurnSuccessful)
    {
        switch (isBlocking)
        {
            case true:
                chakra--;
                break;
            case false:
                healthPoints -= _damage;
                break;
        }

        _isTurnSuccessful = true;
    }

    public void DepleteChakraWithRate(int _chakraDepletionRate)
    {
        chakra -= _chakraDepletionRate;
        //Resets chakra cooldowns timer, once chakra is consumed
        timeBtwChakraRegeneratingProcedure = maximumTimeBtwChakraRegeneratingProcedure;
    }

    public void AssingNewWeapon(WeaponTemplate _newWeapon)
    {
        var _uiGameObject = GameObject.FindGameObjectWithTag("UI").GetComponent<UIManager>();
        bool _isPrimaryWeapon = _newWeapon.isPrimaryWeapon;
        switch (_isPrimaryWeapon)
        {
            case true:
                activePrimaryWeapon = _newWeapon;
                _uiGameObject.ReplacePrimaryWeaponUIIcon(activePrimaryWeapon.weaponSprite);
                break;
            case false:
                activeSecondaryWeapon = _newWeapon;
                _uiGameObject.ReplaceSecondaryWeaponUIIcon(activeSecondaryWeapon.weaponSprite,
                    activeSecondaryWeapon.ammunition.ToString());
                break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(weaponPosition[0].position, attackRadius);
        Gizmos.DrawWireSphere(weaponPosition[1].position, attackRadius);
        Gizmos.color = Color.yellow;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("EnemyTarget"))
        {
            //This is when the bottomcollider detects an enemy, when enabled
            //Used by AirAttack
            //col.GetComponent<EnemyBehavior>().TakeInjury(kickDamage);
        }

        if (col.CompareTag("Flag"))
        {
            var newFlag = col.GetComponent<ItemFlag>();
            newFlag.EnablePickUpPrompt(true);
            flagObjectCollider = col;
            isTouchingFlag = true;
        }


        if (!col.CompareTag("Weapon")) return;

        bool CanPickUpNewWeapon()
        {
            currentWeaponPickUp -= Time.deltaTime;
            return currentWeaponPickUp <= 0;
        }

        switch (col.GetComponent<SecondaryWeaponContainer>().canBePickedUp)
        {
            case true:
                if (!activeSecondaryWeapon) return;
                
                if (col.GetComponent<SecondaryWeaponContainer>().weapon.weaponName !=
                    activeSecondaryWeapon.weaponName && !CanPickUpNewWeapon()) return;
                col.GetComponent<SecondaryWeaponContainer>().IncreaseWeaponAmmunition(this);
                Destroy(col.gameObject);
                break;
            case false:
                healthPoints -= col.GetComponent<SecondaryWeaponContainer>().weaponDamage;
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Flag"))
        {
            ItemFlag newFlag = other.GetComponent<ItemFlag>();
            newFlag.EnablePickUpPrompt(false);
            isTouchingFlag = false;
        }
    }
}
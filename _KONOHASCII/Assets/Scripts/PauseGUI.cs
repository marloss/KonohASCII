﻿using UnityEngine;
using UnityEngine.UI;

public class PauseGUI : MonoBehaviour
{
    public Gamemanager gamemanager;

    [Space] [Space, Tooltip("0=continue; 1=help; 2=options; 3=quit.")]
    public Button[] pauseButtons;

    /// <summary>
    /// Timer values assign elsewhere
    /// </summary>
    private float seconds;

    private int minutes;
    private int hours;
    public TMPro.TextMeshProUGUI timerText;
    [Space] [Space(10f)] public TMPro.TextMeshProUGUI characterDescription;
    public Image characterHeadSlot;
    [Space] public Image secondaryWeaponSlot;
    [Space] public GameObject loadoutTitle;
    [Space] public GameObject[] guiWeaponBatches;
    public Transform[] guiLoadoutPosition;
    [Space] public TMPro.TextMeshProUGUI secondaryWeaponName;
    [Space] public Image primaryWeaponSlot;
    public TMPro.TextMeshProUGUI primaryWeaponName;
    [Space(5f)] public Sprite nullSprite;
    [Space] public bool isGamePaused;

    [Space] public Animator pauseAnimator;

    private void Start()
    {
        gamemanager = GameObject.FindGameObjectWithTag("Gamemanager").GetComponent<Gamemanager>();
    }

    private void Update()
    {
        isGamePaused = fetchPauseMenuState();
        ManageTimer();
    }


    public void LoadPlayableCharacterStatistics(PlayableCharacterTemplate _playableCharacter, PlayerAction _player)
    {
        //Primary Weapon
        switch (_player.activePrimaryWeapon != null)
        {
            case true:
                primaryWeaponName.text = _player.activePrimaryWeapon.weaponName;
                primaryWeaponSlot.sprite = _player.activePrimaryWeapon.weaponSprite;
                break;
            case false:
                primaryWeaponSlot.sprite = nullSprite;
                primaryWeaponName.text = "";
                break;
        }

        //Secondary Weapon
        switch (_player.activeSecondaryWeapon != null)
        {
            case true:
                secondaryWeaponName.text = _player.activeSecondaryWeapon.weaponName;
                secondaryWeaponSlot.sprite = _player.activeSecondaryWeapon.weaponSprite;
                break;
            case false:
                secondaryWeaponSlot.sprite = nullSprite;
                secondaryWeaponName.text = "";
                break;
        }

        //Character description
        characterDescription.text = _playableCharacter.characterDescription;
        characterHeadSlot.sprite = _playableCharacter.headshot;
    }

    private void LateUpdate()
    {
        PauseMenu(isGamePaused);
        ManageLoadout();
    }

    public void ResumeGame()
    {
        gamemanager.isGamePaused = false;
    }

    public void OpenHelpSubMenu()
    {
        pauseAnimator.SetBool("isHelpOpened", true);
    }

    public void CloseHelpSubMenu()
    {
        pauseAnimator.SetBool("isHelpOpened", false);
    }

    public void OpenSettingsSubMenu()
    {
        pauseAnimator.SetBool("isSettingsOpened", true);
    }

    public void CloseSettingsSubMenu()
    {
        pauseAnimator.SetBool("isSettingsOpened", false);
    }

    public void QuitMenu()
    {
    }

    public void PauseMenu(bool _isEnabled)
    {
        pauseAnimator.SetBool("isPaused", _isEnabled);
    }

    private void ManageLoadout()
    {
        loadoutTitle.SetActive(FetchLoadoutGUIShowcase());
        if (FetchEmptyLoadout()) return;
        switch (FetchWeaponCount())
        {
            case 1:
                switch (FetchPrimaryWeapon())
                {
                    case true:
                        guiWeaponBatches[0].transform.position = guiLoadoutPosition[0].position;
                        break;
                    case false:
                        guiWeaponBatches[1].transform.position = guiLoadoutPosition[0].position;
                        break;
                }

                break;
            case 2:
                guiWeaponBatches[0].transform.position = guiLoadoutPosition[0].position;
                guiWeaponBatches[1].transform.position = guiLoadoutPosition[1].position;
                break;
        }
    }

    private void ManageTimer()
    {
        seconds = FetchElapsedSeconds(seconds);
        if (seconds >= 60)
        {
            minutes++;
            seconds = 0;
        }

        if (minutes >= 60)
        {
            hours++;
            minutes = 0;
        }

        timerText.text = AssessTimerComponents(seconds, minutes, hours);
    }

    private bool fetchPauseMenuState()
    {
        bool _isPaused = gamemanager.isGamePaused;
        return _isPaused;
    }

    private bool FetchEmptyLoadout()
    {
        bool _hasPrimaryWeapon = gamemanager.playerEntity.activePrimaryWeapon != null;
        bool _hasSecondaryWeapon = gamemanager.playerEntity.activeSecondaryWeapon != null;
        bool _hasEmptyLoadout = !_hasPrimaryWeapon && !_hasSecondaryWeapon;
        return _hasEmptyLoadout;
    }

    private bool FetchLoadoutGUIShowcase()
    {
        bool _canShow = !FetchEmptyLoadout() && pauseAnimator.GetCurrentAnimatorStateInfo(0).IsName("InActiveState");
        return _canShow;
    }

    private bool FetchPrimaryWeapon()
    {
        bool _hasPrimaryWeapon = gamemanager.playerEntity.activePrimaryWeapon != null;
        return _hasPrimaryWeapon;
    }

    private bool FetchSecondaryWeapon()
    {
        bool _hasSecondaryWeapon = gamemanager.playerEntity.activeSecondaryWeapon != null;
        return _hasSecondaryWeapon;
    }

    private int FetchWeaponCount()
    {
        int _count = 0;
        _count = FetchPrimaryWeapon() ? _count + 1 : _count + 0;
        _count = FetchSecondaryWeapon() ? _count + 1 : _count + 0;
        return _count;
    }

    private float FetchElapsedSeconds(float _timerInput)
    {
        float _currentTimer = _timerInput += Time.deltaTime;
        return _currentTimer;
    }

    private string AssessTimerComponents(float _seconds, int _minutes, int _hours)
    {
        string _secondsString =
            Mathf.RoundToInt(_seconds) < 10 ? $"0{Mathf.RoundToInt(_seconds)}" : Mathf.RoundToInt(_seconds).ToString();
        string _minutesString = _minutes < 10 ? $"0{_minutes}" : _minutes.ToString();
        string _hoursString = hours < 10 ? $"0{_hours}" : _hours.ToString();
        string _assessedStrings = $"{_hoursString}:{_minutesString}:{_secondsString}";
        return _assessedStrings;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CreateCustomMatchMenu : MonoBehaviour
{

    public Button backButton;
    public Button createButton;
    public TMP_Dropdown  gameModeDropdown;
    public TMP_Dropdown  roundsDropdown;
    public TMP_Dropdown  durationsDropdown;

    private List<TMP_Dropdown.OptionData> gameModeDropdownOptions = new List<TMP_Dropdown.OptionData>();
    private List<TMP_Dropdown.OptionData> roundsDropdownOptions = new List<TMP_Dropdown.OptionData>();
    private List<TMP_Dropdown.OptionData> durationsDropdownOptions = new List<TMP_Dropdown.OptionData>();

    enum GameModeEnum
    {
        Elimination,
        Teamdeadmatch
    }
    

    // Start is called before the first frame update
    void Start()
    {
        InitSetup();
        gameModeDropdown.onValueChanged.AddListener((x) => OnGameModeChanged());
    }

    void GameModeOptionsSetup()
    {
        foreach (var enumName in Enum.GetNames(typeof(GameModeEnum)))
        {
            gameModeDropdownOptions.Add(new TMP_Dropdown.OptionData() { text = enumName });
        }
        gameModeDropdown.ClearOptions();
        gameModeDropdown.AddOptions(gameModeDropdownOptions);
    }

    void InitSetup()
    {
        GameModeOptionsSetup();
        RoundOptionsSetup(isInitSetup: true);
        DurationOptionsSetup(isInitSetup: true);
    }
    
    void RoundOptionsSetup(int start = 1, int end = 6, bool isInitSetup = true)
    {
        
        roundsDropdown.ClearOptions();
        roundsDropdownOptions = Enumerable.Range(start, end)
            .Select(x => new TMP_Dropdown.OptionData() { text = $"{x.ToString()} Rounds"})
            .ToList();
        roundsDropdown.AddOptions(roundsDropdownOptions);
        durationsDropdown.RefreshShownValue();
    }


    void DurationOptionsSetup(int start = 1, int end = 6, bool isInitSetup = true)
    {
        durationsDropdown.ClearOptions();

        durationsDropdownOptions = Enumerable.Range(start, end)
            .Select(x => new TMP_Dropdown.OptionData() { text = $"{x.ToString()} Minutes"})
            .ToList();
        durationsDropdown.AddOptions(durationsDropdownOptions);
        durationsDropdown.RefreshShownValue();
        

    }
    
    void OnGameModeChanged()
    {
        switch (gameModeDropdown.value)
        {
            case (int)GameModeEnum.Elimination:
                RoundOptionsSetup(isInitSetup:true);
                DurationOptionsSetup(isInitSetup:true);
                break;
            case (int)GameModeEnum.Teamdeadmatch:
                var roundStart = 1;
                var roundFinish = 6;
                var durationStart = 5;
                var durationFinish = 4;
                RoundOptionsSetup(roundStart, roundFinish, isInitSetup:false);
                DurationOptionsSetup(durationStart, durationFinish,  isInitSetup:false);
                break;
        }
    }
}

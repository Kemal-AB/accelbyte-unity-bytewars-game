using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MainMenuUiConfig", menuName = "MainMenuUI Configuration", order = 1)]
public class MainMenuUiConfig : ScriptableObject
{
    public GameObject starterMainUI;
    public GameObject[] otherMainUI;

}
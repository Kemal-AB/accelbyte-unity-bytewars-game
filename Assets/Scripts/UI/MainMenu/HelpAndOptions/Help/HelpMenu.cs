using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpMenu : MonoBehaviour
{
    [SerializeField] private Button backButton;
    
    void Start()
    {
        backButton.onClick.AddListener(() => MenuManager.Instance.OnBackPressed());   
    }
}

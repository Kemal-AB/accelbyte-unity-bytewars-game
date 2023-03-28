using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private Button backButton;
    [SerializeField] private CreditsData[] creditsNames;

    private float waitTime = 1f;
    private float scrollSpeed = 0.125f;
    private float scrollPosition = 1f;

    private void Start()
    {
        // UI Initialization
        backButton.onClick.AddListener(() => MenuManager.Instance.OnBackPressed());
        
        DisplayCredits();
    }

    private void Update()
    {
        if (waitTime <= 0 && scrollPosition >= 0)
        {
            scrollPosition -= scrollSpeed * Time.deltaTime;
            scrollView.verticalNormalizedPosition = scrollPosition;
        }
        else
        {
            waitTime -= 1f * Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        // reset auto-scroll
        waitTime = 1f;
        scrollPosition = 1f;
        scrollView.verticalNormalizedPosition = scrollPosition;
    }

    private void DisplayCredits()
    {
        // Get Prefab
        GameObject roleGroupPanelPrefab = AssetManager.Singleton.GetAsset(AssetEnum.RoleGroupPanel) as GameObject;
        GameObject memberNameTextPrefab = AssetManager.Singleton.GetAsset(AssetEnum.MemberNameText) as GameObject;

        foreach (CreditsData creditData in creditsNames)
        {
            GameObject roleGroupPanel = Instantiate(roleGroupPanelPrefab, scrollView.content);
            TMP_Text roleGroupNameText = roleGroupPanel.GetComponentInChildren<TMP_Text>();
            roleGroupNameText.text = creditData.roleGroupName;

            foreach (string memberName in creditData.memberNames)
            {
                GameObject memberNameTextObject = Instantiate(memberNameTextPrefab, roleGroupPanel.transform);
                TMP_Text memberNameText = memberNameTextObject.GetComponent<TMP_Text>();
                memberNameText.text = memberName;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarterMenuCanvas : MonoBehaviour
{
    [SerializeField] private ButtonAnimation buttonPrefab;

    [SerializeField] private RectTransform buttonPanel;
    private Dictionary<string, ButtonAnimation> instantiatedButtons= new Dictionary<string, ButtonAnimation>();

    public void InstantiateButtons(MenuButtonData[] buttonsData)
    {
        if (instantiatedButtons.Count == buttonsData.Length)
            return;
        foreach (var buttonData in buttonsData)
        {
            var instantiatedButton = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, buttonPanel);
            instantiatedButton.name = buttonData.name;
            if(buttonData.callback!=null)
            instantiatedButton.button.onClick.AddListener(buttonData.callback);
            instantiatedButton.text.text = buttonData.label;
            if (!instantiatedButtons.TryGetValue(buttonData.name, out var btn))
            {
                instantiatedButtons[buttonData.name] = instantiatedButton;
            }
        }
    }

    public void SetButtonsCallback(MenuButtonData[] buttonsData)
    {
        foreach (var buttonData in buttonsData)
        {
            ButtonAnimation buttonAnimation;
            if (instantiatedButtons.TryGetValue(buttonData.name, out buttonAnimation))
            {
                if(buttonData.callback!=null)
                buttonAnimation.button.onClick.AddListener(buttonData.callback);
            }
        }
    }
}

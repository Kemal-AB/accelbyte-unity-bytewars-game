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

    public void InstantiateButtons(Dictionary<string, MenuButtonData> buttonsData)
    {
        if (instantiatedButtons.Count == buttonsData.Count)
            return;
        foreach (var buttonData in buttonsData)
        {
            var instantiatedButton = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, buttonPanel);
            instantiatedButton.name = buttonData.Key;
            instantiatedButton.button.onClick.AddListener(buttonData.Value.callback);
            instantiatedButton.text.text = buttonData.Value.label;
            if (!instantiatedButtons.TryGetValue(buttonData.Key, out var btn))
            {
                instantiatedButtons[buttonData.Key] = instantiatedButton;
            }
        }
    }

    public void SetButtonsCallback(Dictionary<string, MenuButtonData> buttonsData)
    {
        foreach (var buttonData in buttonsData)
        {
            ButtonAnimation buttonAnimation;
            if (instantiatedButtons.TryGetValue(buttonData.Key, out buttonAnimation))
            {
                buttonAnimation.button.onClick.AddListener(buttonData.Value.callback);
            }
        }
    }
}

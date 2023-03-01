using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    private TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClickAnimation);
    }

    void OnClickAnimation()
    {
        text = GetComponentInChildren<TMP_Text>();
        var color = text.color;
        var fadeoutcolor = color;
        fadeoutcolor.a = 0;

        LeanTween.value(text.gameObject, UpdateValueExampleCallback,
            fadeoutcolor, color, 0.1f).setLoopPingPong(6).setOnComplete(OnComplete);

    }
    
    private void UpdateValueExampleCallback(Color val)
    {
        text.color = val;
    }
    private void OnComplete()
    {
        text.color = Color.white;
    }

}

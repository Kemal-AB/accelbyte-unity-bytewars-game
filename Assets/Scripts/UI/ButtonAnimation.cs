using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    public TMP_Text text;
    public Button button;

    // Start is called before the first frame update
    void Start()
    {
      button.onClick.AddListener(OnClickAnimation);
    }

    void OnClickAnimation()
    {
        var color = text.color;
        var fadeoutcolor = color;
        fadeoutcolor.a = 0;

        LeanTween.value(text.gameObject, UpdateValueExampleCallback,
            fadeoutcolor, color, 0.1f).setLoopPingPong(6).setOnComplete(OnComplete);
        
        AudioManager.Instance.PlaySfx("Click_on_Button");
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

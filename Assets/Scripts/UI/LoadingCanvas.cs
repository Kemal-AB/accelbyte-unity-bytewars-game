using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : MonoBehaviour
{
    [SerializeField] private Image[] loadingImages;

    private int _index = 0;

    private const float animationSpeed = 0.65f;

    private Quaternion initialRotation;
    // Start is called before the first frame update
    void Start()
    {
        initialRotation = loadingImages[0].gameObject.transform.localRotation;
        StartAnimate();
    }

    private void StartAnimate()
    {
        _index = 0;
        for (int i = 0; i < loadingImages.Length; i++)
        {
            loadingImages[i].color = new Color(1, 1, 1, 0);
            loadingImages[i].gameObject.transform.rotation = initialRotation;
            float tempI = i;
            Invoke("AnimateImage", tempI*animationSpeed);
        }
    }

    private void AnimateImage()
    {
        ;
        if (_index == loadingImages.Length - 1)
        {
            LeanTween.color(loadingImages[_index].rectTransform, Color.white, animationSpeed)
                .setLoopCount(2).setLoopType(LeanTweenType.pingPong)
                .setEaseOutQuad()
                .setOnComplete(StartAnimate);
        }
        else
        {
            LeanTween.color(loadingImages[_index].rectTransform, Color.white, animationSpeed)
                .setEaseOutQuad()
                .setLoopCount(2).setLoopType(LeanTweenType.pingPong);
            _index++;
        }
    }
}

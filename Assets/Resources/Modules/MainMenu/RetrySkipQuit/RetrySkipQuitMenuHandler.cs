using UnityEngine;
using UnityEngine.Events;

public class RetrySkipQuitMenuHandler : MonoBehaviour
{
    //check RetrySkipQuitMenu TutorialModuleData to check buttons index retryBtn = 0, skipBtn = 1, quitBtn = 2 
    private TutorialModuleData _moduleData;
    public void SetCallbacks(TutorialModuleData moduleData, UnityAction retryCallback, UnityAction skipCallback)
    {
        _moduleData = moduleData;
        _moduleData.menuCanvasData.buttons[0].callback = retryCallback;
        _moduleData.menuCanvasData.buttons[1].callback = skipCallback;
        _moduleData.menuCanvasData.buttons[2].callback = Application.Quit;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendHelper : MonoBehaviour
{
    private Dictionary<string, AssetEnum> _eventDictionary = new Dictionary<string, AssetEnum>();
    
    private static bool IsModuleActive()
    {
        var module = TutorialModuleManager.Instance.GetModule(TutorialType.ManagingFriends);
        return module.isActive;
    }
    
    private static bool IsStarterModeActive()
    {
        var module = TutorialModuleManager.Instance.GetModule(TutorialType.ManagingFriends);
        return module.isStarterActive;
    }

    public static AssetEnum GetMenuByDependencyModule()
    {
        var moduleStatus = IsModuleActive();

        var starterMode = IsStarterModeActive();
        return moduleStatus && starterMode ? AssetEnum.FriendDetailsMenuCanvas_Starter : AssetEnum.FriendDetailsMenuCanvas;
    }
}
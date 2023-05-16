using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

public class CloudSaveHelper_Starter : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start()
    {
        OptionsMenu.onOptionsMenuActivated += (musicVolume, sfxVolume) => {};
        OptionsMenu.onOptionsMenuDeactivated += (musicVolume, sfxVolume) => {};
    }
    
    
}

using System;
using UnityEngine.Events;

public class MenuButtonData
{
    //name of the button
    public string name;
    //buttton's displayed text string
    public string label;
    //will call this callback upon clicked, can be null
    public UnityAction callback;
    //will go to this canvas upon clicked, can be null
    //public string canvasDestinationName;

}

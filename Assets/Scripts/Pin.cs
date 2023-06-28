using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : LogicComponent
{
    public Type type;
    public enum Type
    {
        INPUT, OUTPUT
    }
    void Update()
    {
        outputNodes[0].signal = inputNodes[0].signal;
        //transform.localScale = new Vector3(1 / transform.parent.parent.localScale.x, 1 / transform.parent.parent.localScale.y, 1);
    }
}

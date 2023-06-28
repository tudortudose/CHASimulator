using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : LogicComponent
{
    public Color onColor;
    public Color offColor;

    public void ChangeSignal()
    {
        outputNodes[0].signal = (outputNodes[0].signal + 1) % 2;

        switch (outputNodes[0].signal)
        {
            case 0:
                transform.GetChild(1).GetComponent<SpriteRenderer>().color = offColor;
                break;
            case 1:
                transform.GetChild(1).GetComponent<SpriteRenderer>().color = onColor;
                break;
        }
    }
}

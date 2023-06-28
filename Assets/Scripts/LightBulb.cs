using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBulb : LogicComponent
{
    public Color onColor;
    public Color offColor;
    private void Update()
    {
        switch (inputNodes[0].signal)
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

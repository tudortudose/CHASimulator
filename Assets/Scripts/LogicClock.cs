using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogicClock : LogicComponent
{
    public float freq;
    public Color onColor;
    public Color offColor;

    public RectTransform text;
    public TMP_InputField hertz;
    private void Start()
    {
        freq = float.Parse(hertz.text);
        InvokeRepeating("Change", 0, 1 / freq);
    }

    private void Update()
    {
        text.localPosition = Vector3.zero;
    }

    public void SetHertz()
    {
        if (hertz.text != "")
        {
            try
            {
                if (float.Parse(hertz.text) < 0.1f)
                {
                    hertz.text = "0.1";
                }
                else if (float.Parse(hertz.text) > 10)
                {
                    hertz.text = "10";
                }
                freq = float.Parse(hertz.text);
                CancelInvoke();
                InvokeRepeating("Change", 0, 1 / freq);
            }
            catch (Exception exp)
            {
                Debug.LogError(exp.Message);
            }
        }
    }

    private void Change()
    {
        outputNodes[0].signal = (outputNodes[0].signal + 1) % 2;

        //switch (outputNodes[0].signal)
        //{
        //    case 0:
        //        transform.GetChild(1).GetComponent<SpriteRenderer>().color = offColor;
        //        break;
        //    case 1:
        //        transform.GetChild(1).GetComponent<SpriteRenderer>().color = onColor;
        //        break;
        //}
    }
}

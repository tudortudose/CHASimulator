using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberOutput : LogicComponent
{
    public TMP_InputField inputField;
    public RectTransform text;

    void Update()
    {
        text.localPosition = Vector3.zero;
        int output = 0;

        for (int i = 0; i < inputNodes.Count; i++)
        {
            output += inputNodes[i].signal * (int)(Mathf.Pow(2, i));
        }

        inputField.text = output.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberInput : LogicComponent
{
    public TMP_InputField inputField;
    public RectTransform text;

    void Update()
    {

        text.localPosition = Vector3.zero;
        int input = int.Parse(inputField.text);

        for (int i = 0; i < outputNodes.Count; i++)
        {
            outputNodes[i].signal = (input / (int)(Mathf.Pow(2, i))) % 2;
        }
    }
}

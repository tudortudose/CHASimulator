using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicGate : LogicComponent
{
    public GateType gateType;

    public enum GateType
    {
        AND, OR, XOR, NOT, NAND, NOR, XNOR
    }

    void Update()
    {
        int sig;
        switch (gateType)
        {
            case GateType.AND:
                sig = inputNodes[0].signal == 1 && inputNodes[1].signal == 1 ? 1 : 0;
                StartCoroutine(SetSignal(sig, 0));
                break;
            case GateType.NAND:
                sig = inputNodes[0].signal == 1 && inputNodes[1].signal == 1 ? 0 : 1;
                StartCoroutine(SetSignal(sig, 0));
                break;
            case GateType.OR:
                sig = inputNodes[0].signal == 1 || inputNodes[1].signal == 1 ? 1 : 0;
                StartCoroutine(SetSignal(sig, 0));
                break;
            case GateType.NOR:
                sig = inputNodes[0].signal == 1 || inputNodes[1].signal == 1 ? 0 : 1;
                StartCoroutine(SetSignal(sig, 0));
                break;
            case GateType.XOR:
                sig = (inputNodes[0].signal + inputNodes[1].signal) % 2;
                StartCoroutine(SetSignal(sig, 0));
                break;
            case GateType.XNOR:
                sig = ((inputNodes[0].signal + inputNodes[1].signal) % 2 + 1) % 2;
                StartCoroutine(SetSignal(sig, 0));
                break;
            case GateType.NOT:
                sig = (inputNodes[0].signal + 1) % 2;
                StartCoroutine(SetSignal(sig, 0));
                break;
        }
    }

    IEnumerator SetSignal(int sig, int index)
    {
        yield return new WaitForSecondsRealtime(0.0f);
        outputNodes[index].signal = sig;
    }
}

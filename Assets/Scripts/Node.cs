using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public int signal;
    public Type type;
    public List<Wire> attachedWires = new();

    public enum Type
    {
        INPUT, OUTPUT
    }

    private void Update()
    {
        if (type == Type.INPUT && signal == 1 && attachedWires.Count == 0)
        {
            signal = 0;
        }
    }

    public void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            WireGenerator wireGenerator = FindObjectOfType<WireGenerator>();
            wireGenerator.ClickNode(this);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GateMovement : MonoBehaviour
{
    public Vector3 previousMousePosition;
    public Vector3 currentMousePosition;
    public bool dragging = false;
    public float movingDelta = 0.01f;
    public float rotationSpeed = 4f;

    private void OnMouseDown()
    {
        previousMousePosition = CamManager.GetMouseToWorldPoint();
        dragging = true;
    }
    private void OnMouseDrag()
    {
        currentMousePosition = CamManager.GetMouseToWorldPoint();
        if (Vector3.Distance(previousMousePosition, currentMousePosition) > movingDelta)
        {
            transform.position = transform.position + (currentMousePosition - previousMousePosition);

            previousMousePosition = currentMousePosition;
            RedrawWires();
        }
    }

    private void OnMouseOver()
    {
        if(Input.mouseScrollDelta.y != 0)
        {
            transform.Rotate(0, 0, Input.mouseScrollDelta.y * rotationSpeed);
            RedrawWires();
        }
    }

    public void RedrawWires()
    {
        LogicComponent component = GetComponent<LogicComponent>();

        if (component != null)
        {
            foreach (Node node in component.inputNodes.Concat(component.outputNodes))
            {
                if(node.attachedWires.Count > 0)
                {
                    foreach (Wire wire in node.attachedWires)
                    {
                        if (wire)
                        {
                            wire.Redraw();
                            
                        }
                    }
                }
            }
        }
    }

    private void OnMouseUp()
    {
        dragging = false;
    }
}

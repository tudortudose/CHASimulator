using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LogicComponent : MonoBehaviour
{
    public List<Node> inputNodes = new();
    public List<Node> outputNodes = new();

    public bool finalPosition;

    private void Awake()
    {
        finalPosition = false;
    }

    public void OnMouseOver()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.E))
        {
            foreach (Node node in inputNodes)
            {
                if (node.attachedWires.Count > 0)
                {
                    foreach (Wire wire in node.attachedWires)
                    {
                        if (wire)
                        {
                            wire.DeleteWire();
                        }
                    }
                }
            }

            foreach (Node node in outputNodes)
            {
                if (node.attachedWires.Count > 0)
                {
                    foreach (Wire wire in node.attachedWires)
                    {
                        if (wire)
                        {
                            wire.inputNode.attachedWires.Clear();
                            wire.DeleteWire();
                        }
                    }
                }
            }
            Destroy(gameObject);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            var newPos = transform.position;
            newPos.x += 2;
            var go = Instantiate(gameObject, newPos, Quaternion.identity, transform.parent);
            foreach (Node node in go.GetComponent<LogicComponent>().inputNodes.Concat(go.GetComponent<LogicComponent>().outputNodes))
            {
                node.attachedWires.Clear();
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.mouseScrollDelta.y != 0)
        {
            transform.Rotate(0, 0, Input.mouseScrollDelta.y * rotationSpeed);
            RedrawWires();
        }

        if (GetComponent<LogicGate>() && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
        {
            transform.GetChild(2).gameObject.SetActive(!transform.GetChild(2).gameObject.activeInHierarchy);
        }
        else if (GetComponent<LogicModule>() && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
        {
            GetComponent<LogicModule>().Maximize();
        }

        if(GetComponent<Switch>() && Input.GetMouseButtonDown(1))
        {
            GetComponent<Switch>().ChangeSignal();
        }
    }



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

    public void RedrawWires()
    {
        LogicComponent component = GetComponent<LogicComponent>();

        if (component != null)
        {
            foreach (Node node in component.inputNodes.Concat(component.outputNodes))
            {
                if (node.attachedWires.Count > 0)
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

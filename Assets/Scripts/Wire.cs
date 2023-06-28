using UnityEngine;

public class Wire : MonoBehaviour
{
    public Node inputNode;
    public Node outputNode;

    public Color onColor;
    public Color offColor;

    void FixedUpdate()
    {
        if (inputNode != null)
        {
            inputNode.signal = outputNode.signal;
            if (outputNode.signal == 1)
            {
                GetComponent<LineRenderer>().material.color = onColor;
            }
            else
            {
                GetComponent<LineRenderer>().material.color = offColor;
            }
        }
    }

    public void Redraw()
    {
        if (inputNode != null && outputNode != null)
        {
            WireGenerator wireGenerator = FindObjectOfType<WireGenerator>();
            try
            {
                var go = transform.parent.parent.parent;
                wireGenerator.ConstructLineRenderer(GetComponent<LineRenderer>(), go.InverseTransformPoint(outputNode.transform.position), go.transform.InverseTransformPoint(inputNode.transform.position));
            }
            catch
            {
                wireGenerator.ConstructLineRenderer(GetComponent<LineRenderer>(), outputNode.transform.position, inputNode.transform.position);
            }
        }
    }

    public void OnMouseOver()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0)) {
            inputNode.attachedWires.Clear();
            DeleteWire();
        }
    }

    public void DeleteWire()
    {
        Destroy(gameObject);
    }
}

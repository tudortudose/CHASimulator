using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireGenerator : MonoBehaviour
{
    public LineRenderer wirePrefab;
    public LineRenderer currentWire;
    public Node startNode;
    public Node endNode;
    public Vector3 lastMousePos;
    public bool startClicked = false;
    public bool finishedClicked = true;
    public int pointsPerUnit;
    public float curvature;

    public Transform wiresHolder;

    public void ConstructLineRenderer(LineRenderer lineRenderer, Vector3 startPos, Vector3 endPos)
    {
        int numSegments = (int)(Vector3.Distance(startPos, endPos) * pointsPerUnit) + 4;
        if (numSegments <= 1) return;
        float curveFactor = 1 / curvature;

        Vector3 p0 = new Vector3(startPos.x, startPos.y, 0);
        Vector3 p3 = new Vector3(endPos.x, endPos.y, 0);
        Vector3 p1 = new Vector3(p0.x + Mathf.Abs(p3.x - p0.x) / curveFactor, p0.y, p0.z);
        Vector3 p2 = new Vector3(p3.x - Mathf.Abs(p3.x - p0.x) / curveFactor, p3.y, p3.z);

        Vector3[] points = new Vector3[numSegments];
        for (int i = 0; i < numSegments; i++)
        {
            float t = (float)i / ((float)(numSegments - 1));
            points[i] = CalculateBezierPoint(t, p0, p3, p1, p2);
        }

        lineRenderer.positionCount = numSegments;
        lineRenderer.SetPositions(points);

        if (lineRenderer.GetComponent<MeshCollider>())
        {
            MeshCollider meshCollider = lineRenderer.GetComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            lineRenderer.BakeMesh(mesh, true);
            meshCollider.sharedMesh = mesh;
        }
        else
        {
            MeshCollider meshCollider = lineRenderer.gameObject.AddComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            lineRenderer.BakeMesh(mesh, true);
            meshCollider.sharedMesh = mesh;
        }
    }

    IEnumerator MakeWire()
    {
        if (currentWire == null)
        {
            currentWire = Instantiate(wirePrefab, wiresHolder);
        }

        Vector3 startPos = startNode.transform.position;
        Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastMousePos = currentPos;

        if (startPos.x > currentPos.x)
        {
            Vector3 aux = new Vector3(startPos.x, startPos.y, 0);

            startPos = new Vector3(currentPos.x, currentPos.y, 0);
            currentPos = new Vector3(aux.x, aux.y, 0);
        }
        else
        {
            startPos = new Vector3(startPos.x, startPos.y, 0);
            currentPos = new Vector3(currentPos.x, currentPos.y, 0);
        }

        ConstructLineRenderer(currentWire, startPos, currentPos);

        yield return new WaitForSecondsRealtime(0.01f);
    }

    private void FixedUpdate()
    {
        StartWire();
    }

    public void StartWire()
    {
        Vector3 currentMousePos;
        if (!finishedClicked)
        {
            currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector3.Distance(lastMousePos, currentMousePos) > 0.05)
            {
                lastMousePos = currentMousePos;
                Debug.Log("redrawing");
                StartCoroutine(MakeWire());
            }
        }
    }

    public void FinishWire(Node _startNode, Node _endNode, Transform holder)
    {
        if (currentWire)
        {
            Destroy(currentWire.gameObject);
        }

        LineRenderer lineRenderer;

        if (_startNode.type == Node.Type.INPUT && _endNode.type == Node.Type.OUTPUT)
        {
            if (_startNode.attachedWires.Count > 0)
            {
                return;
            }
            lineRenderer = Instantiate(wirePrefab, holder);
            lineRenderer.GetComponent<Wire>().inputNode = _startNode;
            lineRenderer.GetComponent<Wire>().outputNode = _endNode;
            _startNode.attachedWires.Add(lineRenderer.GetComponent<Wire>());
            _endNode.attachedWires.Add(lineRenderer.GetComponent<Wire>());
        }
        else if (_startNode.type == Node.Type.OUTPUT && _endNode.type == Node.Type.INPUT)
        {
            if (_endNode.attachedWires.Count > 0)
            {
                return;
            }
            lineRenderer = Instantiate(wirePrefab, holder);
            lineRenderer.GetComponent<Wire>().inputNode = _endNode;
            lineRenderer.GetComponent<Wire>().outputNode = _startNode;
            _startNode.attachedWires.Add(lineRenderer.GetComponent<Wire>());
            _endNode.attachedWires.Add(lineRenderer.GetComponent<Wire>());
        }
        else
        {
            return;
        }

        Vector3 startPos = lineRenderer.GetComponent<Wire>().outputNode.transform.position;
        Vector3 endPos = lineRenderer.GetComponent<Wire>().inputNode.transform.position;

        ConstructLineRenderer(lineRenderer, holder.InverseTransformPoint(startPos), holder.InverseTransformPoint(endPos));
    }

    // Calculate the point on the Bezier spline at time t
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p3, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }

    public void ClickNode(Node node)
    {
        if (startClicked == false)
        {
            startNode = node;
            startClicked = true;
            finishedClicked = false;
            lastMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else
        {
            endNode = node;
            FinishWire(startNode, endNode, wiresHolder);
            startClicked = false;
            finishedClicked = true;
        }
    }
}

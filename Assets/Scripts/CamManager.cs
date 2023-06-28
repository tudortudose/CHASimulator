using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManager : MonoBehaviour
{
    private new Camera camera;

    public float zoomSpeed;

    private Vector3 origin;
    private Vector3 diference;
    private bool drag = false;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space)) 
        {
            Move();
            Rotate();
        }
    }

    public static Vector3 GetMouseToWorldPoint()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }



    void Move()
    {
        if (Input.GetMouseButton(0))
        {
            diference = (camera.ScreenToWorldPoint(Input.mousePosition)) - camera.transform.position;
            Debug.Log(camera.ScreenToWorldPoint(Input.mousePosition));
            if (drag == false)
            {
                drag = true;
                origin = camera.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else
        {
            drag = false;
        }
        if (drag == true)
        {
            camera.transform.position = origin - diference;
        }
        
    }

    private void Rotate()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (camera.orthographicSize > 4)
            {
                camera.orthographicSize -= zoomSpeed;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (camera.orthographicSize < 20)
            {
                camera.orthographicSize += zoomSpeed;
            }
        }
    }
}

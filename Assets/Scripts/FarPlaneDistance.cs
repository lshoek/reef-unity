using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarPlaneDistance : MonoBehaviour
{
    public Vector3 MaxFarPlaneReach;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }
	
    void Update ()
    {
        cam.farClipPlane = Vector3.Dot(cam.transform.position - MaxFarPlaneReach, Vector3.up);
	}
}

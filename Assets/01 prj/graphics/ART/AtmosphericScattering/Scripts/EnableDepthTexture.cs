using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class EnableDepthTexture : MonoBehaviour
{
    public Camera cam;
    void Reset()
    {
        cam = GetComponent<Camera>();
    }

    void Update ()
    {
        if (cam.depthTextureMode == DepthTextureMode.None)
            cam.depthTextureMode = DepthTextureMode.Depth;
	}
}

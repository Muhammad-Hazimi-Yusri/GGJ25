using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PCPlayerController : MonoBehaviour
{

    //pc player camera
    public Camera pcPlayerCamera;

    // Start is called before the first frame update
    void Start()
    {
        
        //set the camera to the pc player camera
        pcPlayerCamera = GameObject.Find("PCPlayer").GetComponentInChildren<Camera>();

    }

    // Update is called once per frame
    void Update()
    {

        //on the keys a and d, rotate camera 90 degrees with the new input system
        if(Keyboard.current.aKey.wasPressedThisFrame)
        {
            pcPlayerCamera.transform.Rotate(0, 90, 0);
        }
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            pcPlayerCamera.transform.Rotate(0, -90, 0);
        }

    }
}

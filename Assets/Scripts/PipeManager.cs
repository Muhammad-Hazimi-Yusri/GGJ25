using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{

    public GameObject PCPipe;
    public GameObject VRPipe;

    public DragManager dragManager;

    bool cooldown = false;

    private Transform transportingGameObject;
    PipeEnd pipeToTransportTo = null;

    public void OnPipeExit()
    {
        cooldown = false;
        dragManager.OnDragEnd -= OnPressEnd;
    }

    public void OnPipeTriggered(PipeEnd pipeEnd, Transform objectTransform)
    {

        Debug.Log("Pipe Triggered");

        if (cooldown)
        {
            return;
        }

        Debug.Log("After Cooldown");

        dragManager.OnDragEnd += OnPressEnd;


        transportingGameObject = objectTransform;
        pipeToTransportTo = pipeEnd;


    }

    public void OnPressEnd()
    {


        //transform the object to the destination of the pipe end +a small z  and have a cooldown so it doesn't teleport back and forth
        transportingGameObject.position = pipeToTransportTo.destination.position + new Vector3(0, 0, -1f);

        //dragManager.endDragAfterTeleport();


        StartCoroutine(waitABit());

        transportingGameObject = null;
    }


    IEnumerator waitABit()
    {
        cooldown = true;
        yield return new WaitForSeconds(1);
        cooldown = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {



        
    }
}

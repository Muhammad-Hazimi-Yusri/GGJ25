using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{

    public GameObject PCPipe;
    public GameObject VRPipe;

    public DragManager dragManager;

    bool cooldown = false;


    public void OnPipeTriggered(PipeEnd pipeEnd, Transform objectTransform)
    {

        Debug.Log("Pipe Triggered");

        if (cooldown)
        {
            return;
        }

        Debug.Log("After Cooldown");

        //transform the object to the destination of the pipe end +a small z  and have a cooldown so it doesn't teleport back and forth
        objectTransform.position = pipeEnd.destination.position + new Vector3(0, 0, -1f);

        dragManager.endDragAfterTeleport();


        StartCoroutine(waitABit());



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

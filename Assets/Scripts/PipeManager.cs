using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PipeManager : MonoBehaviour
{

    public GameObject PCPipe;
    public GameObject VRPipe;

    public DragManager dragManager;

    bool cooldown = false;

    private Transform transportingGameObject;
    PipeEnd pipeToTransportTo = null;


    public GameObject bubbleObject;

    public void OnPipeExit()
    {
        cooldown = false;
        dragManager.OnDragEnd -= OnPressEnd;

        //get the objectTransforms xrgrab interactable, and add a select exited even for vr release object
        transportingGameObject.GetComponent<XRGrabInteractable>().selectExited.RemoveListener(VRReleaseObject);

        transportingGameObject = null;
    }


    public void VRReleaseObject(SelectExitEventArgs args)
    {

        if (cooldown)
        {
            return;
        }

        SpawnBubbleWithItemInside(true);
        //transportingGameObject.position = pipeToTransportTo.spawnPoint.position + pipeToTransportTo.spawnDirecton;

        StartCoroutine(waitABit());
        transportingGameObject = null;


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

        //get the objectTransforms xrgrab interactable, and add a select exited even for vr release object
        objectTransform.GetComponent<XRGrabInteractable>().selectExited.AddListener(VRReleaseObject);


        transportingGameObject = objectTransform;
        pipeToTransportTo = pipeEnd;


    }


    private void SpawnBubbleWithItemInside(bool isVR)
    {
        //spawn a bubble
        GameObject bubble = Instantiate(bubbleObject, pipeToTransportTo.spawnPoint.position, Quaternion.identity);

        //add the spawnDirection to the bubble
        bubble.GetComponent<Rigidbody>().velocity = pipeToTransportTo.spawnDirecton;

        //get the BubblePopper script from the bubble
        BubblePopper bubblePopper = bubble.GetComponent<BubblePopper>();

        //set the original object to the object that is being transported
        bubblePopper.originalObject = transportingGameObject.gameObject;

        //set the original scale to the scale of the object being transported
        bubblePopper.originalScale = transportingGameObject.localScale;

        //spawn the transported object inside the bubble
        //GameObject item = Instantiate(transportingGameObject.gameObject, bubble.transform.position, Quaternion.identity);

        //transport the object to the center of the bubble
        transportingGameObject.position = bubble.transform.position;
        //set the parent of the object to the bubble
        transportingGameObject.SetParent(bubble.transform);
        //scale it down to fit inside the bubble
        transportingGameObject.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        //disable the rigidbody of the item
        transportingGameObject.GetComponent<Rigidbody>().isKinematic = true;

        //disable the collider of the item
        transportingGameObject.GetComponent<Collider>().enabled = false;

        cooldown = false;
        dragManager.OnDragEnd -= OnPressEnd;

        //get the objectTransforms xrgrab interactable, and add a select exited even for vr release object
        transportingGameObject.GetComponent<XRGrabInteractable>().selectExited.RemoveListener(VRReleaseObject);

        //if it's VR, set layer mask to Draggable, if not, set to default
        if (isVR)
        {
            transportingGameObject.gameObject.layer = LayerMask.NameToLayer("Draggable");
        }
        else
        {
            transportingGameObject.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        pipeToTransportTo.OnTransportedObject();


    }

    public void OnPressEnd()
    {


        //transform the object to the destination of the pipe end +a small z  and have a cooldown so it doesn't teleport back and forth
        //transportingGameObject.position = pipeToTransportTo.destination.position + new Vector3(0, 0, -1f);

        SpawnBubbleWithItemInside(false);

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

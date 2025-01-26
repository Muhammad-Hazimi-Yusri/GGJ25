using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BubblePopper : MonoBehaviour
{


    //a scale for the item to go back to when popped
    public Vector3 originalScale;

    //the original item game object
    public GameObject originalObject;

    //on trigger enter, destroy the object
    private void OnTriggerEnter(Collider other)
    {



        //Debug.Log("Bubble Triggered");

        if (other.CompareTag("Player"))
        {

            //unparent the object
            originalObject.transform.parent = null;

            //set the scale back to the original scale
            originalObject.transform.localScale = originalScale;



            //enable the rigidbody of the item
            originalObject.GetComponent<Rigidbody>().isKinematic = false;

            //enable the collider of the item
            originalObject.GetComponent<Collider>().enabled = true;

            Destroy(gameObject);

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(Keyboard.current.anyKey.wasPressedThisFrame)
        {

            //unparent the object
            originalObject.transform.parent = null;

            //set the scale back to the original scale
            originalObject.transform.localScale = originalScale;



            //enable the rigidbody of the item
            originalObject.GetComponent<Rigidbody>().isKinematic = false;

            //enable the collider of the item
            originalObject.GetComponent<Collider>().enabled = true;

            Destroy(gameObject);

        }
        


    }
}

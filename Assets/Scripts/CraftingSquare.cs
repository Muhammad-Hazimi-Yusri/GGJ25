using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingSquare : MonoBehaviour
{

    [SerializeField] private InputActionReference mouseClickAction; // Reference to your MouseClick action

    bool isMousePressed = false;


    private void OnEnable()
    {
        // Subscribe to input actions
        mouseClickAction.action.performed += OnMouseClickPerformed;
        mouseClickAction.action.canceled += OnMouseClickReleased;
    }

    private void OnDisable()
    {
        // Unsubscribe from input actions
        mouseClickAction.action.performed -= OnMouseClickPerformed;
        mouseClickAction.action.canceled -= OnMouseClickReleased;
    }

    private void OnMouseClickPerformed(InputAction.CallbackContext context)
    {
        isMousePressed = true;
    }

    private void OnMouseClickReleased(InputAction.CallbackContext context)
    {
        isMousePressed = false;
    }




    private void OnTriggerEnter(Collider other)
    {
        //// If the other object is a slot cube
        if (other.CompareTag("Draggable"))
        {

            //released the mouse button
            if (isMousePressed)
            {
                // Set the slot cube's color to green
                //other.GetComponent<Renderer>().material.color = Color.green;


            }
            else
            {
                // Set the slot cube's color to white
                //other.GetComponent<Renderer>().material.color = Color.white;
            }



            // Set the slot cube's color to green
            //other.GetComponent<Renderer>().material.color = Color.green;
        }

    }

    private void OnTriggerStay(Collider other)
    {
        // If the other object is a slot cube
        if (other.CompareTag("Draggable"))
        {
            //if mouse released
            if (isMousePressed)
            {
                // Set the slot cube's color to green
               // other.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {

                //call the CradftingGridManager to check if the item can be placed in the slot
                //component is in parent
                //slot is the last number of the name of the game object minus 1

                int slot = int.Parse(gameObject.name.Substring(gameObject.name.Length - 1)) - 1;

                gameObject.GetComponentInParent<CraftingGridManager>().PlaceItemInSlot(slot, other.gameObject);

                // Set the slot cube's color to white
                //other.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the other object is a slot cube
        if (other.CompareTag("Draggable"))
        {
            // Set the slot cube's color to white
            other.GetComponent<Renderer>().material.color = Color.white;
        }
    }



}

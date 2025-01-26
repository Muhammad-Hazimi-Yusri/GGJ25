using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BubblePopper : MonoBehaviour
{
    // A scale for the item to go back to when popped
    public Vector3 originalScale;

    // The original item game object
    public GameObject originalObject;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference mouseClickAction; // For mouse clicks
    [SerializeField] private InputActionReference mousePositionAction; // For mouse position
    [SerializeField] private LayerMask bubbleLayerMask; // Layer for the bubble (ensure bubble objects are on this layer)

    public Camera mainCamera;

    private void Start()
    {
        // Find the main camera if not explicitly set
        //mainCamera = Camera.main;

        //get the camera from object named "PC Camera" 

        mainCamera = GameObject.Find("PC Camera").GetComponent<Camera>();
        // Enable input actions
        mouseClickAction.action.Enable();
        mousePositionAction.action.Enable();
    }

    private void OnDestroy()
    {
        // Disable input actions when this object is destroyed
        //mouseClickAction.action.Disable();
        //mousePositionAction.action.Disable();
    }

    private void Update()
    {
        // Check for mouse click and raycast to the bubble
        if (mouseClickAction.action.WasPerformedThisFrame())
        {

            //Debug.Log("Mouse Clicked at " + mousePositionAction.action.ReadValue<Vector2>());

            Vector2 mousePosition = mousePositionAction.action.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            //Debug.Log("Raycast from " + ray.origin + " in direction " + ray.direction);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, bubbleLayerMask))
            {

                Debug.Log("Hit " + hit.collider.gameObject.name);

                if (hit.collider.gameObject == gameObject)
                {
                    StartCoroutine(PopBubble());
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(PopBubble());
        }
    }

    IEnumerator PopBubble()
    {

        // Play the pop sound
        GetComponent<AudioSource>().Play();

        //disable the mesh renderer
        GetComponent<MeshRenderer>().enabled = false;

        //play the particle system
        GetComponent<ParticleSystem>().Play();

        //wait for 0.5 seconds

        yield return new WaitForSeconds(0.1f);

        // Unparent the object
        originalObject.transform.parent = null;

        // Set the scale back to the original scale
        originalObject.transform.localScale = originalScale;

        // Enable the rigidbody of the item
        originalObject.GetComponent<Rigidbody>().isKinematic = false;

        // Enable the collider of the item
        originalObject.GetComponent<Collider>().enabled = true;

        // Destroy the bubble
        Destroy(gameObject);
    }
}

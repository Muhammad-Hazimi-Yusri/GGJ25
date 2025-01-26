using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A small helper class attached to the slot occupant clone.
/// It stores a reference to the original item so we can restore it later.
/// </summary>
public class SlotOccupantRef : MonoBehaviour
{
    public GameObject originalItem;
}

/// <summary>
/// The main manager for a 3x3 crafting grid in a 3D scene.
/// </summary>
public class CraftingGridManager : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionReference mouseClickAction;     // For mouse down/up
    [SerializeField] private InputActionReference mousePositionAction;  // For pointer position

    [Header("Camera & Layer Settings")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("LayerMask for raycasting. E.g. \"Everything\" or a custom layer.")]
    [SerializeField] private LayerMask dragRaycastLayer;

    [Header("Slots")]
    [Tooltip("Exactly 9 transforms for the 3x3 grid 'slot cubes'.")]
    [SerializeField] private Transform[] slotCubes; // Transparent 3D cubes (with colliders)

    [Header("Scaling")]
    [Tooltip("Scale factor for the occupant clone in the slot.")]
    [SerializeField] private float slotScaleFactor = 0.3f;
    [Tooltip("Distance from camera when 'dragging' an item.")]
    [SerializeField] private float dragDistance = 3f;



    public GameObject metalSheetPrefab;

    // Currently "dragged" original item
    private GameObject currentItem = null;

    // Our array storing references to the occupant clones (not the originals).
    // If null => slot is empty.
    private GameObject[] slotOccupants = new GameObject[9];

    private void Awake()
    {
        // Optional convenience: automatically find child objects named "CraftingGridX" as slots
        List<Transform> tempSlotCubes = new List<Transform>();
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.name.StartsWith("CraftingGrid"))
            {
                tempSlotCubes.Add(child);
            }
        }
        slotCubes = tempSlotCubes.ToArray();
    }

    private void OnEnable()
    {
        // Subscribe to the new input system events
        mouseClickAction.action.performed += OnMouseClickPerformed;
        mouseClickAction.action.canceled += OnMouseClickReleased;
    }

    private void OnDisable()
    {
        mouseClickAction.action.performed -= OnMouseClickPerformed;
        mouseClickAction.action.canceled -= OnMouseClickReleased;
    }

    private void Update()
    {
        // If we're dragging an item, continuously update its position in front of the camera
        if (currentItem != null)
        {
            DragCurrentItem();
        }
    }

    // -------------------------------
    //        INPUT EVENTS
    // -------------------------------

    private void OnMouseClickPerformed(InputAction.CallbackContext ctx)
    {
        // This is like "mouse down"
        AttemptPickup();


        //Debug.Log("Mouse Clicked at " + mousePositionAction.action.ReadValue<Vector2>());

        //check if we hit the button "CraftButton" with a raycast
        Vector2 mousePos = mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Debug.Log("Raycast from " + ray.origin + " in direction " + ray.direction);

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 5f);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity,dragRaycastLayer))
        {
            if (hit.collider.gameObject.name == "CraftButton")
            {

                Debug.Log("Crafting item...");

                //if there are Box items in the slots (name contains Box), then delete it, and spawn a metalsheet
                for (int i = 0; i < slotOccupants.Length; i++)
                {
                    if (slotOccupants[i] != null)
                    {
                        if (slotOccupants[i].name.Contains("Box"))
                        {
                            //get the reference to the original item and destroy that too
                            SlotOccupantRef occupantRef = slotOccupants[i].GetComponent<SlotOccupantRef>();
                            if (occupantRef != null)
                            {
                                Destroy(occupantRef.originalItem);
                            }

                            Destroy(slotOccupants[i]);
                            slotOccupants[i] = null;

                            //spawn a metal sheet
                            GameObject metalSheet = Instantiate(metalSheetPrefab, slotCubes[i].position, Quaternion.identity);

                            //add 1 to the x position of the metal sheet
                            metalSheet.transform.position = new Vector3(metalSheet.transform.position.x + 1, metalSheet.transform.position.y, metalSheet.transform.position.z);


                        }

                        //if it contain "Metal" then just remove the grid item and reactivate the original item
                        if (slotOccupants[i].name.Contains("Metal"))
                        {
                            //get the reference to the original item and destroy that too
                            SlotOccupantRef occupantRef = slotOccupants[i].GetComponent<SlotOccupantRef>();
                            if (occupantRef != null)
                            {
                                occupantRef.originalItem.SetActive(true);
                                //move it a bit to the front on the x axis
                                occupantRef.originalItem.transform.position = new Vector3(occupantRef.originalItem.transform.position.x + 1, occupantRef.originalItem.transform.position.y, occupantRef.originalItem.transform.position.z);
                            }
                            Destroy(slotOccupants[i]);
                            slotOccupants[i] = null;
                        }
                    }
                }


            }
        }


    }

    private void OnMouseClickReleased(InputAction.CallbackContext ctx)
    {
        // This is like "mouse up"
        if (currentItem != null)
        {
            AttemptDrop();
        }
    }

    // -------------------------------
    //        DRAG & DROP
    // -------------------------------

    /// <summary>
    /// Called on mouse down. We see if we clicked:
    ///  - a slot occupant clone (then pick up the original),
    ///  - or a free-floating item with tag=Draggable.
    /// </summary>
    private void AttemptPickup()
    {
        // Raycast from camera using the new Input System pointer position
        Vector2 mousePos = mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, dragRaycastLayer))
        {
            GameObject hitObj = hit.collider.gameObject;

            // A) Did we click a slot occupant clone?
            int occupantSlotIndex = FindSlotIndexForOccupant(hitObj);
            if (occupantSlotIndex >= 0)
            {
                // We picked up a slot occupant. Let's remove it and re-enable the original item.
                RemoveSlotOccupant(occupantSlotIndex);
                return;
            }

            // B) Otherwise, if it's a "Draggable" original object:
            if (hitObj.CompareTag("Draggable"))
            {
                // Only pick it up if not already hidden in a slot
                // (Or if we want to allow picking it up from the world at any time)
                currentItem = hitObj;
            }
        }
    }

    /// <summary>
    /// Called each frame while we have a currentItem in "drag" mode.
    /// Moves it in front of the camera at 'dragDistance'.
    /// </summary>
    private void DragCurrentItem()
    {
        Vector2 mousePos = mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Vector3 targetPos = ray.GetPoint(dragDistance);
        currentItem.transform.position = targetPos;
    }

    /// <summary>
    /// Called on mouse up. Attempts to drop the currently dragged item into a slot.
    /// If we didn't hit a slot, the item just remains wherever it's dropped in the world.
    /// </summary>
    private void AttemptDrop()
    {
        Vector2 mousePos = mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, dragRaycastLayer))
        {
            int slotIndex = FindSlotIndexForCube(hit.collider.gameObject);
            if (slotIndex >= 0)
            {
                // If slot is empty, place it
                if (slotOccupants[slotIndex] == null)
                {
                    PlaceItemInSlot(slotIndex, currentItem);
                }
                // else: the slot is occupied, do nothing (or implement swap logic).
            }
        }
        // Done dragging
        currentItem = null;
    }

    // -------------------------------
    //        SLOT LOGIC
    // -------------------------------

    /// <summary>
    /// Creates a simplified "occupant clone" in the specified slot and hides the original item.
    /// </summary>
    public  void PlaceItemInSlot(int slotIndex, GameObject originalItem)
    {
        // 1) Hide/disable the original item
        originalItem.SetActive(false);

        // 2) Create a clone object that only has MeshFilter & MeshRenderer
        //    so we can display it in the slot at a reduced scale.
        GameObject occupantClone = new GameObject(originalItem.name + "_SlotClone");
        occupantClone.transform.SetParent(slotCubes[slotIndex], false); // local transform
        occupantClone.transform.localPosition = Vector3.zero;
        occupantClone.transform.localRotation = Quaternion.identity;

        // Add a small helper to reference the original item
        SlotOccupantRef occupantRef = occupantClone.AddComponent<SlotOccupantRef>();
        occupantRef.originalItem = originalItem;

        // Copy the mesh data
        MeshFilter origFilter = originalItem.GetComponent<MeshFilter>();
        MeshRenderer origRenderer = originalItem.GetComponent<MeshRenderer>();

        if (origFilter && origRenderer)
        {
            MeshFilter cloneFilter = occupantClone.AddComponent<MeshFilter>();
            cloneFilter.sharedMesh = origFilter.sharedMesh;

            MeshRenderer cloneRenderer = occupantClone.AddComponent<MeshRenderer>();
            cloneRenderer.sharedMaterials = origRenderer.sharedMaterials;
        }

        // Scale it down
        occupantClone.transform.localScale = Vector3.one * slotScaleFactor;

        // Store the occupant clone in our array
        slotOccupants[slotIndex] = occupantClone;
    }

    /// <summary>
    /// Removes the occupant clone from the slot, destroys it, and re-enables the original item for dragging.
    /// </summary>
    private void RemoveSlotOccupant(int slotIndex)
    {
        GameObject occupantClone = slotOccupants[slotIndex];
        if (occupantClone == null) return;

        // Find the original
        SlotOccupantRef occupantRef = occupantClone.GetComponent<SlotOccupantRef>();
        if (occupantRef && occupantRef.originalItem)
        {
            // Re-enable the original item
            occupantRef.originalItem.SetActive(true);

            // Put the original item into our "currentItem" so we can drag it
            currentItem = occupantRef.originalItem;
        }

        // Destroy the occupant clone
        Destroy(occupantClone);

        // Clear the slot
        slotOccupants[slotIndex] = null;
    }


    public bool isObjectInSlot(GameObject obj)
    {
        for (int i = 0; i < slotOccupants.Length; i++)
        {
            if (slotOccupants[i] == obj)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Finds the slot index (0..8) whose occupant clone is the specified GameObject.
    /// </summary>
    private int FindSlotIndexForOccupant(GameObject occupantClone)
    {
        for (int i = 0; i < slotOccupants.Length; i++)
        {
            if (slotOccupants[i] == occupantClone)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Finds the slot index (0..8) if the provided object is one of our 'slotCubes'.
    /// Returns -1 if not found.
    /// </summary>
    private int FindSlotIndexForCube(GameObject slotCubeObject)
    {
        for (int i = 0; i < slotCubes.Length; i++)
        {
            if (slotCubes[i].gameObject == slotCubeObject)
                return i;
        }
        return -1;
    }
}

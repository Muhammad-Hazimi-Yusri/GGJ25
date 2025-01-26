using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingGridManager : MonoBehaviour
{

    [Header("Input Settings")]
    [SerializeField] private InputActionReference mouseClickAction; // Reference to your MouseClick action
    [SerializeField] private InputActionReference mousePositionAction; // Reference to Pointer Position action

    [Header("Camera & Layer Settings")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("LayerMask for raycasting. E.g. \"Everything\" or specific layers.")]
    [SerializeField] private LayerMask dragRaycastLayer;

    [Header("Slots")]
    [Tooltip("Exactly 9 transforms for the 3x3 grid 'slot cubes'.")]
    [SerializeField] private Transform[] slotCubes; // Each is a transparent 3D cube in the scene

    [Header("Scaling")]
    [Tooltip("Scale factor when the item is placed in a slot.")]
    [SerializeField] private float slotScaleFactor = 0.3f;
    [Tooltip("Distance from camera when dragging an item.")]
    [SerializeField] private float dragDistance = 3f;

    // Tracks which item is occupying each slot (null if empty)
    private GameObject[] slotOccupants = new GameObject[9];

    // Currently dragged item
    private GameObject currentItem = null;
    private Vector3 currentItemOriginalScale = Vector3.one;

    // Keep a record of each item's true original scale
    // so we can restore it after removing from a slot.
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    private void Awake()
    {
        //go through each children under the crafting area, and add them to the slotCubes array (only if it starts with the name CraftingGrid

        Transform[] children = GetComponentsInChildren<Transform>();
        List<Transform> tempSlotCubes = new List<Transform>();
        foreach (Transform child in children) {
            if (child.name.StartsWith("CraftingGrid"))
            {
                tempSlotCubes.Add(child);
            }
        }

        slotCubes = tempSlotCubes.ToArray();


    }
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
        // This is equivalent to mouse down
        OnMouseDownPickup();
    }

    private void OnMouseClickReleased(InputAction.CallbackContext context)
    {
        // This is equivalent to mouse up
        if (currentItem != null)
        {
            OnMouseUpDrop();
        }
    }

    private void Update()
    {
        // Drag logic: Update position of the currentItem during drag
        if (currentItem != null)
        {
            DragCurrentItem();
        }
    }

    /// <summary>
    /// When left mouse button is pressed, either:
    /// - Pick up a free-floating item
    /// - OR pick up an item already in a slot (restoring original scale)
    /// </summary>
    private void OnMouseDownPickup()
    {
        Vector2 mousePosition = mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, dragRaycastLayer))
        {
            GameObject hitObj = hit.collider.gameObject;

            // A) If we are hitting a slot occupant?
            int occupantSlotIndex = FindSlotIndexForOccupant(hitObj);
            if (occupantSlotIndex >= 0)
            {
                // This item is already in a slot. Let's pick it up (remove from slot).
                RemoveItemFromSlot(occupantSlotIndex);
                currentItem = hitObj;
                currentItem.transform.localScale = GetOriginalScale(currentItem); // restore original
                return;
            }

            // B) If we are hitting a "free-floating" item (tagged Draggable or something)
            //    We'll pick it up if it’s not already in a slot
            if (hitObj.CompareTag("Draggable"))
            {
                // Check if item is not in a slot
                if (FindSlotIndexForOccupant(hitObj) == -1)
                {
                    currentItem = hitObj;
                    // Store original scale if not already known
                    if (!originalScales.ContainsKey(hitObj))
                    {
                        originalScales[hitObj] = hitObj.transform.localScale;
                    }
                    currentItemOriginalScale = originalScales[hitObj];
                }
            }
        }
    }

    /// <summary>
    /// Called during mouse drag to reposition currentItem in front of the camera at dragDistance.
    /// </summary>
    private void DragCurrentItem()
    {
        Vector2 mousePosition = mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // We'll pick a point in space 'dragDistance' units away from the camera
        Vector3 newPos = ray.GetPoint(dragDistance);
        currentItem.transform.position = newPos;
    }

    /// <summary>
    /// When the user releases the mouse button, try snapping the item into a slot (if any).
    /// Otherwise, just let it stay wherever it was placed.
    /// </summary>
    private void OnMouseUpDrop()
    {
        // Raycast again to see if we're pointing at a slot cube
        Vector2 mousePosition = mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, dragRaycastLayer))
        {
            // Did we hit one of our 9 slot cubes?
            int slotIndex = FindSlotIndexForCube(hit.collider.gameObject);
            if (slotIndex >= 0)
            {
                // 1) If that slot is currently empty, place item there
                if (slotOccupants[slotIndex] == null)
                {
                    PlaceItemInSlot(slotIndex, currentItem);
                }
                else
                {
                    // Optional: if you want to replace or swap
                    // For now, let's do nothing. 
                    // You could do: SwapItems(slotIndex, currentItem);
                }
            }
        }

        // End dragging in either case
        currentItem = null;
    }

    /// <summary>
    /// Places the item into the specified slot: snaps position, shrinks scale, and stores occupant.
    /// </summary>
    public void PlaceItemInSlot(int slotIndex, GameObject item)
    {
        slotOccupants[slotIndex] = item;

        // If we've never tracked its original scale, do so now
        if (!originalScales.ContainsKey(item))
        {
            originalScales[item] = item.transform.localScale;
        }
        currentItemOriginalScale = originalScales[item];

        // Move item to the center of the slot
        Transform slotTransform = slotCubes[slotIndex];
        item.transform.position = slotTransform.position;

        // Optionally parent it to the slot for organization (not strictly necessary)
        item.transform.SetParent(slotTransform);

        // Shrink to fit inside the cube
        item.transform.localScale = currentItemOriginalScale * slotScaleFactor;
    }

    /// <summary>
    /// Removes the occupant item from the slot, freeing it so we can drag it again.
    /// </summary>
    private void RemoveItemFromSlot(int slotIndex)
    {
        GameObject occupant = slotOccupants[slotIndex];
        if (occupant != null)
        {
            // Detach from the slot
            occupant.transform.SetParent(null);

            // Clear occupant
            slotOccupants[slotIndex] = null;
        }
    }

    /// <summary>
    /// Finds which slot index (0..8) has the specified occupant item. Returns -1 if not found.
    /// </summary>
    private int FindSlotIndexForOccupant(GameObject occupant)
    {
        for (int i = 0; i < slotOccupants.Length; i++)
        {
            if (slotOccupants[i] == occupant)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Finds which slot index (0..8) corresponds to the slotCube object we hit. Returns -1 if not found.
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

    /// <summary>
    /// Helper to get the original scale stored for an item. 
    /// If not present, returns item.transform.localScale as fallback.
    /// </summary>
    private Vector3 GetOriginalScale(GameObject item)
    {
        if (originalScales.ContainsKey(item))
            return originalScales[item];
        else
            return item.transform.localScale;
    }
}

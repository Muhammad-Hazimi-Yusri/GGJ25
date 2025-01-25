using UnityEngine;
using UnityEngine.InputSystem;  // Important for the new Input System classes

public class DragManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputActionAsset inputActions;
    // Assign your "PlayerInputActions" asset in the Inspector

    private InputAction _clickAction;
    private InputAction _pointerPosAction;

    private Transform _selectedObject;
    private Vector3 _offset;
    private float _zDistance;

    private void Awake()
    {
        // Find the "Player" action map
        var playerMap = inputActions.FindActionMap("PC Player");

        // Find the individual actions
        _clickAction = playerMap.FindAction("Click");
        _pointerPosAction = playerMap.FindAction("PointerPosition");
    }

    private void OnEnable()
    {
        // Enable the actions so we can read input
        _clickAction.Enable();
        _pointerPosAction.Enable();
    }

    private void OnDisable()
    {
        _clickAction.Disable();
        _pointerPosAction.Disable();
    }

    private void Update()
    {
        // Check if this frame had a "click down"
        if (_clickAction.WasPressedThisFrame())
        {
            StartDrag();
        }

        // If the left button is still held down, continue dragging
        if (_clickAction.IsPressed() && _selectedObject != null)
        {
            ContinueDrag();
        }

        // Check if this frame had a "click release"
        if (_clickAction.WasReleasedThisFrame() && _selectedObject != null)
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        // Get current pointer (mouse) position from the action
        Vector2 mousePos = _pointerPosAction.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if(hitInfo.transform.tag != "Draggable")
            {
                return;
            }
            _selectedObject = hitInfo.transform;

            // Distance from camera to object
            _zDistance = Vector3.Distance(mainCamera.transform.position, _selectedObject.position);

            // Calculate hit point in world space
            Vector3 hitPoint = ray.GetPoint(_zDistance);

            // Remember offset so object doesn't "jump"
            _offset = _selectedObject.position - hitPoint;


            //set the rigidbody to kinematic
            _selectedObject.GetComponent<Rigidbody>().isKinematic = true;

        }
    }

    private void ContinueDrag()
    {
        Vector2 mousePos = _pointerPosAction.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Vector3 hitPoint = ray.GetPoint(_zDistance);
        _selectedObject.position = hitPoint + _offset;
    }

    private void EndDrag()
    {

        //reset the rigidbody to not kinematic
        _selectedObject.GetComponent<Rigidbody>().isKinematic = false;

        // Simply release
        _selectedObject = null;

       
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class DragManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputActionAsset inputActions;

    // Action references
    private InputAction _clickAction;
    private InputAction _pointerPosAction;
    private InputAction _scrollAction;

    // Dragging state
    private Rigidbody _selectedRigidbody;
    private bool _isDragging;

    // Saved rigidbody states
    private bool _savedKinematicState;
    private bool _savedUseGravityState;

    // Distance and offset
    public  float _zDistance;
    private Vector3 _offset;

    // The position we want to move the rigidbody toward
    private Vector3 _targetPosition;


    public event Action OnDragEnd;

    // Min and max distances for clamping
    [SerializeField] private float minZDistance = 7.0f;
    [SerializeField] private float maxZDistance = 9.0f;


    // How fast scrolling moves the object forward/back
    [SerializeField] private float scrollSensitivity = 1.0f;

    private void Awake()
    {
        var playerMap = inputActions.FindActionMap("PC Player");
        _clickAction = playerMap.FindAction("Click");
        _pointerPosAction = playerMap.FindAction("PointerPosition");
        // Make sure your action in the InputActions asset is named exactly "ScrollWheel" or similar
        // If it's literally named "Scroll Wheel", match that string below:
        _scrollAction = playerMap.FindAction("Scroll Wheel");
    }

    private void OnEnable()
    {
        _clickAction.Enable();
        _pointerPosAction.Enable();
        _scrollAction.Enable();
    }

    private void OnDisable()
    {
        _clickAction.Disable();
        _pointerPosAction.Disable();
        _scrollAction.Disable();
    }

    private void Update()
    {
        // 1) Check for starting drag
        if (_clickAction.WasPressedThisFrame())
        {
            StartDrag();
        }

        // 2) If dragging, update our desired target position each frame
        if (_isDragging && _clickAction.IsPressed())
        {
            ContinueDrag();
        }

        // 3) Check for ending drag
        if (_isDragging && _clickAction.WasReleasedThisFrame())
        {
            EndDrag();
        }
    }

    public void endDragAfterTeleport()
    {
        EndDrag();
    }

    private void FixedUpdate()
    {
        // If we have a selected rigidbody and we are dragging, move it via physics
        if (_isDragging && _selectedRigidbody != null)
        {
            _selectedRigidbody.MovePosition(_targetPosition);
        }
    }

    private void StartDrag()
    {
        // Raycast from camera through mouse pointer
        Vector2 mousePos = _pointerPosAction.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // Check tag or however you identify draggable objects
            if (!hitInfo.transform.CompareTag("Draggable"))
            {
                return;
            }

            // Grab the rigidbody
            _selectedRigidbody = hitInfo.transform.GetComponent<Rigidbody>();
            if (_selectedRigidbody == null) return;

            // Save original states
            _savedKinematicState = _selectedRigidbody.isKinematic;
            _savedUseGravityState = _selectedRigidbody.useGravity;

            // Enable gravity and non-kinematic so we get collisions
            _selectedRigidbody.isKinematic = false;
            _selectedRigidbody.useGravity = false;

            // Distance from camera to object
            _zDistance = Vector3.Distance(mainCamera.transform.position, _selectedRigidbody.position);

            // Calculate where we clicked on the object
            Vector3 hitPoint = ray.GetPoint(_zDistance);
            _offset = _selectedRigidbody.position - hitPoint;

            // We are now dragging
            _isDragging = true;
        }
    }

    private void ContinueDrag()
    {
        // Read the current mouse position
        Vector2 mousePos = _pointerPosAction.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // Read scroll input (Vector2.y typically stores vertical scroll)
        Vector2 scrollValue = _scrollAction.ReadValue<Vector2>();
        float scrollDelta = scrollValue.y;
        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            // Adjust _zDistance to move the object forward or backward
            // Scroll up => positive scrollDelta => bring object closer (subtract from distance)
            // Scroll down => negative scrollDelta => push object farther (add to distance)

            _zDistance = Mathf.Clamp(_zDistance, minZDistance, maxZDistance);

            _zDistance -= scrollDelta * scrollSensitivity;
        }

        // Calculate the new target position for the rigidbody
        Vector3 pointOnRay = ray.GetPoint(_zDistance);
        _targetPosition = pointOnRay + _offset;
    }

    private void EndDrag()
    {
        // Restore the rigidbody’s original states
        if (_selectedRigidbody != null)
        {
            _selectedRigidbody.isKinematic = _savedKinematicState;
            _selectedRigidbody.useGravity = _savedUseGravityState;
        }

        _isDragging = false;
        _selectedRigidbody = null;

        OnDragEnd?.Invoke();
    }
}

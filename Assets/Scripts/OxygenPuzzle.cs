using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class OxygenPuzzle : PuzzleBase
{
    [Header("Sliders")]
    [SerializeField] private XRSlider slider1;
    [SerializeField] private XRSlider slider2;
    [SerializeField] private XRSlider slider3;

    [Header("Gauge Rotations")]
    [SerializeField] private GaugePivotRotation gaugePivot1;
    [SerializeField] private GaugePivotRotation gaugePivot2;
    [SerializeField] private GaugePivotRotation gaugePivot3;

    [Header("Target Zones (Unity Rotation)")]
    [SerializeField] private Vector2 gauge1Range = new Vector2(0f, 270f);   // Maps to your 90-360
    [SerializeField] private Vector2 gauge2Range = new Vector2(0f, 140f);   // Maps to your 90-230
    [SerializeField] private Vector2 gauge3Range = new Vector2(0f, 40f);    // Maps to your 90-130

    [Header("Randomness Settings")]
    [SerializeField] private float maxRandomnessStrength1 = 45f;
    [SerializeField] private float maxRandomnessStrength2 = 30f;
    [SerializeField] private float maxRandomnessStrength3 = 15f;
    [SerializeField] private float maxRandomnessSpeed = 3f;
    [SerializeField] private float difficultyRampUpTime = 20f;

    private bool hasPlayerMovedSliders = false;
    private float puzzleStartTime;
    private bool isCheckingCompletion = false;

    private void Start()
    {
        SetInitialState();
    }

    private void SetInitialState()
    {
        slider1.value = 0.5f;
        slider2.value = 0.45f;
        slider3.value = 0.3f;
        SetAllRandomness(0.1f, 0.5f);
        hasPlayerMovedSliders = false;
    }

    public override void InitializePuzzle()
    {
        base.InitializePuzzle();
        
        slider1.value = 0.1f;
        slider2.value = 0.9f;
        slider3.value = 0.7f;

        puzzleStartTime = Time.time;
        isCheckingCompletion = true;
        hasPlayerMovedSliders = false;
        
        Debug.Log("Oxygen Puzzle Started - Using Unity rotation system");
    }

    private void Update()
    {
        if (currentState != PuzzleState.InProgress) return;

        CheckForPlayerInput();
        UpdateDifficulty();
        CheckPuzzleCompletion();
    }

    private void CheckForPlayerInput()
    {
        if (!hasPlayerMovedSliders)
        {
            if (Mathf.Abs(slider1.value - 0.1f) > 0.01f ||
                Mathf.Abs(slider2.value - 0.9f) > 0.01f ||
                Mathf.Abs(slider3.value - 0.7f) > 0.01f)
            {
                hasPlayerMovedSliders = true;
                Debug.Log("Player started moving sliders!");
            }
        }
    }

    private void UpdateDifficulty()
    {
        float timeSinceStart = Time.time - puzzleStartTime;
        float difficultyProgress = Mathf.Clamp01(timeSinceStart / difficultyRampUpTime);

        float currentSpeed = Mathf.Lerp(0.5f, maxRandomnessSpeed, difficultyProgress);
        float strength1 = Mathf.Lerp(0.1f, maxRandomnessStrength1, difficultyProgress);
        float strength2 = Mathf.Lerp(0.1f, maxRandomnessStrength2, difficultyProgress);
        float strength3 = Mathf.Lerp(0.1f, maxRandomnessStrength3, difficultyProgress);

        gaugePivot1.SetRandomness(strength1, currentSpeed);
        gaugePivot2.SetRandomness(strength2, currentSpeed);
        gaugePivot3.SetRandomness(strength3, currentSpeed);
    }

    private void SetAllRandomness(float strength, float speed)
    {
        gaugePivot1.SetRandomness(strength, speed);
        gaugePivot2.SetRandomness(strength, speed);
        gaugePivot3.SetRandomness(strength, speed);
    }

    private void CheckPuzzleCompletion()
    {
        if (!isCheckingCompletion || !hasPlayerMovedSliders) return;

        float angle1 = gaugePivot1.transform.rotation.eulerAngles.x;
        float angle2 = gaugePivot2.transform.rotation.eulerAngles.x;
        float angle3 = gaugePivot3.transform.rotation.eulerAngles.x;

        bool allInTargetZone = 
            IsInRange(angle1, gauge1Range) &&
            IsInRange(angle2, gauge2Range) &&
            IsInRange(angle3, gauge3Range);

        //Debug.Log($"Angles - G1: {angle1:F2} ({gauge1Range.x}-{gauge1Range.y}), " +
        //          $"G2: {angle2:F2} ({gauge2Range.x}-{gauge2Range.y}), " +
        //          $"G3: {angle3:F2} ({gauge3Range.x}-{gauge3Range.y})");

        if (allInTargetZone)
        {
            Debug.Log("Oxygen Puzzle Completed!");
            CompletePuzzle();
            isCheckingCompletion = false;
            SetAllRandomness(0f, 0f);
        }
    }

    private bool IsInRange(float angle, Vector2 range)
    {
        // Normalize angle to 0-360 range
        angle = (angle + 360f) % 360f;
        
        // Standard range check
        if (range.y >= range.x)
        {
            return angle >= range.x && angle <= range.y;
        }
        // Handle wraparound case (like 350° to 10°)
        else
        {
            return angle >= range.x || angle <= range.y;
        }
    }
}
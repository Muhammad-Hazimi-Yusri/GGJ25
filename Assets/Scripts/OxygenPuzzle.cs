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

    [Header("Target Zones")]
    [SerializeField] private Vector2 gauge1TargetZone = new Vector2(0f, 45f);    // White zone around 20-25 degrees
    [SerializeField] private Vector2 gauge2TargetZone = new Vector2(340f, 380f);  // White zone around 358-360 degrees
    [SerializeField] private Vector2 gauge3TargetZone = new Vector2(60f, 75f);    // White zone around 65-70 degrees
    
    [Header("Randomness Settings")]
    [SerializeField] private float maxRandomnessStrength1 = 45f;
    [SerializeField] private float maxRandomnessStrength2 = 30f;
    [SerializeField] private float maxRandomnessStrength3 = 15f;
    [SerializeField] private float maxRandomnessSpeed = 3f;
    [SerializeField] private float difficultyRampUpTime = 20f;

    private float puzzleStartTime;
    private bool isCheckingCompletion = false;

    protected override void Start()
    {
        base.Start();
        SetInitialState();
    }

    private void SetInitialState()
    {
        slider1.value = 0.5f;
        slider2.value = 0.45f;
        slider3.value = 0.3f;
        SetAllRandomness(0.1f, 0.5f);
    }

    public override void InitializePuzzle()
    {
        base.InitializePuzzle();
        
        slider1.value = 0.1f;
        slider2.value = 0.9f;
        slider3.value = 0.7f;

        puzzleStartTime = Time.time;
        isCheckingCompletion = true;
        
        Debug.Log("Oxygen Puzzle Started");
    }

    private void Update()
    {
        if (currentState != PuzzleState.InProgress) return;

        UpdateDifficulty();
        CheckPuzzleCompletion();
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
        if (!isCheckingCompletion) return;

        float gauge1Angle = gaugePivot1.GetCurrentAngle();
        float gauge2Angle = gaugePivot2.GetCurrentAngle();
        float gauge3Angle = gaugePivot3.GetCurrentAngle();

        bool allInTargetZone = 
            IsInTargetZone(gauge1Angle, gauge1TargetZone) &&
            IsInTargetZone(gauge2Angle, gauge2TargetZone) &&
            IsInTargetZone(gauge3Angle, gauge3TargetZone);

        Debug.Log($"Angles - G1: {gauge1Angle:F2}, G2: {gauge2Angle:F2}, G3: {gauge3Angle:F2}");

        if (allInTargetZone)
        {
            Debug.Log("Oxygen Puzzle Completed!");
            CompletePuzzle();
            isCheckingCompletion = false;
            SetAllRandomness(0f, 0f);
        }
    }

    private bool IsInTargetZone(float angle, Vector2 targetZone)
    {
        // Normalize the angle to 0-360 range
        angle = angle % 360f;
        if (angle < 0) angle += 360f;

        // Normalize target zone values
        float minAngle = targetZone.x % 360f;
        float maxAngle = targetZone.y % 360f;

        // Handle wrap-around case (e.g., 340-380 becomes 340-20)
        if (maxAngle < minAngle)
        {
            return angle >= minAngle || angle <= (maxAngle % 360f);
        }

        return angle >= minAngle && angle <= maxAngle;
    }
}
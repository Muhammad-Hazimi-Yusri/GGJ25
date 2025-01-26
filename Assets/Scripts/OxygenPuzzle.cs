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
    [SerializeField] private float targetZoneMin = 0.45f; // White zone minimum
    [SerializeField] private float targetZoneMax = 0.55f; // White zone maximum
    
    [Header("Randomness Settings")]
    [SerializeField] private float maxRandomnessStrength1 = 15f;
    [SerializeField] private float maxRandomnessStrength2 = 10f;
    [SerializeField] private float maxRandomnessStrength3 = 5f;
    [SerializeField] private float maxRandomnessSpeed = 3f;
    [SerializeField] private float difficultyRampUpTime = 30f; // Time to reach max difficulty

    private float puzzleStartTime;
    private bool isCheckingCompletion = false;

    protected override void Start()
    {
        base.Start();
        SetInitialState();
    }

    private void SetInitialState()
    {
        // Set initial slider values
        slider1.value = 0.5f;
        slider2.value = 0.45f;
        slider3.value = 0.3f;

        // Set initial low randomness
        SetAllRandomness(0.1f, 0.5f);
    }

    public override void InitializePuzzle()
    {
        base.InitializePuzzle();
        
        // Set puzzle start values
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

        // Calculate current randomness values based on difficulty
        float currentSpeed = Mathf.Lerp(0.5f, maxRandomnessSpeed, difficultyProgress);
        float strength1 = Mathf.Lerp(0.1f, maxRandomnessStrength1, difficultyProgress);
        float strength2 = Mathf.Lerp(0.1f, maxRandomnessStrength2, difficultyProgress);
        float strength3 = Mathf.Lerp(0.1f, maxRandomnessStrength3, difficultyProgress);

        // Update gauge randomness
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

        bool allInTargetZone = 
            IsInTargetZone(slider1.value) &&
            IsInTargetZone(slider2.value) &&
            IsInTargetZone(slider3.value);

        if (allInTargetZone)
        {
            Debug.Log("Oxygen Puzzle Completed!");
            CompletePuzzle();
            isCheckingCompletion = false;
            SetAllRandomness(0f, 0f); // Stop randomness on completion
        }
    }

    private bool IsInTargetZone(float value)
    {
        return value >= targetZoneMin && value <= targetZoneMax;
    }
}
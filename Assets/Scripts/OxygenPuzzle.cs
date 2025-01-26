using UnityEngine;

public class ModifiedOxygenPuzzle : PuzzleBase
{
    [SerializeField] private PuzzleManager puzzleManager;

    [Header("Gauge References")]
    [SerializeField] private ModifiedGaugeRotation gaugePivot1;
    [SerializeField] private ModifiedGaugeRotation gaugePivot2;
    [SerializeField] private ModifiedGaugeRotation gaugePivot3;

    [Header("Target Ranges")]
    [SerializeField] private Vector2 gauge1Range = new Vector2(90f, 360f);
    [SerializeField] private Vector2 gauge2Range = new Vector2(90f, 230f);
    [SerializeField] private Vector2 gauge3Range = new Vector2(80f, 140f);

    [Header("Difficulty Settings")]
    [SerializeField] private float maxRandomnessStrength1 = 45f;
    [SerializeField] private float maxRandomnessStrength2 = 30f;
    [SerializeField] private float maxRandomnessStrength3 = 15f;
    [SerializeField] private float maxRandomnessSpeed = 3f;
    [SerializeField] private float difficultyRampUpTime = 20f;

    private float puzzleStartTime;
    private bool isCheckingCompletion = false;

    private void Start()
    {
        SetInitialState();
    }

    private void SetInitialState()
    {
        SetAllRandomness(0.1f, 0.5f);
    }

    public override void InitializePuzzle()
    {
        base.InitializePuzzle();
        puzzleStartTime = Time.time;
        isCheckingCompletion = true;
        
        // Enable randomness on all gauges
        gaugePivot1.EnableRandomness(true);
        gaugePivot2.EnableRandomness(true);
        gaugePivot3.EnableRandomness(true);
        
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

        bool gauge1InRange = gaugePivot1.IsAngleInRange(gauge1Range);
        bool gauge2InRange = gaugePivot2.IsAngleInRange(gauge2Range);
        bool gauge3InRange = gaugePivot3.IsAngleInRange(gauge3Range);

        // Debug information
/*         if (Debug.isDebugBuild)
        {
            Debug.Log($"Gauge 1: {gaugePivot1.GetCurrentAngle():F1}° - In Range: {gauge1InRange}");
            Debug.Log($"Gauge 2: {gaugePivot2.GetCurrentAngle():F1}° - In Range: {gauge2InRange}");
            Debug.Log($"Gauge 3: {gaugePivot3.GetCurrentAngle():F1}° - In Range: {gauge3InRange}");
        } */

        if (gauge1InRange && gauge2InRange && gauge3InRange)
        {
            Debug.Log("Oxygen Puzzle Completed!");
            puzzleManager.CompletePuzzle();
        }
    }

    public override void CompletePuzzle()
    {
        base.CompletePuzzle();
        // stop randomizing gauges
        gaugePivot1.EnableRandomness(false);
        gaugePivot2.EnableRandomness(false);
        gaugePivot3.EnableRandomness(false);
        isCheckingCompletion = false;
        currentState = PuzzleState.Completed;
    }
}
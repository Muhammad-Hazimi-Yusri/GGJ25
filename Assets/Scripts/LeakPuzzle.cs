using UnityEngine;

public class LeakPuzzle : PuzzleBase
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource[] leakAudioSources;
    [SerializeField] private AudioSource effectsAudioSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip waterLeakSound;
    [SerializeField] private AudioClip metalPlacementSound;
    
    [Header("Metal Sheet Settings")]
    [SerializeField] private Transform[] leakPoints;  // Positions where leaks/metal sheets should go
    [SerializeField] private GameObject metalSheetPrefab;  // Your metal sheet prefab
    [SerializeField] private float placementSnapDistance = 0.2f;  // How close sheet needs to be to snap
    
    private int currentLeakIndex = 0;
    private GameObject currentMetalSheet;
    private bool isHoldingSheet = false;
    
    protected override void Start()
    {
        foreach (var leakSource in leakAudioSources)
        {
            if (leakSource != null)
            {
                leakSource.clip = waterLeakSound;
                leakSource.loop = true;
                leakSource.spatialBlend = 1f;
                leakSource.Stop();
            }
        }
    }

    public override void InitializePuzzle()
    {
        base.InitializePuzzle();
        currentLeakIndex = 0;
        StartCurrentLeak();
    }

    private void StartCurrentLeak()
    {
        if (currentLeakIndex < leakAudioSources.Length)
        {
            leakAudioSources[currentLeakIndex].Play();
            SpawnNewMetalSheet();
        }
        else
        {
            CompletePuzzle();
        }
    }

    private void SpawnNewMetalSheet()
    {
        // Spawn metal sheet at your designated spawn point
        // This position should be where the mermaid's tube outputs items
        Vector3 spawnPoint = new Vector3(0, 0.3, 2.5); // Adjust this to your spawn point
        currentMetalSheet = Instantiate(metalSheetPrefab, spawnPoint, Quaternion.identity);
    }

    // Call this from your VR input system when grab button is pressed
    public void OnGrabSheet()
    {
        if (currentMetalSheet != null && Vector3.Distance(GetControllerPosition(), currentMetalSheet.transform.position) < placementSnapDistance)
        {
            isHoldingSheet = true;
            // Attach sheet to VR controller
            currentMetalSheet.transform.SetParent(GetControllerTransform());
            currentMetalSheet.transform.localPosition = Vector3.zero;
        }
    }

    // Call this from your VR input system when grab button is released
    public void OnReleaseSheet()
    {
        if (isHoldingSheet && currentMetalSheet != null)
        {
            isHoldingSheet = false;
            currentMetalSheet.transform.SetParent(null);
            
            // Check if near current leak point
            if (Vector3.Distance(currentMetalSheet.transform.position, leakPoints[currentLeakIndex].position) < placementSnapDistance)
            {
                // Correct placement
                currentMetalSheet.transform.position = leakPoints[currentLeakIndex].position;
                currentMetalSheet.transform.rotation = leakPoints[currentLeakIndex].rotation;
                OnMetalSheetPlaced(true);
            }
            else
            {
                // Wrong placement
                OnMetalSheetPlaced(false);
            }
        }
    }

    private void OnMetalSheetPlaced(bool correctPlacement)
    {
        if (effectsAudioSource != null && metalPlacementSound != null)
        {
            effectsAudioSource.PlayOneShot(metalPlacementSound);
        }
        
        if (correctPlacement)
        {
            // Stop current leak sound
            leakAudioSources[currentLeakIndex].Stop();
            
            // Move to next leak
            currentLeakIndex++;
            StartCurrentLeak();
        }
    }

    // These methods need to be implemented based on your VR system (Oculus, SteamVR, etc.)
    private Vector3 GetControllerPosition()
    {
        // Return your VR controller position
        return Vector3.zero; // Implement this
    }

    private Transform GetControllerTransform()
    {
        // Return your VR controller transform
        return null; // Implement this
    }

    public override void CompletePuzzle()
    {
        foreach (var leakSource in leakAudioSources)
        {
            if (leakSource != null)
            {
                leakSource.Stop();
            }
        }
        
        base.CompletePuzzle();
        Debug.Log("Leak Puzzle Completed!");
    }
}
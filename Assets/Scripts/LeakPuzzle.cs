using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LeakPuzzle : PuzzleBase
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource[] leakAudioSources;  // These will also serve as leak points
    [SerializeField] private AudioSource effectsAudioSource;
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip waterLeakSound;
    [SerializeField] private AudioClip metalPlacementSound;
    
    [Header("Metal Sheet Settings")]
    [SerializeField] private GameObject metalSheetPrefab;
    [SerializeField] private Transform spawnPoint;  // Where metal sheets appear from tube
    [SerializeField] private float placementSnapDistance = 0.2f;
    
    private int currentLeakIndex = 0;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable currentMetalSheet;
    
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
        GameObject sheetObj = Instantiate(metalSheetPrefab, spawnPoint.position, spawnPoint.rotation);
        currentMetalSheet = sheetObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        if (currentMetalSheet != null)
        {
            // Subscribe to select/deselect events
            currentMetalSheet.selectExited.AddListener(OnSheetReleased);
        }
    }

    private void OnSheetReleased(SelectExitEventArgs args)
    {
        if (currentMetalSheet != null)
        {
            // Check if near current leak point
            float distanceToLeak = Vector3.Distance(
                currentMetalSheet.transform.position, 
                leakAudioSources[currentLeakIndex].transform.position
            );

            if (distanceToLeak < placementSnapDistance)
            {
                // Correct placement
                currentMetalSheet.transform.position = leakAudioSources[currentLeakIndex].transform.position;
                currentMetalSheet.transform.rotation = leakAudioSources[currentLeakIndex].transform.rotation;
                
                // Disable further interaction with this sheet
                currentMetalSheet.enabled = false;
                if (currentMetalSheet.GetComponent<Rigidbody>() is Rigidbody rb)
                {
                    rb.isKinematic = true;
                }
                
                OnMetalSheetPlaced(true);
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